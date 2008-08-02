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
using System.Web.UI;

namespace JsonFx.Handlers
{
	/// <summary>
	/// A very simple handler which just wraps the requested file as JSONP.
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

				// TODO: secure this?
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
}