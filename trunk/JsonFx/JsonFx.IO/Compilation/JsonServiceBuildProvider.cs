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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.IO;

using JsonFx.JsonRpc;
using JsonFx.JsonRpc.Discovery;
using JsonFx.JsonRpc.Proxy;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	/// <summary>
	/// 
	/// </summary>
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public class JsonServiceBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Constants

		private readonly object SyncLock = new object();

		protected internal const string GeneratedNamespace = "JsonFx.JsonRpc._Generated";

		private const string DefaultDirectiveName = "JsonService";
		private const string ErrorMissingDirective = "The service must have a <%@ {0} class=\"MyNamespace.MyClass\" ... %> directive.";
		private const string ErrorCouldNotCreateType = "Could not create type \"{0}\".";
		private const string ErrorAmbiguousType = "The type \"{0}\" is ambiguous: it could come from assembly \"{1}\" or from assembly \"{2}\". Please specify the assembly explicitly in the type name.";
		private const string ErrorMultipleDirectives = "There can be only one \"{0}\" directive.";
		private const string ErrorUnkownDirective = "The directive \"{0}\" is unknown.";
		private const string ErrorMissingAttrib = "The directive is missing a '{0}' attribute.";

		private static readonly Regex Regex_Directive = new Regex(Pattern_Directive, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.Compiled);
		private const string Pattern_Directive = "<%\\s*@(\\s*(?<attrname>\\w[\\w:]*(?=\\W))(\\s*(?<equal>=)\\s*\"(?<attrval>[^\"]*)\"|\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%>";

		#endregion Constants

		#region Fields

		private List<Assembly> linkedAssemblies = null;
		private string sourceText = null;
		private CompilerType compilerType = null;
		private string serviceTypeName = null;
		private string proxyTypeName = null;
		private bool directiveParsed = false;
		private int lineNumber = 1;
		private bool foundDirective = false;

		#endregion Fields

		#region BuildProvider Methods

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			try
			{
				this.EnsureDirective();

				if (String.IsNullOrEmpty(this.serviceTypeName))
				{
					return;
				}

				Assembly tempAssembly = null;

				if (!String.IsNullOrEmpty(this.sourceText))
				{
					// generate a code snippet for any inline source
					CodeSnippetCompileUnit unit = new CodeSnippetCompileUnit(this.sourceText);
					unit.LinePragma = new CodeLinePragma(base.VirtualPath, this.lineNumber);

					// add known assembly references
					foreach (Assembly assembly in base.ReferencedAssemblies)
					{
						if (!String.IsNullOrEmpty(assembly.Location) &&
						!unit.ReferencedAssemblies.Contains(assembly.Location))
						{
							unit.ReferencedAssemblies.Add(assembly.Location);
						}
					}

					if (this.linkedAssemblies != null)
					{
						// add parsed assembly dependencies
						foreach (Assembly assembly in this.linkedAssemblies)
						{
							assemblyBuilder.AddAssemblyReference(assembly);
							if (!String.IsNullOrEmpty(assembly.Location) &&
								!unit.ReferencedAssemblies.Contains(assembly.Location))
							{
								unit.ReferencedAssemblies.Add(assembly.Location);
							}
						}
					}

					// compile once so we can reflect and build proxy, etc.
					assemblyBuilder.AddCodeCompileUnit(this, unit);
					CompilerResults results = assemblyBuilder.CodeDomProvider.CompileAssemblyFromDom(new CompilerParameters(), unit);
					if (results.Errors.HasErrors)
					{
						CompilerError error = results.Errors[0];
						throw new HttpParseException(error.ErrorText, null, error.FileName, "", error.Line);
					}
					tempAssembly = results.CompiledAssembly;
				}

				Type serviceType = this.GetTypeToCache(this.serviceTypeName, tempAssembly);
				this.GenerateServiceProxyCode(assemblyBuilder, serviceType);
			}
			catch (Exception ex)
			{
				if (ex is HttpParseException)
				{
					throw;
				}
				throw new HttpParseException("GenerateCode: "+ex.Message, ex, base.VirtualPath, this.sourceText, this.lineNumber);
			}
		}

		private void GenerateServiceProxyCode(AssemblyBuilder assemblyBuilder, Type serviceType)
		{
			// build proxy from main service type
			JsonServiceDescription desc = new JsonServiceDescription(serviceType, base.VirtualPath);
			JsonServiceProxyGenerator proxy = new JsonServiceProxyGenerator(desc);
			string proxyOutput = proxy.OutputProxy(false);
			string debugProxyOutput = proxy.OutputProxy(true);

			// generate a service factory
			CodeCompileUnit generatedUnit = new CodeCompileUnit();
			CodeNamespace ns = new CodeNamespace(JsonServiceBuildProvider.GeneratedNamespace);
			generatedUnit.Namespaces.Add(ns);
			CodeTypeDeclaration descriptorType = new CodeTypeDeclaration();
			descriptorType.IsClass = true;
			descriptorType.Name = "_"+serviceType.GUID.ToString("N");
			descriptorType.Attributes = MemberAttributes.Public|MemberAttributes.Final;
			descriptorType.BaseTypes.Add(typeof(JsonServiceInfo));
			ns.Types.Add(descriptorType);

			#region JsonServiceInfo.Proxy

			// add a readonly property with the proxy code string
			CodeMemberProperty proxyProp = new CodeMemberProperty();
			proxyProp.Name = "Proxy";
			proxyProp.Type = new CodeTypeReference(typeof(String));
			proxyProp.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			proxyProp.HasGet = true;
			// get { return proxyOutput; }
			proxyProp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(proxyOutput)));
			descriptorType.Members.Add(proxyProp);

			#endregion JsonServiceInfo.Proxy

			#region JsonServiceInfo.DebugProxy

			// add a readonly property with the debug proxy code string
			proxyProp = new CodeMemberProperty();
			proxyProp.Name = "DebugProxy";
			proxyProp.Type = new CodeTypeReference(typeof(String));
			proxyProp.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			proxyProp.HasGet = true;
			// get { return debugProxyOutput; }
			proxyProp.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(debugProxyOutput)));
			descriptorType.Members.Add(proxyProp);

			#endregion JsonServiceInfo.DebugProxy

			#region JsonServiceInfo.ServiceType

			// add a static field with the service type
			CodeMemberProperty serviceTypeProp = new CodeMemberProperty();
			serviceTypeProp.Name = "ServiceType";
			serviceTypeProp.Type = new CodeTypeReference(typeof(Type));
			serviceTypeProp.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			serviceTypeProp.HasGet = true;
			// get { return typeof(serviceType); }
			serviceTypeProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeTypeOfExpression(serviceType.FullName)));
			descriptorType.Members.Add(serviceTypeProp);

			#endregion JsonServiceInfo.ServiceType

			#region JsonServiceInfo.CreateService()

			CodeMemberMethod createServiceMethod = new CodeMemberMethod();
			createServiceMethod.Name = "CreateService";
			createServiceMethod.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			createServiceMethod.ReturnType = new CodeTypeReference(typeof(Object));
			// return new serviceType();
			createServiceMethod.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(serviceType)));
			descriptorType.Members.Add(createServiceMethod);

			#endregion JsonServiceInfo.CreateService()

			#region JsonServiceInfo.ResolveMethodName()

			CodeMemberMethod resolveMethod = new CodeMemberMethod();
			resolveMethod.Name = "ResolveMethodName";
			resolveMethod.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			resolveMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(String), "name"));
			resolveMethod.ReturnType = new CodeTypeReference(typeof(MethodInfo));
			CodeVariableReferenceExpression nameParam = new CodeVariableReferenceExpression("name");

			// if (String.IsNullOrEmpty(name)) { return null; }
			CodeConditionStatement nullCheck = new CodeConditionStatement();
			nullCheck.Condition = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(String)), "IsNullOrEmpty", nameParam);
			nullCheck.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
			resolveMethod.Statements.Add(nullCheck);

			Dictionary<string, MethodInfo> methodMap = JsonServiceInfo.CreateMethodMap(serviceType);
			foreach (string name in methodMap.Keys)
			{
				CodeConditionStatement nameTest = new CodeConditionStatement();
				// if (String.Equals(name, methodName)) { ... }
				nameTest.Condition = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(String)), "Equals", nameParam, new CodePrimitiveExpression(name));

				// this.ServiceType
				CodePropertyReferenceExpression serviceTypeRef = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ServiceType");

				// method name
				CodePrimitiveExpression methodNameRef = new CodePrimitiveExpression(methodMap[name].Name);

				// this.ServiceType.GetMethod(methodNameRef)
				CodeMethodInvokeExpression methodInfoRef = new CodeMethodInvokeExpression(serviceTypeRef, "GetMethod", methodNameRef);

				// return MethodInfo;
				nameTest.TrueStatements.Add(new CodeMethodReturnStatement(methodInfoRef));
				resolveMethod.Statements.Add(nameTest);
			}

			resolveMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
			descriptorType.Members.Add(resolveMethod);

			#endregion JsonServiceInfo.ResolveMethodName()

			#region JsonServiceInfo.GetMethodParams()

			CodeMemberMethod paramsMethod = new CodeMemberMethod();
			paramsMethod.Name = "GetMethodParams";
			paramsMethod.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			paramsMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(String), "name"));
			paramsMethod.ReturnType = new CodeTypeReference(typeof(String[]));
			CodeVariableReferenceExpression nameParam2 = new CodeVariableReferenceExpression("name");

			// if (String.IsNullOrEmpty(name)) { return new string[0]; }
			CodeConditionStatement nullCheck2 = new CodeConditionStatement();
			nullCheck2.Condition = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(String)), "IsNullOrEmpty", nameParam);
			nullCheck2.TrueStatements.Add(new CodeMethodReturnStatement(new CodeArrayCreateExpression(typeof(String[]), 0)));
			paramsMethod.Statements.Add(nullCheck2);

			foreach (MethodInfo method in methodMap.Values)
			{
				string[] paramMap = JsonServiceInfo.CreateParamMap(method);

				if (paramMap.Length < 1)
					continue;

				CodeConditionStatement nameTest = new CodeConditionStatement();
				// if (String.Equals(name, method.Name)) { ... }
				nameTest.Condition = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(String)), "Equals", nameParam2, new CodePrimitiveExpression(method.Name));

				// = {...}
				CodePrimitiveExpression[] paramList = new CodePrimitiveExpression[paramMap.Length];
				for (int i=0; i<paramMap.Length; i++)
				{
					paramList[i] = new CodePrimitiveExpression(paramMap[i]);
				}

				// new string[] = {...}
				CodeArrayCreateExpression paramArray = new CodeArrayCreateExpression(typeof(String[]), paramList);

				// return string[];
				nameTest.TrueStatements.Add(new CodeMethodReturnStatement(paramArray));
				paramsMethod.Statements.Add(nameTest);
			}

			paramsMethod.Statements.Add(new CodeMethodReturnStatement(new CodeArrayCreateExpression(typeof(String[]), 0)));
			descriptorType.Members.Add(paramsMethod);

			#endregion JsonServiceInfo.GetMethodParams()

			assemblyBuilder.AddCodeCompileUnit(this, generatedUnit);

			this.proxyTypeName = JsonServiceBuildProvider.GeneratedNamespace+"."+descriptorType.Name;
		}

		public override CompilerType CodeCompilerType
		{
			get
			{
				try
				{
					this.EnsureDirective();

					// if directive failed will be null
					return this.compilerType;
				}
				catch (Exception ex)
				{
					if (ex is HttpParseException)
					{
						throw;
					}
					throw new HttpParseException("CodeCompilerType: "+ex.Message, ex, base.VirtualPath, this.sourceText, this.lineNumber);
				}
			}
		}

		public override Type GetGeneratedType(CompilerResults results)
		{
			try
			{
				this.EnsureDirective();

				if (String.IsNullOrEmpty(this.proxyTypeName) ||
					results.Errors.HasErrors)
				{
					return null;
				}

				return this.GetTypeToCache(this.proxyTypeName, results.CompiledAssembly);
			}
			catch (Exception ex)
			{
				if (ex is HttpParseException)
				{
					throw;
				}
				throw new HttpParseException("GetGeneratedType: "+ex.Message, ex, base.VirtualPath, this.sourceText, this.lineNumber);
			}
		}

		#endregion BuildProvider Methods

		#region Directive Methods

		private void EnsureDirective()
		{
			lock (this.SyncLock)
			{
				if (!this.directiveParsed)
				{
					using (System.IO.TextReader reader = base.OpenReader())
					{
						this.sourceText = reader.ReadToEnd();
					}
					if (this.sourceText == null)
					{
						this.sourceText = String.Empty;
					}

					try
					{
						DirectiveParser parser = new DirectiveParser(this.sourceText, base.VirtualPath);
						parser.ProcessDirective += this.ProcessDirective;
						int index = parser.ParseDirectives(out this.lineNumber);
						this.sourceText = this.sourceText.Substring(index).Trim();
					}
					finally
					{
						this.directiveParsed = true;
					}

					if (!this.foundDirective)
					{
						throw new HttpParseException(String.Format(ErrorMissingDirective, DefaultDirectiveName), null, base.VirtualPath, this.sourceText, this.lineNumber);
					}
				}
			}
		}

		private void ProcessDirective(string directiveName, IDictionary<string, string> attribs, int lineNumber)
		{
			this.lineNumber = lineNumber;

			if (DefaultDirectiveName.Equals(directiveName, StringComparison.OrdinalIgnoreCase))
			{
				if (this.foundDirective)
				{
					throw new HttpParseException(String.Format(ErrorMultipleDirectives, DefaultDirectiveName), null, base.VirtualPath, this.sourceText, this.lineNumber);
				}
				this.foundDirective = true;

				// determine source language
				string language = attribs.ContainsKey("Language") ? attribs["Language"] : null;
				if (String.IsNullOrEmpty(language))
				{
					// default to C# because no additional assemblies needed
					language = "C#";
				}

				this.compilerType = this.GetDefaultCompilerTypeForLanguage(language);

				// determine backing class
				this.serviceTypeName = attribs.ContainsKey("Class") ? attribs["Class"] : null;
			}
			else if ("Assembly".Equals(directiveName, StringComparison.OrdinalIgnoreCase))
			{
				string name = attribs.ContainsKey("Name") ? attribs["Name"] : null;
				if (String.IsNullOrEmpty(name))
				{
					throw new HttpParseException(String.Format(ErrorMissingAttrib, "Name"), null, base.VirtualPath, this.sourceText, this.lineNumber);
				}
				this.AddAssemblyDependency(name);
			}
			else
			{
				throw new HttpParseException(String.Format(ErrorUnkownDirective, directiveName), null, base.VirtualPath, this.sourceText, this.lineNumber);
			}
		}

		#endregion Directive Methods

		#region Type Methods

		private void AddAssemblyDependency(string assemblyName)
		{
			Assembly assembly = Assembly.Load(assemblyName);
			this.AddAssemblyDependency(assembly);
		}

		private void AddAssemblyDependency(Assembly assembly)
		{
			if (this.linkedAssemblies == null)
			{
				this.linkedAssemblies = new List<Assembly>();
			}
			this.linkedAssemblies.Add(assembly);
		}

		private Type GetTypeToCache(string typeName, Assembly assembly)
		{
			Type type = null;
			if (assembly != null)
			{
				type = assembly.GetType(typeName);
			}
			if (type == null)
			{
				type = this.GetType(typeName);
			}
			return type;
		}

		private Type GetType(string typeName)
		{
			Type type = null;
			if (CommaIndexInTypeName(typeName) > 0)// typeName contains assembly
			{
				try
				{
					type = Type.GetType(typeName, true);
					return type;
				}
				catch { }
			}
			type = this.GetTypeFromAssemblies(base.ReferencedAssemblies, typeName, false);
			if (type == null)
			{
				type = this.GetTypeFromAssemblies(this.linkedAssemblies, typeName, false);
				if (type == null)
				{
					throw new HttpParseException(String.Format(ErrorCouldNotCreateType, typeName), null, base.VirtualPath, this.sourceText, this.lineNumber);
				}
				return type;
			}
			return type;
		}

		[ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
		private Type GetTypeFromAssemblies(ICollection assemblies, string typeName, bool ignoreCase)
		{
			if (assemblies == null)
				return null;

			Type type = null;
			foreach (Assembly assembly in assemblies)
			{
				Type type2 = assembly.GetType(typeName, false, ignoreCase);
				if (type2 != null)
				{
					if ((type != null) && (type2 != type))
					{
						throw new HttpParseException(String.Format(ErrorAmbiguousType, typeName, type.Assembly.FullName, type2.Assembly.FullName), null, base.VirtualPath, this.sourceText, this.lineNumber);
					}
					type = type2;
				}
			}
			return type;
		}

		private static int CommaIndexInTypeName(string typeName)
		{
			int comma = typeName.LastIndexOf(',');
			if (comma < 0)
			{
				return -1;
			}
			int bracket = typeName.LastIndexOf(']');
			if (bracket > comma)
			{
				return -1;
			}
			return typeName.IndexOf(',', bracket + 1);
		}

		#endregion Type Methods
	}
}
