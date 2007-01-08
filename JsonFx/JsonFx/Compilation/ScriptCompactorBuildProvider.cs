using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Web.RegularExpressions;
using System.IO;

using JsonFx.Services;
using JsonFx.Services.Discovery;
using JsonFx.Services.Proxy;

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

		public override System.Collections.ICollection VirtualPathDependencies
		{
			get { return base.VirtualPathDependencies; }
		}

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			base.GenerateCode(assemblyBuilder);
		}

		public override CompilerType CodeCompilerType
		{
			get { return base.CodeCompilerType; }
		}
		
		public override string GetCustomString(CompilerResults results)
		{
			try
			{
#warning Hard coded script path
				ScriptCompactor.JSLint jsLint = new ScriptCompactor.JSLint(@"D:\Dev\SMM-VSS\PseudoCode\Dev\JsonFx\SolnItems\ScriptCompactor\jslint_wsh.js");
				using (TextReader reader = base.OpenReader())
				{
					jsLint.Run(reader);
				}
			}
			catch (Exception ex)
			{
				Match match = Regex.Match(ex.Message, "Lint at line (?<Line>[\\d]+) character (?<Col>[\\d]+)", RegexOptions.ExplicitCapture|RegexOptions.Compiled);
				if (match.Success)
				{
					string lineStr = match.Groups["Line"].Value;
					int line = 0;
					if (Int32.TryParse(lineStr, out line))
						throw new System.Web.HttpParseException(ex.Message, ex, base.VirtualPath, null, line);
					else
						throw new System.Web.HttpParseException(ex.Message, ex);
				}
				else
				{
					throw new System.Web.HttpParseException(ex.Message, ex);
				}
			}
#if DEBUG
			ScriptCompactor.JSMinifier jsMin = new ScriptCompactor.JSMinifier();
			using (TextReader reader = base.OpenReader())
			{
				using (StringWriter writer = new StringWriter())
				{
					jsMin.Minify(reader, writer, false, null);
					writer.Flush();
					return writer.ToString();
				}
			}
#else
			return base.GetCustomString(results);
#endif
		}

		public override Type GetGeneratedType(CompilerResults results)
		{
			return base.GetGeneratedType(results);
		}

		public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
		{
			return base.GetResultFlags(results);
		}

		#endregion BuildProvider Methods
	}
}
