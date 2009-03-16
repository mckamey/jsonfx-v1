#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Text;

using JsonFx.BuildTools;
using JsonFx.BuildTools.HtmlDistiller;
using JsonFx.BuildTools.HtmlDistiller.Writers;
using JsonFx.BuildTools.ScriptCompactor;
using JsonFx.Json;
using JsonFx.Compilation;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// JBST Template Compiler
	/// </summary>
	public class JbstCompiler : IHtmlWriter
	{
		#region Constants

		private static readonly char[] ImportDelim = { ' ', ',' };

		internal const string PrefixDelim = ":";

		private const string ScriptTagName = "script";

		private const string AnonymousPrefix = "anonymous_";

		private const string DeclarationFormat =
			@"// execute template in the context of ""this""
			(function(){{
				{1}
			}}).call({0});";

		private const string NamespaceFormatPrettyPrint = 
@"/* namespace {0} */
if (""undefined"" === typeof {0}) {{
	{0} = {{}};
}}";

		private const string NamespaceFormat = @"if(""undefined""===typeof {0}){{{0}={{}};}}";

		#endregion Constants

		#region Fields

		private readonly string path;
		private readonly StringBuilder Directives = new StringBuilder();
		private readonly StringBuilder Declarations = new StringBuilder();
		private readonly List<string> Imports = new List<string>();
		private readonly JbstContainerControl document = new JbstContainerControl();

		private JbstContainerControl current = null;
		private string name = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JbstCompiler()
			: this(String.Empty)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="virtualPath"></param>
		public JbstCompiler(string virtualPath)
		{
			this.path = virtualPath;
		}

		#endregion Init

		#region Properties

		public string Name
		{
			get
			{
				if (String.IsNullOrEmpty(this.name))
				{
					this.name = AnonymousPrefix+Guid.NewGuid().ToString("n");
				}
				return this.name;
			}
			set { this.name = value; }
		}

		/// <summary>
		/// Gets the document root
		/// </summary>
		public object Document
		{
			get
			{
				JbstControl control = null;
				foreach (JbstControl child in this.document.ChildControls)
				{
					// tally non-whitespace controls
					JbstLiteral lit = child as JbstLiteral;
					if (lit != null && lit.IsWhitespace)
					{
						continue;
					}

					if (control != null)
					{
						// found 2 or more in root
						// render with document wrapper
						control = this.document;
						break;
					}

					control = child;
				}
				return control;
			}
		}

		#endregion Properties

		#region Parse Methods

		private void AppendChild(string text)
		{
			if (String.IsNullOrEmpty(text))
			{
				return;
			}

			if (this.current == null)
			{
				this.current = this.document;
			}

			// this allows HTML entities to be encoded as unicode
			text = HtmlDistiller.DecodeHtmlEntities(text);

			JbstLiteral literal = this.current.ChildControls.Last as JbstLiteral;
			if (literal != null)
			{
				// combine contiguous literals into single for reduced space and processing
				literal.Text += text;
				return;
			}

			literal = new JbstLiteral(text, true);
			this.current.ChildControls.Add(literal);
		}

		private void AppendChild(JbstControl child)
		{
			if (child == null)
			{
				return;
			}

			if (this.current == null)
			{
				this.current = this.document;
			}

			if (child is JbstLiteral)
			{
				// combine contiguous literals into single for reduced space and processing
				JbstLiteral literal = this.current.ChildControls.Last as JbstLiteral;
				if (literal != null)
				{
					literal.Text += ((JbstLiteral)child).Text;
					return;
				}
			}

			this.current.ChildControls.Add(child);
		}

		private void PushTag(string rawName)
		{
			string tagName;
			string prefix = JbstCompiler.SplitPrefix(rawName, out tagName);

			JbstContainerControl control;
			if (JbstCustomControl.JbstPrefix.Equals(prefix, StringComparison.OrdinalIgnoreCase))
			{
				control = new JbstCustomControl(tagName);
			}
			else
			{
				control = new JbstContainerControl(prefix, tagName);
			}

			if (this.current == null)
			{
				this.current = this.document;
			}

			this.current.ChildControls.Add(control);
			this.current = control;
		}

		private void PopTag(string tagName)
		{
			if (tagName == null)
			{
				tagName = String.Empty;
			}

			if (this.current == null)
			{
				throw new InvalidOperationException("Push/Pop mismatch? (Current is null)");
			}

			if (!String.IsNullOrEmpty(tagName) &&
				!tagName.Equals(this.current.RawName, StringComparison.OrdinalIgnoreCase))
			{
				//throw new InvalidOperationException("Push/Pop mismatch? (tag names do not match)");
				return;
			}

			if (this.current == null)
			{
				throw new InvalidOperationException("Push/Pop mismatch? (Current.Parent is null)");
			}

			if (JbstCompiler.ScriptTagName.Equals(this.current.RawName, StringComparison.OrdinalIgnoreCase))
			{
				// script tags get converted once fully parsed
				this.ConvertControlToDeclaration(this.current);
			}

			this.current = this.current.Parent;
		}

		/// <summary>
		/// Convert a tag and children into declaration block
		/// </summary>
		/// <param name="control"></param>
		private void ConvertControlToDeclaration(JbstControl control)
		{
			if (control is JbstCodeBlock)
			{
				this.Declarations.Append( ((JbstCodeBlock)control).Code );
			}
			else if (control is JbstLiteral)
			{
				this.Declarations.Append( ((JbstLiteral)control).Text );
			}
			else if (control is JbstContainerControl)
			{
				JbstContainerControl parent = (JbstContainerControl)control;

				while (parent.ChildControlsSpecified)
				{
					// this will remove it from the collection
					this.ConvertControlToDeclaration(parent.ChildControls[0]);
				}
			}

			control.Parent.ChildControls.Remove(control);
		}

		private void AddAttribute(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetAttribute(this.current, name, value);
		}

		private void AddAttribute(string name, JbstControl value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.current.Attributes[name] = value;
		}

		private void SetAttribute(JbstContainerControl target, string name, string value)
		{
			value = HtmlDistiller.DecodeHtmlEntities(value);
			if ("style".Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				this.SetStyle(target, null, value);
			}
			else
			{
				target.Attributes[name] = value;
			}
		}

		private void AddStyle(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetStyle(this.current, name, value);
		}

		private void SetStyle(JbstContainerControl target, string name, string value)
		{
			if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(value))
			{
				return;
			}

			if (target == null)
			{
				throw new NullReferenceException("target is null");
			}

			string style =
				target.Attributes.ContainsKey("style") ?
				target.Attributes["style"] as String :
				null;

			if (style != null && !style.EndsWith(";"))
			{
				style += ";";
			}

			if (String.IsNullOrEmpty(name))
			{
				style += value;
			}
			else
			{
				style += name+':'+value;
			}

			target.Attributes["style"] = style;
		}

		public void Clear()
		{
			this.document.ChildControls.Clear();
		}

		#endregion Parse Methods

		#region Render Methods

		public void Render(TextWriter writer, bool prettyPrint, bool isTemplate)
		{
			this.ProcessDirectives();

			if (isTemplate)
			{
				// add JSLINT directives
				if (prettyPrint)
				{
					string globals = this.GetGlobals();
					if (!String.IsNullOrEmpty(globals))
					{
						writer.WriteLine("/*global {0} */", globals);
					}
				}

				string[] namespaces = this.Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i=0; i<namespaces.Length-1; i++)
				{
					string ident = (i == 0) ?
						"window."+namespaces[i] :
						String.Join(".", namespaces, 0, i+1);
					if (prettyPrint)
					{
						writer.WriteLine();
						writer.WriteLine(NamespaceFormatPrettyPrint, ident);
						writer.WriteLine();
					}
					else
					{
						writer.Write(NamespaceFormat, ident);
					}
				}

				// wrap with ctor and assign
				writer.Write(this.Name);
				if (prettyPrint)
				{
					writer.WriteLine(" = JsonML.BST(");
				}
				else
				{
					writer.Write("=JsonML.BST(");
				}
			}

			EcmaScriptWriter jsWriter = new EcmaScriptWriter(writer);
			jsWriter.PrettyPrint = prettyPrint;

			// render root node of content (null is OK)
			jsWriter.Write(this.Document);

			if (isTemplate)
			{
				if (prettyPrint)
				{
					writer.WriteLine(");");
				}
				else
				{
					writer.Write(");");
				}

				// render any declarations
				if (this.Declarations.Length > 0)
				{
					string declarations = String.Format(
						JbstCompiler.DeclarationFormat,
						this.Name,
						this.Declarations);

					if (prettyPrint)
					{
						writer.WriteLine(declarations);
					}
					else
					{
						// min the output for better compaction
						// signal to JSMin that isn't linted so
						// doesn't break users code if they leave
						// off semicolons, etc.
						new JSMin().Run(declarations, writer, false, true);
					}
				}
			}
		}

		#endregion Render Methods

		#region IHtmlWriter Members

		void IHtmlWriter.WriteLiteral(string value)
		{
			this.AppendChild(value);
		}

		void IHtmlWriter.WriteTag(HtmlTag tag)
		{
			switch (tag.TagType)
			{
				case HtmlTagType.Unparsed:
				{
					this.WriteUnparsedTag(tag);
					break;
				}
				case HtmlTagType.FullTag:
				case HtmlTagType.BeginTag:
				{
					this.PushTag(tag.RawName);

					if (tag.HasAttributes)
					{
						this.WriteAttributes(tag);
					}

					if (tag.HasStyles)
					{
						this.WriteStyles(tag);
					}

					if (tag.TagType == HtmlTagType.FullTag)
					{
						this.PopTag(tag.RawName);
					}
					break;
				}
				case HtmlTagType.EndTag:
				{
					this.PopTag(tag.RawName);
					break;
				}
			}
		}

		private void WriteUnparsedTag(HtmlTag tag)
		{
			switch (tag.TagName)
			{
				case "%@":
				{
					// store directive for specialized parsing
					this.Directives.Append(tag.ToString());
					break;
				}
				case "%!":
				{
					// analogous to static code, or JSP declarations
					// executed only on initialization of template
					// output from declarations are appended after the template
					this.Declarations.Append(tag.Content);
					break;
				}
				case "%#": // databinding expression
				{
					// unparsed expressions are emitted directly into JBST
					JbstUnparsedBlock code = new JbstUnparsedBlock(tag.Content);
					this.AppendChild(code);
					break;
				}
				case "%=": // inline expression
				{
					// expressions are emitted directly into JBST
					JbstExpressionBlock code = new JbstExpressionBlock(tag.Content);
					this.AppendChild(code);
					break;
				}
				case "%$":
				{
					// expressions are emitted directly into JBST
					JbstExtensionBlock code = new JbstExtensionBlock(tag.Content, this.path);
					this.AppendChild(code);
					break;
				}
				case "%":
				{
					// statements are emitted directly into JBST
					JbstStatementBlock code = new JbstStatementBlock(tag.Content);
					this.AppendChild(code);
					break;
				}
				case "%--":
				{
					// server-side comments are omitted even for debug
					break;
				}
				case "!--":
				{
					// HTML Comments are emitted directly into JBST
					JbstCommentBlock code = new JbstCommentBlock(tag.Content);
					this.AppendChild(code);
					break;
				}
				default:
				{
					// unrecognized sequences get emitted as encoded text
					this.AppendChild(tag.ToString());
					break;
				}
			}
		}

		private void WriteStyles(HtmlTag tag)
		{
			foreach (KeyValuePair<string, string> style in tag.FilteredStyles)
			{
				this.AddStyle(style.Key, style.Value);
			}
		}

		private void WriteAttributes(HtmlTag tag)
		{
			foreach (KeyValuePair<string, object> attrib in tag.FilteredAttributes)
			{
				// normalize JBST command names
				string key = attrib.Key.StartsWith(JbstCustomControl.JbstPrefix, StringComparison.OrdinalIgnoreCase) ?
					attrib.Key.ToLowerInvariant() : attrib.Key;

				if (attrib.Value is string)
				{
					this.AddAttribute(attrib.Key, (string)attrib.Value);
				}
				else if (attrib.Value is HtmlTag)
				{
					HtmlTag codeVal = (HtmlTag)attrib.Value;
					switch (codeVal.TagName)
					{
						case "%@":
						{
							// store directive for specialized parsing
							this.Directives.Append(codeVal.ToString());
							break;
						}
						case "%!":
						{
							// analogous to static code, or JSP declarations
							// executed only on initialization of template
							// output from declarations are appended after the template
							this.Declarations.Append(codeVal.Content);
							break;
						}
						case "%#": // databinding expression
						//{
						//    // unparsed expressions are emitted directly into JBST
						//    JbstUnparsedBlock code = new JbstUnparsedBlock(codeVal.Content);
						//    this.AddAttribute(key, code);
						//    break;
						//}
						case "%=": // inline expression
						{
							// expressions are emitted directly into JBST
							JbstExpressionBlock code = new JbstExpressionBlock(codeVal.Content);
							this.AddAttribute(key, code);
							break;
						}
						case "%$":
						{
							// expressions are emitted directly into JBST
							JbstExtensionBlock code = new JbstExtensionBlock(codeVal.Content, this.path);
							this.AddAttribute(key, code);
							break;
						}
						case "%":
						{
							// statements are emitted directly into JBST
							JbstStatementBlock code = new JbstStatementBlock(codeVal.Content);
							this.AddAttribute(key, code);
							break;
						}
						case "%--":
						{
							// server-side comments are omitted even for debug
							break;
						}
						case "!--":
						{
							// HTML Comments are emitted directly into JBST
							JbstCommentBlock code = new JbstCommentBlock(codeVal.Content);
							this.AddAttribute(key, code);
							break;
						}
						default:
						{
							// unrecognized sequences get emitted as encoded text
							this.AddAttribute(key, codeVal.ToString());
							break;
						}
					}
				}
			}
		}

		#endregion IHtmlWriter Members

		#region Directive Methods

		private void ProcessDirectives()
		{
			DirectiveParser parser = new DirectiveParser(this.Directives.ToString(), String.Empty);
			parser.ProcessDirective += this.ProcessDirective;

			int index = 0;
			parser.ParseDirectives(out index);
		}

		private void ProcessDirective(string directiveName, IDictionary<string, string> attribs, int lineNumber)
		{
			if (String.IsNullOrEmpty(directiveName))
			{
				return;
			}

			switch (directiveName.ToLowerInvariant())
			{
				case "page":
				case "control":
				{
					this.Name = attribs.ContainsKey("name") ?
						JsonWriter.EnsureValidIdentifier(attribs["name"], true) :
						null;

					string package = attribs.ContainsKey("import") ? attribs["import"] : null;
					if (!String.IsNullOrEmpty(package))
					{
						string[] packages = package.Split(ImportDelim, StringSplitOptions.RemoveEmptyEntries);
						this.Imports.AddRange(packages);
					}
					break;
				}
				case "import":
				{
					string package = attribs.ContainsKey("namespace") ? attribs["namespace"] : null;
					if (!String.IsNullOrEmpty(package))
					{
						this.Imports.Add(package);
					}
					break;
				}
				default:
				{
					// not implemented
					break;
				}
			}
		}

		/// <summary>
		/// Generates a globals list from import directives
		/// </summary>
		/// <returns></returns>
		private string GetGlobals()
		{
			StringBuilder globals = new StringBuilder();

			this.Imports.Insert(0, "JsonML.BST");
			this.Imports.Add(this.Name);

			foreach (string import in this.Imports)
			{
				string ident = JsonWriter.EnsureValidIdentifier(import, true);

				if (String.IsNullOrEmpty(ident))
				{
					continue;
				}

				if (globals.Length > 0)
				{
					globals.Append(", ");
				}

				int dot = ident.IndexOf('.');
				globals.Append((dot < 0) ? ident : ident.Substring(0, dot));
			}

			return globals.ToString();
		}

		#endregion Directive Methods

		#region Utility Methods

		/// <summary>
		/// Splits the prefix and tag name
		/// </summary>
		/// <param name="rawName"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		private static string SplitPrefix(string rawName, out string tagName)
		{
			int index = String.IsNullOrEmpty(rawName) ?
				-1 : rawName.IndexOf(PrefixDelim);

			if (index < 0)
			{
				tagName = rawName;
				return String.Empty;
			}

			tagName = rawName.Substring(index+1);
			return rawName.Substring(0, index+1);
		}

		#endregion Utility Methods
	}
}
