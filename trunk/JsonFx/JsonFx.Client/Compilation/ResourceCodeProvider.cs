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
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;

using BuildTools;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	/// <summary>
	/// Base class for all build-time resource compaction implementations.
	/// </summary>
	/// <remarks>
	/// This was implemented as a CodeProvider rather than a BuildProvider
	/// in order to gain access to the CompilerResults object.  This enables
	/// a custom compiler to correctly report its errors in the Visual Studio
	/// Error List.  Double clicking these errors takes the user to the actual
	/// source at the point where the error occurred.
	/// </remarks>
	public abstract class ResourceCodeProvider : Microsoft.CSharp.CSharpCodeProvider
	{
		#region Properties

		/// <summary>
		/// Gets the MIME type of the output.
		/// </summary>
		public abstract string ContentType { get; }

		/// <summary>
		/// Gets the file extension of the output.
		/// </summary>
		public override string FileExtension
		{
			get { return base.FileExtension; }
		}

		#endregion Properties

		#region Methods

		protected internal List<ParseException> CompileResource(
			IResourceBuildHelper helper,
			string virtualPath,
			out string preProcessed,
			out string compacted)
		{
			// read the resource contents
			using (TextReader reader = helper.OpenReader(virtualPath))
			{
				preProcessed = reader.ReadToEnd();
			}

			List<ParseException> errors = new List<ParseException>();
			using (StringWriter writer = new StringWriter(new StringBuilder(preProcessed.Length)))
			{
				// preprocess the resource
				IList<ParseException> parseErrors = this.PreProcess(
					helper,
					virtualPath,
					preProcessed,
					writer);

				if (parseErrors != null && parseErrors.Count > 0)
				{
					// combine any errors
					errors.AddRange(parseErrors);
				}

				writer.Flush();
				preProcessed = writer.ToString();
			}

			using (StringWriter writer = new StringWriter(new StringBuilder(preProcessed.Length)))
			{
				// compact the resource
				IList<ParseException> parseErrors = this.Compact(
					helper,
					virtualPath,
					preProcessed,
					writer);

				if (parseErrors != null && parseErrors.Count > 0)
				{
					// combine any errors
					errors.AddRange(parseErrors);
				}
				writer.Flush();
				compacted = writer.ToString();
			}

			return errors;
		}

		#endregion Methods

		#region Compaction Methods

		/// <summary>
		/// PreProcesses the source.  Default implementation simply writes through.
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="sourceText"></param>
		/// <param name="writer"></param>
		/// <returns>Errors</returns>
		protected internal virtual IList<ParseException> PreProcess(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			TextWriter writer)
		{
			writer.Write(sourceText);

			return null;
		}

		/// <summary>
		/// Compacts the source.
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="sourceText"></param>
		/// <param name="writer"></param>
		/// <returns>Errors</returns>
		protected internal abstract IList<ParseException> Compact(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			TextWriter writer);

		#endregion Compaction Methods
	}
}
