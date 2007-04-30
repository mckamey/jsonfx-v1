using System;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
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
		#region Init

		public CssCompactorBuildProvider()
		{
		}

		#endregion Init

		#region BuildProvider Methods

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			string sourceText;
			using (TextReader reader = this.OpenCssSource())
			{
				sourceText = reader.ReadToEnd();
			}

			using (Stream stream = assemblyBuilder.CreateEmbeddedResource(this, CssHandler.GetEmbeddedResourceName(base.VirtualPath)))
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
		}

		public override CompilerType CodeCompilerType
		{
			get { return base.GetDefaultCompilerTypeForLanguage("CSS"); }
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

		protected string GetCompactedResourceName(string resource)
		{
			return Path.Combine(Path.GetDirectoryName(resource),
				CssHandler.CompactedCssPath+Path.GetFileName(resource));
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

			string compactedPath = this.GetCompactedResourceName(options.EmbeddedResources[0]);
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
