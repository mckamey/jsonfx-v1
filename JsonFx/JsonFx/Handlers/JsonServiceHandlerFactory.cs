using System;
using System.Web;
using System.Web.Compilation;

using JsonFx.Services;

namespace JsonFx.Handlers
{
	public class JsonServiceHandlerFactory : System.Web.IHttpHandlerFactory
	{
		#region IHttpHandlerFactory Methods

		IHttpHandler IHttpHandlerFactory.GetHandler(HttpContext context, string verb, string url, string path)
		{
			Type descriptorType = BuildManager.GetCompiledType(url);
			JsonServiceInfo serviceInfo = BuildManager.CreateInstanceFromVirtualPath(url, descriptorType) as JsonServiceInfo;

			if ("GET".Equals(verb, StringComparison.InvariantCultureIgnoreCase) &&
				String.IsNullOrEmpty(context.Request.PathInfo) &&
				String.IsNullOrEmpty(context.Request.Url.Query))
			{
				// output service javascript proxy
				return new JsonFx.Handlers.JsonServiceProxyHandler(serviceInfo, url);
			}

			// handle service requests
			return new JsonFx.Handlers.JsonServiceHandler(serviceInfo, url);
		}

		void IHttpHandlerFactory.ReleaseHandler(IHttpHandler handler)
		{
		}

		#endregion IHttpHandlerFactory Methods
	}
}
