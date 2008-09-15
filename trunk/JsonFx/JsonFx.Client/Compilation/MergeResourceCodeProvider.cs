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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

using BuildTools;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	public class MergeResourceCodeProvider : JsonFx.Compilation.ResourceCodeProvider
	{
		#region Constants

		private static readonly char[] LineDelims = { '\r', '\n' };
		private static readonly char[] TypeDelims = { ',' };

		#endregion Constants

		#region Fields

		private string contentType = "text/plain";
		private string fileExtension = "txt";
		private bool isMimeSet = false;

		#endregion Fields

		#region ResourceCodeProvider Properties

		public override string FileExtension
		{
			get { return this.fileExtension; }
		}

		public override string ContentType
		{
			get { return this.contentType; }
		}

		#endregion ResourceCodeProvider Properties

		#region ResourceCodeProvider Methods

		protected internal override void ProcessResource(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			out string resource,
			out string compacted,
			List<ParseException> errors)
		{
			if (String.IsNullOrEmpty(sourceText))
			{
				resource = null;
				compacted = null;
				return;
			}

			StringBuilder resources = new StringBuilder();
			StringBuilder compacts = new StringBuilder();
			string[] files = sourceText.Split(LineDelims, StringSplitOptions.RemoveEmptyEntries);

			for (int i=0; i<files.Length; i++)
			{
				try
				{
					// skip blank and comment lines
					if (String.IsNullOrEmpty(files[i]) ||
						files[i].StartsWith("//") ||
						files[i].StartsWith("#"))
					{
						continue;
					}

					// process embedded resource
					if (files[i].IndexOf(',') >= 0)
					{
						string preProcessed, compact;

						this.ProcessEmbeddedResource(helper, files[i], out preProcessed, out compact, errors);

						compacts.Append(compact);
						resources.Append(preProcessed);
						continue;
					}

					// ensure app-relative BuildManager paths
					if (files[i].StartsWith("/"))
					{
						files[i] = "~"+files[i];
					}

					// try to get as a CompiledBuildResult
					CompiledBuildResult result = CompiledBuildResult.Create(files[i]);
					if (result != null)
					{
						if (!this.isMimeSet &&
							!String.IsNullOrEmpty(result.ContentType) &&
							!String.IsNullOrEmpty(result.FileExtension))
						{
							this.contentType = result.ContentType;
							this.fileExtension = result.FileExtension;
							this.isMimeSet = true;
						}

						if (result is GlobalizedCompiledBuildResult)
						{
							this.GlobalizationKeys.AddRange(((GlobalizedCompiledBuildResult)result).GlobalizationKeys);
						}

						helper.AddVirtualPathDependency(files[i]);

						ICollection dependencies = BuildManager.GetVirtualPathDependencies(files[i]);
						if (dependencies != null)
						{
							foreach (string dependency in dependencies)
							{
								helper.AddVirtualPathDependency(dependency);
							}
						}

						dependencies = result.VirtualPathDependencies;
						if (dependencies != null)
						{
							foreach (string dependency in dependencies)
							{
								helper.AddVirtualPathDependency(dependency);
							}
						}

						resources.Append(result.PrettyPrinted);
						compacts.Append(result.Compacted);
						continue;
					}

					// ask BuildManager if compiles down to a string
					string text = BuildManager.GetCompiledCustomString(files[i]);
					if (!String.IsNullOrEmpty(text))
					{
						helper.AddVirtualPathDependency(files[i]);

						resources.Append(text);
						compacts.Append(text);
						continue;
					}

					// use the contents of the virtual path
					text = helper.OpenReader(files[i]).ReadToEnd();
					if (!String.IsNullOrEmpty(text))
					{
						helper.AddVirtualPathDependency(files[i]);

						resources.Append(text);
						compacts.Append(text);
						continue;
					}
				}
				catch (Exception ex)
				{
					errors.Add(new ParseError(ex.Message, virtualPath, i+1, 1, ex));
				}
			}

			resource = resources.ToString();
			compacted = compacts.ToString();
		}

		private void ProcessEmbeddedResource(
			IResourceBuildHelper helper,
			string source,
			out string preProcessed,
			out string compacted,
			List<ParseException> errors)
		{
			preProcessed = source.Replace(" ", "");
			string[] parts = preProcessed.Split(TypeDelims, 2, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length < 2 ||
				String.IsNullOrEmpty(parts[0]) ||
				String.IsNullOrEmpty(parts[1]))
			{
				compacted = preProcessed = null;
				return;
			}

			parts[0] = MergeResourceCodeProvider.ScrubResourceName(parts[0]);

			// load resources from Assembly
			Assembly assembly = Assembly.Load(parts[1]);
			helper.AddAssemblyDependency(assembly);

			ManifestResourceInfo info = assembly.GetManifestResourceInfo(parts[0]);
			if (info == null)
			{
				compacted = preProcessed = null;
				return;
			}

			using (Stream stream = assembly.GetManifestResourceStream(parts[0]))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					preProcessed = reader.ReadToEnd();
				}
			}

			string ext = Path.GetExtension(parts[0]).Trim('.');
			CompilerType compiler = helper.GetDefaultCompilerTypeForLanguage(ext);
			if (!typeof(ResourceCodeProvider).IsAssignableFrom(compiler.CodeDomProviderType))
			{
				// don't know how to process any further
				compacted = preProcessed;
				return;
			}

			ResourceCodeProvider provider = (ResourceCodeProvider)Activator.CreateInstance(compiler.CodeDomProviderType);

			// concatenate the preprocessed source for current merge phase
			provider.ProcessResource(
				helper,
				parts[0],
				preProcessed,
				out preProcessed,
				out compacted,
				errors);

			if (!this.isMimeSet &&
				!String.IsNullOrEmpty(provider.ContentType) &&
				!String.IsNullOrEmpty(provider.FileExtension))
			{
				this.contentType = provider.ContentType;
				this.fileExtension = provider.FileExtension;
				this.isMimeSet = true;
			}
		}

		#endregion ResourceCodeProvider Methods

		#region Utility Methods

		private static string ScrubResourceName(string resource)
		{
			if (String.IsNullOrEmpty(resource))
			{
				return resource;
			}

			StringBuilder builder = new StringBuilder(resource);
			builder.Replace('/', '.');
			builder.Replace('\\', '.');
			builder.Replace('?', '.');
			builder.Replace('*', '.');
			builder.Replace(':', '.');
			return builder.ToString().TrimStart('.');
		}

		#endregion Utility Methods
	}
}
