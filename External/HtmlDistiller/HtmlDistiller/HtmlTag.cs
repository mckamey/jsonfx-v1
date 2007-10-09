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
using System.Text;
using System.Collections.Generic;

using BuildTools.HtmlDistiller.Filters;

namespace BuildTools.HtmlDistiller
{
	#region Enums

	/// <summary>
	/// Defines the type of tag
	/// </summary>
	public enum HtmlTagType
	{
		/// <summary>
		/// Not defined
		/// </summary>
		Unknown,

		/// <summary>
		/// Comment tag
		/// </summary>
		Comment,

		/// <summary>
		/// Opening tag
		/// </summary>
		BeginTag,

		/// <summary>
		/// Closing tag
		/// </summary>
		EndTag,

		/// <summary>
		/// Empty tag
		/// </summary>
		FullTag
	}

	/// <summary>
	/// Defines a general idea of the level of tag complexity
	/// </summary>
	/// <remarks>
	/// http://www.w3.org/TR/html401/sgml/dtd.html#inline
	/// </remarks>
	public enum HtmlTagBoxType
	{
		/// <summary>
		/// Not defined
		/// </summary>
		None = 0x0,

		/// <summary>
		/// character level elements and text strings
		/// </summary>
		Inline = 0x1,

		/// <summary>
		/// block-like elements (e.g. paragraphs and lists)
		/// </summary>
		Block = 0x2
	}

	#endregion Enums

	/// <summary>
	/// Represents an HTML/XHMTL tag
	/// </summary>
	/// <remarks>
	/// http://www.w3.org/TR/html401/
	/// http://www.w3.org/TR/xhtml1/
	/// </remarks>
	public class HtmlTag
	{
		#region Constants

		private const int DefaultAttributeCapacity = 3;
		private const int DefaultStyleCapacity = 3;
		public const string CommentTagName = "!--";
		public const string CommentAttributeName = "";

		#endregion Constants

		#region Fields

		private readonly HtmlTagType tagType = HtmlTagType.Unknown;
		private readonly HtmlTagBoxType boxType = HtmlTagBoxType.None;
		private readonly IHtmlFilter HtmlFilter;
		private readonly string rawName;
		private string tagName;
		private Dictionary<string, string> attributes;
		private Dictionary<string, string> styles;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="filter"></param>
		public HtmlTag(string name, IHtmlFilter filter)
		{
			this.HtmlFilter = filter;

			if (name == null)
			{
				name = String.Empty;
			}
			this.rawName = name.Trim();

			if (this.rawName.StartsWith("/"))
			{
				this.tagType = HtmlTagType.EndTag;
				this.rawName = this.rawName.Substring(1);
			}
			else if (this.rawName == HtmlTag.CommentTagName)
			{
				this.tagType = HtmlTagType.Comment;
			}
			else if (HtmlTag.IsFullTag(this.TagName)) // this.TagName is lowercase
			{
				this.tagType = HtmlTagType.FullTag;
			}
			else
			{
				this.tagType = HtmlTagType.BeginTag;
			}

			this.boxType = HtmlTag.GetBoxType(this.TagName);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the tag type
		/// </summary>
		public HtmlTagType TagType
		{
			get { return this.tagType; }
		}

		/// <summary>
		/// Gets the box type of the tag
		/// </summary>
		public HtmlTagBoxType BoxType
		{
			get { return this.boxType; }
		}

		/// <summary>
		/// Gets the tag name
		/// </summary>
		/// <remarks>
		/// Note: always lowercase
		/// </remarks>
		public string TagName
		{
			get
			{
				if (this.tagName == null)
				{
					this.tagName = this.rawName.ToLowerInvariant();
				}
				return this.tagName;
			}
		}

		/// <summary>
		/// Gets the collection of HTML attributes
		/// </summary>
		/// <remarks>
		/// Note: allocates space for attributes as a side effect
		/// </remarks>
		public Dictionary<string, string> Attributes
		{
			get
			{
				if (this.attributes == null)
				{
					this.attributes = new Dictionary<string, string>(HtmlTag.DefaultAttributeCapacity, StringComparer.InvariantCultureIgnoreCase);
				}
				return this.attributes;
			}
		}

		/// <summary>
		/// Tests whether any attributes exist
		/// </summary>
		/// <remarks>
		/// Note: does NOT allocate space for attributes as a side effect
		/// </remarks>
		public bool HasAttributes
		{
			get { return (this.attributes != null && this.attributes.Count > 0); }
		}

		/// <summary>
		/// Gets the collection of CSS styles
		/// </summary>
		/// <remarks>
		/// Note: allocates space for styles as a side effect
		/// </remarks>
		public Dictionary<string, string> Styles
		{
			get
			{
				if (this.styles == null)
				{
					this.styles = new Dictionary<string, string>(HtmlTag.DefaultStyleCapacity, StringComparer.InvariantCultureIgnoreCase);
				}
				return this.styles;
			}
		}

		/// <summary>
		/// Tests whether any styles exist
		/// </summary>
		/// <remarks>
		/// Note: does NOT allocate space for styles as a side effect
		/// </remarks>
		public bool HasStyles
		{
			get { return (this.styles != null && this.styles.Count > 0); }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Renders the tag to the output
		/// </summary>
		/// <param name="output"></param>
		/// <returns>true if rendered, false if not</returns>
		public bool WriteTag(StringBuilder output)
		{
			if (this.TagType == HtmlTagType.Unknown)
			{
				return false;
			}
			if (!this.HtmlFilter.FilterTag(this))
			{
				return false;
			}

			output.Append('<');
			if (this.TagType == HtmlTagType.EndTag)
			{
				output.Append('/');
			}

			output.Append(this.rawName);

			this.WriteAttributes(output);
			this.WriteStyles(output);

			if (this.TagType == HtmlTagType.Comment)
			{
				output.Append("--");
			}
			else if (this.TagType == HtmlTagType.FullTag)
			{
				output.Append(" /");
			}
			output.Append('>');

			return true;
		}

		/// <summary>
		/// Renders the attributes to the output
		/// </summary>
		/// <param name="output"></param>
		protected void WriteAttributes(StringBuilder output)
		{
			if (this.HasAttributes)
			{
				if (this.TagType == HtmlTagType.Comment)
				{
					output.Append(this.Attributes[HtmlTag.CommentAttributeName]);
					return;
				}

				foreach (string key in this.Attributes.Keys)
				{
					string val =
							/*(key == null) ? null :*/
							this.attributes[key];

					if (this.HtmlFilter.FilterAttribute(this.TagName, key, ref val))
					{
						if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(val))
						{
							output.AppendFormat(" {0}{1}", key, val);
						}
						else
						{
							output.AppendFormat(" {0}=\"{1}\"", key, val);
						}
					}
				}
			}
		}

		/// <summary>
		/// Renders the style property to the output
		/// </summary>
		/// <param name="output"></param>
		protected void WriteStyles(StringBuilder output)
		{
			if (this.HasStyles)
			{
				output.Append(" style=\"");

				foreach (string key in this.Styles.Keys)
				{
					string val =
							/*(key == null) ? null :*/
							this.styles[key];

					if (this.HtmlFilter.FilterStyle(this.TagName, key, ref val))
					{
						if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(val))
						{
							output.AppendFormat("{0}:{1};", key, val);
						}
					}
				}

				output.Append('\"');
			}
		}

