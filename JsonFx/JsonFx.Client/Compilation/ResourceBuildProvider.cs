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
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Security.Cryptography;

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
	/// The compilation result is a CompiledBuildResult class which has references
	/// to both resources.
	/// </summary>
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
	public class ResourceBuildProvider : System.Web.Compilation.BuildProvider, IResourceBuildHelper
	{
		#region Constants

		private static readonly MD5 MD5HashProvider = MD5.Create();

		#endregion Constants

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
			string contentType, fileExtension;
			string prettyPrintResource, compactedResource;

			ResourceCodeProvider provider = assemblyBuilder.CodeDomProvider as ResourceCodeProvider;
			if (provider != null)
			{
				provider.CompileResource(
					this,
					base.VirtualPath,
					out prettyPrintResource,
					out compactedResource);

				contentType = provider.ContentType;
				fileExtension = provider.FileExtension;
			}
			else
			{
				// read the resource contents
				using (TextReader reader = this.OpenReader())
				{
					compactedResource = prettyPrintResource = reader.ReadToEnd();
				}

				contentType = "text/plain";
				fileExtension = "txt";
			}

			byte[] gzippedBytes, deflatedBytes;
			ResourceBuildProvider.Compress(compactedResource, out gzippedBytes, out deflatedBytes);
			byte[] hash = ResourceBuildProvider.MD5Hash(compactedResource);

			// generate a resource container
			CodeCompileUnit generatedUnit = new CodeCompileUnit();

			#region namespace ResourceNamespace

			CodeNamespace ns = new CodeNamespace(this.ResourceNamespace);
			generatedUnit.Namespaces.Add(ns);

			#endregion namespace ResourceNamespace

			#region public sealed class ResourceTypeName : CompiledBuildResult

			CodeTypeDeclaration resourceType = new CodeTypeDeclaration();
			resourceType.IsClass = true;
			resourceType.Name = this.ResourceTypeName;
			resourceType.Attributes = MemberAttributes.Public|MemberAttributes.Final;
			resourceType.BaseTypes.Add(typeof(CompiledBuildResult));
			ns.Types.Add(resourceType);

			#endregion public sealed class ResourceTypeName

			#region private static readonly byte[] GzippedBytes

			CodeMemberField field = new CodeMemberField();
			field.Name = "GzippedBytes";
			field.Type = new CodeTypeReference(typeof(byte[]));
			field.Attributes = MemberAttributes.Private|MemberAttributes.Static|MemberAttributes.Final;

			CodeArrayCreateExpression arrayInit = new CodeArrayCreateExpression(field.Type, gzippedBytes.Length);
			foreach (byte b in gzippedBytes)
			{
				arrayInit.Initializers.Add(new CodePrimitiveExpression(b));
			}
			field.InitExpression = arrayInit;

			resourceType.Members.Add(field);

			#endregion private static static readonly byte[] GzippedBytes

			#region private static readonly byte[] DeflatedBytes;

			field = new CodeMemberField();
			field.Name = "DeflatedBytes";
			field.Type = new CodeTypeReference(typeof(byte[]));
			field.Attributes = MemberAttributes.Private|MemberAttributes.Static|MemberAttributes.Final;

			arrayInit = new CodeArrayCreateExpression(field.Type, deflatedBytes.Length);
			foreach (byte b in deflatedBytes)
			{
				arrayInit.Initializers.Add(new CodePrimitiveExpression(b));
			}
			field.InitExpression = arrayInit;

			resourceType.Members.Add(field);

			#endregion private static readonly byte[] DeflatedBytes;

			#region public override string PrettyPrinted { get; }

			// add a readonly property with the resource data
			CodeMemberProperty property = new CodeMemberProperty();
			property.Name = "PrettyPrinted";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return resource; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(prettyPrintResource)));
			resourceType.Members.Add(property);

			#endregion public override string PrettyPrinted { get; }

			#region public override string Compacted { get; }

			// add a readonly property with the compacted resource data
			property = new CodeMemberProperty();
			property.Name = "Compacted";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return compactedResource; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(compactedResource)));
			resourceType.Members.Add(property);

			#endregion public override string Compacted { get; }

			#region public override byte[] Gzipped { get; }

			// add a readonly property with the gzipped resource data
			property = new CodeMemberProperty();
			property.Name = "Gzipped";
			property.Type = new CodeTypeReference(typeof(byte[]));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return GzippedBytes; }
			property.GetStatements.Add(new CodeMethodReturnStatement(
				new CodeFieldReferenceExpression(
					new CodeTypeReferenceExpression(this.ResourceTypeName),
					"GzippedBytes")));
			resourceType.Members.Add(property);

			#endregion public override byte[] Gzipped { get; }

			#region public override byte[] Deflated { get; }

			// add a readonly property with the deflated resource data
			property = new CodeMemberProperty();
			property.Name = "Deflated";
			property.Type = new CodeTypeReference(typeof(byte[]));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return DeflatedBytes; }
			property.GetStatements.Add(new CodeMethodReturnStatement(
				new CodeFieldReferenceExpression(
					new CodeTypeReferenceExpression(this.ResourceTypeName),
					"DeflatedBytes")));
			resourceType.Members.Add(property);

			#endregion public override byte[] Deflated { get; }

			#region public override string ContentType { get; }

			// add a readonly property with the MIME type
			property = new CodeMemberProperty();
			property.Name = "ContentType";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return contentType; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(contentType)));
			resourceType.Members.Add(property);

			#endregion public override string ContentType { get; }

			#region public override string FileExtension { get; }

			// add a readonly property with the MIME type
			property = new CodeMemberProperty();
			property.Name = "FileExtension";
			property.Type = new CodeTypeReference(typeof(String));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return fileExtension; }
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(fileExtension)));
			resourceType.Members.Add(property);

			#endregion public override string FileExtension { get; }

			#region public override Guid MD5 { get; }

			// add a readonly property with the resource data
			property = new CodeMemberProperty();
			property.Name = "MD5";
			property.Type = new CodeTypeReference(typeof(Guid));
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return new Guid(hash)); }

			arrayInit = new CodeArrayCreateExpression(typeof(byte[]), hash.Length);
			foreach (byte b in hash)
			{
				arrayInit.Initializers.Add(new CodePrimitiveExpression(b));
			}

			CodeObjectCreateExpression newGuid = new CodeObjectCreateExpression(property.Type, arrayInit);
			property.GetStatements.Add(new CodeMethodReturnStatement(newGuid));
			resourceType.Members.Add(property);

			#endregion public override Guid MD5 { get; }

			assemblyBuilder.AddCodeCompileUnit(this, generatedUnit);
		}

		public static void Compress(string source, out byte[] gzipped, out byte[] deflated)
		{
			if (String.IsNullOrEmpty(source))
			{
				gzipped = deflated = null;
				return;
			}

			using (MemoryStream memStream = new MemoryStream())
			{
				using (GZipStream gzipStream = new GZipStream(memStream, CompressionMode.Compress, true))
				{
					using (StreamWriter writer = new StreamWriter(gzipStream))
					{
						writer.Write(source);
						writer.Flush();
					}
				}

				memStream.Seek(0L, SeekOrigin.Begin);
				gzipped = new byte[memStream.Length];
				memStream.Read(gzipped, 0, gzipped.Length);
			}

			using (MemoryStream memStream = new MemoryStream())
			{
				using (DeflateStream gzipStream = new DeflateStream(memStream, CompressionMode.Compress, true))
				{
					using (StreamWriter writer = new StreamWriter(gzipStream))
					{
						writer.Write(source);
						writer.Flush();
					}
				}

				memStream.Seek(0L, SeekOrigin.Begin);
				deflated = new byte[memStream.Length];
				memStream.Read(deflated, 0, deflated.Length);
			}
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

		#region Utility Methods

		/// <summary>
		/// Generates a unique MD5 hash from string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static byte[] MD5Hash(string value)
		{
			// get String as a Byte[]
			byte[] buffer = Encoding.Unicode.GetBytes(value);

			return ResourceBuildProvider.MD5Hash(buffer);
		}

		/// <summary>
		/// Generates an MD5 hash from byte[]
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static byte[] MD5Hash(byte[] value)
		{
			byte[] hash;
			lock (MD5HashProvider)
			{
				// generate MD5 hash
				hash = MD5HashProvider.ComputeHash(value);
			}

			return hash;
		}

		#endregion Utility Methods
	}
}
