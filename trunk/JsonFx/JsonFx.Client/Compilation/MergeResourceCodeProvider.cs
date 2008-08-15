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
using System.Collections;
using System.Collections.Generic;
using System.Web.Compilation;
using System.Reflection;

using BuildTools;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	public interface CompiledBuildResult
	{
		string GetBuildResult(bool prettyPrint);
	}

	public class MergeResourceCodeProvider : JsonFx.Compilation.ResourceCodeProvider
	{
		#region Constants

		private static readonly char[] LineDelims = { '\r', '\n' };

		#endregion Constants

		#region Fields

		private string[] sources = null;

		#endregion Fields

		#region ResourceCodeProvider Members

		protected override IList<ParseException> PreProcess(ResourceBuildHelper helper, string virtualPath, string sourceText, StringWriter writer)
		{
			if (String.IsNullOrEmpty(sourceText))
			{
				return null;
			}
			else
			{
				this.sources = sourceText.Split(LineDelims, StringSplitOptions.RemoveEmptyEntries);
			}

			for (int i=0; i<this.sources.Length; i++)
			{
				if (String.IsNullOrEmpty(this.sources[i]))
				{
					continue;
				}

				if (this.sources[i].StartsWith("//"))
				{
					this.sources[i] = null;
					continue;
				}

				if (this.sources[i].IndexOf(':') >= 0)
				{
					string[] resource = this.sources[i].Split(':');
					if (resource.Length < 2)
					{
						this.sources[i] = null;
						continue;
					}

					// TODO: load resource from Assembly
					Assembly assembly = Assembly.Load(resource[1]);
					ManifestResourceInfo info = assembly.GetManifestResourceInfo(resource[0]);
					if (info == null)
					{
						this.sources[i] = null;
						continue;
					}

					using (Stream stream = assembly.GetManifestResourceStream(resource[0]))
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							this.sources[i] = reader.ReadToEnd();
							writer.WriteLine(this.sources[i]);
						}
					}
					continue;
				}

				CompiledBuildResult result = BuildManager.CreateInstanceFromVirtualPath(this.sources[i], typeof(CompiledBuildResult)) as CompiledBuildResult;
				if (result != null)
				{
					helper.AddVirtualPathDependency(this.sources[i]);

					writer.WriteLine(result.GetBuildResult(true));
					this.sources[i] = result.GetBuildResult(false);
					continue;
				}

				string source = BuildManager.GetCompiledCustomString(this.sources[i]);
				if (!String.IsNullOrEmpty(source))
				{
					helper.AddVirtualPathDependency(this.sources[i]);

					writer.WriteLine(source);
					this.sources[i] = source;
					continue;
				}

				source = helper.OpenReader(this.sources[i]).ReadToEnd();
				if (!String.IsNullOrEmpty(source))
				{
					helper.AddVirtualPathDependency(this.sources[i]);

					writer.WriteLine(source);
					this.sources[i] = source;
					continue;
				}
			}

			return null;
		}

		protected override IList<ParseException> Compact(ResourceBuildHelper helper, string virtualPath, string sourceText, TextWriter writer)
		{
			foreach (string source in this.sources)
			{
				if (String.IsNullOrEmpty(source))
				{
					continue;
				}

				writer.WriteLine(source);
			}

			return null;
		}

		#endregion ResourceCodeProvider Members
	}
}
