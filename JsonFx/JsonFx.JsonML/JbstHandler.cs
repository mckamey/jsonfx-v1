using System;
using System.IO;
using System.Web;
using System.Web.UI;

using JsonFx.JsonML.Builder;

namespace JsonFx.JsonML
{
	public class JbstHandler : IHttpHandler
	{
		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			context.Response.Clear();
			context.Response.ClearContent();
			context.Response.ClearHeaders();
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.AddHeader("Content-Disposition", "inline;filename=jbst.txt");
			context.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

			try
			{
				string jsonp = context.Request.QueryString["jsonp"];
				if (!String.IsNullOrEmpty(jsonp))
				{
					context.Response.Output.Write(jsonp);
				}
				context.Response.Output.Write('(');

				JsonControlBuilder builder = new JsonControlBuilder(context.Response.Output);
				builder.AllowLiteralsInRoot = false;

				// TODO: secure this!
				builder.Parse(File.ReadAllText(context.Request.MapPath(context.Request.FilePath)));

				context.Response.Output.Write(");");

				builder.Flush();
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
}