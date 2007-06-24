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
			ETag eTag = new StringETag(response);
			if (!eTag.HandleETag(context))
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

			// this safely ends request without causing "Transfer-Encoding: Chunked" which chokes IE6
			context.ApplicationInstance.CompleteRequest();
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}