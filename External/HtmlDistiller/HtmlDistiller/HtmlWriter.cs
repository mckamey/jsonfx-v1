using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using BuildTools.HtmlDistiller.Filters;

namespace BuildTools.HtmlDistiller.Writers
{
	public interface IHtmlWriter
	{
		#region Properties

		bool Initialized { get; }

		#endregion Properties

		#region Methods

		void Reset();

		void Init(int capacity);

		void WriteLiteral(char literal);

		void WriteLiteral(string literal);

		void WriteLiteral(string source, int start, int count);

		bool WriteTag(HtmlTag tag, IHtmlFilter filter);

		#endregion Methods

		char PrevChar(int peek);
	}

	public class HtmlWriter : IHtmlWriter
	{
		#region Fields

		private StringBuilder output = null;

		#endregion Fields

		#region Init

		//public HtmlWriter(TextWriter writer)
		//{

		//}

		#endregion Init

		#region Properties

		public string Output
		{
			get
			{
				if (this.output == null)
				{
					return null;
				}

				return this.output.ToString();
			}
		}

		#endregion Properties

		#region IHtmlWriter Members

		bool IHtmlWriter.Initialized
		{
			get { return (this.output != null); }
		}

		void IHtmlWriter.Reset()
		{
			this.output = null;
		}

		void IHtmlWriter.Init(int capacity)
		{
			this.output = new StringBuilder(capacity);
		}

		void IHtmlWriter.WriteLiteral(char literal)
		{
			this.output.Append(literal);
		}

		public void WriteLiteral(string literal)
		{
			this.output.Append(literal);
		}

		void IHtmlWriter.WriteLiteral(string source, int start, int count)
		{
			this.output.Append(source, start, count);
		}

		/// <summary>
		/// Renders the tag to the output
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="filter"></param>
		/// <returns>true if rendered, false if not</returns>
		bool IHtmlWriter.WriteTag(HtmlTag tag, IHtmlFilter filter)
		{
			if (tag.TagType == HtmlTagType.Unknown)
			{
				return false;
			}
			if (filter != null && !filter.FilterTag(tag))
			{
				return false;
			}

			this.output.Append('<');
			if (tag.TagType == HtmlTagType.EndTag)
			{
				this.output.Append('/');
			}

			this.output.Append(tag.RawName);

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
				this.output.Append(tag.Attributes[HtmlTag.Key_EndDelim]);
			}
			else if (tag.TagType == HtmlTagType.FullTag)
			{
				this.output.Append(" /");
			}
			this.output.Append('>');

			return true;
		}

		char IHtmlWriter.PrevChar(int peek)
		{
			if (this.output.Length < peek)
			{
				return HtmlDistiller.NullChar;
			}

			return this.output[this.output.Length-peek];
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
				this.output.Append(tag.Attributes[HtmlTag.Key_Contents]);
				return;
			}

			foreach (string key in tag.Attributes.Keys)
			{
				string val = tag.Attributes[key];

				if (filter.FilterAttribute(tag.TagName, key, ref val))
				{
					if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(val))
					{
						this.output.AppendFormat(" {0}{1}", key, val);
					}
					else
					{
						this.output.AppendFormat(" {0}=\"{1}\"", key, val);
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
			this.output.Append(" style=\"");

			foreach (string key in tag.Styles.Keys)
			{
				string val = tag.Styles[key];

				if (filter.FilterStyle(tag.TagName, key, ref val))
				{
					if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(val))
					{
						this.output.AppendFormat("{0}:{1};", key, val);
					}
				}
			}

			this.output.Append('\"');
		}

		#endregion Methods
	}
}
