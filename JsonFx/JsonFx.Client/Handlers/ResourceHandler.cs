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
using System.IO;
using System.Web;
using System.Reflection;
using System.Text;
using System.Web.Compilation;

namespace JsonFx.Handlers
{
	public class ResourceHandler : IHttpHandler
	{
		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			bool isDebug = "debug".Equals(context.Request.QueryString[null], StringComparison.InvariantCultureIgnoreCase);

			context.Response.ClearHeaders();
			context.Response.BufferOutput = true;

			// specifying "DEBUG" in the query string gets the non-compacted form
			CompiledBuildResult info = this.GetResourceInfo(context, isDebug);
			if (info == null)
			{
				// either eTag 304 was sent or no resource found
				return;
			}

			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.ContentType = info.ContentType;

			context.Response.AppendHeader(
				"Content-Disposition",
				"inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+'.'+info.FileExtension);

			TextWriter writer = context.Response.Output;
			if (isDebug)
			{
//				context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				writer.Write(info.Resource);
			}
			else
			{
				writer.Write(info.CompactedResource);
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members

		#region ResourceHandler Members

		/// <summary>
		/// Determines the appropriate source for the incomming request
		/// </summary>
		/// <param name="context"></param>
		/// <param name="isDebug"></param>
		/// <returns>ResourceHandlerInfo</returns>
		protected virtual CompiledBuildResult GetResourceInfo(HttpContext context, bool isDebug)
		{
			string virtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
			CompiledBuildResult info = CompiledBuildResult.Create(virtualPath);
			if (info == null)
			{
				return null;
			}

			// check if client has cached copy
			Type resourceType = info.GetType();
			ETag etag = new EmbeddedResourceETag(// should this be StringETag?
				resourceType.Assembly,
				resourceType.FullName);
			if (etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
			{
				return null;
			}

			return info;
		}

		#endregion ResourceHandler Members
	}
}