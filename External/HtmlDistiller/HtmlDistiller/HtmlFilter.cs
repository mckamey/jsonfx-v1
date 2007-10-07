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

namespace BuildTools.HtmlDistiller
{
	/// <summary>
	/// Defines a set of HTML tag/attribute/style filters
	/// </summary>
	public interface IHtmlFilter
	{
		#region Methods

		/// <summary>
		/// Filters tags
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <returns></returns>
		bool FilterTag(string tag);

		/// <summary>
		/// Filters attributes
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">attribute name</param>
		/// <param name="value">attribute value</param>
		/// <returns></returns>
		bool FilterAttribute(string tag, string attribute, ref string value);

		/// <summary>
		/// Filters styles
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">style name</param>
		/// <param name="value">style value</param>
		/// <returns></returns>
		bool FilterStyle(string tag, string style, ref string value);

		#endregion Methods
	}

	/// <summary>
	/// HtmlFilter which strips all tags
	/// </summary>
	public class StripHtmlFilter : IHtmlFilter
	{
		#region IHtmlFilter Members

		/// <summary>
		/// Strips all tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public virtual bool FilterTag(string tag)
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
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
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
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows only simple tags/attributes
	/// </summary>
	public class StrictHtmlFilter : IHtmlFilter
	{
		#region IHtmlFilter Members

		/// <summary>
		/// Allows a restrictive set of simple tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public virtual bool FilterTag(string tag)
		{
			tag = tag.ToLowerInvariant();
			return
				(tag == "a") ||
				(tag == "b") ||
				(tag == "blink") ||
				(tag == "blockquote") ||
                (tag == "br") ||
                (tag == "center") ||
                (tag == "dd") ||
                (tag == "dl") ||
                (tag == "dt") ||
                (tag == "em") ||
                (tag == "font") ||
                (tag == "h1") ||
                (tag == "h2") ||
                (tag == "h3") ||
                (tag == "h4") ||
                (tag == "h5") ||
                (tag == "h6") ||
                (tag == "hr") ||
                (tag == "i") ||
                (tag == "img") ||
                (tag == "ol") ||
                (tag == "li") ||
                (tag == "p") ||
                (tag == "span") ||
                (tag == "strong") ||
                (tag == "table") ||
                (tag == "tbody") ||
                (tag == "td") ||
                (tag == "thead") ||
                (tag == "th") ||
                (tag == "tr") ||
                (tag == "tt") ||
				(tag == "u") ||
                (tag == "ul");
		}

		/// <summary>
		/// Allows a restrictive set of simple attributes
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
		{
			attribute = attribute.ToLowerInvariant();
			tag = tag.ToLowerInvariant();

			switch (tag)
			{
				case "a":
				{
					return
						(attribute == "href") ||
						(attribute == "target");
				}
				case "img":
				{
					return
						(attribute == "alt") ||
						(attribute == "height") ||
						(attribute == "src") ||
						(attribute == "title") ||
						(attribute == "width");
				}
				default:
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Strips all styles
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows only safe tags/attributes/styles
	/// </summary>
	public class SafeHtmlFilter : IHtmlFilter
	{
		#region IHtmlFilter Members

		/// <summary>
		/// Allows a permissive set of safe tags
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// </remarks>
		public virtual bool FilterTag(string tag)
		{
			// whitelist of safe tags
			switch (tag.ToLowerInvariant())
			{
				case HtmlTag.CommentTagName:
				case "a":
				case "b":
				case "blink":
				case "blockquote":
				case "br":
				case "center":
				case "dd":
				case "dl":
				case "dt":
				case "em":
				case "font":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "hr":
				case "i":
				case "img":
				case "ol":
				case "li":
				case "p":
				case "span":
				case "strong":
				case "table":
				case "tbody":
				case "td":
				case "thead":
				case "th":
				case "tr":
				case "tt":
				case "u":
				case "ul":
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
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
		{
			attribute = attribute.ToLowerInvariant();
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
						case "target":
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
						case "color":
						case "noshade":
						case "size":
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
						case "height":
						case "src":
						case "title":
						case "width":
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
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
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
	public class UnsafeHtmlFilter : IHtmlFilter
	{
		#region IHtmlFilter Members

		/// <summary>
		/// Allows all tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public bool FilterTag(string tag)
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
		public bool FilterAttribute(string tag, string attribute, ref string value)
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
		public bool FilterStyle(string tag, string style, ref string value)
		{
			return true;
		}

		#endregion IHtmlFilter Members
	}
}
