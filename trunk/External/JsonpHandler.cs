using System;
using System.IO;
using System.Web;
using System.Web.UI;

/// <summary>
/// A very simple handler which wraps the requested file as JSONP.
/// </summary>
public class JsonpHandler : IHttpHandler
{
	#region IHttpHandler Members

	void IHttpHandler.ProcessRequest(HttpContext context)
	{
		context.Response.Clear();
		context.Response.ClearContent();
		context.Response.ClearHeaders();
		context.Response.ContentEncoding = System.Text.Encoding.UTF8;
		context.Response.AddHeader("Content-Disposition", "inline;filename=jsonp.txt");
		context.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

		try
		{
			string jsonp = context.Request.QueryString["jsonp"];
			bool isJsonp = !String.IsNullOrEmpty(jsonp);
			if (isJsonp)
			{
				context.Response.Output.Write(jsonp);
				context.Response.Output.Write('(');
			}

			// TODO: secure this!
			context.Response.TransmitFile(context.Request.PhysicalPath);

			if (isJsonp)
			{
				context.Response.Output.Write(");");
			}
		}
		finally
		{
			if (context != null && context.ApplicationInstance != null)
			{
				// prevents "Transfer-Encoding: Chunked" header which chokes IE6 (unlike Response.Flush/Close)
				// and prevents ending response too early (unlike Response.End)
				context.ApplicationInstance.CompleteRequest();
			}
		}
	}

	bool IHttpHandler.IsReusable
	{
		get { return true; ; }
	}

	#endregion IhhtpHandler Members
}
