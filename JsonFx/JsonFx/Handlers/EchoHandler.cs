using System;
using System.Web;

namespace JsonFx.Handlers
{
	public class EchoHandler : System.Web.IHttpHandler
	{
		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			context.Response.Clear();
			context.Response.ContentType = "text/plain";
			context.Response.AddHeader("Content-Disposition", "inline;filename=Echo.txt");
			string response = context.Request.Url.Query;
			if (!String.IsNullOrEmpty(response))
			{
				context.Response.Write(HttpUtility.UrlDecode(context.Request.Url.Query.Substring(1)));
			}
			else
			{
				// this is needed for Firefox
				context.Response.Write("\0");
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}