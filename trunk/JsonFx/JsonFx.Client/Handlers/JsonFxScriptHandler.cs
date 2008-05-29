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
using System.Reflection;
using System.Web;

namespace JsonFx.Handlers
{
	public class JsonFxScriptHandler : ClientScriptHandler
	{
		#region Handler Members

		protected override Stream GetResourceStream(HttpContext context, bool isDebug)
		{
			if (context.Request.FilePath.EndsWith(this.ResourceExtension, StringComparison.InvariantCultureIgnoreCase))
			{
				return base.GetResourceStream(context, isDebug);
			}

			string virtualPath = context.Request.FilePath;
			string resourcePath = isDebug ? ClientScriptHandler.ScriptPath : ClientScriptHandler.CompactedScriptPath;
			resourcePath += Path.GetFileNameWithoutExtension(virtualPath)+this.ResourceExtension;

			Assembly assembly = Assembly.GetAssembly(typeof(ClientScriptHandler));
			ManifestResourceInfo info = assembly.GetManifestResourceInfo(resourcePath);
			if (info == null)
			{
				// file does not exist in Assembly
				throw new FileNotFoundException("Invalid file path");
			}

			// check if client has cached copy
			ETag etag = new EmbeddedResourceETag(assembly, resourcePath);
			if (etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
			{
				return Stream.Null;
			}

			return assembly.GetManifestResourceStream(resourcePath);
		}

		#endregion Handler Members
	}
}