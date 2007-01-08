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
	public class JsonFxScriptBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Init

		public JsonFxScriptBuildProvider()
		{
		}

		#endregion Init

		#region BuildProvider Methods

		public override string GetCustomString(CompilerResults results)
		{
			string virtualPath = base.VirtualPath;
			string script = Path.GetFileNameWithoutExtension(virtualPath)+".js";
			Assembly assembly = Assembly.GetAssembly(typeof(JsonFx.Scripts.ClientScript));
			using (Stream input = assembly.GetManifestResourceStream(JsonFx.Scripts.ClientScript.ScriptPath+script))
			{
				using (StreamReader reader = new StreamReader(input))
				{
					return reader.ReadToEnd();
				}
			}
		}

		#endregion BuildProvider Methods
	}
}
