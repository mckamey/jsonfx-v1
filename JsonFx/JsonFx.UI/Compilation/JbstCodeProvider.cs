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
using System.Collections.Generic;
using System.Web.Compilation;

using BuildTools;
using JsonFx.JsonML.BST;

namespace JsonFx.Compilation
{
	public class JbstCodeProvider : JsonFx.Compilation.ResourceCodeProvider
	{
		#region Fields

		private string name = null;
		private bool isJsonp = false;

		#endregion Fields

		#region ResourceCodeProvider

		public override string ContentType
		{
			get { return "text/javascript"; }
		}

		public override string FileExtension
		{
			get { return "js"; }
		}

		protected override void ProcessResource(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			out string resource,
			out string compacted,
			List<ParseException> errors)
		{
			resource = this.ParseDirective(sourceText, virtualPath);

			// parse JBST markup
			JbstCompiler parser = new JbstCompiler();
			parser.Parse(resource);

			using (StringWriter sw = new StringWriter())
			{
				// render a pretty-print debug version
				this.Render(parser, sw, true);
				sw.Flush();
				resource = sw.ToString();
			}

			using (StringWriter sw = new StringWriter())
			{
				// render the compacted version
				this.Render(parser, sw, false);
				sw.Flush();
				compacted = sw.ToString();
			}
		}

		#endregion ResourceCodeProvider

		#region Methods

		private void Render(JbstCompiler parser, TextWriter writer, bool prettyPrint)
		{
			// TODO: allow import statements for better de-linting?
			int dot = this.name.IndexOf('.');
			string global = (dot < 0) ? this.name : this.name.Substring(0, dot);
			writer.WriteLine("/*global JsonML, {0} */", global);

			// wrap in JsonP
			writer.Write(name);
			if (this.isJsonp)
			{
				writer.Write("(");
			}
			else
			{
				if (prettyPrint)
				{
					writer.Write(" = ");
				}
				else
				{
					writer.Write("=");
				}
			}

			if (prettyPrint)
			{
				writer.WriteLine("new JsonML.BST(");
			}
			else
			{
				writer.Write("new JsonML.BST(");
			}

			parser.Render(writer, prettyPrint);

			if (this.isJsonp)
			{
				writer.Write("));");
			}
			else
			{
				if (prettyPrint)
				{
					writer.WriteLine(");");
				}
				else
				{
					writer.Write(");");
				}
			}
		}

		private string ParseDirective(string sourceText, string virtualPath)
		{
			int lineNumber;

			DirectiveParser parser = new DirectiveParser(sourceText, virtualPath);
			parser.ProcessDirective += new DirectiveParser.ProcessDirectiveEvent(this.ProcessDirective);

			int index = parser.ParseDirectives(out lineNumber);

			return sourceText.Substring(index).Trim();
		}

		private void ProcessDirective(string directiveName, IDictionary<string, string> attribs, int lineNumber)
		{
			string name = attribs.ContainsKey("Name") ? attribs["Name"] : null;
			if (!String.IsNullOrEmpty(name))
			{
				this.name = name;
				this.isJsonp = false;
				return;
			}

			string method = attribs.ContainsKey("Callback") ? attribs["Callback"] : null;
			if (!String.IsNullOrEmpty(method))
			{
				this.name = method;
				this.isJsonp = true;
				return;
			}

			this.name = "throw new Error";
			this.isJsonp = true;
		}

		#endregion Methods
	}
}
