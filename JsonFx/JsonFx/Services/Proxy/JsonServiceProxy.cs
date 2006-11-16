using System;
using System.IO;
using System.Text.RegularExpressions;

using JsonFx.Services.Discovery;

namespace JsonFx.Services.Proxy
{
	/// <summary>
	/// Generates a JavaScript proxy class for communicating with a JsonService.
	/// </summary>
	public class JsonServiceProxy
	{
		#region Constants

		private const string Pattern_Identifier = @"^[a-zA-Z_][0-9a-zA-Z_]+$";
		private Regex Regex_Identifier = new Regex(Pattern_Identifier, RegexOptions.Compiled|RegexOptions.CultureInvariant);

		private const char NamespaceDelim = '.';
		private static readonly char[] NamespaceDelims = new char[] { '.' };

#if DEBUG
		private const string ExternFormat = "/*extern JsonFx, {0}*/\r\n";

		private const string NamespaceFormat = "if (typeof({0}) == \"undefined\") {{\r\n\t{0} = {{}};\r\n}}\r\n";

		private const string ClassBeginFormat = "if (typeof({0}{1}) == \"undefined\") {{\r\n\r\n\t/*ctor*/\r\n\t{0}{1} = function(/*string*/ serviceUrl) {{\r\n\t\tif (serviceUrl) {{\r\n\t\t\tthis.address = serviceUrl;\r\n\t\t}}\r\n\t}};\r\n\r\n\t/*base*/\r\n\t{0}{1}.prototype = new JsonFx.IO.JsonServiceBase();\r\n\r\n";
		private const string ClassEnd = "}\r\n";
		private const string PropertyFormat = "\t/*string*/ {0}{1}.prototype.{2} = \"{3}\";\r\n\r\n";
		private const string SafePropertyFormat = "\t/*string*/ {0}{1}.prototype[\"{2}\"] = \"{3}\";\r\n\r\n";

		private const string MethodBeginFormat = "\t/*{2}*/ {0}{1}.prototype.{3} = function(";
		private const string SafeMethodBeginFormat = "\t/*{2}*/ {0}{1}.prototype[\"{3}\"] = function(";
		private const string MethodMiddleFormat = "/*function(object,object)*/ callback, /*object*/ context) {{\r\n\t\tthis.callService(\"{0}\", ";
		private const string MethodEndFormat = ", callback, context);\r\n\t};\r\n\r\n";

		private const string ParamFormat = "/*{0}*/ {1}, ";
#else
		private const string ExternFormat = "/*extern JsonFx,{0}*/";

		private const string NamespaceFormat = "if(typeof({0})==\"undefined\"){{{0}={{}};}}";

		private const string ClassBeginFormat = "if(typeof({0}{1})==\"undefined\"){{{0}{1}=function(url){{if(url){{this.address=url;}}}};{0}{1}.prototype=new JsonFx.IO.JsonServiceBase();";
		private const string ClassEnd = "}";
		private const string PropertyFormat = "{0}{1}.prototype.{2}=\"{3}\";";
		private const string SafePropertyFormat = "{0}{1}.prototype[\"{2}\"]=\"{3}\";";

		private const string MethodBeginFormat = "{0}{1}.prototype.{3}=function(";
		private const string SafeMethodBeginFormat = "{0}{1}.prototype[\"{3}\"]=function(";
		private const string MethodMiddleFormat = "cb,cx){{this.callService(\"{0}\",";
		private const string MethodEndFormat = ",cb,cx);};";

		private const string ParamFormat = "{1},";
#endif

		private const string ArgsFormat = "[{0}]";

		#endregion Constants

		#region Fields

		private JsonServiceDescription service;
		private string proxyNamespace = null;

		#endregion Fields

		#region Init

		internal JsonServiceProxy(JsonServiceDescription service)
		{
			this.service = service;
		}

		internal JsonServiceProxy(JsonServiceDescription service, string proxyNamespace) : this(service)
		{
			if (!String.IsNullOrEmpty(proxyNamespace))
				this.proxyNamespace = proxyNamespace + JsonServiceProxy.NamespaceDelim;
		}

		public JsonServiceProxy(Type serviceType, string serviceUrl)
			: this(new JsonServiceDescription(serviceType, serviceUrl), serviceType.Namespace)
		{
		}

		public JsonServiceProxy(Type serviceType, string serviceUrl, string proxyNamespace)
			: this(new JsonServiceDescription(serviceType, serviceUrl), proxyNamespace)
		{
		}

		#endregion Init

		#region Properties

