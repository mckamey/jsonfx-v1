using System;
using System.Reflection;
using System.Web.Compilation;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.IO;

using BuildTools;
using BuildTools.ScriptCompactor;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public class ScriptCompactorBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Init

		public ScriptCompactorBuildProvider()
		{
		}

		#endregion Init

		#region BuildProvider Methods

		public override string GetCustomString(CompilerResults results)
		{
			try
			{
				JSLint jsLint = new JSLint();
				using (TextReader reader = this.OpenScriptSource())
				{
					jsLint.Run(reader, base.VirtualPath);
				}
			}
			catch (ParseException ex)
			{
				Console.Error.WriteLine(ex.GetCompilerMessage(this.ForceCompaction));
				if (!this.ForceCompaction)
				{
					throw new System.Web.HttpParseException(ex.Message, null/*ex*/, ex.File, ex.Source, ex.Line);
				}
				//if (results == null)
				//{
				//    results = new System.CodeDom.Compiler.CompilerResults(null);
				//}
				//System.CodeDom.Compiler.CompilerError error = new System.CodeDom.Compiler.CompilerError(ex.FilePath, ex.Line, ex.Column, "", ex.Message);
				//error.IsWarning = true;
				//results.Errors.Add(error);
				//throw new System.Web.HttpCompileException(results, ex.SourceCode);
				Console.Error.WriteLine("Failed to validate "+base.VirtualPath+"!!Compaction forced: need to manually check for errors.");
			}
			catch (Exception ex)
			{
				if (!this.ForceCompaction)
				{
					throw new System.Web.HttpParseException(ex.Message, ex, base.VirtualPath, null, 0);
				}

				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine("Failed to validate "+base.VirtualPath+"!!Compaction forced: need to manually check for errors.");
			}

			try
			{
				JSMin jsMin = new JSMin();
				using (TextReader reader = this.OpenScriptSource())
				{
					using (StringWriter writer = new StringWriter())
					{
						jsMin.Run(reader, writer);
						writer.Flush();
						return writer.ToString();
					}
				}
			}
			catch (Exception ex)
			{
				throw new System.Web.HttpParseException(ex.Message, ex, base.VirtualPath, null, 0);
			}
		}

		#endregion BuildProvider Methods

		#region ScriptCompactorBuildProvider Methods

		protected virtual bool ForceCompaction
		{
			get { return false; }
		}

		protected virtual TextReader OpenScriptSource()
		{
			return base.OpenReader();
		}

		#endregion ScriptCompactorBuildProvider Methods
	}
}
