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
	/// Compiler of JsonML+BST nodes.
	/// </summary>
	internal class JbstCompiler :
		IHtmlWriter,
		IDisposable
	{
		#region Constants

		private static readonly Regex RegexWhitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);

		#endregion Constants

		#region Fields

		TextWriter writer;
		JbstControlCollection controls = new JbstControlCollection(null);
		JbstControl current = null;
		JbstControl next = null;
		bool allowLiteralsInRoot = false;
		private bool dirty = false;
		private bool disposed = false;

		private bool isParsing = false;
		private readonly HtmlDistiller parser = new HtmlDistiller();

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writer"></param>
		public JbstCompiler(TextWriter writer)
		{
			this.writer = writer;

			this.parser.EncodeNonAscii = false;
			this.parser.BalanceTags = true;
			this.parser.NormalizeWhitespace = false;
			this.parser.HtmlFilter = new UnsafeHtmlFilter();
			this.parser.HtmlWriter = this;
			this.parser.BeginIncrementalParsing();
		}

		#endregion Init

		#region Properties

		public JbstControlCollection Controls
		{
			get { return this.controls; }
		}

		public bool AllowLiteralsInRoot
		{
			get { return this.allowLiteralsInRoot; }
			set { this.allowLiteralsInRoot = value; }
		}

		#endregion Properties

		#region Builder Methods

		/// <summary>
		/// Parses markup.
		/// </summary>
		/// <param name="literal"></param>
		/// <returns>a list of any exeptions which occurred during the parsing.</returns>
		public List<ParseException> Parse(string source)
		{
			List<ParseException> exceptions = new List<ParseException>();
			try
			{
				this.isParsing = true;

				this.parser.Parse(source);
			}
			catch (Exception ex)
			{
				exceptions.Add(new ParseError(ex.Message, null, 1, 1, ex));
			}
			finally
			{
				this.isParsing = false;
			}
			return exceptions;
		}

		/// <summary>
		/// Writes any buffered text
		/// </summary>
		public void Flush()
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

		public void AppendChild(string text)
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
			text = HtmlDistiller.DecodeHtmlEntities(text);

			JbstControlCollection childControls = (this.current == null) ?
				this.controls : this.current.ChildControls;

			JbstLiteral jsonLiteral = childControls.Last as JbstLiteral;
			if (jsonLiteral == null)
			{
				jsonLiteral = new JbstLiteral(text);
				childControls.Add(jsonLiteral);
			}
			else
			{
				jsonLiteral.Text += text;
			}

			// normalize whitespace
			jsonLiteral.Text = RegexWhitespace.Replace(jsonLiteral.Text, " ");
		}

		internal void AppendChild(IJbstControl child)
		{
			if (child == null)
			{
				return;
			}

			if (this.current == null && !this.AllowLiteralsInRoot)
			{
				return;
			}

			JbstControlCollection childControls = (this.current == null) ?
				this.controls : this.current.ChildControls;

			childControls.Add(child);
		}

		public void PushTag(string tagName)
		{
			// flush any accumulated literals
			this.Flush();

			JbstControl control = this.next;
			if (control == null)
			{
				control = new JbstControl(tagName);
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
				this.next = new JbstControl();
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

		public void SetAttribute(JbstControl target, string name, string value)
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

		public void StoreStyle(string name, string value)
		{
			if (this.next == null)
			{
				this.next = new JbstControl();
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

		public void SetStyle(JbstControl target, string name, string value)
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

		///// <summary>Converts a string that has been HTML-encoded for HTTP transmission into a decoded string.</summary>
		///// <returns>A decoded string.</returns>
		///// <param name="s">The string to decode. </param>
		//public static string HtmlDecode(string s)
		//{
		//    if (s == null)
		//    {
		//        return null;
		//    }
		//    if (s.IndexOf('&') < 0)
		//    {
		//        return s;
		//    }
		//    StringBuilder sb = new StringBuilder();
		//    StringWriter output = new StringWriter(sb);
		//    HtmlDecode(s, output);
		//    return sb.ToString();
		//}

		//private static char[] s_entityEndingChars = new char[] { ';', '&' };

		///// <summary>Converts a string that has been HTML-encoded into a decoded string, and sends the decoded string to a <see cref="T:System.IO.TextWriter"></see> output stream.</summary>
		///// <param name="s">The string to decode. </param>
		///// <param name="output">A <see cref="T:System.IO.TextWriter"></see> stream of output. </param>
		//private static void HtmlDecode(string s, TextWriter output)
		//{
		//    if (s != null)
		//    {
		//        if (s.IndexOf('&') < 0)
		//        {
		//            output.Write(s);
		//        }
		//        else
		//        {
		//            int length = s.Length;
		//            for (int i = 0; i < length; i++)
		//            {
		//                char ch = s[i];
		//                if (ch == '&')
		//                {
		//                    int num3 = s.IndexOfAny(s_entityEndingChars, i + 1);
		//                    if ((num3 > 0) && (s[num3] == ';'))
		//                    {
		//                        string entity = s.Substring(i + 1, (num3 - i) - 1);
		//                        if ((entity.Length > 1) && (entity[0] == '#'))
		//                        {
		//                            try
		//                            {
		//                                if ((entity[1] == 'x') || (entity[1] == 'X'))
		//                                {
		//                                    ch = (char)int.Parse(entity.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
		//                                }
		//                                else
		//                                {
		//                                    ch = (char)int.Parse(entity.Substring(1));
		//                                }
		//                                i = num3;
		//                            }
		//                            catch (FormatException)
		//                            {
		//                                i++;
		//                            }
		//                            catch (ArgumentException)
		//                            {
		//                                i++;
		//                            }
		//                        }
		//                        else
		//                        {
		//                            i = num3;
		//                            char ch2 = HtmlEntities.Lookup(entity);
		//                            if (ch2 != '\0')
		//                            {
		//                                ch = ch2;
		//                            }
		//                            else
		//                            {
		//                                output.Write('&');
		//                                output.Write(entity);
		//                                output.Write(';');
		//                                goto Label_0103;
		//                            }
		//                        }
		//                    }
		//                }
		//                output.Write(ch);
		//            Label_0103:
		//                ;
		//            }
		//        }
		//    }
		//}

		#endregion Builder Methods

		#region Render Methods

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
					this.PushTag(tag.TagName);

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
