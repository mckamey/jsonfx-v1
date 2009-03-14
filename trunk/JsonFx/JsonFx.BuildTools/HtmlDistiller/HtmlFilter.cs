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
using System.Text;
using System.Text.RegularExpressions;

namespace JsonFx.BuildTools.HtmlDistiller.Filters
{
	/// <summary>
	/// Defines a set of HTML tag/attribute/style filters
	/// </summary>
	public interface IHtmlFilter
	{
		#region Methods

		/// <summary>
		/// Filters tags, optionally allowing altering of tag
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <returns>if tag should be rendered</returns>
		bool FilterTag(HtmlTag tag);

		/// <summary>
		/// Filters attributes, optionally allowing altering of attribute value
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">attribute name</param>
		/// <param name="value">attribute value</param>
		/// <returns>if attribute should be rendered</returns>
		bool FilterAttribute(string tag, string attribute, ref string value);

		/// <summary>
		/// Filters styles, optionally allowing altering of style value
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">style name</param>
		/// <param name="value">style value</param>
		/// <returns>if style should be rendered</returns>
		bool FilterStyle(string tag, string style, ref string value);

		/// <summary>
		/// Filters literals, optionally allowing replacement of literal value
		/// </summary>
		/// <param name="source">original string</param>
		/// <param name="start">starting index inclusive</param>
		/// <param name="end">ending index exclusive</param>
		/// <param name="replacement">a replacement string</param>
		/// <returns>true if <paramref name="replacement"/> should be used to replace literal</returns>
		/// <remarks>
		/// This uses the original source string, start and end rather than passing a substring
		/// in order to not generate a strings for every literal.  The internals of HtmlDistiller
		/// do not produce extra strings for literals so for efficiency sake, care should be taken
		/// so that filters do not produce excessive extra strings either.
		/// </remarks>
		bool FilterLiteral(string source, int start, int end, out string replacement);

		#endregion Methods
	}

	/// <summary>
	/// Defines a literal filter which optionally breaks words at a certain length
	/// </summary>
	public abstract class WordBreakFilter : IHtmlFilter
	{
		#region Constants

		private const string WordBreak = "<wbr />&shy;";
		private readonly int MaxWordLength;

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public WordBreakFilter()
			: this(0)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		public WordBreakFilter(int maxWordLength)
		{
			this.MaxWordLength = maxWordLength;
		}

		#endregion Init

		#region IHtmlFilter Members

		public abstract bool FilterTag(HtmlTag tag);

		public abstract bool FilterAttribute(string tag, string attribute, ref string value);

		public abstract bool FilterStyle(string tag, string style, ref string value);

