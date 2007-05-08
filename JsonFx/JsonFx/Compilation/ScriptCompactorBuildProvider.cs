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
using BuildTools.ScriptCompactor;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
	public class ScriptCompactorBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Fields

		private string descriptorTypeName = null;

		#endregion Fields

		#region Init

		public ScriptCompactorBuildProvider()
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
			using (TextReader reader = this.OpenScriptSource())
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

			ScriptCodeProvider provider = assemblyBuilder.CodeDomProvider as ScriptCodeProvider;
			if (provider != null)
			{
				provider.SetVirtualPath(base.VirtualPath);
				provider.SetScript(sourceText);
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

			#region HandlerInfo.ResourceName

			// add a readonly property with the resource name
			CodeMemberProperty resourceProp = new CodeMemberProperty();
			resourceProp.Name = "ResourceName";
			resourceProp.Type = new CodeTypeReference(typeof(String));
			resourceProp.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			resourceProp.HasGet = true;
			// get { return resourceName; }
			resourceProp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(resourceName)));
			descriptorType.Members.Add(resourceProp);

			#endregion HandlerInfo.ResourceName

			assemblyBuilder.AddCodeCompileUnit(this, generatedUnit);

			this.descriptorTypeName = ResourceHandlerInfo.GeneratedNamespace+"."+descriptorType.Name;
		}

		public override CompilerType CodeCompilerType
		{
			get { return base.GetDefaultCompilerTypeForLanguage("JavaScript"); }
		}

		#endregion BuildProvider Methods

		#region ScriptCompactorBuildProvider Methods

		protected virtual TextReader OpenScriptSource()
		{
			return base.OpenReader();
		}

		#endregion ScriptCompactorBuildProvider Methods
	}

	public class ScriptCodeProvider : Microsoft.CSharp.CSharpCodeProvider
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

		protected internal void SetScript(string source)
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
				errors = ScriptCompactor.Compact(this.virtualPath, this.sourceText, writer, null, null, ScriptCompactor.Options.None);
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
				errors = ScriptCompactor.Compact(virtualPath, sourceText, writer, null, null, ScriptCompactor.Options.None);

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
