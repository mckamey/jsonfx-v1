#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

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
using System.IO;
using System.Collections.Generic;
using System.Text;

using BuildTools.HtmlDistiller.Filters;

namespace BuildTools.HtmlDistiller.Writers
{
	public interface IHtmlWriter
	{
		#region Methods

		char PrevChar(int peek);

		void WriteLiteral(char literal);

		void WriteLiteral(string literal);

		bool WriteTag(HtmlTag tag, IHtmlFilter filter);

		#endregion Methods
	}

	public class HtmlWriter : IHtmlWriter, IDisposable
	{
		#region Fields

		private TextWriter writer = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public HtmlWriter() : this(null)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="writer">the underlying TextWriter</param>
		public HtmlWriter(TextWriter writer)
		{
			this.writer = (writer != null) ?
				writer : new StringWriter();
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the underlying TextWriter.
		/// </summary>
		public TextWriter TextWriter
		{
			get { return this.writer; }
		}

		#endregion Properties

		#region IHtmlWriter Members

		public void WriteLiteral(char literal)
		{
			this.writer.Write(literal);
		}

		public void WriteLiteral(string literal)
		{
			this.writer.Write(literal);
		}

		/// <summary>
		/// Renders the tag to the output
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="filter"></param>
		/// <returns>true if rendered, false if not</returns>
		public bool WriteTag(HtmlTag tag, IHtmlFilter filter)
		{
			if (tag.TagType == HtmlTagType.Unknown)
			{
				return false;
			}
			if (filter != null && !filter.FilterTag(tag))
			{
				return false;
			}

			this.writer.Write('<');
			if (tag.TagType == HtmlTagType.EndTag)
			{
				this.writer.Write('/');
			}

			this.writer.Write(tag.RawName);

			if (tag.HasAttributes)
			{
				this.WriteAttributes(tag, filter);
			}
			if (tag.HasStyles)
			{
				this.WriteStyles(tag, filter);
			}

			if (tag.TagType == HtmlTagType.Comment)
			{
				this.writer.Write(tag.Attributes[HtmlTag.Key_EndDelim]);
			}
			else if (tag.TagType == HtmlTagType.FullTag)
			{
				this.writer.Write(" /");
			}
			this.writer.Write('>');

			return true;
		}

		char IHtmlWriter.PrevChar(int peek)
		{
			// TODO: determine if this is really needed

			if (!(this.writer is StringWriter))
			{
				return HtmlDistiller.NullChar;
			}

			StringBuilder builder = ((StringWriter)this.writer).GetStringBuilder();

			if (builder.Length < peek)
			{
				return HtmlDistiller.NullChar;
			}

			return builder[builder.Length-peek];
		}

		#endregion IHtmlWriter Members

		#region Methods

		/// <summary>
		/// Renders the attributes to the output
		/// </summary>
		/// <param name="output"></param>
		protected void WriteAttributes(HtmlTag tag, IHtmlFilter filter)
		{
			if (tag.TagType == HtmlTagType.Comment)
			{
				this.writer.Write(tag.Attributes[HtmlTag.Key_Contents]);
				return;
			}

			foreach (string key in tag.Attributes.Keys)
			{
				string val = tag.Attributes[key];

				if (filter.FilterAttribute(tag.TagName, key, ref val))
				{
					if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(val))
					{
						this.writer.Write(" {0}{1}", key, val);
					}
					else
					{
						this.writer.Write(" {0}=\"{1}\"", key, val);
					}
				}
			}
		}

		/// <summary>
		/// Renders the style property to the output
		/// </summary>
		/// <param name="output"></param>
		protected void WriteStyles(HtmlTag tag, IHtmlFilter filter)
		{
			this.writer.Write(" style=\"");

			foreach (string key in tag.Styles.Keys)
			{
				string val = tag.Styles[key];

				if (filter.FilterStyle(tag.TagName, key, ref val))
				{
					if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(val))
					{
						this.writer.Write("{0}:{1};", key, val);
					}
				}
			}

			this.writer.Write('\"');
		}

		#endregion Methods

		#region Object Overrides

		/// <summary>
		/// Returns a System.String that represents the current TextWriter.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.TextWriter.ToString();
		}

		#endregion Object Overrides

		#region IDisposable Members

		/// <summary>
		/// Releases all resources used by the System.IO.TextWriter object.
		/// </summary>
		void IDisposable.Dispose()
		{
			this.writer.Dispose();
		}

		#endregion IDisposable Members
	}
}
