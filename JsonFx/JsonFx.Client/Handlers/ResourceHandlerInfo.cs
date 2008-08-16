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
using System.Web.Compilation;
using System.Reflection;

using JsonFx.Compilation;

namespace JsonFx.Handlers
{
	public abstract class ResourceHandlerInfo : CompiledBuildResult
	{
		#region Constants

		protected internal const string GeneratedNamespace = "JsonFx._Generated";

		private const string CompactedPath = "Compacted_";

		#endregion Constants

		#region Properties

		public abstract string ResourceName { get; }

		public abstract string CompactResourceName { get; }

		#endregion Properties

		#region Methods

		internal static string GetEmbeddedResourceName(string virtualPath)
		{
			if (String.IsNullOrEmpty(virtualPath))
			{
				return virtualPath;
			}

			StringBuilder builder = new StringBuilder(virtualPath);
			builder.Replace('/', '.');
			builder.Replace('\\', '.');
			builder.Replace('?', '.');
			builder.Replace('*', '.');
			builder.Replace(':', '.');
			return builder.ToString().TrimStart('.');
		}

		internal static string GetCompactedResourceName(string resourceName)
		{
			string dir = Path.GetDirectoryName(resourceName);
			if (!String.IsNullOrEmpty(dir))
			{
				dir += Path.DirectorySeparatorChar;
			}

			return dir +
				ResourceHandlerInfo.CompactedPath +
				Path.GetFileName(resourceName);
		}

		internal static ResourceHandlerInfo GetHandlerInfo(string virtualPath)
		{
			return BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(ResourceHandlerInfo)) as ResourceHandlerInfo;
		}

		#endregion Methods

		#region CompiledBuildResult Members

		public abstract string ContentType { get; }

		public abstract string FileExtension { get; }

		string CompiledBuildResult.GetBuildResult(bool prettyPrint)
		{
			string resourcePath = prettyPrint ? this.ResourceName : this.CompactResourceName;
			Assembly assembly = this.GetType().Assembly;
			using (TextReader reader = new StreamReader(assembly.GetManifestResourceStream(resourcePath)))
			{
				return reader.ReadToEnd();
			}
		}

		#endregion CompiledBuildResult Members
	}
}
