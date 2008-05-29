#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Web;

using JsonFx.JsonRpc;
using JsonFx.JsonRpc.Discovery;
using JsonFx.JsonRpc.Proxy;

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
			if (!etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
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
