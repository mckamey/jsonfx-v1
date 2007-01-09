using System;
using System.Reflection;
using System.Web.Compilation;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.IO;

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
				ScriptCompactor.JSLint jsLint = new ScriptCompactor.JSLint();
				using (TextReader reader = this.OpenScriptSource())
				{
					jsLint.Run(reader);
				}
			}
			catch (Exception ex)
			{
				int line = 0;
				Match match = Regex.Match(ex.Message, "Lint at line (?<Line>[\\d]+) character (?<Char>[\\d]+)", RegexOptions.ExplicitCapture|RegexOptions.Compiled);
				if (match.Success)
				{
					string lineStr = match.Groups["Line"].Value;
					Int32.TryParse(lineStr, out line);
				}
				throw new System.Web.HttpParseException(ex.Message, ex, base.VirtualPath, null, line);
			}

			try
			{
				ScriptCompactor.JSMinifier jsMin = new ScriptCompactor.JSMinifier();
				using (TextReader reader = this.OpenScriptSource())
				{
					using (StringWriter writer = new StringWriter())
					{
						jsMin.Minify(reader, writer, false, null);
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

		protected virtual TextReader OpenScriptSource()
		{
			return base.OpenReader();
		}

		#endregion ScriptCompactorBuildProvider Methods
	}
}
