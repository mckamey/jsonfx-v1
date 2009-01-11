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
using System.Web;
using System.Web.UI;
using System.Text;
using System.Text.RegularExpressions;

using JsonFx.UI.Jbst;

namespace JsonFx.UI
{
	/// <summary>
	/// Adapter between HtmlTextWriter interface and JbstCompiler internals
	/// </summary>
	public class JsonMLTextWriter : XhtmlTextWriter
	{
		#region Constants

		public const string JsonMLMimeType = "application/jsonml+json";

		#endregion Constants

		#region Fields

		private readonly TextWriter writer;
		private readonly JbstCompiler builder;
		private bool renderingIncompleteTag = false;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writer"></param>
		public JsonMLTextWriter(TextWriter writer)
			: base(new NullTextWriter(writer.Encoding))
		{
			HttpContext context = HttpContext.Current;
			string virtualPath =
				(context != null) ?
				context.Request.AppRelativeCurrentExecutionFilePath :
				String.Empty;

			this.writer = writer;
			this.builder = new JbstCompiler(virtualPath, false);
			this.builder.DocumentReady += new EventHandler(this.OnDocumentReady);
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="tabString"></param>
		public JsonMLTextWriter(TextWriter writer, string tabString)
			: base(new NullTextWriter(writer.Encoding), tabString)
		{
			HttpContext context = HttpContext.Current;
			string virtualPath =
				(context != null) ?
				context.Request.AppRelativeCurrentExecutionFilePath :
				String.Empty;

			this.writer = writer;
			this.builder = new JbstCompiler(virtualPath, false);
			this.builder.DocumentReady += new EventHandler(this.OnDocumentReady);
		}

		#endregion Init

		#region Methods

		private void OnDocumentReady(object sender, EventArgs e)
		{
			this.builder.Render(this.writer);
			this.builder.Clear();
		}

		public override void AddAttribute(string name, string value)
		{
			this.builder.StoreAttribute(name, value);
		}

		public override void AddAttribute(string name, string value, bool fEncode)
		{
			this.builder.StoreAttribute(name, value);
		}

		protected override void AddAttribute(string name, string value, HtmlTextWriterAttribute key)
		{
			this.builder.StoreAttribute(base.GetAttributeName(key), value);
		}

		public override void AddAttribute(HtmlTextWriterAttribute key, string value)
		{
			this.builder.StoreAttribute(base.GetAttributeName(key), value);
		}

		public override void AddAttribute(HtmlTextWriterAttribute key, string value, bool fEncode)
		{
			this.builder.StoreAttribute(base.GetAttributeName(key), value);
		}

		public override void AddStyleAttribute(string name, string value)
		{
			this.builder.StoreStyle(name, value);
		}

		protected override void AddStyleAttribute(string name, string value, HtmlTextWriterStyle key)
		{
			this.builder.StoreStyle(base.GetStyleName(key), value);
		}

		public override void AddStyleAttribute(HtmlTextWriterStyle key, string value)
		{
			this.builder.StoreStyle(base.GetStyleName(key), value);
		}

		//public override void BeginRender()
		//{
		//    base.BeginRender();
		//}

		public override void Close()
		{
			base.Close();
			this.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			((IDisposable)this.builder).Dispose();
			base.Dispose(disposing);
		}

		protected override string EncodeAttributeValue(HtmlTextWriterAttribute attrKey, string value)
		{
			// trap this, we don't want HtmlEncoded attributes
			return value;
		}

		public override Encoding Encoding
		{
			get { return this.writer.Encoding; }
		}

		public override void EndRender()
		{
			this.builder.Flush();
			base.EndRender();
		}

		//public override void EnterStyle(System.Web.UI.WebControls.Style style)
		//{
		//    base.EnterStyle(style);
		//}

		//public override void EnterStyle(System.Web.UI.WebControls.Style style, HtmlTextWriterTag tag)
		//{
		//    base.EnterStyle(style, tag);
		//}

		//public override void ExitStyle(System.Web.UI.WebControls.Style style)
		//{
		//    base.ExitStyle(style);
		//}

		//public override void ExitStyle(System.Web.UI.WebControls.Style style, HtmlTextWriterTag tag)
		//{
		//    base.ExitStyle(style, tag);
		//}

		protected override void FilterAttributes()
		{
			base.FilterAttributes();
		}

		public override void Flush()
		{
			this.builder.Flush();
			base.Flush();
		}

		//public override IFormatProvider FormatProvider
		//{
		//    get { return base.FormatProvider; }
		//}

		//public override int GetHashCode()
		//{
		//    return base.GetHashCode();
		//}

		//protected override HtmlTextWriterTag GetTagKey(string tagName)
		//{
		//    return base.GetTagKey(tagName);
		//}

		//protected override string GetTagName(HtmlTextWriterTag tagKey)
		//{
		//    return base.GetTagName(tagKey);
		//}

		//public override object InitializeLifetimeService()
		//{
		//    return base.InitializeLifetimeService();
		//}

		//public override bool IsValidFormAttribute(string attribute)
		//{
		//    return base.IsValidFormAttribute(attribute);
		//}

		//public override string NewLine
		//{
		//    get { return base.NewLine; }
		//    set { base.NewLine = value; }
		//}

		//protected override bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key)
		//{
		//    return base.OnAttributeRender(name, value, key);
		//}

		//protected override bool OnStyleAttributeRender(string name, string value, HtmlTextWriterStyle key)
		//{
		//    return base.OnStyleAttributeRender(name, value, key);
		//}

		//protected override bool OnTagRender(string name, HtmlTextWriterTag key)
		//{
		//    return base.OnTagRender(name, key);
		//}

		protected override void OutputTabs()
		{
			// trap this as we don't need extra whitespace
			//base.OutputTabs();
		}

		//protected override string RenderAfterContent()
		//{
		//    return base.RenderAfterContent();
		//}

		//protected override string RenderAfterTag()
		//{
		//    return base.RenderAfterTag();
		//}

		//protected override string RenderBeforeContent()
		//{
		//    return base.RenderBeforeContent();
		//}

		//protected override string RenderBeforeTag()
		//{
		//    return base.RenderBeforeTag();
		//}

		public override void RenderBeginTag(string tagName)
		{
			this.builder.PushTag(tagName);
		}

		public override void RenderBeginTag(HtmlTextWriterTag tagKey)
		{
			this.builder.PushTag(base.GetTagName(tagKey));
		}

		public override void RenderEndTag()
		{
			this.builder.PopTag(String.Empty);
		}

		public override void Write(bool value)
		{
			this.Write("{0}", value);
		}

		public override void Write(char value)
		{
			this.Write("{0}", value);
		}

		public override void Write(char[] buffer)
		{
			this.Write(new String(buffer));
		}

		public override void Write(char[] buffer, int index, int count)
		{
			this.Write(new String(buffer, index, count));
		}

		public override void Write(decimal value)
		{
			this.Write("{0}", value);
		}

		public override void Write(double value)
		{
			this.Write("{0}", value);
		}

		public override void Write(float value)
		{
			this.Write("{0}", value);
		}

		public override void Write(int value)
		{
			this.Write("{0}", value);
		}

		public override void Write(long value)
		{
			this.Write("{0}", value);
		}

		public override void Write(object value)
		{
			this.Write("{0}", value);
		}

		public override void Write(string format, object arg0)
		{
			this.Write(String.Format(format, arg0));
		}

		public override void Write(string format, object arg0, object arg1)
		{
			this.Write(String.Format(format, arg0, arg1));
		}

		public override void Write(string format, object arg0, object arg1, object arg2)
		{
			this.Write(String.Format(format, arg0, arg1, arg2));
		}

		public override void Write(string format, params object[] arg)
		{
			this.Write(String.Format(format, arg));
		}

		public override void Write(string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return;
			}

			if (this.renderingIncompleteTag &&
				value.Equals(HtmlTextWriter.TagRightChar.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				this.renderingIncompleteTag = false;
				return;
			}

			this.builder.Parse(value);
		}

		public override void Write(uint value)
		{
			this.Write("{0}", value);
		}

		public override void Write(ulong value)
		{
			this.Write("{0}", value);
		}

		public override void WriteAttribute(string name, string value)
		{
			this.builder.AddAttribute(name, value);
		}

		public override void WriteAttribute(string name, string value, bool encode)
		{
			this.builder.AddAttribute(name, value);
		}

		public override void WriteBeginTag(string tagName)
		{
			this.renderingIncompleteTag = true;
			this.builder.PushTag(tagName);
		}

		public override void WriteBreak()
		{
			// writes out <br /> tag
			base.WriteBreak();
		}

		public override void WriteEncodedText(string text)
		{
			this.Write(text);
		}

		public override void WriteEncodedUrl(string url)
		{
			this.Write(url);
		}

		public override void WriteEncodedUrlParameter(string urlText)
		{
			this.Write(urlText);
		}

		public override void WriteEndTag(string tagName)
		{
			this.builder.PopTag(tagName);
		}

		public override void WriteFullBeginTag(string tagName)
		{
			this.builder.PushTag(tagName);
			this.builder.PopTag(tagName);
		}

		public override void WriteLine()
		{
			// trap this as we don't need extra whitespace
			//base.WriteLine();
		}

		public override void WriteLine(bool value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(char value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(char[] buffer)
		{
			this.Write(buffer);
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			this.Write(new String(buffer, index, count));
		}

		public override void WriteLine(decimal value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(double value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(float value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(int value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(long value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(object value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(string format, object arg0)
		{
			this.Write(format, arg0);
		}

		public override void WriteLine(string format, object arg0, object arg1)
		{
			this.Write(format, arg0, arg1);
		}

		public override void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			this.Write(format, arg0, arg1, arg2);
		}

		public override void WriteLine(string format, params object[] args)
		{
			this.Write(format, args);
		}

		public override void WriteLine(string s)
		{
			this.Write(s);
		}

		public override void WriteLine(uint value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(ulong value)
		{
			this.Write("{0}", value);
		}

		public override void WriteStyleAttribute(string name, string value)
		{
			this.builder.AddStyle(name, value);
		}

		public override void WriteStyleAttribute(string name, string value, bool encode)
		{
			this.builder.AddStyle(name, value);
		}

		#endregion Methods

		#region NullTextWriter

		private class NullTextWriter : System.IO.TextWriter
		{
			#region Fields

			private readonly Encoding encoding;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="encoding"></param>
			public NullTextWriter(Encoding encoding)
			{
				this.encoding = encoding;
			}

			#endregion Init

			#region Properties

			public override Encoding Encoding
			{
				get { return this.encoding; }
			}

			#endregion Properties
		}

		#endregion NullTextWriter
	}
}
