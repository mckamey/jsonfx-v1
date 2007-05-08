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
using BuildTools.CssCompactor;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
	public class CssCompactorBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Fields

		private string descriptorTypeName = null;

		#endregion Fields

		#region Init

		public CssCompactorBuildProvider()
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
			using (TextReader reader = this.OpenCssSource())
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

			CssCodeProvider provider = assemblyBuilder.CodeDomProvider as CssCodeProvider;
			if (provider != null)
			{
				provider.SetVirtualPath(base.VirtualPath);
				provider.SetCss(sourceText);
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

			#region CssHandlerInfo.ResourceName

			// add a readonly property with the resource name
			CodeMemberProperty resourceProp = new CodeMemberProperty();
			resourceProp.Name = "ResourceName";
			resourceProp.Type = new CodeTypeReference(typeof(String));
			resourceProp.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			resourceProp.HasGet = true;
			// get { return resourceName; }
			resourceProp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(resourceName)));
			descriptorType.Members.Add(resourceProp);

			#endregion CssHandlerInfo.ResourceName

			assemblyBuilder.AddCodeCompileUnit(this, generatedUnit);

			this.descriptorTypeName = ResourceHandlerInfo.GeneratedNamespace+"."+descriptorType.Name;
		}

		public override CompilerType CodeCompilerType
		{
			get { return base.GetDefaultCompilerTypeForLanguage("StyleSheet"); }
		}

		#endregion BuildProvider Methods

		#region CssCompactorBuildProvider Methods

		protected virtual TextReader OpenCssSource()
		{
			return base.OpenReader();
		}

		#endregion CssCompactorBuildProvider Methods
	}

	public class CssCodeProvider : Microsoft.CSharp.CSharpCodeProvider
	{
		#region Fields

		string virtualPath = null;
		string sourceText = null;

		#endregion Fields

		#region Methods

		protected internal void SetVirtualPath(string path)
		{
			this.virtualPath = path;
		}

		protected internal void SetCss(string source)
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
				errors = CssCompactor.Compact(this.virtualPath, this.sourceText, writer, null, null, CssCompactor.Options.None);
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

		protected virtual void Compact(string virtualPath, string sourceText)
		{
			List<ParseException> errors;
			try
			{
				StringWriter writer = new StringWriter(new StringBuilder(sourceText.Length));
				errors = CssCompactor.Compact(virtualPath, sourceText, writer, null, null, CssCompactor.Options.None);

				string compactedText = writer.ToString();
			}
			catch (ParseException ex)
			{
				errors = new List<ParseException>();
				errors.Add(ex);
			}
		}

		#endregion Compaction Methods
	}
}
