using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace JsonFx.Handlers
{
	public class JsonFxScriptHandler : ClientScriptHandler
	{
		#region Handler Members

		protected override Stream GetResourceStream(HttpContext context, bool isDebug)
		{
			if (context.Request.FilePath.EndsWith(this.ResourceExtension, StringComparison.InvariantCultureIgnoreCase))
			{
				return base.GetResourceStream(context, isDebug);
			}

			string virtualPath = context.Request.FilePath;
			string resourcePath = isDebug ? ClientScriptHandler.ScriptPath : ClientScriptHandler.CompactedScriptPath;
			resourcePath += Path.GetFileNameWithoutExtension(virtualPath)+this.ResourceExtension;

			Assembly assembly = Assembly.GetAssembly(typeof(ClientScriptHandler));
			ManifestResourceInfo info = assembly.GetManifestResourceInfo(resourcePath);
			if (info == null)
			{
				// file does not exist in Assembly
				throw new FileNotFoundException("Invalid file path");
			}

			// check if client has cached copy
			ETag etag = new EmbeddedResourceETag(assembly, resourcePath);
			if (etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
			{
				return Stream.Null;
			}

			return assembly.GetManifestResourceStream(resourcePath);
		}

		#endregion Handler Members
	}
}