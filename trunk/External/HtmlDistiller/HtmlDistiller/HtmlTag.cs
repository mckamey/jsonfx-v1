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
	/// Defines a general taxonomy of tags
	/// </summary>
	/// <remarks>
	/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#sec_5.2.
	/// </remarks>
	[Flags]
	public enum HtmlModuleType
	{
		/// <summary>
		/// Plain text, no tag
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// HTML comments
		/// </summary>
		Comment = 0x0001,

		/// <summary>
		/// textual elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/struct/text.html
		/// </remarks>
		Text = 0x0002,

		/// <summary>
		/// character level elements and text strings
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/struct/text.html
		/// </remarks>
		Inline = 0x0004,

		/// <summary>
		/// block-like elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/struct/text.html
		/// </remarks>
		Block = 0x0008,

		/// <summary>
		/// list elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/struct/lists.html
		/// </remarks>
		List = 0x0010,

		/// <summary>
		/// tabular elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/struct/tables.html
		/// </remarks>
		Table = 0x0020,

		/// <summary>
		/// style elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/present/styles.html
		/// http://www.w3.org/TR/html401/present/graphics.html
		/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#s_presentationmodule
		/// </remarks>
		Style = 0x0040,

		/// <summary>
		/// form elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#s_forms
		/// </remarks>
		Form = 0x0080,

		/// <summary>
		/// script elements
		/// </summary>
		Script = 0x0100,

		/// <summary>
		/// embedded elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/struct/objects.html
		/// </remarks>
		Embeded = 0x0200,

		/// <summary>
		/// document elements
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/html401/struct/global.html
		/// </remarks>
		Document = 0x0400,

		/// <summary>
		/// unknown elements
		/// </summary>
		Unknown = 0x8000
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
		private readonly HtmlModuleType moduleTypes = HtmlModuleType.None;
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

			this.moduleTypes = HtmlTag.GetModuleType(this.TagName);
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
		/// Gets the HTML module types for the tag
		/// </summary>
		public HtmlModuleType ModuleTypes
		{
			get { return this.moduleTypes; }
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

		public bool WriteTag(StringBuilder output)
		{
			return this.WriteTag(output, true);
		}

		/// <summary>
		/// Renders the tag to the output
		/// </summary>
		/// <param name="output"></param>
		/// <returns>true if rendered, false if not</returns>
		protected bool WriteTag(StringBuilder output, bool filter)
		{
			if (this.TagType == HtmlTagType.Unknown)
			{
				return false;
			}
			if (filter && !this.HtmlFilter.FilterTag(this))
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
			if (this.TagType != HtmlTagType.BeginTag &&
				HtmlTag.CloseTagRequired(this.TagName))
			{
				return null;
			}

			return new HtmlTag('/'+this.rawName, this.HtmlFilter);
		}

		#endregion Methods

		#region Object Overrides

		/// <summary>
		/// Renders the tag.  Used mainly for Debugging.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			this.WriteTag(builder, false);
			return builder.ToString();
		}

		#endregion Object Overrides

		#region Static Methods

		/// <summary>
		/// Determines if is full (i.e. empty) tag
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>if is a full tag</returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#sec_5.2.
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
		/// Determines if the tag is required to be closed (in HTML 4.01)
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// </remarks>
		protected static bool CloseTagRequired(string tag)
		{
			switch (tag)
			{
				//case "body":
				case "dd":
				case "dt":
				//case "head":
				//case "html":
				case "li":
				case "option":
				case "p":
				//case "tbody":
				case "td":
				//case "tfoot":
				//case "thead":
				case "th":
				case "tr":
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>the box type for a particular element</returns>
		protected static HtmlModuleType GetModuleType(string tag)
		{
			// http://www.w3.org/TR/html401/
			// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html
			switch (tag)
			{
				case "!--":
				{
					return HtmlModuleType.Comment;
				}

				case "a":
				case "abbr":
				case "acronym":
				case "address":
				case "area":
				case "bdo":
				case "cite":
				case "code":
				case "dfn":
				case "em":
				case "img":
				case "isindex":
				case "kbd":
				case "label":
				case "legend":
				case "map":
				case "q":
				case "samp":
				case "span":
				case "strong":
				case "var":
				{
					return HtmlModuleType.Text|HtmlModuleType.Inline;
				}

				case "b":
				case "big":
				case "font":
				case "i":
				case "s":
				case "small":
				case "strike":
				case "sub":
				case "sup":
				case "tt":
				case "u":
				{
					return HtmlModuleType.Text|HtmlModuleType.Style|HtmlModuleType.Inline;
				}

				case "blockquote":
				case "br":
				case "center":
				case "del":
				case "div":
				case "fieldset":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "hr":
				case "ins":
				case "p":
				case "pre":
				{
					return HtmlModuleType.Text|HtmlModuleType.Block;
				}

				case "dl":
				case "dd":
				case "dir":
				case "dt":
				case "li":
				case "menu":
				case "ol":
				case "ul":
				{
					return HtmlModuleType.List;
				}

				case "table":
				case "tbody":
				case "td":
				case "th":
				case "thead":
				case "tfoot":
				case "tr":
				case "caption":
				case "col":
				case "colgroup":
				{
					return HtmlModuleType.Table;
				}

				case "button":
				case "form":
				case "input":
				case "optgroup":
				case "option":
				case "select":
				case "textarea":
				{
					return HtmlModuleType.Form;
				}

				case "applet":
				case "embed":
				case "object":
				case "param":
				{
					return HtmlModuleType.Embeded;
				}

				case "basefont":
				case "style":
				{
					return HtmlModuleType.Style|HtmlModuleType.Document;
				}

				case "noscript":
				case "script":
				{
					return HtmlModuleType.Script|HtmlModuleType.Document;
				}

				case "!doctype":
				case "base":
				case "body":
				case "head":
				case "html":
				case "frameset":
				case "frame":
				case "iframe":
				case "link":
				case "meta":
				case "noframes":
				case "title":
				{
					return HtmlModuleType.Document;
				}
			}
			return HtmlModuleType.Unknown;
		}

		#endregion Static Methods
	}
}
