#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

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
#endregion BuildTools License

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using BuildTools.HtmlDistiller.Filters;

namespace BuildTools.HtmlDistiller
{
	/// <summary>
	/// Parses HTML, repairing and scrubbing against various whitelist filters.
	/// </summary>
	/// <remarks>
	/// Note: this class is thread-safe (all external changes are locked first)
	/// </remarks>
	public class HtmlDistiller
	{
		#region Fields

		private readonly object SyncLock = new object();

		private string source;
		private IHtmlFilter htmlFilter;
		private int maxLength = 0;
		private bool normalizeWhitespace = true;

		private StringBuilder output;
		private int index;
		private int start;
		private int textSize;
		private Stack<HtmlTag> openTags;
		private HtmlTaxonomy taxonomy;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public HtmlDistiller()
			: this(null, 0, null)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="text">the text to parse</param>
		public HtmlDistiller(string text, int maxLength)
			: this(text, maxLength, null)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="text">the text to parse</param>
		/// <param name="filter"></param>
		public HtmlDistiller(string text, int maxLength, IHtmlFilter filter)
		{
			this.source = (text == null) ? String.Empty : text;
			this.maxLength = maxLength;
			this.htmlFilter = filter;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the IHtmlFilter used in parsing
		/// </summary>
		public IHtmlFilter HtmlFilter
		{
			get { return this.htmlFilter; }
			set
			{
				lock (this.SyncLock)
				{
					this.htmlFilter = value;
				}
			}
		}

		/// <summary>
		/// Gets and sets the maximum number of text chars (tags don't count)
		/// </summary>
		public int MaxLength
		{
			get { return this.maxLength; }
			set
			{
				lock (this.SyncLock)
				{
					this.maxLength = value;
				}
			}
		}

		/// <summary>
		/// Gets and sets if whitespace should be normalized
		/// </summary>
		public bool NormalizeWhitespace
		{
			get { return this.normalizeWhitespace; }
			set
			{
				lock (this.SyncLock)
				{
					this.normalizeWhitespace = value;
				}
			}
		}

		/// <summary>
		/// Gets and sets the source text
		/// </summary>
		public string Source
		{
			get { return this.source; }
			set
			{
				lock (this.SyncLock)
				{
					this.output = null;
					this.source = (value == null) ? String.Empty : value;
				}
			}
		}

		/// <summary>
		/// Gets the output text
		/// </summary>
		public string Output
		{
			get
			{
				if (this.output == null)
				{
					this.Parse();
				}
				return this.output.ToString();
			}
		}

		/// <summary>
		/// Gets a value indicating the taxonomy of tags rendered
		/// </summary>
		public HtmlTaxonomy Taxonomy
		{
			get
			{
				if (this.output == null)
				{
					this.Parse();
				}
				return this.taxonomy;
			}
		}

		#endregion Properties

		#region Parse Properties

		/// <summary>
		/// Gets the current character.
		/// </summary>
		private char Current
		{
			get
			{
				if (this.IsEOF)
				{
					return '\0';
				}
				return this.source[this.index];
			}
		}

		/// <summary>
		/// Gets if at the end of source text.
		/// </summary>
		private bool IsEOF
		{
			get
			{
				// allow the text to arbitrarily end short
				if (this.MaxLength > 0 && this.textSize >= this.MaxLength)
				{
					return true;
				}

				return (this.index >= this.source.Length);
			}
		}

		#endregion Parse Properties

		#region Parse Methods

		/// <summary>
		/// Parses the source using the current settings.
		/// </summary>
		/// <returns>the output text</returns>
		public string Parse()
		{
			lock (this.SyncLock)
			{
				this.Reset();

				while (!this.IsEOF)
				{
					char ch = this.Current;
					if (ch == '<')
					{
						#region found potential tag

						// write out all before CR
						this.WriteBuffer();

						HtmlTag tag = this.ParseTag();
						if (tag != null)
						{
							switch (tag.TagType)
							{
								case HtmlTagType.Comment:
								case HtmlTagType.FullTag:
								{
									this.RenderTag(tag);
									break;
								}
								case HtmlTagType.BeginTag:
								{
									// keep copy for pairing
									this.openTags.Push(tag);
									this.RenderTag(tag);
									break;
								}
								case HtmlTagType.EndTag:
								{
									if (this.openTags.Count == 0)
									{
										// no open tags so no need for EndTag
										break;
									}

									// check for matching pair
									HtmlTag openTag = this.openTags.Pop();
									if (tag.TagName == openTag.TagName)
									{
										// found match
										this.RenderTag(tag);
										break;
									}

									#region repair mismatched tags

									// if isn't in stack then it doesn't help to attempt to repair
									bool repairable = this.openTags.Contains(tag.CreateOpenTag());

									if (!repairable)
									{
										// put the tag back on
										this.openTags.Push(openTag);

										// ignore end tag
										break;
									}
									else
									{
										// try to repair mismatch
										Stack<HtmlTag> mismatched = new Stack<HtmlTag>(this.openTags.Count);

										do
										{
											// close mismatched tags
											this.RenderCloseTag(openTag);

											// store for re-opening
											mismatched.Push(openTag);
											if (this.openTags.Count == 0)
											{
												// no match found
												openTag = null;
												break;
											}

											// get next
											openTag = this.openTags.Pop();
										} while (tag.TagName != openTag.TagName);

										if (openTag != null)
										{
											// found matching tag
											this.RenderTag(tag);
										}

										// reopen mismatched tags
										while (mismatched.Count > 0)
										{
											openTag = mismatched.Pop();
											this.openTags.Push(openTag);
											this.RenderTag(openTag);
										}
									}
									break;

									#endregion repair mismatched tags
								}
								default:
								{
									break;
								}
							}
						}
						else
						{
							#region encode LessThan char

							// encode LessThan char
							this.output.Append("&lt;");

							// remove from stream
							this.FlushBuffer(1);

							// count toward total text length
							this.textSize++;

							#endregion encode LessThan char
						}

						#endregion found potential tag
					}
					else if (this.normalizeWhitespace && Char.IsWhiteSpace(ch))
					{
						#region normalize whitespace

						while (Char.IsWhiteSpace(ch))
						{
							if (ch == '\r')
							{
								#region normalize line endings (CR/CRLF -> LF)

								// write out all before CR
								this.WriteBuffer();

								if (this.Peek(1) != '\n')
								{
									// just CR so replace CR with LF
									this.output.Append('\n');

									// count toward total text length
									this.textSize++;
								}

								// skip CR
								this.FlushBuffer(1);

								#endregion normalize line endings (CR/CRLF -> LF)
							}
							else if (ch == '\n')
							{
								#region limit line endings (no more than 2 LF)

								// write out all before LF
								this.WriteBuffer();

								char prev1 = this.PrevChar(1);
								char prev2 = this.PrevChar(2);
								if ((prev1 == '\n' || prev1 == '\0') &&
									(prev2 == '\n' || prev2 == '\0'))
								{
									// skip 3rd+ LFs
									while (true)
									{
										this.Advance();
										ch = this.Current;
										if (ch != '\n' && ch != '\r')
										{
											break;
										}
									}
									this.FlushBuffer();
								}
								else
								{
									// keep going, will copy out as larger buffer
									this.Advance();

									// count towards text chars
									this.IncTextCount();
								}

								#endregion limit line endings (no more than 2 LF)
							}
							else
							{
								#region normalize spaces and tabs

								char prev1 = this.PrevChar(1);
								if (Char.IsWhiteSpace(prev1) || prev1 == '\0')
								{
									// write out all before extra whitespace
									this.WriteBuffer();

									// eat extra whitespace
									this.FlushBuffer(1);
								}
								else
								{
									// keep going, will copy out as larger buffer
									this.Advance();

									// count towards text chars
									this.IncTextCount();
								}

								#endregion normalize spaces and tabs
							}

							ch = this.Current;
						}

						#endregion normalize whitespace
					}
					else if (ch > 0x7F)
					{
						#region encode non-ASCII chars

						// write out all before non-ASCII char
						this.WriteBuffer();

						// encode the non-ASCII char
						string entity = this.EncodeHtmlEntity(ch);
						this.output.Append(entity);

						// remove char from stream
						this.FlushBuffer(1);

						// count toward total text length
						this.textSize++;

						#endregion encode non-ASCII chars
					}
					else
					{
						#region all other chars

						// keep going, will copy out as larger buffer
						this.Advance();

						// count towards text chars
						this.IncTextCount();

						#endregion all other chars
					}
				}

				this.WriteBuffer();

				#region close any open tags

				while (this.openTags.Count > 0)
				{
					// write out any unclosed tags
					HtmlTag tag = this.openTags.Pop();
					this.RenderCloseTag(tag);
				}

				#endregion close any open tags

				if (this.MaxLength > 0 && this.textSize >= this.MaxLength)
				{
					// source was cut off so add ellipsis
					this.output.Append("&hellip;");
				}

				return this.output.ToString();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>null if no tag was found (e.g. just LessThan char)</returns>
		private HtmlTag ParseTag()
		{
			HtmlTag tag = this.ParseComment("<!--", "-->");
			if (tag != null)
			{
				// standard HTML/XML/SGML comment found
				return tag;
			}
			tag = this.ParseComment("<![CDATA[", "]]>");
			if (tag != null)
			{
				// CDATA section found
				return tag;
			}
			tag = this.ParseComment("<!", ">");
			if (tag != null)
			{
				// SGML processing instruction (usually DOCTYPE)
				return tag;
			}
			tag = this.ParseComment("<?", "?>");
			if (tag != null)
			{
				// XML/SGML processing instruction (usually XML declaration)
				return tag;
			}

			char ch = Char.ToLowerInvariant(this.Peek(1));
			if ((ch < 'a' || ch > 'z') && (ch != '/'))
			{
				// not a tag, treat as LessThan char
				return null;
			}

			// remove tag open char
			this.FlushBuffer(1);

			while (!this.IsEOF)
			{
				// first tag char
				this.Advance();

				ch = Char.ToLowerInvariant(this.Current);
				if ((ch < 'a' || ch > 'z') && (ch < '0' || ch > '9'))
				{
					break;
				}
			}

			string buffer = this.FlushBuffer();
			tag = new HtmlTag(buffer, this.htmlFilter);

			// for now just include raw attributes
			while (!this.IsEOF && this.Current != '>' && this.Current != '<')
			{
				string name = this.ParseAttributeName();
				string value = String.Empty;

				if (this.Current != '>' && this.Current != '<')
				{
					// Get the value(if any)
					value = this.ParseAttributeValue();
				}

				if (!String.IsNullOrEmpty(name))
				{
					if ("style".Equals(name, StringComparison.InvariantCultureIgnoreCase))
					{
						this.ParseStyles(tag, value);
					}
					else
					{
						tag.Attributes[name] = value;
					}
				}
			}

			if (!this.IsEOF && this.Current != '<')
			{
				// remove GreaterThan char from source
				this.FlushBuffer(1);
			}

			return tag;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>null if no comment found</returns>
		/// <param name="startDelim"></param>
		/// <param name="endDelim"></param>
		/// <remarks>
		/// This supports standard comments, DocType declarations, and CDATA sections.
		/// </remarks>
		private HtmlTag ParseComment(string startDelim, string endDelim)
		{
			int i=0;
			for (i=0; i<startDelim.Length; i++)
			{
				if (this.Peek(i) != startDelim[i])
				{
					return null;
				}
			}

			// flush LessThan
			this.FlushBuffer(1);

			string commentName = this.FlushBuffer(startDelim.Length-1);

			i = 0;
			while (!this.IsEOF)
			{
				if (this.Peek(i) == endDelim[i])
				{
					i++;
					if (i == endDelim.Length)
					{
						break;
					}
				}
				else
				{
					i = 0;

					// add to comment contents
					this.Advance();
				}
			}

			string contents = this.FlushBuffer();
			if (!this.IsEOF)
			{
				this.FlushBuffer(endDelim.Length);
			}

			HtmlTag comment = new HtmlTag(commentName, this.htmlFilter);
			if (!String.IsNullOrEmpty(contents))
			{
				comment.Attributes[HtmlTag.Key_Contents] = contents;
			}
			comment.Attributes[HtmlTag.Key_EndDelim] = endDelim.Substring(0, endDelim.Length-1);

			return comment;
		}

		private string ParseAttributeName()
		{
			this.SkipWhiteSpace();

			if (this.Current == '/')
			{
				this.FlushBuffer(1);
				return String.Empty;
			}

			while (!this.IsEOF)
			{
				char ch = this.Current;
				if (Char.IsWhiteSpace(ch) ||
					ch == '=' ||
					ch == '>' ||
					ch == '<')
				{
					break;
				}

				// add to attribute name
				if (ch != '<')
				{
					this.Advance();
				}
			}

			return this.FlushBuffer();
		}

		private string ParseAttributeValue()
		{
			this.SkipWhiteSpace();

			if (this.Current != '=')
			{
				return String.Empty;
			}

			this.FlushBuffer(1);
			this.SkipWhiteSpace();

			char ch, quot = this.Current;
			bool isQuoted = quot == '\'' || quot == '\"';
			if (isQuoted)
			{
				this.FlushBuffer(1);
				while (!this.IsEOF)
				{
					ch = this.Current;
					if (ch == quot || ch == '>' || ch == '<')
					{
						break;
					}

					// add to attribute value
					this.Advance();
				}
			}
			else
			{
				while (!this.IsEOF)
				{
					ch = this.Current;
					if (ch == '>' || ch == '<' || Char.IsWhiteSpace(ch))
					{
						break;
					}

					// add to attribute value
					this.Advance();
				}
			}

			string value = this.FlushBuffer();
			if (isQuoted && !this.IsEOF)
			{
				this.FlushBuffer(1);
			}
			return value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="style"></param>
		private void ParseStyles(HtmlTag tag, string style)
		{
			string name, value;
			int start=0, i=0;

			while (i<style.Length)
			{
				name = value = String.Empty;

				// skip whitespace
				while (i < style.Length && Char.IsWhiteSpace(style, i))
				{
					start = ++i;
				}

				// style name
				while (i < style.Length && style[i] != ':')
				{
					i++;
				}

				// copy style name
				if (start < style.Length)
				{
					name = style.Substring(start, i-start);
				}

				start = ++i;

				// skip whitespace
				while (i < style.Length && Char.IsWhiteSpace(style, i))
				{
					start = ++i;
				}

				// style value
				while (i < style.Length && style[i] != ';')
				{
					// TODO: handle HTML entities (e.g. "&quot;")
					i++;
				}

				if (!String.IsNullOrEmpty(name) && start < style.Length)
				{
					// copy style value
					value = style.Substring(start, i-start);

					// apply style to tag
					tag.Styles[name.ToLowerInvariant()] = value;
				}

				start = ++i;
			}
		}

		private void RenderTag(HtmlTag tag)
		{
			if (tag.WriteTag(this.output))
			{
				this.taxonomy |= tag.Taxonomy;
			}
		}

		private void RenderCloseTag(HtmlTag tag)
		{
			tag = tag.CreateCloseTag();

			if (tag != null)
			{
				this.RenderTag(tag);
			}
		}

		#endregion Parse Methods

		#region Methods

		/// <summary>
		/// Remove whitespace from the input source
		/// </summary>
		private void SkipWhiteSpace()
		{
			while (!this.IsEOF && Char.IsWhiteSpace(this.Current))
			{
				this.FlushBuffer(1);
			}
		}

		/// <summary>
		/// Gets a previous char whether buffered or written out
		/// </summary>
		/// <returns></returns>
		private char PrevChar(int peek)
		{
			if (this.index-this.start >= peek)
			{
				// use buffered
				int pos = this.index-peek;
				if (pos < 0 || pos >= this.source.Length)
				{
					return '\0';
				}
				return this.source[pos];
			}

			if (this.output.Length < peek)
			{
				return '\0';
			}

			return this.output[this.output.Length-peek];
		}

		private char Peek(int peek)
		{
			if ((this.index + peek) >= this.source.Length)
			{
				return '\0';
			}

			return this.source[this.index + peek];
		}

		private void WriteBuffer()
		{
			if (this.start < this.index)
			{
				this.output.Append(this.source, this.start, this.index-this.start);
			}
			this.start = this.index;
		}

		private string FlushBuffer()
		{
			return this.FlushBuffer(0);
		}

		private string FlushBuffer(int skipCount)
		{
			this.index += skipCount;
			string buffer = (this.index > this.start) ?
				this.source.Substring(this.start, this.index-this.start):
				String.Empty;
			this.start = this.index;
			return buffer;
		}

		private void Advance()
		{
			// move index ahead
			this.index++;
		}

		/// <summary>
		/// Keeps running tally of the plain text length
		/// </summary>
		private void IncTextCount()
		{
			// count toward total text length
			this.textSize++;
		}

		/// <summary>
		/// Reset all fields used for parsing
		/// </summary>
		private void Reset()
		{
			if (this.htmlFilter == null)
			{
				this.htmlFilter = new SafeHtmlFilter();
			}
			this.index = this.start = this.textSize = 0;
			this.output = new StringBuilder(this.source.Length);
			this.openTags = new Stack<HtmlTag>(10);
			this.taxonomy = HtmlTaxonomy.None;
		}

		private string EncodeHtmlEntity(char ch)
		{
			return String.Format("&#x{0:X2};", (int)ch);
		}

		#endregion Methods
	}
}
