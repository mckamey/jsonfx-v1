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
		#region ParseState

		/// <summary>
		/// Exception which halts parsing preserving last sync point
		/// </summary>
		private class UnexpectedEofException : System.IO.EndOfStreamException
		{
		}

		#endregion ParseState

		#region Constants

		private const char NullChar = '\0';
		private const char CRChar = '\r';
		private const char LFChar = '\n';
		private const char OpenTagChar = '<';
		private const char CloseTagChar = '>';
		private const char EndTagChar = '/';
		private const char AttrDelimChar = '=';
		private const char SingleQuoteChar = '\'';
		private const char DoubleQuoteChar = '\"';
		private const char StylePropChar = ':';
		private const char StyleDelimChar = ';';
		private const char AlphaStartChar = 'a';
		private const char AlphaEndChar = 'z';
		private const char NumStartChar = '0';
		private const char NumEndChar = '9';
		private const char AsciiHighChar = (char)0x7F;

		private const string StyleAttrib = "style";
		private const string Ellipsis = "&hellip;";
		private const string LessThan = "&lt;";

		#endregion Constants

		#region Fields

		private readonly object SyncLock = new object();

		private string source;
		private IHtmlFilter htmlFilter;
		private int maxLength = 0;
		private bool normalizeWhitespace = false;
		private bool balanceTags = true;
		private bool encodeNonAscii = true;
		private bool incrementalParsing = false;

		private StringBuilder output;
		private int index;		// current char in source
		private int start;		// last written char in source
		private int textSize;	// length of plain text processed
		private int syncPoint;	// last sync point (for incremental parsing)
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
		/// Gets and sets if tags should be auto-balance
		/// </summary>
		public bool BalanceTags
		{
			get { return this.balanceTags; }
			set
			{
				lock (this.SyncLock)
				{
					this.balanceTags = value;
				}
			}
		}

		/// <summary>
		/// Gets and sets if non-ASCII chars should be encoded as HTML entities
		/// </summary>
		public bool EncodeNonAscii
		{
			get { return this.encodeNonAscii; }
			set
			{
				lock (this.SyncLock)
				{
					this.encodeNonAscii = value;
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
					if (this.incrementalParsing)
					{
						if (this.syncPoint >= 0)
						{
							// prepend remaining unparsed source
							value = this.source.Substring(this.syncPoint) + value;
						}
					}
					else
					{
						this.output = null;
					}

					this.source = (value == null) ?
						String.Empty :
						value;
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
					return NullChar;
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
		/// Starts parsing to be performed incrementally
		/// </summary>
		/// <remarks>
		/// There is a performance hit for parsing in chunks.
		/// </remarks>
		public void BeginIncrementalParsing()
		{
			lock (this.SyncLock)
			{
				this.Reset();
				this.incrementalParsing = true;
			}
		}

		/// <summary>
		/// Stops incremental parsing and completes tag balancing, etc.
		/// </summary>
		/// <returns>the output text</returns>
		public string EndIncrementalParsing()
		{
			this.Source = null;
			lock (this.SyncLock)
			{
				this.incrementalParsing = false;
			}
			return this.Parse(false);
		}

		/// <summary>
		/// Parses the provided source using the current settings.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>the output text</returns>
		public string Parse(string source)
		{
			this.Source = source;
			return this.Parse();
		}

		/// <summary>
		/// Parses the source using the current settings.
		/// </summary>
		/// <returns>the output text</returns>
		public string Parse()
		{
			return this.Parse(!this.incrementalParsing);
		}

		/// <summary>
		/// Parses the source using the current settings.
		/// </summary>
		/// <param name="fullReset">clears incremental state as well</param>
		private string Parse(bool fullReset)
		{
			try
			{
				this.Reset(fullReset);

				while (!this.IsEOF)
				{
					// store syncPoint
					this.syncPoint = this.index;

					char ch = this.Current;
					if (ch == OpenTagChar)
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
									if (!this.balanceTags || !this.openTags.Contains(tag.CreateOpenTag()))
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
							this.output.Append(LessThan);

							// remove from stream
							this.EmptyBuffer(1);

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
							if (ch == CRChar)
							{
								#region normalize line endings (CR/CRLF -> LF)

								// write out all before CR
								this.WriteBuffer();

								if (this.Peek(1) != LFChar)
								{
									// just CR so replace CR with LF
									this.output.Append(LFChar);

									// count toward total text length
									this.textSize++;
								}

								// skip CR
								this.EmptyBuffer(1);

								#endregion normalize line endings (CR/CRLF -> LF)
							}
							else if (ch == LFChar)
							{
								#region limit line endings (no more than 2 LF)

								// write out all before LF
								this.WriteBuffer();

								char prev1 = this.PrevChar(1);
								char prev2 = this.PrevChar(2);
								if ((prev1 == LFChar || prev1 == NullChar) &&
										(prev2 == LFChar || prev2 == NullChar))
								{
									// skip 3rd+ LFs
									while (true)
									{
										this.Advance();
										ch = this.Current;
										if (ch != LFChar &&
												ch != CRChar)
										{
											break;
										}
									}
									this.EmptyBuffer();
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
								if (Char.IsWhiteSpace(prev1) ||
										prev1 == NullChar)
								{
									// write out all before extra whitespace
									this.WriteBuffer();

									// eat extra whitespace
									this.EmptyBuffer(1);
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
					else if (this.encodeNonAscii &&
							ch > AsciiHighChar)
					{
						#region encode non-ASCII chars

						// write out all before non-ASCII char
						this.WriteBuffer();

						// encode the non-ASCII char
						string entity = this.EncodeHtmlEntity(ch);
						this.output.Append(entity);

						// remove char from stream
						this.EmptyBuffer(1);

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

				// reset syncPoint
				this.syncPoint = -1;

				#region close any open tags

				if (!this.incrementalParsing)
				{
					while (this.openTags.Count > 0)
					{
						// write out any unclosed tags
						HtmlTag tag = this.openTags.Pop();
						this.RenderCloseTag(tag);
					}
				}

				#endregion close any open tags
			}
			catch (UnexpectedEofException)
			{
				// nothing needs to be done
				// the source is preserved via last sync point
			}

			if (this.MaxLength > 0 && this.textSize >= this.MaxLength)
			{
				// source was cut off so add ellipsis
				this.output.Append(Ellipsis);
			}

			return this.output.ToString();
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
			if ((ch < AlphaStartChar || ch > AlphaEndChar) &&
				(ch != EndTagChar) && (ch != NullChar))
			{
				// not a tag, treat as LessThan char
				return null;
			}

			// remove tag open char
			this.EmptyBuffer(1);

			while (!this.IsEOF)
			{
				// tag name chars
				this.Advance();

				ch = Char.ToLowerInvariant(this.Current);
				if ((ch < AlphaStartChar || ch > AlphaEndChar) &&
					(ch < NumStartChar || ch > NumEndChar))
				{
					break;
				}
			}

			tag = new HtmlTag(this.FlushBuffer(), this.htmlFilter);

			this.ParseSyncPoint();

			this.ParseAttributes(tag);

			if (this.Current == CloseTagChar)
			{
				// remove GreaterThan char from source
				this.EmptyBuffer(1);
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

			// consume LessThan
			this.EmptyBuffer(1);

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

			this.ParseSyncPoint();

			string contents = this.FlushBuffer();
			if (!this.IsEOF)
			{
				this.EmptyBuffer(endDelim.Length);
			}

			HtmlTag comment = new HtmlTag(commentName, this.htmlFilter);
			if (!String.IsNullOrEmpty(contents))
			{
				comment.Attributes[HtmlTag.Key_Contents] = contents;
			}
			comment.Attributes[HtmlTag.Key_EndDelim] = endDelim.Substring(0, endDelim.Length-1);

			return comment;
		}

		private void ParseAttributes(HtmlTag tag)
		{
			char ch = this.Current;

			while (!this.IsEOF &&
				ch != CloseTagChar &&
				ch != OpenTagChar)
			{
				string name = this.ParseAttributeName();

				this.ParseSyncPoint();

				string value = String.Empty;

				ch = this.Current;
				if (ch != CloseTagChar &&
					ch != OpenTagChar)
				{
					// Get the value(if any)
					value = this.ParseAttributeValue();
				}

				this.ParseSyncPoint();

				if (!String.IsNullOrEmpty(name))
				{
					if (StyleAttrib.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					{
						this.ParseStyles(tag, value);
					}
					else
					{
						tag.Attributes[name] = value;
					}
				}

				ch = this.Current;
			}
		}

		private string ParseAttributeName()
		{
			this.SkipWhiteSpace();

			char ch = this.Current;
			if (ch == EndTagChar)
			{
				this.EmptyBuffer(1);
				return String.Empty;
			}

			while (!this.IsEOF)
			{
				ch = this.Current;
				if ((ch == AttrDelimChar) ||
					(ch == CloseTagChar) ||
					(ch == OpenTagChar) ||
					Char.IsWhiteSpace(ch))
				{
					break;
				}

				// add to attribute name
				if (ch != OpenTagChar)
				{
					this.Advance();
				}
			}

			return this.FlushBuffer();
		}

		private string ParseAttributeValue()
		{
			this.SkipWhiteSpace();

			char ch = this.Current;
			if (ch != AttrDelimChar)
			{
				return String.Empty;
			}

			this.EmptyBuffer(1);
			this.SkipWhiteSpace();

			char quot = this.Current;
			bool isQuoted =
				(quot == SingleQuoteChar) ||
				(quot == DoubleQuoteChar);

			if (isQuoted)
			{
				// consume open quote
				this.EmptyBuffer(1);

				while (!this.IsEOF)
				{
					ch = this.Current;

					if ((ch == quot) ||
						(ch == CloseTagChar) ||
						(ch == OpenTagChar))
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

					if ((ch == CloseTagChar) ||
						(ch == OpenTagChar) ||
						(Char.IsWhiteSpace(ch)))
					{
						break;
					}

					// add to attribute value
					this.Advance();
				}
			}

			string value = this.FlushBuffer();
			if (isQuoted && this.Current == quot)
			{
				// consume close quote
				this.EmptyBuffer(1);
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

			while (i < style.Length)
			{
				name = value = String.Empty;

				// skip whitespace
				while (i < style.Length &&
					Char.IsWhiteSpace(style, i))
				{
					start = ++i;
				}

				// style name
				while (i < style.Length &&
					style[i] != StylePropChar)
				{
					i++;
				}

				// copy style name
				if (start < style.Length)
				{
					name = style.Substring(start, i-start);
				}

				// inc first
				start = ++i;

				// skip whitespace
				while (i < style.Length &&
					Char.IsWhiteSpace(style, i))
				{
					// inc first
					start = ++i;
				}

				// style value
				while (i < style.Length &&
					style[i] != StyleDelimChar)
				{
					// TODO: handle HTML entities (e.g. "&quot;")
					i++;
				}

				if (!String.IsNullOrEmpty(name) &&
					start < style.Length)
				{
					// copy style value
					value = style.Substring(start, i-start);

					// apply style to tag
					tag.Styles[name.ToLowerInvariant()] = value;
				}

				// inc first
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

		/// <summary>
		/// Resets state used for parsing
		/// </summary>
		public void Reset()
		{
			lock (this.SyncLock)
			{
				this.Reset(true);
			}
		}

		/// <summary>
		/// Reset state used for parsing
		/// </summary>
		/// <param name="fullReset">clears incremental state as well</param>
		/// <remarks>Does not SyncLock, call inside lock</remarks>
		private void Reset(bool fullReset)
		{
			if (this.htmlFilter == null)
			{
				this.htmlFilter = new SafeHtmlFilter();
			}
			this.index = this.start = 0;

			if (fullReset || this.output == null)
			{
				// in incremental parse mode, continue as if same document
				this.textSize = 0;
				this.syncPoint = -1;
				this.openTags = new Stack<HtmlTag>(10);
				this.taxonomy = HtmlTaxonomy.None;
				this.output = new StringBuilder(this.source.Length);
			}
		}

		/// <summary>
		/// Causes parsing to end preserving partial source
		/// </summary>
		private void ParseSyncPoint()
		{
			if (this.incrementalParsing && this.IsEOF)
			{
				throw new UnexpectedEofException();
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
				this.EmptyBuffer(1);
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
					return NullChar;
				}
				return this.source[pos];
			}

			if (this.output.Length < peek)
			{
				return NullChar;
			}

			return this.output[this.output.Length-peek];
		}

		private char Peek(int peek)
		{
			if ((this.index + peek) >= this.source.Length)
			{
				return NullChar;
			}

			return this.source[this.index + peek];
		}

		private void WriteBuffer()
		{
			// do not callback on empty strings
			if (this.start < this.index)
			{
				string replacement;
				if (this.htmlFilter.FilterLiteral(this.source, this.start, this.index, out replacement))
				{
					// filter has altered the literal
					this.output.Append(replacement);
				}
				else
				{
					// use the original
					this.output.Append(this.source, this.start, this.index-this.start);
				}
			}
			this.start = this.index;
		}

		private void EmptyBuffer()
		{
			this.EmptyBuffer(0);
		}

		private void EmptyBuffer(int skipCount)
		{
			this.index += skipCount;
			this.start = this.index;
		}

		private string FlushBuffer()
		{
			return this.FlushBuffer(0);
		}

		private string FlushBuffer(int skipCount)
		{
			this.index += skipCount;
			string buffer = (this.start < this.index) ?
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
		/// Encodes special characters into safe representation
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		private string EncodeHtmlEntity(char ch)
		{
			return String.Format("&#x{0:X2};", (int)ch);
		}

		#endregion Methods
	}
}