		/// <summary>
		/// Optionally allows breaking of long words
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns>true if literal was changed</returns>
		public virtual bool FilterLiteral(string source, int start, int end, out string replacement)
		{
			replacement = null;

			if (this.MaxWordLength <= 0)
			{
				return false;
			}

			int lastOutput = start,
				sinceSpace = 0;

			for (int i=start; i<end; i++)
			{
				if (Char.IsWhiteSpace(source[i]))
				{
					sinceSpace = 0;
					continue;
				}

				if (sinceSpace >= this.MaxWordLength)
				{
					// append all before break
					replacement += source.Substring(lastOutput, i-lastOutput);
					replacement += SafeHtmlFilter.WordBreak;
					lastOutput = i;
					sinceSpace = 0;
				}
				else
				{
					sinceSpace++;
				}

				if (source[i] == HtmlDistiller.EntityStartChar)
				{
					char entityChar;
					int entityLength = HtmlDistiller.DecodeHtmlEntity(source, i, out entityChar);
					if (entityLength > 1)
					{
						// compensating for the length of this entity
						// so that doesn't break within the entity symbol
						i += entityLength-1;
					}
				}
			}

			if (replacement != null)
			{
				if (lastOutput < end)
				{
					// append remaining string
					replacement += source.Substring(lastOutput, end-lastOutput);
				}

				// a replacement was generated
				return true;
			}

			// don't replace since didn't need to
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// Defines a filter which optionally auto-links URLs in literals
	/// </summary>
	public abstract class HyperlinkFilter : WordBreakFilter
	{
		#region Constants

		private const string Pattern_Url = @"\b[a-z]+\://[a-z0-9][a-z0-9\-\.]*(?:\:[0-9]+)?(?:/[\w/\.\,\;\?\'\+\(\)&%\$#\=~\-]*)?";
		private static readonly Regex Regex_Url = new Regex(Pattern_Url, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ECMAScript);

		private const string AutoLinkStart = "<a href=\"";
		private const string AutoLinkMiddle = "\">";
		private const string AutoLinkEnd = "</a>";

		#endregion Constants

		#region Fields

		private bool autoLink;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public HyperlinkFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		public HyperlinkFilter(int maxWordLength)
			: base(maxWordLength)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="autoLink"></param>
		public HyperlinkFilter(bool autoLink)
			: this(0, autoLink)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public HyperlinkFilter(int maxWordLength, bool autoLink)
			: base(maxWordLength)
		{
			this.autoLink = autoLink;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets if should auto-wrap URLs in hyperlinks
		/// </summary>
		public virtual bool AutoLink
		{
			get { return this.autoLink; }
			set { this.autoLink = value; }
		}

		#endregion Properties

		#region IHtmlFilter Members

		/// <summary>
		/// Optionally finds any URLs and wraps them with a hyperlink
		/// </summary>
		/// <param name="source">original string</param>
		/// <param name="start">starting index inclusive</param>
		/// <param name="end">ending index exclusive</param>
		/// <param name="replacement">a replacement string</param>
		/// <returns>true if <paramref name="replacement"/> should be used to replace literal</returns>
		public override bool FilterLiteral(string source, int start, int end, out string replacement)
		{
			if (!this.AutoLink)
			{
				return base.FilterLiteral(source, start, end, out replacement);
			}

			StringBuilder builder = null;

			int index,			// the start index of the match
				length,			// the length of the match
				last = start;	// the last index written out

			while (this.DetectUrl(source, last, end, out index, out length))
			{
				if (builder == null)
				{
					// populate initial string builder with leading unmatched substring
					if (base.FilterLiteral(source, last, index, out replacement))
					{
						// use the word broken replacement text
						builder = new StringBuilder(replacement, 2*end-start);
					}
					else
					{
						builder = new StringBuilder(source, last, index-last, 2*end-start);
					}
				}
				else
				{
					// append next unmatched substring
					if (base.FilterLiteral(source, last, index, out replacement))
					{
						// use the word broken replacement text
						builder.Append(replacement);
					}
					else
					{
						builder.Append(source, last, index-last);
					}
				}

				// wrap the match in a hyperlink
				builder.Append(AutoLinkStart);
				if (this.FilterUrl(source, index, index+length, out replacement))
				{
					// allow additional filtering on the safety of the URL
					builder.Append(replacement);
				}
				else
				{
					builder.Append(source, index, length);
				}
				builder.Append(AutoLinkMiddle);
				if (base.FilterLiteral(source, index, index+length, out replacement))
				{
					builder.Append(replacement);
				}
				else
				{
					builder.Append(source, index, length);
				}
				builder.Append(AutoLinkEnd);

				// continue searching from the end of the match
				last = index + length;
			}

			if (builder == null)
			{
				// no matches were found
				replacement = null;
				return false;
			}

			// append trailing unmatched substring
			if (base.FilterLiteral(source, index, index+length, out replacement))
			{
				// use the word broken replacement text
				builder.Append(replacement);
			}
			else
			{
				builder.Append(source, last, end-last);
			}

			// render composite output
			replacement = builder.ToString();
			return true;
		}

		#endregion IHtmlFilter Members

		#region Utility Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement">the value to replace with</param>
		/// <returns>true if a replacement value was specified</returns>
		protected virtual bool FilterUrl(string source, int start, int end, out string replacement)
		{
			if (String.IsNullOrEmpty(source) || (end <= start))
			{
				replacement = null;
				return false;
			}

			// TODO: see http://ha.ckers.org/xss.html for other attacks
			string protocol = source.Substring(start, source.IndexOf(':', start, end-start)-start+1);
			switch (protocol.TrimStart().ToLowerInvariant())
			{
				case "http:":
				case "https:":
				case "mailto:":
				case "":
				{
					// replace with empty string
					replacement = null;
					return false;
				}
				default:
				{
					// do not need to replace
					replacement = String.Empty;
					return true;
				}
			}
		}

		/// <summary>
		/// Finds the location of URLs within the literal string.
		/// </summary>
		/// <param name="source">original source string</param>
		/// <param name="start">starting index to check inclusive</param>
		/// <param name="end">ending index to check exclusive</param>
		/// <param name="index">the index where match starts</param>
		/// <param name="length">the length of the match</param>
		/// <returns>true if any found</returns>
		private bool DetectUrl(string source, int start, int end, out int index, out int length)
		{
			if (String.IsNullOrEmpty(source) || (end <= start))
			{
				index = -1;
				length = 0;
				return false;
			}

			Match match = Regex_Url.Match(source, start, end - start);
			index = match.Index;
			length = match.Length;

			return match.Success;
		}

		#endregion Utility Methods
	}

	/// <summary>
	/// HtmlFilter which strips all tags from the input
	/// </summary>
	public class StripHtmlFilter : HyperlinkFilter
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public StripHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		public StripHtmlFilter(int maxWordLength)
			: base(maxWordLength)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public StripHtmlFilter(bool autoLink)
			: base(autoLink)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public StripHtmlFilter(int maxWordLength, bool autoLink)
			: base(maxWordLength, autoLink)
		{
		}

		#endregion Init

		#region IHtmlFilter Members

		/// <summary>
		/// Strips all tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public override bool FilterTag(HtmlTag tag)
		{
			return false;
		}

		/// <summary>
		/// Strips all attributes
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool FilterAttribute(string tag, string attribute, ref string value)
		{
			return false;
		}

		/// <summary>
		/// Strips all styles
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool FilterStyle(string tag, string style, ref string value)
		{
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows only simple tags/attributes
	/// </summary>
	public class StrictHtmlFilter : HyperlinkFilter
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public StrictHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		public StrictHtmlFilter(int maxWordLength)
			: base(maxWordLength)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public StrictHtmlFilter(bool autoLink)
			: base(autoLink)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public StrictHtmlFilter(int maxWordLength, bool autoLink)
			: base(maxWordLength, autoLink)
		{
		}

		#endregion Init

		#region IHtmlFilter Members

		/// <summary>
		/// Allows a restrictive set of simple tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public override bool FilterTag(HtmlTag tag)
		{
			// whitelist of safe tags
			switch (tag.TagName)
			{
				case "a":
				case "b":
				case "blockquote":
				case "br":
				case "em":
				case "i":
				case "img":
				case "li":
				case "ol":
				case "strong":
				case "u":
				case "ul":
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Allows a restrictive set of simple attributes
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool FilterAttribute(string tag, string attribute, ref string value)
		{
			attribute = attribute.ToLowerInvariant();

			// whitelist of safe attributes
			switch (tag.ToLowerInvariant())
			{
				case "a":
				{
					switch (attribute)
					{
						case "href":
						{
							string replacement;
							if (value != null &&
								this.FilterUrl(value, 0, value.Length, out replacement))
							{
								value = replacement;
							}
							return !String.IsNullOrEmpty(value);
						}
						case "target":
						{
							return true;
						}
					}
					return false;
				}
				case "img":
				{
					switch (attribute)
					{
						case "alt":
						case "title":
						{
							return true;
						}
						case "src":
						{
							string replacement;
							if (value != null &&
								this.FilterUrl(value, 0, value.Length, out replacement))
							{
								value = replacement;
							}
							return !String.IsNullOrEmpty(value);
						}
					}
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Strips all styles
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool FilterStyle(string tag, string style, ref string value)
		{
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows only safe tags/attributes/styles
	/// </summary>
	public class SafeHtmlFilter : HyperlinkFilter
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public SafeHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		public SafeHtmlFilter(int maxWordLength)
			: base(maxWordLength)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public SafeHtmlFilter(bool autoLink)
			: base(autoLink)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public SafeHtmlFilter(int maxWordLength, bool autoLink)
			: base(maxWordLength, autoLink)
		{
		}

		#endregion Init

		#region IHtmlFilter Members

		/// <summary>
		/// Allows a permissive set of safe tags
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// http://webdesign.about.com/od/htmltags/a/bl_index.htm
		/// </remarks>
		public override bool FilterTag(HtmlTag tag)
		{
			// whitelist of safe tags
			switch (tag.TagName)
			{
				//case "!--":// comments can hold IE conditional logic
				case "a":
				case "abbr":
				case "acronym":
				case "address":
				case "area":
				case "b":
				case "bdo":
				case "bgsound":
				case "big":
				case "blink":
				case "blockquote":
				case "br":
				case "caption":
				case "center":
				case "cite":
				case "code":
				case "col":
				case "colgroup":
				case "dd":
				case "del":
				case "dfn":
				case "dir":
				case "div":
				case "dl":
				case "dt":
				case "em":
				case "fieldset":
				case "font":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "hr":
				case "i":
				case "iframe":
				case "img":
				case "ins":
				case "isindex":
				case "kbd":
				case "label":
				case "legend":
				case "li":
				case "map":
				case "marquee":
				case "menu":
				//case "meta": // use for redirects, is this safe?
				case "nobr":
				case "ol":
				case "p":
				case "pre":
				case "q":
				case "s":
				case "samp":
				case "small":
				case "sound":
				case "span":
				case "strike":
				case "strong":
				case "sub":
				case "sup":
				case "table":
				case "tbody":
				case "td":
				case "tfoot":
				case "th":
				case "thead":
				case "tr":
				case "tt":
				case "u":
				case "ul":
				case "var":
				case "wbr":
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Allows a permissive set of safe attributes
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">attribute name</param>
		/// <param name="value">attribute value</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/attributes.html
		/// </remarks>
		public override bool FilterAttribute(string tag, string attribute, ref string value)
		{
			attribute = attribute.ToLowerInvariant();
			if (attribute == "id" || attribute.StartsWith("on"))
			{
				// deny for all tags
				return false;
			}
			if (attribute == "class")
			{
				// allow for all tags
				return true;
			}

			// whitelist of safe attributes
			switch (tag.ToLowerInvariant())
			{
				case "a":
				{
					switch (attribute)
					{
						case "href":
						{
							string replacement;
							if (value != null &&
								this.FilterUrl(value, 0, value.Length, out replacement))
							{
								value = replacement;
							}
							return !String.IsNullOrEmpty(value);
						}
						case "target":
						{
							return true;
						}
					}
					return false;
				}
				case "bgsound":
				case "sound":
				{
					switch (attribute)
					{
						case "balance":
						case "loop":
						case "volume":
						{
							return true;
						}
						case "src":
						{
							string replacement;
							if (value != null &&
								this.FilterUrl(value, 0, value.Length, out replacement))
							{
								value = replacement;
							}
							return !String.IsNullOrEmpty(value);
						}
					}
					return false;
				}
				case "div":
				{
					switch (attribute)
					{
						case "align":
						{
							return true;
						}
					}
					return false;
				}
				case "font":
				{
					switch (attribute)
					{
						case "color":
						case "face":
						case "size":
						{
							return true;
						}
					}
					return false;
				}
				case "hr":
				{
					switch (attribute)
					{
						case "align":
						case "color":
						case "noshade":
						case "size":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
				case "iframe":
				{
					switch (attribute)
					{
						case "align":
						case "allowtransparency":
						case "frameborder":
						case "height":
						case "longdesc":
						case "marginheight":
						case "marginwidth":
						case "scrolling":
						case "width":
						case "z-index":
						{
							return true;
						}
						case "src":
						{
							string replacement;
							if (value != null &&
								this.FilterUrl(value, 0, value.Length, out replacement))
							{
								value = replacement;
							}
							return !String.IsNullOrEmpty(value);
						}
					}
					return false;
				}
				case "img":
				{
					switch (attribute)
					{
						case "alt":
						case "border":
						case "height":
						case "title":
						case "width":
						{
							return true;
						}
						case "lowsrc":
						case "dynsrc":
						case "src":
						{
							string replacement;
							if (value != null &&
								this.FilterUrl(value, 0, value.Length, out replacement))
							{
								value = replacement;
							}
							return !String.IsNullOrEmpty(value);
						}
					}
					return false;
				}
				case "marquee":
				{
					switch (attribute)
					{
						case "align":
						case "behavior":
						case "bgcolor":
						case "direction":
						case "height":
						case "loop":
						case "scrollamount":
						case "scrolldelay":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
				case "p":
				{
					switch (attribute)
					{
						case "align":
						{
							return true;
						}
					}
					return false;
				}
				case "ol":
				case "ul":
				{
					switch (attribute)
					{
						case "type":
						{
							return true;
						}
					}
					return false;
				}
				case "table":
				{
					switch (attribute)
					{
						case "bgcolor":
						case "border":
						case "bordercolor":
						case "cellpadding":
						case "cellspacing":
						case "height":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
				case "td":
				case "th":
				{
					switch (attribute)
					{
						case "align":
						case "colspan":
						case "rowspan":
						case "bgcolor":
						case "bordercolor":
						case "height":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Allows a permissive set of safe attributes
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="style">style name</param>
		/// <param name="value">style value</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/CSS21/propidx.html
		/// </remarks>
		public override bool FilterStyle(string tag, string style, ref string value)
		{
			if (value != null)
			{
				if (value.IndexOf("expression", StringComparison.OrdinalIgnoreCase) >= 0 ||
					value.IndexOf("javascript:", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					// IE CSS expressions are JavaScript
					// JavaScript URLs are JavaScript
					return false;
				}
			}

			// blacklist of unsafe styles
			switch (style.ToLowerInvariant())
			{
				case "display":
				case "position":
				case "z-index":
				{
					return false;
				}
			}
			return true;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows all tags/attributes/styles
	/// </summary>
	public class UnsafeHtmlFilter : HyperlinkFilter
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public UnsafeHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		public UnsafeHtmlFilter(int maxWordLength)
			: base(maxWordLength)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public UnsafeHtmlFilter(bool autoLink)
			: base(autoLink)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxWordLength"></param>
		/// <param name="autoLink"></param>
		public UnsafeHtmlFilter(int maxWordLength, bool autoLink)
			: base(maxWordLength, autoLink)
		{
		}

		#endregion Init

		#region IHtmlFilter Members

		/// <summary>
		/// Allows all tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public override bool FilterTag(HtmlTag tag)
		{
			return true;
		}

		/// <summary>
		/// Allows all attributes
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool FilterAttribute(string tag, string attribute, ref string value)
		{
			return true;
		}

		/// <summary>
		/// Allows all styles
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool FilterStyle(string tag, string style, ref string value)
		{
			return true;
		}

		#endregion IHtmlFilter Members
	}
}
