using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace JsonFx.Handlers
{
	public class ClientScriptHandler : ResourceHandler
	{
		#region Constants

		// full source scripts: good for dev and debugging
		internal const string ScriptPath = "JsonFx.Scripts.";

		// compacted scripts: better for distribution and speed
		internal const string CompactedScriptPath = "JsonFx.Scripts.Compacted.";

		#endregion Constants

		#region Properties

		protected override string ResourceContentType
		{
			get { return "application/javascript"; }
		}

		protected override string  ResourceExtension
		{
			get { return ".js"; }
		}

		#endregion Properties

		#region Handler Members

		protected override Stream GetResourceStream(HttpContext context, bool isDebug)
		{
			if (context.Request.FilePath.EndsWith(this.ResourceExtension, StringComparison.InvariantCultureIgnoreCase))
			{
				return base.GetResourceStream(context, isDebug);
			}

			string virtualPath = context.Request.FilePath;
			string script = isDebug ? ClientScriptHandler.ScriptPath : ClientScriptHandler.CompactedScriptPath;
			script += Path.GetFileNameWithoutExtension(virtualPath)+this.ResourceExtension;

			Assembly assembly = Assembly.GetAssembly(typeof(ClientScriptHandler));
			Stream input = assembly.GetManifestResourceStream(script);
			if (input == null)
			{
				return base.GetResourceStream(context, isDebug);
			}

			return input;
		}

		#endregion Handler Members
	}
}