		public JsonServiceDescription Service
		{
			get { return this.service; }
		}

		public string ProxyNamespace
		{
			get { return this.proxyNamespace; }
		}

		#endregion Properties

		#region Public Methods

		public string OutputProxy()
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			this.OutputProxy(builder);
			return builder.ToString();
		}

		public void OutputProxy(System.Text.StringBuilder builder)
		{
			using (TextWriter writer = new StringWriter(builder, System.Globalization.CultureInfo.InvariantCulture))
			{
				this.OutputProxy(writer);
			}
		}

		public void OutputProxy(Stream output)
		{
			using (TextWriter writer = new StreamWriter(output, System.Text.Encoding.UTF8))
			{
				this.OutputProxy(writer);
			}
		}

		public void OutputProxy(TextWriter writer)
		{
			this.WriteNamespaces(writer);

			writer.Write(JsonServiceProxy.ClassBeginFormat, this.ProxyNamespace, this.Service.Name);

			this.WriteProperty(writer, "name", this.service.Name);

			this.WriteProperty(writer, "id", this.service.ID);

			this.WriteProperty(writer, "version", this.service.Version);

			this.WriteProperty(writer, "summary", this.service.Summary);

			this.WriteProperty(writer, "help", this.service.Help);

			this.WriteProperty(writer, "address", this.service.Address);

			foreach (JsonMethodDescription method in this.Service.Methods)
			{
				this.WriteMethod(writer, method);
			}

			writer.Write(JsonServiceProxy.ClassEnd);
		}

		#endregion Public Methods

		#region Methods

		private void WriteNamespaces(TextWriter writer)
		{
			if (!String.IsNullOrEmpty(this.ProxyNamespace))
			{
				string[] namespaces = this.ProxyNamespace.Split(JsonServiceProxy.NamespaceDelims, StringSplitOptions.RemoveEmptyEntries);
				string combined = String.Empty;
				foreach (string ns in namespaces)
				{
					if (!String.IsNullOrEmpty(combined))
					{
						combined += JsonServiceProxy.NamespaceDelim;
					}
					else
					{
						writer.Write(JsonServiceProxy.ExternFormat, ns);
					}
					combined += ns;
					writer.Write(JsonServiceProxy.NamespaceFormat, combined);
				}
			}
			else
			{
				writer.Write(JsonServiceProxy.ExternFormat, this.Service.Name);
			}
		}

		private void WriteProperty(TextWriter writer, string name, string value)
		{
			if (Regex_Identifier.Match(name).Success)
			{
				writer.Write(JsonServiceProxy.PropertyFormat, this.ProxyNamespace, this.Service.Name, name, value);
			}
			else
			{
				writer.Write(JsonServiceProxy.SafePropertyFormat, this.ProxyNamespace, this.Service.Name, name, value);
			}
		}

		private void WriteMethod(TextWriter writer, JsonMethodDescription method)
		{
			if (Regex_Identifier.Match(method.Name).Success)
			{
				writer.Write(JsonServiceProxy.MethodBeginFormat, this.ProxyNamespace, this.Service.Name, this.ConvertParamType(method.Return.Type), method.Name);
			}
			else
			{
				writer.Write(JsonServiceProxy.SafeMethodBeginFormat, this.ProxyNamespace, this.Service.Name, this.ConvertParamType(method.Return.Type), method.Name);
			}

			foreach (JsonNamedParameterDescription param in method.Params)
			{
				this.WriteParameter(writer, param);
			}

			writer.Write(JsonServiceProxy.MethodMiddleFormat, method.Name);

			if (method.Params.Length > 0)
			{
				string[] args = new string[method.Params.Length];
				for (int i=0; i<method.Params.Length; i++)
				{
					args[i] = method.Params[i].Name;
				}
				writer.Write(JsonServiceProxy.ArgsFormat, String.Join(",", args));
			}
			else
			{
				writer.Write("null");
			}

			writer.Write(JsonServiceProxy.MethodEndFormat);
		}

		private void WriteParameter(TextWriter writer, JsonNamedParameterDescription param)
		{
			string paramType = this.ConvertParamType(param.Type);
			writer.Write(JsonServiceProxy.ParamFormat, paramType, param.Name);
		}

		private string ConvertParamType(JsonParameterType paramType)
		{
			switch (paramType)
			{
				case JsonParameterType.Any:
				{
					return "object";
				}
				case JsonParameterType.None:
				{
					return "void";
				}
				default:
				{
					return paramType.ToString().ToLowerInvariant();
				}
			}
		}

		#endregion Methods
	}
}
