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
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Reflection;

using JsonFx.Compilation;

namespace JsonFx.Handlers
{
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
		public const string GzipContentEncoding = "gzip";
		public const string DeflateContentEncoding = "deflate";

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

			if (this.Gzipped != null &&
				acceptEncoding.Contains("gzip"))
			{
				return CompiledBuildResultType.Gzip;
			}

			if (this.Deflated != null &&
				acceptEncoding.Contains("deflate"))
			{
				return CompiledBuildResultType.Deflate;
			}

			return CompiledBuildResultType.Compact;
		}

		#endregion Properties

		#region Factory Methods

		/// <summary>
		/// CompiledBuildResult Factory method
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <returns></returns>
		internal static CompiledBuildResult Create(string virtualPath)
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
