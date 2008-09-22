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
using System.Text.RegularExpressions;

using JsonFx.JsonRpc.Discovery;

namespace JsonFx.JsonRpc.Proxy
{
	/// <summary>
	/// Generates a JavaScript proxy class for communicating with a JsonService.
	/// </summary>
	internal class JsonServiceProxyGenerator
	{
		#region Constants

		private readonly object SyncLock = new object();

		private const string Pattern_Identifier = @"^[a-zA-Z_][0-9a-zA-Z_]+$";
		private Regex Regex_Identifier = new Regex(Pattern_Identifier, RegexOptions.Compiled|RegexOptions.CultureInvariant);

		private const char NamespaceDelim = '.';
		private static readonly char[] NamespaceDelims = { '.' };

		#endregion Constants

		#region Fields

		private JsonServiceDescription service;
		private string proxyNamespace = null;
		private JsonServiceProxyFormat formatter = null;

		#endregion Fields

		#region Init

		internal JsonServiceProxyGenerator(JsonServiceDescription service)
		{
			this.service = service;
			if (!String.IsNullOrEmpty(service.Namespace))
			{
				this.proxyNamespace = service.Namespace + JsonServiceProxyGenerator.NamespaceDelim;
			}
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

		public string OutputProxy(bool prettyPrint)
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			this.OutputProxy(builder, prettyPrint);
			return builder.ToString();
		}

		public void OutputProxy(System.Text.StringBuilder builder, bool prettyPrint)
		{
			using (TextWriter writer = new StringWriter(builder, System.Globalization.CultureInfo.InvariantCulture))
			{
				this.OutputProxy(writer, prettyPrint);
			}
		}

		public void OutputProxy(Stream output, bool prettyPrint)
		{
			using (TextWriter writer = new StreamWriter(output, System.Text.Encoding.UTF8))
			{
				this.OutputProxy(writer, prettyPrint);
			}
		}

		public void OutputProxy(TextWriter writer, bool prettyPrint)
		{
			lock (this.SyncLock)
			{
				// locking because channging Formatter based upon debug switch

				if (prettyPrint)
				{
					this.formatter = new DebugJsonServiceProxyFormat();
				}
				else
				{
					this.formatter = new CompactJsonServiceProxyFormat();
				}

				this.WriteNamespaces(writer);

				writer.Write(this.formatter.ClassBeginFormat, this.ProxyNamespace, this.Service.Name);

				this.WriteProperty(writer, "proxyName", proxyNamespace+this.service.Name);

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

				writer.Write(this.formatter.ClassEnd);

				writer.Write(this.formatter.ProxyInstantiation, this.ProxyNamespace, this.Service.Name);
			}
		}

		#endregion Public Methods

		#region Methods

		private void WriteNamespaces(TextWriter writer)
		{
			if (!String.IsNullOrEmpty(this.ProxyNamespace))
			{
				string[] namespaces = this.ProxyNamespace.Split(JsonServiceProxyGenerator.NamespaceDelims, StringSplitOptions.RemoveEmptyEntries);
				string combined = String.Empty;
				foreach (string ns in namespaces)
				{
					if (!String.IsNullOrEmpty(combined))
					{
						combined += JsonServiceProxyGenerator.NamespaceDelim;
					}
					else
					{
						writer.Write(this.formatter.ExternFormat, ns);
					}
					combined += ns;
					writer.Write(this.formatter.NamespaceFormat, combined);
				}
			}
			else
			{
				writer.Write(this.formatter.ExternFormat, this.Service.Name);
			}
		}

		private void WriteProperty(TextWriter writer, string name, string value)
		{
			if (Regex_Identifier.Match(name).Success)
			{
				writer.Write(this.formatter.PropertyFormat, this.ProxyNamespace, this.Service.Name, name, value);
			}
			else
			{
				writer.Write(this.formatter.SafePropertyFormat, this.ProxyNamespace, this.Service.Name, name, value);
			}
		}

		private void WriteMethod(TextWriter writer, JsonMethodDescription method)
		{
			if (Regex_Identifier.Match(method.Name).Success)
			{
				writer.Write(this.formatter.MethodBeginFormat, this.ProxyNamespace, this.Service.Name, this.ConvertParamType(method.Return.Type), method.Name);
			}
			else
			{
				writer.Write(this.formatter.SafeMethodBeginFormat, this.ProxyNamespace, this.Service.Name, this.ConvertParamType(method.Return.Type), method.Name);
			}

			foreach (JsonNamedParameterDescription param in method.Params)
			{
				this.WriteParameter(writer, param);
			}

			writer.Write(this.formatter.MethodMiddleFormat, method.Name);

			if (method.Params.Length > 0)
			{
				string[] args = new string[method.Params.Length];
				for (int i=0; i<method.Params.Length; i++)
				{
					args[i] = method.Params[i].Name;
				}
				writer.Write(this.formatter.ArgsFormat, String.Join(",", args));
			}
			else
			{
				writer.Write("null");
			}

			writer.Write(this.formatter.MethodEndFormat);
		}

