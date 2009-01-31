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

	public interface IOptimizedResult : IBuildResultMeta
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
		#region Utility Methods

		[Obsolete("This method is being moved.")]
		public static void EnableStreamCompression(HttpContext context)
		{
			ResourceHandler.EnableStreamCompression(context);
		}

		/// <summary>
		/// CompiledBuildResult Factory method
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <returns></returns>
		public static T Create<T>(string virtualPath)
		{
			if (virtualPath.StartsWith("/"))
			{
				virtualPath = "~"+virtualPath;
			}

			return (T)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(T));
		}

		#endregion Utility Methods
	}
}
