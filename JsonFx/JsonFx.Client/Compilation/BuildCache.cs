#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Compilation;

namespace JsonFx.Compilation
{
	public interface IBuildCacheKey
	{
		string VirtualPath
		{
			get;
		}
	}

	public sealed class BuildCache
	{
		#region Fields

		public static readonly BuildCache Instance = new BuildCache();
		private readonly IDictionary<string, string> Cache = new Dictionary<string, string>(StringComparer.Ordinal);

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		private BuildCache()
		{
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Creates an instance from the type name
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public T Create<T>(string typeName)
			where T : class
		{
			string virtualPath = this.GetPath(typeName);
			if (!String.IsNullOrEmpty(virtualPath))
			{
				return BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(object)) as T;
			}

			Type type = BuildManager.GetType(typeName, false, false);
			if (type == null)
			{
				foreach (string asm in Directory.GetFiles(AppDomain.CurrentDomain.DynamicDirectory, "*.dll", SearchOption.TopDirectoryOnly))
				{
					type = Assembly.LoadFrom(asm).GetType(typeName, false, false);
					if (type != null)
					{
						break;
					}
				}
			}

			if (type == null)
			{
				return default(T);
			}

			object value = Activator.CreateInstance(type);

			IBuildCacheKey cacheKey = value as IBuildCacheKey;
			if (cacheKey != null && !String.IsNullOrEmpty(cacheKey.VirtualPath))
			{
				virtualPath = cacheKey.VirtualPath;
				lock (this.Cache)
				{
					this.Cache[typeName] = this.Cache[type.FullName] = virtualPath;
				}
			}

			return value as T;
		}

		/// <summary>
		/// Gets the path
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public string GetPath(string typeName)
		{
			lock (this.Cache)
			{
				string virtualPath;
				if (this.Cache.TryGetValue(typeName, out virtualPath))
				{
					return virtualPath;
				}
			}

			return null;
		}

		#endregion Methods
	}
}
