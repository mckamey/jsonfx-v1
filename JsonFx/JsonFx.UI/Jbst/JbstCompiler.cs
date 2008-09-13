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
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Text;

using BuildTools;
using BuildTools.HtmlDistiller;
using BuildTools.HtmlDistiller.Filters;
using BuildTools.HtmlDistiller.Writers;
using BuildTools.ScriptCompactor;
using JsonFx.Compilation;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// JsonML+BST Template Compiler
	/// </summary>
	internal class JbstCompiler : IHtmlWriter
	{
		#region Constants

		private static readonly char[] ImportDelim = { ' ', ',' };

		private const string AnonymousPrefix = "anonymous_";

		private const string DeclarationFormat =
			@"try {{
				// setup context for declarations
				{0}[""$jbst.init""] = function() {{
					{1}
				}};
				// execute in the context of template as ""this""
				{0}[""$jbst.init""]();
			}} finally {{
				delete {0}[""$jbst.init""];
			}}";

		private const string NamespaceFormatPrettyPrint = 
@"/* namespace {0} */
if (""undefined"" === typeof {0}) {{
	{0} = {{}};
}}";

		private const string NamespaceFormat = @"if(""undefined""===typeof {0}){{{0}={{}};}}";

		#endregion Constants

		#region Fields

		private List<ParseException> errors = new List<ParseException>();
		private JbstControl document = new JbstControl(String.Empty);
		private JbstControl current = null;
		private JbstControl next = null;

		private bool isTemplate = false;
		private bool isParsing = false;
		private readonly string path;
		private readonly HtmlDistiller parser = new HtmlDistiller();
		private readonly StringBuilder Directives = new StringBuilder();
		private readonly StringBuilder Declarations = new StringBuilder();
		private readonly List<string> Imports = new List<string>();

		private string name = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="isTemplate">JsonML+BST</param>
		internal JbstCompiler(string virtualPath, bool isTemplate)
		{
			this.path = virtualPath;
			this.isTemplate = isTemplate;

			this.parser.EncodeNonAscii = false;
			this.parser.BalanceTags = false;
			this.parser.NormalizeWhitespace = false;
			this.parser.HtmlFilter = new UnsafeHtmlFilter();
			this.parser.HtmlWriter = this;
			this.parser.BeginIncrementalParsing();
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets a list of any errors that were encountered during parsing
		/// </summary>
		internal List<ParseException> Errors
		{
			get { return this.errors; }
		}

		internal event EventHandler DocumentReady;

		protected string Name
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
		public JbstControl Document
		{
			get { return this.document; }
		}

		#endregion Properties

		#region Parse Methods

		/// <summary>
		/// Parses markup.
		/// </summary>
		/// <param name="literal"></param>
		internal void Parse(string source)
		{
			try
			{
				this.isParsing = true;

				this.parser.Parse(source);
			}
			catch (Exception ex)
			{
				this.errors.Add(new ParseError(ex.Message, null, 1, 1, ex));
			}
			finally
			{
				this.isParsing = false;
			}
		}

		/// <summary>
		/// Writes any buffered text
		/// </summary>
		internal void Flush()
		{
			if (this.isParsing)
			{
				return;
			}

			try
			{
				this.isParsing = true;

				// flush remaining
				this.parser.EndIncrementalParsing();

				// reset for additional
				this.parser.BeginIncrementalParsing();
			}
			finally
			{
				this.isParsing = false;
			}
		}

		internal void AppendChild(string text)
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
			if (literal == null)
			{
				literal = new JbstLiteral(text);
				this.current.ChildControls.Add(literal);
			}
			else
			{
				literal.Text += text;
			}
		}

		internal void AppendChild(IJbstControl child)
		{
			if (child == null)
			{
				return;
			}

			if (this.current == null)
			{
				this.current = this.document;
			}

			this.current.ChildControls.Add(child);
		}

		internal void PushTag(string tagName)
		{
			// flush any accumulated literals
			this.Flush();

			JbstControl control;
			if (tagName != null &&
				tagName.StartsWith(JbstCustomControl.JbstPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				control = new JbstCustomControl(tagName);
			}
			else
			{
				control = this.next;
				if (control == null)
				{
					control = new JbstControl(tagName);
				}
				else
				{
					control.TagName = tagName;
					this.next = null;
				}
			}

			if (this.current == null)
			{
				this.current = this.document;
			}

			this.current.ChildControls.Add(control);
			this.current = control;
		}

		internal void PopTag(string tagName)
		{
			if (tagName == null)
			{
				tagName = String.Empty;
			}

			// flush any accumulated literals
			this.Flush();

			if (this.next != null)
			{
				//throw new InvalidOperationException("Push/Pop mismatch? (Next is not null)");
				this.next = null;
			}

			if (this.current == null)
			{
				throw new InvalidOperationException("Push/Pop mismatch? (Current is null)");
			}

			if (!String.IsNullOrEmpty(tagName) &&
				!tagName.Equals(this.current.RawName, StringComparison.InvariantCultureIgnoreCase))
			{
				//throw new InvalidOperationException("Push/Pop mismatch? (tag names do not match)");
				return;
			}

			if (this.current == null)
			{
				throw new InvalidOperationException("Push/Pop mismatch? (Current.Parent is null)");
			}

			this.current = this.current.Parent;

			if (this.current == this.document &&
				this.DocumentReady != null)
			{
				this.DocumentReady(this, EventArgs.Empty);
			}
		}

		internal void StoreAttribute(string name, string value)
		{
			if (this.next == null)
			{
				this.next = new JbstControl();
			}

			this.SetAttribute(this.next, name, value);
		}

		internal void AddAttribute(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetAttribute(this.current, name, value);
		}

		internal void SetAttribute(JbstControl target, string name, string value)
		{
			// flush any accumulated literals
			this.Flush();

			value = HtmlDistiller.DecodeHtmlEntities(value);
			if ("style".Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				this.SetStyle(target, null, value);
			}
			else
			{
				target.Attributes[name] = value;
			}
		}

		internal void AddAttribute(string name, IJbstControl value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.current.Attributes[name] = value;
		}

		internal void StoreStyle(string name, string value)
		{
			if (this.next == null)
			{
				this.next = new JbstControl();
			}

			this.SetStyle(this.next, name, value);
		}

		internal void AddStyle(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetStyle(this.current, name, value);
		}

		internal void SetStyle(JbstControl target, string name, string value)
		{
			// flush any accumulated literals
			this.Flush();

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

		public void Render(TextWriter writer)
		{
			this.Render(writer, false);
		}

		public void Render(TextWriter writer, bool prettyPrint)
		{
			this.Flush();

			this.ProcessDirectives();

			if (this.isTemplate)
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
					writer.Write(" = ");
				}
				else
				{
					writer.Write("=");
				}

				if (prettyPrint)
				{
					writer.WriteLine("new JsonML.BST(");
				}
				else
				{
					writer.Write("new JsonML.BST(");
				}
			}

			JsonFx.Json.JsonWriter jw = new JsonFx.Json.JsonWriter(writer);
			jw.PrettyPrint = prettyPrint;

			IJbstControl control = null;
			foreach (IJbstControl child in this.document.ChildControls)
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

			// render root node of content (null is OK)
			jw.Write(control);

			if (this.isTemplate)
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
						this.Declarations.ToString());

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
						new JSMin().Run(declarations, writer);
					}
				}
			}
		}

		#endregion Render Methods

		#region IHtmlWriter Members

		void IHtmlWriter.WriteLiteral(string literal)
		{
			this.AppendChild(literal);
		}

		void IHtmlWriter.WriteLiteral(char literal)
		{
			this.AppendChild(literal.ToString());
		}

		bool IHtmlWriter.WriteTag(HtmlTag tag, IHtmlFilter filter)
		{
			switch (tag.TagType)
			{
				case HtmlTagType.FullTag:
				case HtmlTagType.BeginTag:
				{
					this.PushTag(tag.RawName);

					if (tag.HasAttributes)
					{
						foreach (string key in tag.Attributes.Keys)
						{
							string value = tag.Attributes[key];

							// TODO: replace this re-parsing
							if (value.Length >= 4 &&
								value.StartsWith("<%") &&
								value.EndsWith("%>"))
							{
								switch (value[2])
								{
									case '-':
									{
										// TODO: fix as this might be produce false positives
										break;
									}
									case '@':
									{
										this.Directives.Append(value);
										break;
									}
									case '!':
									{
										this.Declarations.Append(value.Substring(3, value.Length-5));
										break;
									}
									case '=':
									{
										JbstExpressionBlock code = new JbstExpressionBlock(
											value.Substring(3, value.Length-5),
											this.path);
										this.AddAttribute(key, code);
										break;
									}
									case '$':
									{
										JbstExtensionBlock code = new JbstExtensionBlock(
											value.Substring(3, value.Length-5),
											this.path);
										this.AddAttribute(key, code);
										break;
									}
									default:
									{
										JbstStatementBlock code = new JbstStatementBlock(
											value.Substring(2, value.Length-4),
											this.path);
										this.AddAttribute(key, code);
										break;
									}
								}
							}
							else
							{
								this.AddAttribute(key, value);
							}
						}
					}

					if (tag.HasStyles)
					{
						foreach (string key in tag.Styles.Keys)
						{
							this.AddStyle(key, tag.Styles[key]);
						}
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
				case HtmlTagType.Unparsed:
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
						case "%=":
						{
							// expressions are emitted directly into JBST
							JbstExpressionBlock code = new JbstExpressionBlock(
								tag.Content,
								this.path);
							this.AppendChild(code);
							break;
						}
						case "%$":
						{
							// expressions are emitted directly into JBST
							JbstExtensionBlock code = new JbstExtensionBlock(
								tag.Content,
								this.path);
							this.AppendChild(code);
							break;
						}
						case "%":
						{
							// statements are emitted directly into JBST
							JbstStatementBlock code = new JbstStatementBlock(
								tag.Content,
								this.path);
							this.AppendChild(code);
							break;
						}
						case "%--":
						case "!--":
						{
							// Comments are emitted directly into JBST
							JbstCommentBlock code = new JbstCommentBlock(
								tag.Content,
								this.path);
							this.AppendChild(code);
							break;
						}
						default:
						{
							JbstLiteral literal = new JbstLiteral(tag.ToString());
							this.AppendChild(literal);
							break;
						}
					}
					break;
				}
				default:
				{
					break;
				}
			}

			return true;
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
						JbstCompiler.EnsureIdent(attribs["name"]) :
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
				case "include":
				case "taglib":
				default:
				{
					// not implemented
					break;
				}
			}
		}

		/// <summary>
		/// Validates that the value is a valid JavaScript identifier.
		/// </summary>
		/// <param name="ident"></param>
		/// <returns></returns>
		public static string EnsureIdent(string ident)
		{
			// TODO: scrub name for valid identifier
			return ident;
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
				string ident = JbstCompiler.EnsureIdent(import);

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
	}
}
