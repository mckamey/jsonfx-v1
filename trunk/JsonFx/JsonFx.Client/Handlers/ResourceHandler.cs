#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
using System.IO.Compression;
using System.Web;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using System.Web.Compilation;

namespace JsonFx.Handlers
{
	/// <remarks>
	/// The possible encoding methods for build results
	/// </remarks>
	internal enum BuildResultType
	{
		PrettyPrint,
		Compact,
		Gzip,
		Deflate
	}

	public class ResourceHandler : IHttpHandler
	{
		#region Constants

		public const string DebugFlag = "debug";
		internal const string GlobalizationQuery = "lang";

		private const string GzipContentEncoding = "gzip";
		private const string DeflateContentEncoding = "deflate";

		private const string HeaderAcceptEncoding = "Accept-Encoding";
		private const string HeaderContentEncoding = "Content-Encoding";
		private const string HeaderContentDisposition= "Content-Disposition";

		#endregion Constants

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			string cacheKey = context.Request.QueryString[null];
			bool isDebug = ResourceHandler.DebugFlag.Equals(cacheKey, StringComparison.OrdinalIgnoreCase);

			context.Response.ClearHeaders();
			context.Response.BufferOutput = true;

			// specifying "DEBUG" in the query string gets the non-compacted form
			IOptimizedResult info = this.GetResourceInfo(context, isDebug);
			if (info == null)
			{
				// either eTag 304 was sent or no resource found
				return;
			}

			bool isCached = StringComparer.OrdinalIgnoreCase.Equals(info.Hash, cacheKey);
			if (isCached)
			{
				// if the content changes then so will the hash
				// so we can effectively cache this forever
				context.Response.ExpiresAbsolute = DateTime.UtcNow.AddYears(1);
			}

			context.Response.ContentType = info.ContentType;

