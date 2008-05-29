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
using System.Web;
using System.Text.RegularExpressions;

using BuildTools.HtmlDistiller;
using BuildTools.HtmlDistiller.Filters;

namespace JsonFx.JsonML.Builder
{
	internal class JsonControlBuilder : IDisposable, IHtmlFilter
	{
		#region Constants

		private static readonly Regex RegexWhitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);

		#endregion Constants

		#region Fields

		TextWriter writer;
		JsonControlCollection controls = new JsonControlCollection(null);
		JsonControl current = null;
		JsonControl next = null;
		bool allowLiteralsInRoot = false;
		private bool dirty = false;
		private bool disposed = false;

		private bool parsingHtml = false;
		private readonly HtmlDistiller parser = new HtmlDistiller();

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writer"></param>
		public JsonControlBuilder(TextWriter writer)
		{
			this.writer = writer;

			this.parser.EncodeNonAscii = false;
			this.parser.BalanceTags = false;
			this.parser.NormalizeWhitespace = false;
			this.parser.HtmlFilter = this;
			this.parser.BeginIncrementalParsing();
		}

		#endregion Init

		#region Properties

		public JsonControlCollection Controls
		{
			get { return this.controls; }
		}

		public bool AllowLiteralsInRoot
		{
			get { return this.allowLiteralsInRoot; }
			set { this.allowLiteralsInRoot = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Adds literal text to the output
		/// </summary>
		/// <param name="literal"></param>
		public void AddLiteral(string literal)
		{
			try
			{
				this.parsingHtml = true;

				this.parser.Parse(literal);
			}
			finally
			{
				this.parsingHtml = false;
			}
		}

		/// <summary>
		/// Writes any buffered text
		/// </summary>
		public void Flush()
		{
			if (this.parsingHtml)
			{
				return;
			}

			try
			{
				this.parsingHtml = true;

				// flush remaining
				this.parser.EndIncrementalParsing();

				// reset for additional
				this.parser.BeginIncrementalParsing();
			}
			finally
			{
				this.parsingHtml = false;
			}
		}

		protected void OutputLiteral(string text)
		{
			if (String.IsNullOrEmpty(text))
			{
				return;
			}

			if (this.current == null && !this.AllowLiteralsInRoot)
			{
				return;
			}

			// this allows HTML entities to be encoded as unicode
			text = HttpUtility.HtmlDecode(text);

			JsonControlCollection childControls = (this.current == null) ?
				this.controls : this.current.ChildControls;

			JsonLiteral jsonLiteral = childControls.Last as JsonLiteral;
			if (jsonLiteral == null)
			{
				jsonLiteral = new JsonLiteral(text);
				childControls.Add(jsonLiteral);
			}
			else
			{
				jsonLiteral.Text += text;
			}

			// normalize whitespace
			jsonLiteral.Text = RegexWhitespace.Replace(jsonLiteral.Text, " ");
		}

		public void PushTag(string tagName)
		{
			// flush any accumulated literals
			this.Flush();

			JsonControl control = this.next;
			if (control == null)
			{
				control = new JsonControl(tagName);
			}
			else
			{
				control.TagName = tagName;
				this.next = null;
			}

			if (this.current == null)
			{
				this.controls.Add(control);
			}
			else
			{
				this.current.ChildControls.Add(control);
			}

			this.current = control;
		}

		public void PopTag()
		{
			// flush any accumulated literals
			this.Flush();

			if (this.next != null)
			{
				throw new InvalidOperationException("Pop mismatch? (Next is null)");
			}

			if (this.current == null)
			{
				throw new InvalidOperationException("Pop mismatch? (Current is null)");
			}

			this.current = this.current.Parent;

			if (this.current == null)
			{
				this.RenderControls();
			}
		}

		public void StoreAttribute(string name, string value)
		{
			if (this.next == null)
			{
				this.next = new JsonControl();
			}

			this.SetAttribute(this.next, name, value);
		}

		public void AddAttribute(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetAttribute(this.current, name, value);
		}

		public void SetAttribute(JsonControl target, string name, string value)
		{
			// flush any accumulated literals
			this.Flush();

			value = HttpUtility.HtmlDecode(value);
			if ("style".Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				this.SetStyle(target, null, value);
			}
			else
			{
				target.Attributes[name] = value;
			}
		}

		public void StoreStyle(string name, string value)
		{
			if (this.next == null)
			{
				this.next = new JsonControl();
			}

			this.SetStyle(this.next, name, value);
		}

		public void AddStyle(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetStyle(this.current, name, value);
		}

		public void SetStyle(JsonControl target, string name, string value)
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

		#endregion Methods

		#region Methods

		private void RenderControls()
		{
			if (this.dirty)
			{
				// hacky but seems to work:
				// if this builder is dirty it means it has already written out a control graph
				// adding another back to back will not be valid JSON
				// for some reason Crockford's JSON parse method appears to treat multiple graphs
				// as if they are in an array if delimited by commas.
				this.writer.Write(",");
			}
			using (JsonFx.Json.JsonWriter jw = new JsonFx.Json.JsonWriter(this.writer))
			{
				if (this.Controls.Count > 1)
				{
					jw.Write(this.Controls);
				}
				else if (this.Controls.Count > 0)
				{
					jw.Write(this.Controls[0]);
				}
			}
			this.Controls.Clear();
			this.dirty = true;
		}

		#endregion Methods

		#region IHtmlFilter Members

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterTag(HtmlTag tag)
		{
			switch (tag.TagType)
			{
				case HtmlTagType.FullTag:
				case HtmlTagType.BeginTag:
				{
					this.PushTag(tag.TagName);

					if (tag.HasAttributes)
					{
						foreach (string key in tag.Attributes.Keys)
						{
							this.AddAttribute(key, tag.Attributes[key]);
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
				case HtmlTagType.Comment:
				default:
				{
					break;
				}
			}

			// suppress writing of HtmlDistiller output
			return false;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterAttribute(string tag, string attribute, ref string value)
		{
			// suppress writing of HtmlDistiller output
			return false;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="style"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterStyle(string tag, string style, ref string value)
		{
			// suppress writing of HtmlDistiller output
			return false;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterLiteral(string source, int start, int end, out string replacement)
		{
			this.OutputLiteral(source.Substring(start, end-start));

			// suppress writing of HtmlDistiller output
			replacement = null;
			return true;
		}

		#endregion IHtmlFilter Members

		#region IDisposable Members

		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				if (this.writer != null)
				{
					using (this.writer)
					{
						while (this.current != null)
						{
							this.PopTag();
						}

						// flush any accumulated literals
						this.Flush();

						this.writer.Flush();
						this.writer.Close();
						this.writer = null;
					}
				}
			}
		}

		#endregion IDisposable Members
	}
}
