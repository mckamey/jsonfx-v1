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
using System.Collections.Generic;
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

	public interface IBuildResultMeta
	{
		#region Properties

		/// <summary>
		/// Gets the file hash for the resulting resource data
		/// </summary>
		string ContentType { get; }

		/// <summary>
		/// Gets the file extension for the resulting resource data
		/// </summary>
		string FileExtension { get; }

		/// <summary>
		/// Gets the file hash for the compacted resource data
		/// </summary>
		string Hash { get; }

		#endregion Properties
	}

	public interface IOptimizedResult :
		IBuildResultMeta
	{
		#region Properties

		/// <summary>
		/// Gets the pretty-printed resource data
		/// </summary>
		string PrettyPrinted { get; }

		/// <summary>
		/// Gets the compacted resource data
		/// </summary>
		string Compacted { get; }

		/// <summary>
		/// Gets the compacted resource data compressed with Gzip
		/// </summary>
		byte[] Gzipped { get; }

		/// <summary>
		/// Gets the compacted resource data compressed with Deflate
		/// </summary>
		byte[] Deflated { get; }

		#endregion Properties
	}

	public interface IDependentResult
	{
		#region Properties

		/// <summary>
		/// Gets the virtual paths which this resource is dependent upon
		/// </summary>
		IEnumerable<string> VirtualPathDependencies { get; }

		#endregion Properties
	}

	public static class CompiledBuildResult
	{
		#region Constants

		private const string RootNamespace = "__JsonFx";
		internal const string GzipContentEncoding = "gzip";
		internal const string DeflateContentEncoding = "deflate";

		#endregion Constants

		#region Static Methods

		/// <summary>
		/// Determines the most compact Content-Encoding supported by request.
		/// </summary>
		/// <param name="acceptEncoding"></param>
		/// <param name="isDebug"></param>
		/// <returns>optimal format</returns>
		public static CompiledBuildResultType GetOutputEncoding(IOptimizedResult result, HttpContext context, bool isDebug)
		{
			if (isDebug)
			{
				// short cut all debug builds
				return CompiledBuildResultType.PrettyPrint;
			}

			string acceptEncoding = context.Request.Headers["Accept-Encoding"];
			if (String.IsNullOrEmpty(acceptEncoding))
			{
				// not compressed but fully compacted
				return CompiledBuildResultType.Compact;
			}

			acceptEncoding = acceptEncoding.ToLowerInvariant();

			if (result.Deflated != null &&
				result.Deflated.Length > 0 &&
				acceptEncoding.Contains("deflate"))
			{
				// compressed with Deflate
				return CompiledBuildResultType.Deflate;
			}

			if (result.Gzipped != null &&
				result.Gzipped.Length > 0 &&
				acceptEncoding.Contains("gzip"))
			{
				// compressed with Gzip
				return CompiledBuildResultType.Gzip;
			}

			// not compressed but fully compacted
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
			string setting = context.Request.QueryString[null];
			bool isDebug = ResourceHandler.DebugFlag.Equals(setting, StringComparison.OrdinalIgnoreCase);
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

		#endregion Static Methods

		#region Factory Methods

		/// <summary>
		/// CompiledBuildResult Factory method
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <returns></returns>
		internal static T Create<T>(string virtualPath)
		{
			if (virtualPath.StartsWith("/"))
			{
				virtualPath = "~"+virtualPath;
			}

			return (T)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(T));
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
	}
}