		/// <summary>
		/// Generates a closing tag which matches this tag
		/// </summary>
		/// <returns></returns>
		public HtmlTag CreateCloseTag()
		{
			if (this.TagType != HtmlTagType.BeginTag)
			{
				return null;
			}

			return new HtmlTag('/'+this.rawName, this.HtmlFilter);
		}

		#endregion Methods

		#region Static Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag"></param>
		/// <returns>if is a full tag</returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// </remarks>
		protected static bool IsFullTag(string tag)
		{
			switch (tag)
			{
				case "area":
				case "base":
				case "basefont":
				case "br":
				case "col":
				case "frame":
				case "hr":
				case "img":
				case "input":
				case "isindex":
				case "link":
				case "meta":
				case "param":
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>the box type for a particular element</returns>
		protected static HtmlTagBoxType GetBoxType(string tag)
		{
			// http://www.w3.org/TR/html401/sgml/dtd.html#inline
			// Inline elements are all of the elements in:
			// inline, fontstyle, phrase, special, formctrl

			switch (tag)
			{
				case "address":
				case "a":
				case "abbr":
				case "acronym":
				case "b":
				case "bdo":
				case "big":
				case "br":
				case "button":
				case "caption":
				case "cite":
				case "code":
				case "dfn":
				case "dt":
				case "em":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "i":
				case "img":
				case "input":
				case "kbd":
				case "label":
				case "legend":
				case "map":
				case "p":
				case "pre":
				case "q":
				case "samp":
				case "select":
				case "small":
				case "span":
				case "strong":
				case "sub":
				case "sup":
				case "textarea":
				case "tt":
				case "var":
				{
					return HtmlTagBoxType.Inline;
				}
			}
			return HtmlTagBoxType.Block;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// </remarks>
		protected static bool OptionalClose(string tag)
		{
			switch (tag)
			{
				case "li":
				{
					return true;
				}
				default:
				{
					return false;
				}
			}
		}

		#endregion Static Methods
	}
}
