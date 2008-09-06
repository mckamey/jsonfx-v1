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
using System.IO.Compression;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Reflection;

using JsonFx.Compilation;

namespace JsonFx.Handlers
{
	/// <remarks>
	/// Good request compression summary: http://www.west-wind.com/WebLog/posts/102969.aspx
	/// </remarks>
	public enum CompiledBuildResultType
	{
		PrettyPrint,
		Compact,
		Gzip,
		Deflate
	}

	public abstract class CompiledBuildResult
	{
		#region Constants

		private const string RootNamespace = "__JsonFx";
		internal const string GzipContentEncoding = "gzip";
		internal const string DeflateContentEncoding = "deflate";

		#endregion Constants

		#region Properties

		/// <summary>
		/// Gets the pretty-printed resource data
		/// </summary>
		public abstract string PrettyPrinted { get; }

		/// <summary>
		/// Gets the compacted resource data
		/// </summary>
		public abstract string Compacted { get; }

		/// <summary>
		/// Gets the compacted resource data compressed with Gzip
		/// </summary>
		public abstract byte[] Gzipped { get; }

		/// <summary>
		/// Gets the compacted resource data compressed with Deflate
		/// </summary>
		public abstract byte[] Deflated { get; }

		/// <summary>
		/// Gets the MD5 hash for the compacted resource data
		/// </summary>
		public abstract Guid MD5 { get; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Determines the most compact Content-Encoding supported by request.
		/// </summary>
		/// <param name="acceptEncoding"></param>
		/// <param name="isDebug"></param>
		/// <returns>optimal format</returns>
		public CompiledBuildResultType GetOutputEncoding(HttpContext context, bool isDebug)
		{
			if (isDebug)
			{
				return CompiledBuildResultType.PrettyPrint;
			}

			string acceptEncoding = context.Request.Headers["Accept-Encoding"];
			if (String.IsNullOrEmpty(acceptEncoding))
			{
				return CompiledBuildResultType.Compact;
			}

			acceptEncoding = acceptEncoding.ToLowerInvariant();

			if (this.Deflated != null &&
				acceptEncoding.Contains("deflate"))
			{
				return CompiledBuildResultType.Deflate;
			}

			if (this.Gzipped != null &&
				acceptEncoding.Contains("gzip"))
			{
				return CompiledBuildResultType.Gzip;
			}

			return CompiledBuildResultType.Compact;
		}

		/// <summary>
		/// If supported, adds a runtime compression filter to the response output.
		/// </summary>
		/// <param name="context"></param>
		public static void EnableStreamCompression(HttpContext context)
		{
			switch (CompiledBuildResult.GetOutputEncoding(context))
			{
				case CompiledBuildResultType.Gzip:
				{
					context.Response.AppendHeader("Content-Encoding", CompiledBuildResult.GzipContentEncoding);
					context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress, true);
					break;
				}
				case CompiledBuildResultType.Deflate:
				{
					context.Response.AppendHeader("Content-Encoding", CompiledBuildResult.DeflateContentEncoding);
					context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress, true);
					break;
				}
			}
		}

		/// <summary>
		/// Determines appropriate content-encoding.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private static CompiledBuildResultType GetOutputEncoding(HttpContext context)
		{
			bool isDebug = "debug".Equals(context.Request.QueryString[null], StringComparison.InvariantCultureIgnoreCase);
			if (isDebug)
			{
				return CompiledBuildResultType.PrettyPrint;
			}

			string acceptEncoding = context.Request.Headers["Accept-Encoding"];
			if (String.IsNullOrEmpty(acceptEncoding))
			{
				return CompiledBuildResultType.Compact;
			}

			acceptEncoding = acceptEncoding.ToLowerInvariant();

			if (acceptEncoding.Contains("deflate"))
			{
				return CompiledBuildResultType.Deflate;
			}

			if (acceptEncoding.Contains("gzip"))
			{
				return CompiledBuildResultType.Gzip;
			}

			return CompiledBuildResultType.Compact;
		}

		#endregion Methods

		#region Factory Methods

		/// <summary>
		/// CompiledBuildResult Factory method
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <returns></returns>
		protected internal static CompiledBuildResult Create(string virtualPath)
		{
			try
			{
				return (CompiledBuildResult)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(CompiledBuildResult));
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Generates a Type name for the compiled resource
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <returns></returns>
		public static string GenerateTypeName(string virtualPath)
		{
			if (String.IsNullOrEmpty(virtualPath))
			{
				return CompiledBuildResult.RootNamespace+"._"+Guid.NewGuid().ToString("N");
			}

			StringBuilder builder = new StringBuilder(virtualPath);
			if (builder[0] == '~')
			{
				builder.Remove(0, 1);
			}

			for (int i=0; i<builder.Length; i++)
			{
				if (Char.IsLetterOrDigit(builder[i]))
				{
					continue;
				}

				switch (builder[i])
				{
					case '\\':
					case '/':
					{
						builder[i] = '.';
						break;
					}
					case '-':
					case '.':
					default:
					{
						builder[i] = '_';
						break;
					}
				}
			}

			return CompiledBuildResult.RootNamespace+builder.ToString();
		}

		#endregion Factory Methods

		#region CompiledBuildResult Members

		public abstract string ContentType { get; }

		public abstract string FileExtension { get; }

		#endregion CompiledBuildResult Members
	}
}
