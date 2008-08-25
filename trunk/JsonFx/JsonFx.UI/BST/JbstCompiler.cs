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
using System.Text.RegularExpressions;

using BuildTools;
using BuildTools.HtmlDistiller;
using BuildTools.HtmlDistiller.Filters;
using BuildTools.HtmlDistiller.Writers;

namespace JsonFx.JsonML.BST
{
	/// <summary>
	/// JsonML+BST Template Compiler
	/// </summary>
	internal class JbstCompiler : IHtmlWriter
	{
		#region Constants

		private static readonly Regex RegexWhitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);

		#endregion Constants

		#region Fields

		private List<ParseException> errors = new List<ParseException>();
		private JbstControl document = new JbstControl(String.Empty);
		private JbstControl current = null;
		private JbstControl next = null;

		private bool isParsing = false;
		private readonly HtmlDistiller parser = new HtmlDistiller();

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writer"></param>
		internal JbstCompiler()
		{
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

		#endregion Properties

		#region Builder Methods

		/// <summary>
		/// Parses markup.
		/// </summary>
		/// <param name="literal"></param>
		/// <returns>a list of any exeptions which occurred during the parsing.</returns>
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

			// normalize whitespace
			literal.Text = RegexWhitespace.Replace(literal.Text, JbstLiteral.Whitespace);
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
			int prefix = String.IsNullOrEmpty(tagName) ? -1 : tagName.IndexOf(':');
			if (prefix > 0 &&
				tagName.Substring(0, prefix).Equals("jbst", StringComparison.InvariantCultureIgnoreCase))
			{
				control = new JbstCustomControl(tagName.Substring(prefix+1));
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

		internal void PopTag()
		{
			// flush any accumulated literals
			this.Flush();

			if (this.next != null)
			{
				throw new InvalidOperationException("Push/Pop mismatch? (Next is not null)");
			}

			if (this.current == null)
			{
				throw new InvalidOperationException("Push/Pop mismatch? (Current is null)");
			}

			this.current = this.current.Parent;

			if (this.current == null)
			{
				throw new InvalidOperationException("Push/Pop mismatch? (Current is null)");
			}

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

		#endregion Builder Methods

		#region Render Methods

		public void Render(TextWriter writer)
		{
			this.Render(writer, false);
		}

		public void Render(TextWriter writer, bool prettyPrint)
		{
			JsonFx.Json.JsonWriter jw = new JsonFx.Json.JsonWriter(writer);
			jw.PrettyPrint = prettyPrint;

			IJbstControl control = null;
			foreach (IJbstControl child in this.document.ChildControls)
			{
				// tally non-whitespace controls
				JbstLiteral lit = child as JbstLiteral;
				if (lit != null && lit.Text == JbstLiteral.Whitespace)
				{
					continue;
				}

				if (control != null)
				{
					// found 2 or more in root
					// render with document wrapper
					jw.Write(this.document);
					return;
				}

				control = child;
			}

			// only render single node found (null is OK)
			jw.Write(control);
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
							if (value.StartsWith("<%") && value.EndsWith("%>"))
							{
								this.AddAttribute(key, new JbstCodeBlock(value.Trim('<', '%', '>')));
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
						this.PopTag();
					}
					break;
				}
				case HtmlTagType.EndTag:
				{
					this.PopTag();
					break;
				}
				case HtmlTagType.Unparsed:
				{
					if (tag.TagName == "%")
					{
						JbstCodeBlock code = new JbstCodeBlock(tag.Content);
						this.AppendChild(code);
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
	}
}
