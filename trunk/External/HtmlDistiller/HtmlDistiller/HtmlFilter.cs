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
#endregion BuildTools License

using System;

namespace BuildTools.HtmlDistiller.Filters
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
		/// <returns>true if <paramref name="replace"/> should be used to replace literal</returns>
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
		/// Ctor.
		/// </summary>
		public WordBreakFilter()
			: this(0)
		{
		}

		/// <summary>
		/// Ctor.
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
		/// <returns></returns>
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

		#region Utility Methods

		/// <summary>
		/// Filters URLs based upon protocol
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected virtual bool FilterUrl(ref string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return false;
			}

			// TODO: see http://ha.ckers.org/xss.html for other attacks
			value = value.Trim();
			string protocol = value.Substring(0, value.IndexOf(':')+1);
			switch (protocol.ToLowerInvariant())
			{
				case "http:":
				case "https:":
				case "mailto:":
				case "":
				{
					return true;
				}
				default:
				{
					return false;
				}
			}
		}

		#endregion Utility Methods
	}

	/// <summary>
	/// HtmlFilter which strips all tags
	/// </summary>
	public class StripHtmlFilter : WordBreakFilter
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public StripHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="maxWordLength"></param>
		public StripHtmlFilter(int maxWordLength)
			: base(maxWordLength)
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
	public class StrictHtmlFilter : WordBreakFilter
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public StrictHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="maxWordLength"></param>
		public StrictHtmlFilter(int maxWordLength)
			: base(maxWordLength)
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
							return this.FilterUrl(ref value);
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
							return this.FilterUrl(ref value);
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
	public class SafeHtmlFilter : WordBreakFilter
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public SafeHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="maxWordLength"></param>
		public SafeHtmlFilter(int maxWordLength)
			: base(maxWordLength)
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
							return this.FilterUrl(ref value);
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
							return this.FilterUrl(ref value);
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
							return this.FilterUrl(ref value);
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
							return this.FilterUrl(ref value);
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
	public class UnsafeHtmlFilter : WordBreakFilter
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public UnsafeHtmlFilter()
			: base(0)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="maxWordLength"></param>
		public UnsafeHtmlFilter(int maxWordLength)
			: base(maxWordLength)
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
