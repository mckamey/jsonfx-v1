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
			context.Response.ClearHeaders();
			context.Response.ContentType = "application/javascript";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.AddHeader("Content-Disposition", String.Format("inline;filename={0}.js", this.serviceInfo.ServiceType.FullName));

			string proxyScript = this.serviceInfo.Proxy;
			if (String.IsNullOrEmpty(proxyScript))
			{
				// if wasn't generated, generate on the fly with reflection
				JsonServiceDescription desc = new JsonServiceDescription(this.serviceInfo.ServiceType, this.serviceUrl);
				JsonServiceProxy proxy = new JsonServiceProxy(desc, this.serviceInfo.ServiceType.Namespace);
				proxy.OutputProxy(context.Response.Output);
			}
			else
			{
				// use generated code
				context.Response.Output.Write(proxyScript);
			}
		}

		#endregion IHttpHandler Members
	}
}
