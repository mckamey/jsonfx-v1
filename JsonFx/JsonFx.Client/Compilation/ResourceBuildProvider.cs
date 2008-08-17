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
	public interface IResourceBuildHelper
	{
		void AddVirtualPathDependency(string virtualPath);
		void AddAssemblyDependency(Assembly assembly);
		TextReader OpenReader(string virtualPath);
		CompilerType GetDefaultCompilerTypeForLanguage(string language);
	}

	/// <summary>
	/// The BuildProvider for all build-time resource compaction implementations.
	/// This provider processes the source storing a debug and a release output.
	/// The compilation result is a ResourceHandlerInfo class which has references
	/// to both resources.
	/// </summary>
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
	public class ResourceBuildProvider : System.Web.Compilation.BuildProvider, IResourceBuildHelper
	{
		#region Fields

		private List<string> pathDependencies = null;
		private List<Assembly> assemblyDependencies = null;
		private string resourceFullName = null;
		private string resourceTypeName = null;
		private string resourceNamespace = null;

		#endregion Fields

		#region Properties

		protected virtual string ResourceFullName
		{
			get
			{
				if (String.IsNullOrEmpty(this.resourceFullName))
				{
					this.resourceFullName = CompiledBuildResult.GenerateTypeName(base.VirtualPath);
				}
				return this.resourceFullName;
			}
		}

		protected string ResourceNamespace
		{
			get
			{
				if (String.IsNullOrEmpty(this.resourceNamespace))
				{
					string type = this.ResourceFullName;
					int dot = type.LastIndexOf('.');
					this.resourceNamespace = type.Substring(0, dot);
				}
				return this.resourceNamespace;
			}
		}

		protected string ResourceTypeName
		{
			get
			{
				if (String.IsNullOrEmpty(this.resourceTypeName))
				{
					string type = this.ResourceFullName;
					int dot = type.LastIndexOf('.');
					this.resourceTypeName = type.Substring(dot+1);
				}
				return this.resourceTypeName;
			}
		}

		#endregion Properties

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
			System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(this.resourceFullName));

			return results.CompiledAssembly.GetType(this.resourceFullName);
		}

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			string resource, compactedResource, contentType, fileExtension;

			ResourceCodeProvider provider = assemblyBuilder.CodeDomProvider as ResourceCodeProvider;
			if (provider != null)
			{
				provider.CompileResource(
					this,
					base.VirtualPath,
					out resource,
					out compactedResource);

				contentType = provider.ContentType;
				fileExtension = provider.FileExtension;
			}
			else
			{
				// read the resource contents
				using (TextReader reader = this.OpenReader())
				{
					compactedResource = resource = reader.ReadToEnd();
				}

				contentType = "text/plain";
				fileExtension = "txt";
			}

			// generate a static class
			CodeCompileUnit generatedUnit = new CodeCompileUnit();
			CodeNamespace ns = new CodeNamespace(this.ResourceNamespace);
			generatedUnit.Namespaces.Add(ns);
			CodeTypeDeclaration resourceType = new CodeTypeDeclaration();
			resourceType.IsClass = true;
			resourceType.Name = this.ResourceTypeName;
			resourceType.Attributes = MemberAttributes.Public|MemberAttributes.Final;
			resourceType.BaseTypes.Add(typeof(CompiledBuildResult));
			ns.Types.Add(resourceType);

			#region ResourceHandlerInfo.Resource

			// add a readonly property with the resource data
			CodeMemberProperty property = new CodeMemberProperty();
			property.Name = "Resource";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return resource; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(resource)));
			resourceType.Members.Add(property);

			#endregion ResourceHandlerInfo.Resource

			#region ResourceHandlerInfo.CompactedResource

			// add a readonly property with the compacted resource data
			property = new CodeMemberProperty();
			property.Name = "CompactedResource";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return compactedResource; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(compactedResource)));
			resourceType.Members.Add(property);

			#endregion ResourceHandlerInfo.CompactedResource

			#region ResourceHandlerInfo.ContentType

			// add a readonly property with the MIME type
			property = new CodeMemberProperty();
			property.Name = "ContentType";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return contentType; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(contentType)));
			resourceType.Members.Add(property);

			#endregion ResourceHandlerInfo.ContentType

			#region ResourceHandlerInfo.FileExtension

			// add a readonly property with the MIME type
			property = new CodeMemberProperty();
			property.Name = "FileExtension";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return fileExtension; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(fileExtension)));
			resourceType.Members.Add(property);

			#endregion ResourceHandlerInfo.FileExtension

			assemblyBuilder.AddCodeCompileUnit(this, generatedUnit);

			assemblyBuilder.GenerateTypeFactory(this.ResourceFullName);
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

		void IResourceBuildHelper.AddVirtualPathDependency(string virtualPath)
		{
			if (this.pathDependencies == null)
			{
				this.pathDependencies = new List<string>();
				this.pathDependencies.Add(base.VirtualPath);
			}

			this.pathDependencies.Add(virtualPath);
		}

		void IResourceBuildHelper.AddAssemblyDependency(Assembly assembly)
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

		TextReader IResourceBuildHelper.OpenReader(string virtualPath)
		{
			return this.OpenReader(virtualPath);
		}

		CompilerType IResourceBuildHelper.GetDefaultCompilerTypeForLanguage(string language)
		{
			return this.GetDefaultCompilerTypeForLanguage(language);
		}

		#endregion ResourceBuildHelper Members
	}
}
