#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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
using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

using JsonFx.Json;

namespace JsonFx.Client
{
	/// <summary>
	/// An EcmaScriptWriter which also generates a markup version of the data.
	/// </summary>
	public class JsonMarkupWriter : EcmaScriptWriter
	{
		#region Constants

		private const string LiteralTrue = "true";
		private const string LiteralFalse = "false";

		#endregion Constants

		#region Fields

		private readonly TextWriter markup = new StringWriter();
		private bool suppressWrite = false;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JsonMarkupWriter(TextWriter markupWriter)
			: this(TextWriter.Null, markupWriter)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">TextWriter for data</param>
		/// <param name="output">TextWriter for markup</param>
		public JsonMarkupWriter(TextWriter output, TextWriter markupWriter)
			: base(output)
		{
			if (markupWriter == null)
			{
				markupWriter = new StringWriter();
			}
			this.markup = markupWriter;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">Stream for data</param>
		/// <param name="output">TextWriter for markup</param>
		public JsonMarkupWriter(Stream output, TextWriter markupWriter)
			: base(output)
		{
			if (markupWriter == null)
			{
				markupWriter = new StringWriter();
			}
			this.markup = markupWriter;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">File name for data</param>
		/// <param name="output">TextWriter for markup</param>
		public JsonMarkupWriter(string outputFileName, TextWriter markupWriter)
			: base(outputFileName)
		{
			if (markupWriter == null)
			{
				markupWriter = new StringWriter();
			}
			this.markup = markupWriter;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">StringBuilder for data</param>
		/// <param name="output">TextWriter for markup</param>
		public JsonMarkupWriter(StringBuilder output, TextWriter markupWriter)
			: base(output)
		{
			if (markupWriter == null)
			{
				markupWriter = new StringWriter();
			}
			this.markup = markupWriter;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the TextWriter used for markup
		/// </summary>
		public TextWriter MarkupWriter
		{
			get { return this.markup; }
		}

		#endregion Properties

		#region Writer Methods

		protected override void WriteArray(IEnumerable value)
		{
			this.MarkupWriteLine();
			this.markup.Write("<ul>");

			base.WriteArray(value);

			this.MarkupWriteLine();
			this.markup.Write("</ul>");
		}

		protected override void WriteArrayItem(object item)
		{
			this.MarkupWriteLine();
			this.markup.Write("<li>");

			base.WriteArrayItem(item);

			this.markup.Write("</li>");
		}

		protected override void WriteArrayItemDelim()
		{
			// nothing needed
			base.WriteArrayItemDelim();
		}

		protected override void Write(object value, bool isProperty)
		{
			// this calls other write methods
			base.Write(value, isProperty);
		}

		protected override void WriteObject(IDictionary value)
		{
			// this calls other write methods
			base.WriteObject(value);
		}

		protected override void WriteDictionary(IEnumerable value)
		{
			this.MarkupWriteLine();
			this.markup.Write("<dl>");

			base.WriteDictionary(value);

			this.MarkupWriteLine();
			this.markup.Write("</dl>");
		}

		protected override void WriteObject(object value, Type type)
		{
			this.MarkupWriteLine();
			this.markup.Write("<dl>");

			base.WriteObject(value, type);

			this.MarkupWriteLine();
			this.markup.Write("</dl>");
		}

		protected override void WriteObjectPropertyName(string name)
		{
			this.MarkupWriteLine();
			this.markup.Write("<dt>");

			this.markup.Write(name);

			this.suppressWrite = true;
			try
			{
				base.WriteObjectPropertyName(name);
			}
			finally
			{
				this.suppressWrite = false;
			}

			this.markup.Write("</dt>");
		}

		protected override void WriteObjectPropertyValue(object value)
		{
			this.MarkupWriteLine();
			this.markup.Write("<dd>");

			base.WriteObjectPropertyValue(value);

			this.markup.Write("</dd>");
		}

		protected override void WriteObjectPropertyDelim()
		{
			// nothing needed
			base.WriteObjectPropertyDelim();
		}

		protected override void WriteLine()
		{
			// nothing needed
			base.WriteLine();
		}

		private void MarkupWriteLine()
		{
			if (!this.Settings.PrettyPrint)
			{
				return;
			}

			string tab = this.Settings.Tab;
			for (int i=this.Depth; i>0; i--)
			{
				this.markup.Write(tab);
			}
			this.markup.Write(this.Settings.NewLine);
		}

		#endregion Writer Methods

		#region Primative Writer Methods

		public override void Write(bool value)
		{
			this.markup.Write(value ? JsonMarkupWriter.LiteralTrue : JsonMarkupWriter.LiteralFalse);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(byte value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(char value)
		{
			// this calls other write methods
			base.Write(value);
		}

		public override void Write(DateTime value)
		{
			this.markup.Write(value.ToString("yyyy-MM-dd HH:mm:ss zzz"));

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(decimal value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(double value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(float value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(int value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(long value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(sbyte value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(short value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(string value)
		{
			if (!this.suppressWrite && !String.IsNullOrEmpty(value))
			{
				this.markup.Write(value);
			}

			base.Write(value);
		}

		public override void Write(uint value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(ulong value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void Write(ushort value)
		{
			this.markup.Write("{0:g}", value);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		#endregion Primative Writer Methods

		#region Convenience Writer Methods

		public override void Write(Enum value)
		{
			// this calls other write methods
			base.Write(value);
		}

		public override void Write(Guid value)
		{
			// this calls other write methods
			base.Write(value);
		}

		public override void Write(TimeSpan value)
		{
			// this calls other write methods
			base.Write(value);
		}

		public override void Write(Uri value)
		{
			this.markup.Write("<a href=\"");
			this.markup.Write(HttpUtility.HtmlAttributeEncode(value.AbsoluteUri));
			this.markup.Write("\">");

			base.Write(value);

			this.markup.Write("</a>");
		}

		public override void Write(Version value)
		{
			// this calls other write methods
			base.Write(value);
		}

		public override void Write(XmlNode value)
		{
			this.markup.Write(value.OuterXml);

			this.suppressWrite = true;
			try
			{
				base.Write(value);
			}
			finally
			{
				this.suppressWrite = false;
			}
		}

		public override void WriteBase64(byte[] value)
		{
			// this calls other write methods
			base.WriteBase64(value);
		}

		public override void WriteHexString(byte[] value)
		{
			// this calls other write methods
			base.WriteHexString(value);
		}

		#endregion Convenience Writer Methods
	}
}
