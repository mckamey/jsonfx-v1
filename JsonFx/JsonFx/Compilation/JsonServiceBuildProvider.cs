using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.IO;

using JsonFx.Services;
using JsonFx.Services.Discovery;
using JsonFx.Services.Proxy;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public class JsonServiceBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Constants

		protected internal const string GeneratedNamespace = "JsonFx._Generated";

		protected const string DefaultDirectiveName = "JsonService";
		protected const string ErrorMissingDirective = "The page must have a <%@ {0} class=\"MyNamespace.MyClass\" ... %> directive.";
		protected const string ErrorDuplicateAttrib = "The directive contains duplicate \"{0}\" attributes.";
		protected const string ErrorAttribNotSupported = "The \"{0}\" attribute is not supported by the \"{1}\" directive.";
		protected const string ErrorCouldNotCreateType = "Could not create type \"{0}\".";
		//protected const string ErrorInsufficientTrust = "The current trust level does not allow use of the \"{0}\" attribute";
		protected const string ErrorAmbiguousType = "The type \"{0}\" is ambiguous: it could come from assembly \"{1}\" or from assembly \"{2}\". Please specify the assembly explicitly in the type name.";
		protected const string ErrorMultipleDirectives = "There can be only one \"{0}\" directive.";
		protected const string ErrorUnkownDirective = "The directive \"{0}\" is unknown.";
		protected const string ErrorAttribMutExcl = "The \"{0}\" and \"{1}\" attributes are mutually exclusive.";
		protected const string ErrorMissingAttrib = "The directive is missing a '{0}' attribute.";

		private static readonly Regex Regex_Directive = new Regex(Pattern_Directive, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.Compiled);
		private const string Pattern_Directive = "<%\\s*@(\\s*(?<attrname>\\w[\\w:]*(?=\\W))(\\s*(?<equal>=)\\s*\"(?<attrval>[^\"]*)\"|\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%>";

		#endregion Constants

		#region Fields

		private List<Assembly> linkedAssemblies = null;
		private string sourceText = null;
		private CompilerType compilerType = null;
		private string serviceTypeName = null;
		private string descriptorTypeName = null;
		private bool sourceParsed = false;
		private int lineNumber = 1;

		#endregion Fields

		#region Init

		public JsonServiceBuildProvider()
		{
		}

		#endregion Init

		#region BuildProvider Methods

		public override System.Collections.ICollection VirtualPathDependencies
		{
			get { return base.VirtualPathDependencies; }
		}

		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			this.EnsureDirective();

			Assembly tempAssembly = null;

			if (!String.IsNullOrEmpty(this.sourceText))
			{
				// generate a code snippet for any inline source
				CodeSnippetCompileUnit unit = new CodeSnippetCompileUnit(sourceText);
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
				tempAssembly = results.CompiledAssembly;
			}

			// build proxy from main service type
			Type serviceType = this.GetTypeToCache(this.serviceTypeName, tempAssembly);
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

			this.descriptorTypeName = JsonServiceBuildProvider.GeneratedNamespace+"."+descriptorType.Name;
		}

		public override CompilerType CodeCompilerType
		{
			get
			{
				this.EnsureDirective();

				if (this.compilerType == null)
				{
					return base.CodeCompilerType;
				}

				return this.compilerType;
			}
		}
		
		public override string GetCustomString(CompilerResults results)
		{
			this.EnsureDirective();

			return base.GetCustomString(results);
		}

		public override Type GetGeneratedType(CompilerResults results)
		{
			this.EnsureDirective();

			return this.GetTypeToCache(this.descriptorTypeName, results.CompiledAssembly);
		}

		public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
		{
			return base.GetResultFlags(results);
		}

		#endregion BuildProvider Methods

		#region Methods

		private void EnsureDirective()
		{
			if (!this.sourceParsed)
			{
				this.ParseReader();
			}
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
			try
			{
				this.ValidateBaseType(type);
			}
			catch (Exception ex)
			{
				throw new System.Web.HttpParseException(ex.Message, ex, base.VirtualPath, this.sourceText, this.lineNumber);
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
				}
				catch (Exception ex)
				{
					throw new System.Web.HttpParseException(null, ex, base.VirtualPath, this.sourceText, this.lineNumber);
				}
				return type;
			}
			type = this.GetTypeFromAssemblies(base.ReferencedAssemblies, typeName, false);
			if (type == null)
			{
				type = this.GetTypeFromAssemblies(this.linkedAssemblies, typeName, false);
				if (type == null)
				{
					throw new System.Web.HttpParseException(String.Format(ErrorCouldNotCreateType, typeName), null, base.VirtualPath, this.sourceText, this.lineNumber);
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
						throw new System.Web.HttpParseException(String.Format(ErrorAmbiguousType, typeName, type.Assembly.FullName, type2.Assembly.FullName), null, base.VirtualPath, this.sourceText, this.lineNumber);
					}
					type = type2;
				}
			}
			return type;
		}

		private void ValidateBaseType(Type type)
		{
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

		#endregion Methods

		#region Parsing Methods

		private void ParseReader()
		{
			try
			{
				// TODO: add source dependency on virtual path?

				using (System.IO.TextReader reader = base.OpenReader())
				{
					this.sourceText = reader.ReadToEnd();
				}

				bool foundDirective = false;
				int index = 0;
				int oldIndex = 0;
				string directiveName;
				IDictionary attribs;
				while (this.ProcessDirective(this.sourceText, out directiveName, out attribs, ref index))
				{
					while (oldIndex < index)
					{
						oldIndex = this.sourceText.IndexOf('\n', oldIndex);
						if (oldIndex < 0 || oldIndex >= index)
							break;
						oldIndex++;// move past char
						this.lineNumber++;// inc line count
					}
					oldIndex = index;

					if (DefaultDirectiveName.Equals(directiveName, StringComparison.OrdinalIgnoreCase))
					{
						if (foundDirective)
						{
							throw new System.Web.HttpParseException(String.Format(ErrorMultipleDirectives, DefaultDirectiveName), null, base.VirtualPath, this.sourceText, this.lineNumber);
						}
						foundDirective = true;

						// determine source language
						string language = GetAndRemove(attribs, "Language");
						this.compilerType = (language != null) ?
							base.GetDefaultCompilerTypeForLanguage(language) :
							base.GetDefaultCompilerType();

						// determine backing class
						this.serviceTypeName = GetAndRemove(attribs, "Class");

						attribs.Remove("Description");
						attribs.Remove("CodeBehind");

						this.ProcessCompilationParams(attribs, compilerType.CompilerParameters);
					}
					else if ("Assembly".Equals(directiveName, StringComparison.OrdinalIgnoreCase))
					{
						string name = GetAndRemove(attribs, "Name");
						string src = GetAndRemove(attribs, "src");
						if ((name != null) && (src != null))
						{
							throw new System.Web.HttpParseException(String.Format(ErrorAttribMutExcl, "Name", "Src"), null, base.VirtualPath, this.sourceText, this.lineNumber);
						}
						if (name != null)
						{
							  this.AddAssemblyDependency(name);
						}
						else if (src != null)
						{
							  this.ImportSourceFile(src);
						}
						else
						{
							throw new System.Web.HttpParseException(String.Format(ErrorMissingAttrib, "Name"), null, base.VirtualPath, this.sourceText, this.lineNumber);
						}
					}
					else
					{
						throw new System.Web.HttpParseException(String.Format(ErrorUnkownDirective, directiveName), null, base.VirtualPath, this.sourceText, this.lineNumber);
					}

					this.CheckUnknownDirectiveAttributes(directiveName, attribs);
				}

				if (!foundDirective)
				{
					throw new System.Web.HttpParseException(String.Format(ErrorMissingDirective, DefaultDirectiveName), null, base.VirtualPath, this.sourceText, this.lineNumber);
				}

				this.sourceText = this.sourceText.Substring(index).TrimEnd();
			}
			finally
			{
				this.sourceParsed = true;
			}
		}

		private bool ProcessDirective(string source, out string directiveName, out IDictionary attribs, ref int index)
		{
			Match match = Regex_Directive.Match(source, index);
			if (!match.Success)
			{
				attribs = null;
				directiveName = null;
				return false;
			}

			index = match.Index+match.Length;
			attribs = new SortedList(CaseInsensitiveComparer.Default);
			directiveName = this.ProcessAttributes(match, attribs);

			return true;
		}

		/// <summary>
		/// Parses the directive for its attributes.
		/// </summary>
		/// <param name="match"></param>
		/// <param name="attribs"></param>
		/// <returns>directive name</returns>
		private string ProcessAttributes(Match match, IDictionary attribs)
		{
			string directiveName = String.Empty;
			CaptureCollection names = match.Groups["attrname"].Captures;
			CaptureCollection values = match.Groups["attrval"].Captures;
			CaptureCollection equals = match.Groups["equal"].Captures;
			for (int i = 0; i < names.Count; i++)
			{
				string name = names[i].ToString();
				string value = values[i].ToString();
				bool isAttribute = equals[i].ToString().Length > 0;
				if (name != null)
				{
					if (!isAttribute && (i == 0))
					{
						directiveName = name;
					}
					else
					{
						try
						{
							if (attribs != null)
							{
								attribs.Add(name, value);
							}
						}
						catch (ArgumentException)
						{
							throw new System.Web.HttpParseException(String.Format(ErrorDuplicateAttrib, name), null, base.VirtualPath, this.sourceText, this.lineNumber);
						}
					}
				}
			}
			return directiveName;
		}

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

		private void ImportSourceFile(string src)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		private void CheckUnknownDirectiveAttributes(string directiveName, IDictionary attribs)
		{
			if (attribs.Count > 0)
			{
				throw new System.Web.HttpParseException(String.Format(ErrorAttribNotSupported, GetFirstKey(attribs), directiveName), null, base.VirtualPath, this.sourceText, this.lineNumber);
			}
		}

		private void ProcessCompilationParams(IDictionary directive, CompilerParameters compilerParams)
		{
			bool debug = false;
			string debugStr = GetAndRemove(directive, "Debug");
			if (Boolean.TryParse(debugStr, out debug))
			{
				compilerParams.IncludeDebugInformation = debug;
			}
			//if (compilerParams.IncludeDebugInformation && !System.Web.HttpRuntime.HasAspNetHostingPermission(System.Web.AspNetHostingPermissionLevel.Medium))
			//{
			//    throw new System.Web.HttpParseException(String.Format(ErrorInsufficientTrust, "Debug"), null, base.VirtualPath, this.sourceText, this.lineNumber);
			//}

			uint warningLevel = 0;
			string warningStr = GetAndRemove(directive, "WarningLevel");
			if (UInt32.TryParse(warningStr, out warningLevel))
			{
				compilerParams.WarningLevel = (int)warningLevel;
				if (warningLevel > 0)
				{
					compilerParams.TreatWarningsAsErrors = true;
				}
			}
			string compilerOptions = GetAndRemove(directive, "CompilerOptions");
			if (compilerOptions != null)
			{
				//if (!String.IsNullOrEmpty(compilerOptions) && !System.Web.HttpRuntime.HasUnmanagedPermission())
				//{
				//    throw new System.Web.HttpParseException(String.Format(ErrorInsufficientTrust, "CompilerOptions"), null, base.VirtualPath, this.sourceText, this.lineNumber);
				//}
				compilerParams.CompilerOptions = compilerOptions;
			}
		}

		#endregion Parsing Methods

		#region Dictionary Methods

		private static string GetAndRemove(IDictionary dictionary, string key)
		{
			string value = (string)dictionary[key];
			if (value != null)
			{
				dictionary.Remove(key);
				value = value.Trim();
			}
			return value;
		}

		private static string GetFirstKey(IDictionary dictionary)
		{
			IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
			enumerator.MoveNext();
			return (string)enumerator.Key;
		}

		#endregion Dictionary Methods
	}
}
