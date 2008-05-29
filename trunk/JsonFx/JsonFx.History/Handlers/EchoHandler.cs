using System;
using System.Web;

namespace JsonFx.Handlers
{
	/// <summary>
	/// HttpHandler which simply echos back the query string
	/// </summary>
	public class EchoHandler : System.Web.IHttpHandler
	{
		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			string response = context.Request.Url.Query;

			context.Response.Clear();

			// check if client has cached copy
			ETag etag = new StringETag(response);
			if (!etag.HandleETag(context))
			{
				context.Response.ContentType = "text/plain";
				context.Response.AddHeader("Content-Disposition", "inline;filename=Echo.txt");
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
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}