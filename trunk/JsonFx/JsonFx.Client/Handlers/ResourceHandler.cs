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
		#region Constants

		public const string DebugSetting = "debug";
		public const string GlobalizationSetting = "/i18n";

		#endregion Constants

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			string cacheKey = context.Request.QueryString[null];
			bool isDebug = ResourceHandler.DebugSetting.Equals(cacheKey, StringComparison.InvariantCultureIgnoreCase);

			context.Response.ClearHeaders();
			context.Response.BufferOutput = true;

			// specifying "DEBUG" in the query string gets the non-compacted form
			CompiledBuildResult info = this.GetResourceInfo(context, isDebug);
			if (info == null)
			{
				// either eTag 304 was sent or no resource found
				return;
			}

			bool isCached = info.MD5.ToString("N").Equals(cacheKey, StringComparison.InvariantCultureIgnoreCase);
			if (isCached)
			{
				// if the content changes then so will the MD5
				// so we can effectively cache this forever
				context.Response.ExpiresAbsolute = DateTime.UtcNow.AddYears(5);
			}

			context.Response.ContentType = info.ContentType;

			context.Response.AppendHeader(
				"Content-Disposition",
				"inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+'.'+info.FileExtension);

			switch (info.GetOutputEncoding(context, isDebug))
			{
				case CompiledBuildResultType.PrettyPrint:
				{
					context.Response.ContentEncoding = System.Text.Encoding.UTF8;
					context.Response.Output.Write(info.PrettyPrinted);
					break;
				}
				case CompiledBuildResultType.Gzip:
				{
					context.Response.AppendHeader("Content-Encoding", CompiledBuildResult.GzipContentEncoding);
					context.Response.OutputStream.Write(info.Gzipped, 0, info.Gzipped.Length);
					break;
				}
				case CompiledBuildResultType.Deflate:
				{
					context.Response.AppendHeader("Content-Encoding", CompiledBuildResult.DeflateContentEncoding);
					context.Response.OutputStream.Write(info.Deflated, 0, info.Deflated.Length);
					break;
				}
				case CompiledBuildResultType.Compact:
				default:
				{
					context.Response.ContentEncoding = System.Text.Encoding.UTF8;
					context.Response.Output.Write(info.Compacted);
					break;
				}
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members

		#region ResourceHandler Members

		public static string GetStringsUrl(string path)
		{
			int index = path.IndexOf('?');
			if (index >= 0)
			{
				path = path.Substring(0, index);
			}

			return path+ResourceHandler.GlobalizationSetting;
		}

		public static string GetDebugStringsUrl(string path)
		{
			int index = path.IndexOf('?');
			if (index >= 0)
			{
				path = path.Substring(0, index);
			}

			return path+ResourceHandler.GlobalizationSetting+'?'+ResourceHandler.DebugSetting;
		}

		public static string GetDebugUrl(string path)
		{
			return GetCacheUrl(path, true);
		}

		public static string GetCacheUrl(string path)
		{
			return GetCacheUrl(path, false);
		}

		private static string GetCacheUrl(string path, bool isDebug)
		{
			CompiledBuildResult info = CompiledBuildResult.Create(path);
			if (info == null)
			{
				return path;
			}

			string cache;
			if (isDebug)
			{
				cache = '?'+ResourceHandler.DebugSetting;
			}
			else
			{
				cache = '?'+info.MD5.ToString("N");
			}

			int index = path.IndexOf('?');
			if (index >= 0)
			{
				path = path.Substring(0, index);
			}

			return path+cache;
		}

		/// <summary>
		/// Determines the appropriate source for the incomming request
		/// </summary>
		/// <param name="context"></param>
		/// <param name="isDebug"></param>
		/// <returns>CompiledBuildResult</returns>
		protected virtual CompiledBuildResult GetResourceInfo(HttpContext context, bool isDebug)
		{
			string virtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
			CompiledBuildResult info = CompiledBuildResult.Create(virtualPath);
			if (info == null)
			{
				return null;
			}

			// check if client has cached copy
			ETag etag = new GuidETag(info.MD5);
			if (etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
			{
				return null;
			}

			return info;
		}

		#endregion ResourceHandler Members
	}
}