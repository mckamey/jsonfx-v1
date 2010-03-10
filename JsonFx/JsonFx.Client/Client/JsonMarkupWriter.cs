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

using JsonFx.Json;

namespace JsonFx.Client
{
	/// <summary>
	/// An EcmaScriptWriter which also generates a markup version of the data.
	/// </summary>
	public class JsonMarkupWriter : EcmaScriptWriter
	{
		#region Fields

		private StringBuilder markup = new StringBuilder();

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JsonMarkupWriter()
			: this(TextWriter.Null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">TextWriter for writing</param>
		public JsonMarkupWriter(TextWriter output)
			: base(output)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">Stream for writing</param>
		public JsonMarkupWriter(Stream output)
			: base(output)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">File name for writing</param>
		public JsonMarkupWriter(string outputFileName)
			: base(outputFileName)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="output">StringBuilder for appending</param>
		public JsonMarkupWriter(StringBuilder output)
			: base(output)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets
		/// </summary>
		public string MarkupContent
		{
			get { return this.markup.ToString(); }
		}

		#endregion Properties

		#region Serialization Methods

		protected override void WriteArray(IEnumerable value)
		{
			this.markup.Append("<ul>");

			base.WriteArray(value);

			this.markup.Append("</ul>");
		}

		protected override void WriteArrayItem(object item)
		{
			this.markup.Append("<li>");

			base.WriteArrayItem(item);

			this.markup.Append("</li>");
		}

		protected override void WriteDictionary(IEnumerable value)
		{
			this.markup.Append("<dl>");

			base.WriteDictionary(value);

			this.markup.Append("</dl>");
		}

		protected override void WriteObject(object value, Type type)
		{
			this.markup.Append("<dl>");

			base.WriteObject(value, type);

			this.markup.Append("</dl>");
		}

		protected override void WriteObjectPropertyName(string name)
		{
			this.markup.Append("<dt>");
			this.markup.Append(HttpUtility.HtmlEncode(name));

			base.WriteObjectPropertyName(name);

			this.markup.Append("</dt>");
		}

		protected override void WriteObjectPropertyValue(object value)
		{
			this.markup.Append("</dd>");

			base.WriteObjectPropertyValue(value);

			this.markup.Append("</dd>");
		}

		public override void Write(bool value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(byte value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(char value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(DateTime value)
		{
			this.markup.Append(value.ToString("yyyy-MM-dd HH:mm:ss zzz"));

			base.Write(value);
		}

		public override void Write(decimal value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(double value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(Enum value)
		{
			this.markup.Append(value.ToString("F"));

			base.Write(value);
		}

		public override void Write(float value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(Guid value)
		{
			this.markup.Append(value.ToString("N"));

			base.Write(value);
		}

		public override void Write(int value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(long value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(sbyte value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(short value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(string value)
		{
			this.markup.Append(HttpUtility.HtmlEncode(value));

			base.Write(value);
		}

		public override void Write(System.Xml.XmlNode value)
		{
			// TODO: ???
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(TimeSpan value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(uint value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(ulong value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(Uri value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(ushort value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void Write(Version value)
		{
			this.markup.Append(value);

			base.Write(value);
		}

		public override void WriteBase64(byte[] value)
		{
			// TODO: ???
			this.markup.Append(value);

			base.WriteBase64(value);
		}

		public override void WriteHexString(byte[] value)
		{
			// TODO: ???
			this.markup.Append(value);

			base.WriteHexString(value);
		}

		protected override void Write(object value, bool isProperty)
		{
			string tag = isProperty ? "span>" : "div>";

			this.markup.Append('<');
			this.markup.Append(tag);

			// this calls other overrides
			base.Write(value, isProperty);

			this.markup.Append("</");
			this.markup.Append(tag);
		}

		protected override void WriteObject(IDictionary value)
		{
			// this calls other overrides
			base.WriteObject(value);
		}

		protected override void WriteArrayItemDelim()
		{
			// nothing needed
			base.WriteArrayItemDelim();
		}

		protected override void WriteObjectPropertyDelim()
		{
			// nothing needed
			base.WriteObjectPropertyDelim();
		}

		protected override void WriteLine()
		{
			if (this.Settings.PrettyPrint)
			{
				string tab = this.Settings.Tab;
				for (int i=0; i<this.Depth; i++)
				{
					this.markup.Append(tab);
				}
				this.markup.Append(this.Settings.NewLine);
			}

			base.WriteLine();
		}

		#endregion Serialization Methods
	}
}
