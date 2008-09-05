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
		#region Fields

		private List<ParseException> errors = new List<ParseException>();

		#endregion Fields

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

		/// <summary>
		/// Gets the Type which is ultimately built
		/// </summary>
		protected internal virtual Type CompiledBuildResultType
		{
			get { return typeof(CompiledBuildResult); }
		}

		#endregion Properties

		#region Methods

		internal Type GetCompiledBuildResultType()
		{
			Type type = this.CompiledBuildResultType;
			if (type == null)
			{
				throw new NullReferenceException("CompiledBuildResultType is null.");
			}

			if (!typeof(CompiledBuildResult).IsAssignableFrom(type))
			{
				throw new ArgumentException(type.Name + "does not inherit from CompiledBuildResult");
			}

			return type;
		}

		/// <summary>
		/// Delegates compilation to the compiler implementation
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="virtualPath"></param>
		/// <param name="preProcessed"></param>
		/// <param name="compacted"></param>
		protected internal void CompileResource(
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

			// preprocess the resource
			this.ProcessResource(
				helper,
				virtualPath,
				preProcessed,
				out preProcessed,
				out compacted,
				this.errors);
		}

		/// <summary>
		/// Adds any existing errors to the CompilerResults
		/// </summary>
		/// <param name="results"></param>
		private void ReportErrors(CompilerResults results)
		{
			foreach (ParseException ex in errors)
			{
				CompilerError error = new CompilerError(ex.File, ex.Line, ex.Column, ex.ErrorCode, ex.Message);
				error.IsWarning = (ex is ParseWarning);
				results.Errors.Add(error);
			}
		}

		#endregion Methods

		#region CodeDomProvider Methods

		protected internal virtual void GenerateCodeExtensions(CodeTypeDeclaration resourceType)
		{
		}

		public override CompilerResults CompileAssemblyFromFile(CompilerParameters options, params string[] fileNames)
		{
			CompilerResults results = base.CompileAssemblyFromFile(options, fileNames);

			this.ReportErrors(results);

			return results;
		}

		public override CompilerResults CompileAssemblyFromDom(CompilerParameters options, params CodeCompileUnit[] compilationUnits)
		{
			CompilerResults results = base.CompileAssemblyFromDom(options, compilationUnits);

			this.ReportErrors(results);

			return results;
		}

		public override CompilerResults CompileAssemblyFromSource(CompilerParameters options, params string[] sources)
		{
			CompilerResults results = base.CompileAssemblyFromSource(options, sources);

			this.ReportErrors(results);

			return results;
		}

		#endregion CodeDomProvider Methods

		#region Compaction Methods

		/// <summary>
		/// Processes the source.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="virtualPath"></param>
		/// <param name="sourceText"></param>
		/// <param name="resource"></param>
		/// <param name="compacted"></param>
		/// <param name="errors"></param>
		protected internal abstract void ProcessResource(
			IResourceBuildHelper helper,
			string virtualPath,
			string source,
			out string resource,
			out string compacted,
			List<ParseException> errors);

		#endregion Compaction Methods
	}
}
