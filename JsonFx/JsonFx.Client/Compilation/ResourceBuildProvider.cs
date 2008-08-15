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
	public interface ResourceBuildHelper
	{
		void AddVirtualPathDependency(string virtualPath);
		void AddAssemblyDependency(Assembly assembly);
		TextReader OpenReader(string virtualPath);
	}

	/// <summary>
	/// The BuildProvider for all build-time resource compaction implementations.
	/// This provider processes the source storing a debug and a release output.
	/// The compilation result is a ResourceHandlerInfo class which has references
	/// to both resources.
	/// </summary>
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
	public class ResourceBuildProvider : System.Web.Compilation.BuildProvider, ResourceBuildHelper
	{
		#region Fields

		private string descriptorTypeName = null;
		private List<string> pathDependencies = null;
		private List<Assembly> assemblyDependencies = null;

		#endregion Fields

		#region BuildProvider Methods

		public override ICollection VirtualPathDependencies
		{
			get
			{
				if (this.pathDependencies == null)
				{
					return base.VirtualPathDependencies;
				}
				return this.pathDependencies;
			}
		}

		protected new ICollection ReferencedAssemblies
		{
			get
			{
				if (this.assemblyDependencies == null)
				{
					return base.ReferencedAssemblies;
				}
				return this.assemblyDependencies;
			}
		}

		public override Type GetGeneratedType(CompilerResults results)
		{
			System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(this.descriptorTypeName));

			return results.CompiledAssembly.GetType(this.descriptorTypeName);
		}

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			string sourceText;
			using (TextReader reader = base.OpenReader())
			{
				sourceText = reader.ReadToEnd();
			}

			string resourceName = ResourceHandlerInfo.GetEmbeddedResourceName(base.VirtualPath);

			ResourceCodeProvider provider = assemblyBuilder.CodeDomProvider as ResourceCodeProvider;
			if (provider != null)
			{
				provider.AddResourceTarget(base.VirtualPath, resourceName, sourceText, this);
			}

			// generate a static class
			CodeCompileUnit generatedUnit = new CodeCompileUnit();
			CodeNamespace ns = new CodeNamespace(ResourceHandlerInfo.GeneratedNamespace);
			generatedUnit.Namespaces.Add(ns);
			CodeTypeDeclaration descriptorType = new CodeTypeDeclaration();
			descriptorType.IsClass = true;
			descriptorType.Name = "_"+Guid.NewGuid().ToString("N");
			descriptorType.Attributes = MemberAttributes.Public|MemberAttributes.Final;
			descriptorType.BaseTypes.Add(typeof(ResourceHandlerInfo));
			ns.Types.Add(descriptorType);

			#region ResourceHandlerInfo.ResourceName

			// add a readonly property with the resource name
			CodeMemberProperty resourceProp = new CodeMemberProperty();
			resourceProp.Name = "ResourceName";
			resourceProp.Type = new CodeTypeReference(typeof(String));
			resourceProp.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			resourceProp.HasGet = true;
			// get { return resourceName; }
			resourceProp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(resourceName)));
			descriptorType.Members.Add(resourceProp);

			#endregion ResourceHandlerInfo.ResourceName

			assemblyBuilder.AddCodeCompileUnit(this, generatedUnit);

			System.Diagnostics.Debug.Assert(String.IsNullOrEmpty(this.descriptorTypeName));
			this.descriptorTypeName = ResourceHandlerInfo.GeneratedNamespace+"."+descriptorType.Name;

			assemblyBuilder.GenerateTypeFactory(this.descriptorTypeName);
		}

		public override CompilerType CodeCompilerType
		{
			get
			{
				string extension = Path.GetExtension(base.VirtualPath);
				if (extension == null || extension.Length < 2)
				{
					return base.CodeCompilerType;
				}
				CompilerType compilerType = base.GetDefaultCompilerTypeForLanguage(extension.Substring(1));
				// set compilerType.CompilerParameters options here
				return compilerType;
			}
		}

		#endregion BuildProvider Methods

		#region ResourceBuildHelper Members

		void ResourceBuildHelper.AddVirtualPathDependency(string virtualPath)
		{
			if (this.pathDependencies == null)
			{
				this.pathDependencies = new List<string>();
				this.pathDependencies.Add(base.VirtualPath);
			}

			this.pathDependencies.Add(virtualPath);
		}

		void ResourceBuildHelper.AddAssemblyDependency(Assembly assembly)
		{
			if (this.assemblyDependencies == null)
			{
				this.assemblyDependencies = new List<Assembly>();
				foreach (Assembly asm in base.ReferencedAssemblies)
				{
					this.assemblyDependencies.Add(asm);
				}
			}

			this.assemblyDependencies.Add(assembly);
		}

		TextReader ResourceBuildHelper.OpenReader(string virtualPath)
		{
			return this.OpenReader(virtualPath);
		}

		#endregion ResourceBuildHelper Members
	}

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
		#region ResourceData

		private class ResourceData
		{
			#region Fields

			public readonly string VirtualPath;
			public readonly string ResourceName;
			public readonly string SourceText;
			public readonly ResourceBuildHelper BuildHelper;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			public ResourceData(string virtualPath, string resourceName, string sourceText, ResourceBuildHelper helper)
			{
				this.VirtualPath = (virtualPath == null)? "" : virtualPath;
				this.ResourceName = (resourceName == null)? "" : resourceName;
				this.SourceText = (sourceText == null)? "" : sourceText;
				this.BuildHelper = helper;
			}

			#endregion Init
		}

		#endregion ResourceData

		#region Fields

		private List<ResourceData> sources = new List<ResourceData>();

		#endregion Fields

		#region Methods

		/// <summary>
		/// Adds to the listing of the resources which have to be built.
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="resourceName"></param>
		/// <param name="sourceText"></param>
		protected internal void AddResourceTarget(
			string virtualPath,
			string resourceName,
			string sourceText,
			ResourceBuildHelper helper)
		{
			this.sources.Add(new ResourceData(virtualPath, resourceName, sourceText, helper));
		}

		#endregion Methods

		#region CodeDomProvider Members

		/// <summary>
		/// Processes all the resources added by the BuildProvider, compacting and adding each one as another resource
		/// </summary>
		/// <param name="options"></param>
		/// <param name="fileNames"></param>
		/// <returns></returns>
		/// <remarks>
		/// This is implemented here simply because this is where the CodeProvider gets called in this circumstance.
		/// We don't actually need to "compile from files" but we need access to the CompilerParameters object to
		/// add additional embedded resources, and we need access to the CompilerResult to report any errors found.
		/// </remarks>
		public override CompilerResults CompileAssemblyFromFile(CompilerParameters options, params string[] fileNames)
		{
			List<ParseException> errors = new List<ParseException>();

			foreach (ResourceData source in this.sources)
			{
				// compact each resource added by the BuildProvider
				this.ProcessSingleSource(options, source, errors);
			}

			// build the assembly with the types describing which resources correspond to which paths
			CompilerResults results = base.CompileAssemblyFromFile(options, fileNames);

			foreach (ParseException ex in errors)
			{
				CompilerError error = new CompilerError(ex.File, ex.Line, ex.Column, ex.ErrorCode, ex.Message);
				error.IsWarning = (ex is ParseWarning);
				results.Errors.Add(error);
			}

			return results;
		}

		private void ProcessSingleSource(CompilerParameters options, ResourceData source, List<ParseException> errors)
		{
			// read the resource contents
			string sourceText = source.SourceText;

			using (StringWriter writer = new StringWriter(new StringBuilder(sourceText.Length)))
			{
				// preprocess the resource
				IList<ParseException> parseErrors = this.PreProcess(
					source.BuildHelper,
					source.VirtualPath,
					sourceText,
					writer);

				if (parseErrors != null && parseErrors.Count > 0)
				{
					// report any errors
					errors.AddRange(parseErrors);
				}
				writer.Flush();
				sourceText = writer.ToString();
			}

			// write out source version to a resource
			string resourcePath = Path.Combine(options.TempFiles.TempDir, source.ResourceName);
			File.WriteAllText(resourcePath, sourceText);

			// add source copy to the embedded resource list
			options.EmbeddedResources.Add(resourcePath);

			string compactedText;
			using (StringWriter writer = new StringWriter(new StringBuilder(sourceText.Length)))
			{
				// compact the resource
				IList<ParseException> parseErrors = this.Compact(
					source.BuildHelper,
					source.SourceText.Equals(sourceText) ? source.VirtualPath : resourcePath,
					sourceText,
					writer);

				if (parseErrors != null && parseErrors.Count > 0)
				{
					// report any errors
					errors.AddRange(parseErrors);
				}
				writer.Flush();
				compactedText = writer.ToString();
			}

			// write out compacted version to another resource
			string compactedPath = ResourceHandlerInfo.GetCompactedResourceName(resourcePath);
			File.WriteAllText(compactedPath, compactedText);

			// add compacted copy to the embedded resource list
			options.EmbeddedResources.Add(compactedPath);
		}

		#endregion CodeDomProvider Members

		#region Compaction Methods

		/// <summary>
		/// PreProcesses the source.  Default implementation simply writes through.
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="sourceText"></param>
		/// <param name="writer"></param>
		/// <returns>Errors</returns>
		protected virtual IList<ParseException> PreProcess(ResourceBuildHelper helper, string virtualPath, string sourceText, StringWriter writer)
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
		protected abstract IList<ParseException> Compact(ResourceBuildHelper helper, string virtualPath, string sourceText, TextWriter writer);

		#endregion Compaction Methods
	}
}
