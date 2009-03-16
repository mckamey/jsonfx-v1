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
using System.IO;
using System.Collections.Generic;
using System.Text;

using JsonFx.BuildTools.HtmlDistiller.Filters;

namespace JsonFx.BuildTools.HtmlDistiller.Writers
{
	public interface IHtmlWriter
	{
		#region Methods

		void SetHtmlFilter(IHtmlFilter filter);

		void WriteLiteral(string value);

		void WriteLiteral(string source, int start, int end);

		bool WriteTag(HtmlTag tag);

		#endregion Methods
	}

	public interface IReversePeek
	{
		char PrevChar(int peek);
	}

	public class HtmlWriter : IHtmlWriter, IReversePeek, IDisposable
	{
		#region Fields

		private TextWriter writer = null;
		private IHtmlFilter filter = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public HtmlWriter()
			: this((TextWriter)null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writer">the underlying Stream</param>
		public HtmlWriter(Stream stream)
			: this((stream != null) ? new StreamWriter(stream) : null)
		{
		}

		/// <summary>
		/// Ctor
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

		void IHtmlWriter.SetHtmlFilter(IHtmlFilter filter)
		{
			this.filter = filter;
		}

		public void WriteLiteral(string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				this.WriteLiteral(value, 0, value.Length);
			}
		}

		public virtual void WriteLiteral(string source, int start, int end)
		{
			string replacement;
			if (this.filter != null && this.filter.FilterLiteral(source, start, end, out replacement))
			{
				// filter has altered the literal
				this.writer.Write(replacement);
			}
			else
			{
				// use the original substring
				this.writer.Write(source.Substring(start, end-start));
			}
		}

		/// <summary>
		/// Renders the tag to the output
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="filter"></param>
		/// <returns>true if rendered, false if not</returns>
		public virtual bool WriteTag(HtmlTag tag)
		{
			if (tag.TagType == HtmlTagType.Unknown)
			{
				return false;
			}
			if (this.filter != null && !this.filter.FilterTag(tag))
			{
				return false;
			}

			if (tag.TagType == HtmlTagType.Unparsed)
			{
				return this.WriteUnparsedTag(tag);
			}

			this.writer.Write('<');
			if (tag.TagType == HtmlTagType.EndTag)
			{
				this.writer.Write('/');
			}

			this.writer.Write(tag.RawName);

			if (tag.HasAttributes)
			{
				this.WriteAttributes(tag);
			}
			if (tag.HasStyles)
			{
				this.WriteStyles(tag);
			}

			if (tag.TagType == HtmlTagType.FullTag)
			{
				this.writer.Write(" /");
			}
			this.writer.Write('>');

			return true;
		}

		#endregion IHtmlWriter Members

		#region Methods

		private bool WriteUnparsedTag(HtmlTag tag)
		{
			this.writer.Write('<');
			this.writer.Write(tag.RawName);
			this.writer.Write(tag.Content);
			this.writer.Write(tag.EndDelim);
			this.writer.Write('>');
			return true;
		}

		/// <summary>
		/// Renders the attributes to the output
		/// </summary>
		/// <param name="output"></param>
		private void WriteAttributes(HtmlTag tag)
		{
			foreach (string key in tag.Attributes.Keys)
			{
				object valObj = tag.Attributes[key];
				if (valObj is HtmlTag)
				{
					if (this.filter != null && !this.filter.FilterTag((HtmlTag)valObj))
					{
						valObj = null;
					}
				}

				string val = (valObj != null) ?
					valObj.ToString() :
					String.Empty;

				if (this.filter == null || this.filter.FilterAttribute(tag.TagName, key, ref val))
				{
					this.writer.Write(' ');
					if (String.IsNullOrEmpty(val))
					{
						this.writer.Write(HtmlAttributeEncode(key));
					}
					else if (String.IsNullOrEmpty(key))
					{
						this.writer.Write(HtmlAttributeEncode(val));
					}
					else
					{
						this.writer.Write(HtmlAttributeEncode(key));
						this.writer.Write("=\"");
						this.writer.Write(HtmlAttributeEncode(val));
						this.writer.Write("\"");
					}
				}
			}
		}

		/// <summary>
		/// Renders the style property to the output
		/// </summary>
		/// <param name="output"></param>
		private void WriteStyles(HtmlTag tag)
		{
			this.writer.Write(" style=\"");

			foreach (string key in tag.Styles.Keys)
			{
				string val = tag.Styles[key];

				if (this.filter == null || this.filter.FilterStyle(tag.TagName, key, ref val))
				{
					if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(val))
					{
						this.writer.Write(HtmlAttributeEncode(key));
						this.writer.Write(':');
						this.writer.Write(HtmlAttributeEncode(val));
						this.writer.Write(';');
					}
				}
			}

			this.writer.Write('\"');
		}

		private string HtmlAttributeEncode(string value)
		{
			if (value == null)
			{
				return String.Empty;
			}

			if (value.IndexOf('"') < 0)
			{
				return value;
			}

			return value.Replace("\"", "&quot;");
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
			this.writer.Flush();
			this.writer.Close();
			this.writer.Dispose();
		}

		#endregion IDisposable Members

		#region IReversePeek Members

		char IReversePeek.PrevChar(int peek)
		{
			// TODO: determine if there is a better way for this

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

		#endregion IReversePeek Members
	}
}
