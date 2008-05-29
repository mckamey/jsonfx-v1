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
	}
}