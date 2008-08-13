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
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
	public class CompactorBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Fields

		private string descriptorTypeName = null;

		#endregion Fields

		#region BuildProvider Methods

		public override Type GetGeneratedType(CompilerResults results)
		{
			System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(this.descriptorTypeName));

			return results.CompiledAssembly.GetType(this.descriptorTypeName);
		}

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			string resourceName = ResourceHandlerInfo.GetEmbeddedResourceName(base.VirtualPath);
			using (Stream stream = assemblyBuilder.CreateEmbeddedResource(this, resourceName))
			{
				// truncate any previous contents
				stream.SetLength(0);

				using (TextWriter writer = new StreamWriter(stream))
				{
					this.PreProcessSource(writer);
				}
			}

			CompactorCodeProvider provider = assemblyBuilder.CodeDomProvider as CompactorCodeProvider;
			if (provider != null)
			{
				provider.AddCompactorTarget(base.VirtualPath, resourceName);
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

		protected virtual void PreProcessSource(TextWriter writer)
		{
			string sourceText;
			using (TextReader reader = base.OpenReader())
			{
				sourceText = reader.ReadToEnd();
			}

			writer.Write(sourceText);
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
	}

	/// <summary>
	/// Base class for all build-time resource compaction implementations.
	/// </summary>
	public abstract class CompactorCodeProvider : Microsoft.CSharp.CSharpCodeProvider
	{
		#region Fields

		private Dictionary<string, string> sources = new Dictionary<string, string>();

		#endregion Fields

		#region Methods

		/// <summary>
		/// Gets a listing of all the resources which have been built
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="resourceName"></param>
		protected internal void AddCompactorTarget(string virtualPath, string resourceName)
		{
			this.sources[resourceName] = virtualPath;
		}

		#endregion Methods

		#region CodeDomProvider Members

		/// <summary>
		/// Processes all the resources added by the BuildProvider, compacting and adding each one as another resource
		/// </summary>
		/// <param name="options"></param>
		/// <param name="fileNames"></param>
		/// <returns></returns>
		public override CompilerResults CompileAssemblyFromFile(CompilerParameters options, params string[] fileNames)
		{
			List<ParseException> errors = new List<ParseException>();
			string compactedText;

			// compact each resource added by the BuildProvider
			foreach (KeyValuePair<string, string> item in this.sources)
			{
				string resourceName = Path.Combine(options.TempFiles.TempDir, item.Key);
				string virtualPath = item.Value;

				System.Diagnostics.Debug.Assert(File.Exists(resourceName));

				// read the preprocessed resource contents
				string sourceText = File.ReadAllText(resourceName);
				using (StringWriter writer = new StringWriter(new StringBuilder(sourceText.Length)))
				{
					// compact the resource
					IList<ParseException> parseErrors = this.Compact(virtualPath, sourceText, writer);
					if (parseErrors != null && parseErrors.Count > 0)
					{
						errors.AddRange(parseErrors);
					}
					writer.Flush();
					compactedText = writer.ToString();
				}

				// write out compacted version to another resource
				string compactedPath = ResourceHandlerInfo.GetCompactedResourceName(resourceName);
				File.WriteAllText(compactedPath, compactedText);
				options.EmbeddedResources.Add(compactedPath);
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

		#endregion CodeDomProvider Members

		#region Compaction Methods

		/// <summary>
		/// Compacts the source.
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="sourceText"></param>
		/// <param name="writer"></param>
		/// <returns></returns>
		protected abstract IList<ParseException> Compact(string virtualPath, string sourceText, TextWriter writer);

		#endregion Compaction Methods
	}
}
