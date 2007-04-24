using System;
using System.Reflection;
using System.Web.Compilation;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.IO;

using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public class JsonFxScriptBuildProvider : JsonFx.Compilation.ScriptCompactorBuildProvider
	{
		#region Init

		public JsonFxScriptBuildProvider()
		{
		}

		#endregion Init

		#region ScriptCompactorBuildProvider Methods

		protected override TextReader OpenScriptSource()
		{
			string script = Path.GetFileNameWithoutExtension(base.VirtualPath)+ClientScriptHandler.JavaScriptExtension;
			Assembly assembly = Assembly.GetAssembly(typeof(ClientScriptHandler));
			Stream input = assembly.GetManifestResourceStream(ClientScriptHandler.ScriptPath+script);
			return new StreamReader(input);
		}

		protected override bool ForceCompaction
		{
			get { return true; }
		}

		#endregion ScriptCompactorBuildProvider Methods
	}
}
