using System;
using System.Web;

using JsonFx.Services;
using JsonFx.Services.Discovery;
using JsonFx.Services.Proxy;

namespace JsonFx.Handlers
{
	internal class JsonServiceProxyHandler : System.Web.IHttpHandler
	{
		#region Fields

		private JsonServiceInfo serviceInfo;
		private string serviceUrl;

		#endregion Fields

		#region Init

		public JsonServiceProxyHandler(JsonServiceInfo serviceInfo, string serviceUrl)
		{
			this.serviceInfo = serviceInfo;
			this.serviceUrl = serviceUrl;
		}

		#endregion Init

		#region IHttpHandler Members

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			bool isDebug = "debug".Equals(context.Request.QueryString[null], StringComparison.InvariantCultureIgnoreCase);

			context.Response.Clear();
			context.Response.BufferOutput = true;

			// check if client has cached copy
			ETag etag = new EmbeddedResourceETag(// should this be StringETag?
				this.serviceInfo.ServiceType.Assembly,
				this.serviceInfo.ServiceType.FullName);
			if (!etag.HandleETag(context, isDebug))
			{
				context.Response.ContentEncoding = System.Text.Encoding.UTF8;
				context.Response.ContentType = "application/javascript";

				context.Response.AppendHeader(
					"Content-Disposition",
					String.Format("inline;filename={0}.js", this.serviceInfo.ServiceType.FullName));

				string proxyScript = isDebug ? this.serviceInfo.DebugProxy :  this.serviceInfo.Proxy;
				if (String.IsNullOrEmpty(proxyScript))
				{
					// if wasn't generated, generate on the fly with reflection
					JsonServiceDescription desc = new JsonServiceDescription(this.serviceInfo.ServiceType, this.serviceUrl);
					JsonServiceProxyGenerator proxy = new JsonServiceProxyGenerator(desc);
					proxy.OutputProxy(context.Response.Output, isDebug);
				}
				else
				{
					// use generated code
					context.Response.Output.Write(proxyScript);
				}
				context.Response.Output.Write(this.serviceUrl);
				context.Response.Output.Write(JsonServiceProxyGenerator.ProxyEnd);
				if (isDebug)
				{
					context.Response.Output.WriteLine();
				}
			}
		}

		#endregion IHttpHandler Members
	}
}