		private void WriteParameter(TextWriter writer, JsonNamedParameterDescription param)
		{
			string paramType = this.ConvertParamType(param.Type);
			writer.Write(this.formatter.ParamFormat, paramType, param.Name);
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

	internal abstract class JsonServiceProxyFormat
	{
		#region Properties

		internal abstract string ExternFormat { get; }

		internal abstract string NamespaceFormat { get; }

		internal abstract string ClassBeginFormat { get; }

		internal abstract string ClassEnd { get; }

		internal abstract string ProxyInstantiation { get; }

		internal abstract string PropertyFormat { get; }

		internal abstract string SafePropertyFormat { get; }

		internal abstract string MethodBeginFormat { get; }

		internal abstract string SafeMethodBeginFormat { get; }

		internal abstract string MethodMiddleFormat { get; }

		internal abstract string MethodEndFormat { get; }

		internal abstract string ParamFormat { get; }

		internal virtual string ArgsFormat
		{
			get { return "[{0}]"; }
		}

		#endregion Properties
	}

	internal class CompactJsonServiceProxyFormat : JsonServiceProxyFormat
	{
		#region Properties

		internal override string ExternFormat
		{
			get { return ""; }
		}

		internal override string NamespaceFormat
		{
			get { return "if(\"undefined\"===typeof window.{0}){{window.{0}={{}};}}"; }
		}

		internal override string ClassBeginFormat
		{
			get { return "if(\"undefined\"===typeof {0}{1}){{{0}{1}=function(url){{if(url){{this.address=url;}}}};{0}{1}.prototype=new JsonFx.IO.Service();"; }
		}

		internal override string ClassEnd
		{
			get { return "}"; }
		}

		internal override string ProxyInstantiation
		{
			get { return "{0}{1}=new {0}{1}();"; }
		}

		internal override string PropertyFormat
		{
			get { return "{0}{1}.prototype.{2}=\"{3}\";"; }
		}

		internal override string SafePropertyFormat
		{
			get { return "{0}{1}.prototype[\"{2}\"]=\"{3}\";"; }
		}

		internal override string MethodBeginFormat
		{
			get { return "{0}{1}.prototype.{3}=function("; }
		}

		internal override string SafeMethodBeginFormat
		{
			get { return "{0}{1}.prototype[\"{3}\"]=function("; }
		}

		internal override string MethodMiddleFormat
		{
			get { return "opt){{this.callService(\"{0}\","; }
		}

		internal override string MethodEndFormat
		{
			get { return ",opt);};"; }
		}

		internal override string ParamFormat
		{
			get { return "{1},"; }
		}

		#endregion Properties
	}

	internal class DebugJsonServiceProxyFormat : JsonServiceProxyFormat
	{
		#region Properties

		internal override string ExternFormat
		{
			get { return "/*global JsonFx, {0}*/\r\n"; }
		}

		internal override string NamespaceFormat
		{
			get { return "if (\"undefined\" === typeof window.{0}) {{\r\n\twindow.{0} = {{}};\r\n}}\r\n"; }
		}

		internal override string ClassBeginFormat
		{
			get { return "if (\"undefined\" === typeof {0}{1}) {{\r\n\r\n\t/*ctor*/\r\n\t{0}{1} = function(/*string*/ serviceUrl) {{\r\n\t\tif (serviceUrl) {{\r\n\t\t\tthis.address = serviceUrl;\r\n\t\t}}\r\n\t}};\r\n\r\n\t/*base*/\r\n\t{0}{1}.prototype = new JsonFx.IO.Service();\r\n\r\n"; }
		}

		internal override string ClassEnd
		{
			get { return "}\r\n"; }
		}

		internal override string ProxyInstantiation
		{
			get { return "/* create singleton instance destroying the ctor */\r\n/*{0}{1}*/ {0}{1} = new {0}{1}();"; }
		}

		internal override string PropertyFormat
		{
			get { return "\t/*string*/ {0}{1}.prototype.{2} = \"{3}\";\r\n\r\n"; }
		}

		internal override string SafePropertyFormat
		{
			get { return "\t/*string*/ {0}{1}.prototype[\"{2}\"] = \"{3}\";\r\n\r\n"; }
		}

		internal override string MethodBeginFormat
		{
			get { return "\t/*{2}*/ {0}{1}.prototype.{3} = function("; }
		}

		internal override string SafeMethodBeginFormat
		{
			get { return "\t/*{2}*/ {0}{1}.prototype[\"{3}\"] = function("; }
		}

		internal override string MethodMiddleFormat
		{
			get { return "/*RequestOptions*/ options) {{\r\n\t\tthis.callService(\"{0}\", "; }
		}

		internal override string MethodEndFormat
		{
			get { return ", options);\r\n\t};\r\n\r\n"; }
		}

		internal override string ParamFormat
		{
			get { return "/*{0}*/ {1}, "; }
		}

		#endregion Properties
	}
}