			context.Response.AppendHeader(
				ResourceHandler.HeaderContentDisposition,
				"inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+'.'+info.FileExtension);

			switch (ResourceHandler.GetOutputEncoding(info, context, isDebug))
			{
				case BuildResultType.PrettyPrint:
				{
					context.Response.ContentEncoding = System.Text.Encoding.UTF8;
					context.Response.Output.Write(info.PrettyPrinted);
					break;
				}
				case BuildResultType.Gzip:
				{
					context.Response.AppendHeader(ResourceHandler.HeaderContentEncoding, ResourceHandler.GzipContentEncoding);
					context.Response.OutputStream.Write(info.Gzipped, 0, info.Gzipped.Length);
					break;
				}
				case BuildResultType.Deflate:
				{
					context.Response.AppendHeader(ResourceHandler.HeaderContentEncoding, ResourceHandler.DeflateContentEncoding);
					context.Response.OutputStream.Write(info.Deflated, 0, info.Deflated.Length);
					break;
				}
				case BuildResultType.Compact:
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

		protected internal static string GetLocalizationUrl(string path, bool isDebug, string culture)
		{
			int index = path.IndexOf('?');
			if (index >= 0)
			{
				path = path.Substring(0, index);
			}

			string query = '?'+ResourceHandler.GlobalizationQuery+'='+culture;
			if (isDebug)
			{
				query += '&'+ResourceHandler.DebugFlag;
			}
			return path+query;
		}

		protected internal static string GetResourceUrl(IBuildResult info, string path, bool isDebug)
		{
			if (info == null)
			{
				info = ResourceHandler.Create<IBuildResult>(path);
				if (info == null)
				{
					return path;
				}
			}

			string cache;
			if (isDebug)
			{
				cache = '?'+ResourceHandler.DebugFlag;
			}
			else if (!String.IsNullOrEmpty(info.Hash))
			{
				cache = '?'+info.Hash;
			}
			else
			{
				cache = "";
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
		protected virtual IOptimizedResult GetResourceInfo(HttpContext context, bool isDebug)
		{
			string virtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
			IOptimizedResult info = ResourceHandler.Create<IOptimizedResult>(virtualPath);
			if (info == null)
			{
				throw new HttpException(404, "Resource not found: "+virtualPath);
			}

			// check if client has cached copy
			ETag etag = new HashETag(info.Hash);
			if (etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
			{
				return null;
			}

			return info;
		}

		#endregion ResourceHandler Members

		#region Utility Methods

		/// <summary>
		/// If supported, adds a runtime compression filter to the response output.
		/// </summary>
		/// <param name="context"></param>
		public static void EnableStreamCompression(HttpContext context)
		{
			// Good request compression summary: http://www.west-wind.com/WebLog/posts/102969.aspx
			switch (ResourceHandler.GetOutputEncoding(context))
			{
				case BuildResultType.Gzip:
				{
					context.Response.AppendHeader(ResourceHandler.HeaderContentEncoding, ResourceHandler.GzipContentEncoding);
					context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress, true);
					break;
				}
				case BuildResultType.Deflate:
				{
					context.Response.AppendHeader(ResourceHandler.HeaderContentEncoding, ResourceHandler.DeflateContentEncoding);
					context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress, true);
					break;
				}
			}
		}

		/// <summary>
		/// Strongly typed build result factory method
		/// </summary>
		/// <param name="virtualPath">app-relative virtual path</param>
		/// <returns>strongly typed compiled object</returns>
		public static T Create<T>(string virtualPath)
		{
			if (virtualPath.StartsWith("/"))
			{
				virtualPath = "~"+virtualPath;
			}

			return (T)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(T));
		}

		/// <summary>
		/// Determines appropriate content-encoding.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private static BuildResultType GetOutputEncoding(HttpContext context)
		{
			string setting = context.Request.QueryString[null];
			bool isDebug = ResourceHandler.DebugFlag.Equals(setting, StringComparison.OrdinalIgnoreCase);
			if (isDebug)
			{
				return BuildResultType.PrettyPrint;
			}

			string acceptEncoding = context.Request.Headers[ResourceHandler.HeaderAcceptEncoding];
			if (String.IsNullOrEmpty(acceptEncoding))
			{
				return BuildResultType.Compact;
			}

			acceptEncoding = acceptEncoding.ToLowerInvariant();

			if (acceptEncoding.Contains(ResourceHandler.DeflateContentEncoding))
			{
				return BuildResultType.Deflate;
			}

			if (acceptEncoding.Contains(ResourceHandler.GzipContentEncoding))
			{
				return BuildResultType.Gzip;
			}

			return BuildResultType.Compact;
		}

		/// <summary>
		/// Determines the most compact Content-Encoding supported by request.
		/// </summary>
		/// <param name="acceptEncoding"></param>
		/// <param name="isDebug"></param>
		/// <returns>optimal format</returns>
		private static BuildResultType GetOutputEncoding(IOptimizedResult result, HttpContext context, bool isDebug)
		{
			if (isDebug)
			{
				// short cut all debug builds
				return BuildResultType.PrettyPrint;
			}

			string acceptEncoding = context.Request.Headers[ResourceHandler.HeaderAcceptEncoding];
			if (String.IsNullOrEmpty(acceptEncoding))
			{
				// not compressed but fully compacted
				return BuildResultType.Compact;
			}

			acceptEncoding = acceptEncoding.ToLowerInvariant();

			if (result.Deflated != null &&
				result.Deflated.Length > 0 &&
				acceptEncoding.Contains(ResourceHandler.DeflateContentEncoding))
			{
				// compressed with Deflate
				return BuildResultType.Deflate;
			}

			if (result.Gzipped != null &&
				result.Gzipped.Length > 0 &&
				acceptEncoding.Contains(ResourceHandler.GzipContentEncoding))
			{
				// compressed with Gzip
				return BuildResultType.Gzip;
			}

			// not compressed but fully compacted
			return BuildResultType.Compact;
		}

		public static string EnsureAppRelative(string path)
		{
			// TODO: consolidate app relative path conversion
			// ensure app-relative BuildManager paths
			string appRoot = HostingEnvironment.ApplicationVirtualPath;
			if (appRoot != null && appRoot.Length > 1 &&
						path.StartsWith(appRoot, StringComparison.OrdinalIgnoreCase))
			{
				path = "~"+path.Substring(appRoot.Length);
			}
			else if (path.StartsWith("/") ||
						path.StartsWith("\\"))
			{
				path = "~"+path;
			}
			return path;
		}

		#endregion Utility Methods
	}
}