using System;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text;
using System.IO;
using System.Collections.Generic;

using BuildTools;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
	public class CompactorBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Fields

		private string descriptorTypeName = null;

		#endregion Fields

		#region Init

		public CompactorBuildProvider()
		{
		}

		#endregion Init

		#region BuildProvider Methods

		public override Type GetGeneratedType(CompilerResults results)
		{
			return results.CompiledAssembly.GetType(this.descriptorTypeName);
		}

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			string sourceText;
			using (TextReader reader = base.OpenReader())
			{
				sourceText = reader.ReadToEnd();
			}

			string resourceName = ResourceHandlerInfo.GetEmbeddedResourceName(base.VirtualPath);
			using (Stream stream = assemblyBuilder.CreateEmbeddedResource(this, resourceName))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.Write(sourceText);
				}
			}

			CompactorCodeProvider provider = assemblyBuilder.CodeDomProvider as CompactorCodeProvider;
			if (provider != null)
			{
				provider.SetVirtualPath(base.VirtualPath);
				provider.SetSource(sourceText);
			}

			// generate a static class
			CodeCompileUnit generatedUnit = new CodeCompileUnit();
			CodeNamespace ns = new CodeNamespace(ResourceHandlerInfo.GeneratedNamespace);
			generatedUnit.Namespaces.Add(ns);
			CodeTypeDeclaration descriptorType = new CodeTypeDeclaration();
			descriptorType.IsClass = true;
			descriptorType.Name = "_"+Guid.NewGuid().ToString("N");
			descriptorType.Attributes = MemberAttributes.Public|MemberAttributes.Final;
			descriptorType.BaseTypes.Add(typeof(ResourceHandlerInfo));
			ns.Types.Add(descriptorType);

			#region ResourceHandlerInfo.ResourceName

			// add a readonly property with the resource name
			CodeMemberProperty resourceProp = new CodeMemberProperty();
			resourceProp.Name = "ResourceName";
			resourceProp.Type = new CodeTypeReference(typeof(String));
			resourceProp.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			resourceProp.HasGet = true;
			// get { return resourceName; }
			resourceProp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(resourceName)));
			descriptorType.Members.Add(resourceProp);

			#endregion ResourceHandlerInfo.ResourceName

			assemblyBuilder.AddCodeCompileUnit(this, generatedUnit);

			this.descriptorTypeName = ResourceHandlerInfo.GeneratedNamespace+"."+descriptorType.Name;
		}

		public override CompilerType CodeCompilerType
		{
			get
			{
				string extension = Path.GetExtension(base.VirtualPath);
				if (extension == null || extension.Length < 2)
				{
					return base.CodeCompilerType;
				}
				return base.GetDefaultCompilerTypeForLanguage(extension.Substring(1));
			}
		}

		#endregion BuildProvider Methods
	}

	public abstract class CompactorCodeProvider : Microsoft.CSharp.CSharpCodeProvider
	{
		#region Fields

		private string virtualPath = null;
		private string sourceText = null;

		#endregion Fields

		#region Methods

		protected internal void SetVirtualPath(string path)
		{
			this.virtualPath = path;
		}

		protected internal void SetSource(string source)
		{
			this.sourceText = source;
		}

		#endregion Methods

		#region CodeDomProvider Members

		public override CompilerResults CompileAssemblyFromFile(CompilerParameters options, params string[] fileNames)
		{
			List<ParseException> errors;
			string compactedText;

			using (StringWriter writer = new StringWriter(new StringBuilder(this.sourceText.Length)))
			{
				errors = this.Compact(this.virtualPath, this.sourceText, writer);
				writer.Flush();
				compactedText = writer.ToString();
			}

			string compactedPath = ResourceHandlerInfo.GetCompactedResourceName(options.EmbeddedResources[0]);
			using (StreamWriter writer = File.CreateText(compactedPath))
			{
				writer.Write(compactedText);
				writer.Flush();
				options.EmbeddedResources.Add(compactedPath);
			}

			CompilerResults results = base.CompileAssemblyFromFile(options, fileNames);

			foreach (ParseException error in errors)
			{
				results.Errors.Add(new CompilerError(error.File, error.Line, error.Column, error.ErrorCode, error.Message));
			}

			return results;
		}

		#endregion CodeDomProvider Members

		#region Compaction Methods

		protected abstract List<ParseException> Compact(string virtualPath, string sourceText, TextWriter writer);

		#endregion Compaction Methods
	}
}
