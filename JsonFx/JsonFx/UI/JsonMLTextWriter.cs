using System;
using System.IO;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace JsonFx.UI
{
	public class JsonMLTextWriter : System.Web.UI.XhtmlTextWriter
	{
		#region Fields

		private const string Pattern_Comment = @"(<!DOCTYPE[^>]*>)|(<!--[^->]*-->)";
		private const string Pattern_Tag = "\\G<(?<tagname>[\\w:\\.]+)(\\s+(?<attrname>\\w[-\\w:]*)(\\s*=\\s*\"(?<attrval>[^\"]*)\"|\\s*=\\s*'(?<attrval>[^']*)'|\\s*=\\s*(?<attrval><%#.*?%>)|\\s*=\\s*(?<attrval>[^\\s=/>]*)|(?<attrval>\\s*?)))*\\s*(?<empty>/)?>";
		private const string Pattern_EndTag = @"\G</(?<tagname>[\w:\.]+)\s*>";

		private JsonControlBuilder builder;
		private static readonly Regex Regex_Tag = new Regex(Pattern_Tag, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.Compiled);
		private static readonly Regex Regex_EndTag = new Regex(Pattern_EndTag, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.Compiled);
		private static readonly Regex Regex_Comment = new Regex(Pattern_Comment, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.IgnoreCase);

		#endregion Fields

		#region Init

		public JsonMLTextWriter(TextWriter writer) : base(new NullTextWriter())
		{
			this.builder = new JsonControlBuilder(writer);
			this.builder.PreProcess = new PreProcessLiteral(this.ParseLiteral);
		}

		public JsonMLTextWriter(TextWriter writer, string tabString) : base(new NullTextWriter(), tabString)
		{
			this.builder = new JsonControlBuilder(writer);
		}

		#endregion Init

		#region Methods

		public override void AddAttribute(string name, string value)
		{
			// calls AddAttribute(string name, string value, HtmlTextWriterAttribute key)
			//base.AddAttribute(name, value);

			this.builder.StoreAttribute(name, value);
		}

		public override void AddAttribute(string name, string value, bool fEncode)
		{
			//base.AddAttribute(name, value, fEncode);

			this.builder.StoreAttribute(name, value);
		}

		protected override void AddAttribute(string name, string value, HtmlTextWriterAttribute key)
		{
			///base.AddAttribute(name, value, key);
			this.builder.StoreAttribute(base.GetAttributeName(key), value);
		}

		public override void AddAttribute(HtmlTextWriterAttribute key, string value)
		{
			//base.AddAttribute(key, value);
			this.builder.StoreAttribute(base.GetAttributeName(key), value);
		}

		public override void AddAttribute(HtmlTextWriterAttribute key, string value, bool fEncode)
		{
			//base.AddAttribute(key, value, fEncode);
			this.builder.StoreAttribute(base.GetAttributeName(key), value);
		}

		public override void AddStyleAttribute(string name, string value)
		{
			// calls AddStyleAttribute(string name, string value, HtmlTextWriterStyle key)
			//base.AddStyleAttribute(name, value);

			this.builder.StoreStyle(name, value);
		}

		protected override void AddStyleAttribute(string name, string value, HtmlTextWriterStyle key)
		{
			//base.AddStyleAttribute(name, value, key);
			this.builder.StoreStyle(base.GetStyleName(key), value);
		}

		public override void AddStyleAttribute(HtmlTextWriterStyle key, string value)
		{
			// calls AddStyleAttribute(string name, string value, HtmlTextWriterStyle key)
			//base.AddStyleAttribute(key, value);

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

		//public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType)
		//{
		//    return base.CreateObjRef(requestedType);
		//}

		protected override void Dispose(bool disposing)
		{
			try
			{
				base.Dispose(disposing);
			}
			finally
			{
				this.builder.Dispose();
			}
		}

		protected override string EncodeAttributeValue(HtmlTextWriterAttribute attrKey, string value)
		{
			// trap this, we don't want HtmlEncoded attributes

			//return base.EncodeAttributeValue(attrKey, value);
			return value;
		}

		//public override System.Text.Encoding Encoding
		//{
		//    get { return System.Text.Encoding.UTF8; }
		//}

		//public override void EndRender()
		//{
		//    base.EndRender();
		//}

		//public override void EnterStyle(System.Web.UI.WebControls.Style style)
		//{
		//    base.EnterStyle(style);
		//}

		//public override void EnterStyle(System.Web.UI.WebControls.Style style, HtmlTextWriterTag tag)
		//{
		//    base.EnterStyle(style, tag);
		//}

		//public override bool Equals(object obj)
		//{
		//    return base.Equals(obj);
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

		//public override void Flush()
		//{
		//    base.Flush();
		//}

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

		protected override bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key)
		{
			return base.OnAttributeRender(name, value, key);
		}

		protected override bool OnStyleAttributeRender(string name, string value, HtmlTextWriterStyle key)
		{
			return base.OnStyleAttributeRender(name, value, key);
		}

		protected override bool OnTagRender(string name, HtmlTextWriterTag key)
		{
			return base.OnTagRender(name, key);
		}

		//protected override void OutputTabs()
		//{
		//    base.OutputTabs();
		//}

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
			//base.RenderBeginTag(tagName);
			this.builder.PushTag(tagName);
		}

		public override void RenderBeginTag(HtmlTextWriterTag tagKey)
		{
			//base.RenderBeginTag(tagKey);
			this.builder.PushTag(base.GetTagName(tagKey));
		}

		public override void RenderEndTag()
		{
			//base.RenderEndTag();
			this.builder.PopTag();
		}

		//public override string ToString()
		//{
		//    return base.ToString();
		//}

		public override void Write(bool value)
		{
			this.Write("{0}", value);
		}

		public override void Write(char value)
		{
#warning Need to remove this filter
			if (value != '>')
				this.Write("{0}", value);
		}

		public override void Write(char[] buffer)
		{
			//this.Write(buffer);
			this.Write(new String(buffer));
		}

		public override void Write(char[] buffer, int index, int count)
		{
			//base.Write(buffer, index, count);
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
			//base.Write(format, arg0);
			this.builder.AddLiteral(String.Format(format, arg0));
		}

		public override void Write(string format, object arg0, object arg1)
		{
			//base.Write(format, arg0, arg1);
			this.builder.AddLiteral(String.Format(format, arg0, arg1));
		}

		public override void Write(string format, object arg0, object arg1, object arg2)
		{
			//base.Write(format, arg0, arg1, arg2);
			this.builder.AddLiteral(String.Format(format, arg0, arg1, arg2));
		}

		public override void Write(string format, params object[] arg)
		{
			//base.Write(format, arg);
			this.builder.AddLiteral(String.Format(format, arg));
		}

		public override void Write(string value)
		{
			//base.Write(value);

			if (String.IsNullOrEmpty(value))
			{
				return;
			}

			this.builder.AddLiteral(value);
		}

		public override void Write(uint value)
		{
			this.Write("{0}", value);
		}

		public override void Write(ulong value)
		{
			this.Write("{0}", value);
		}

		//public override void WriteAttribute(string name, string value)
		//{
		//    // this calls WriteAttribute(string name, string value, bool fEncode)
		//    base.WriteAttribute(name, value);
		//}

		public override void WriteAttribute(string name, string value, bool fEncode)
		{
			//base.WriteAttribute(name, value, fEncode);
			this.builder.AddAttribute(name, value);
		}

		public override void WriteBeginTag(string tagName)
		{
			//base.WriteBeginTag(tagName);
			this.builder.PushTag(tagName);
		}

		//public override void WriteBreak()
		//{
		//    base.WriteBreak();
		//}

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
			//base.WriteEndTag(tagName);
			this.builder.PopTag();
		}

		public override void WriteFullBeginTag(string tagName)
		{
			//base.WriteFullBeginTag(tagName);
			this.builder.PushTag(tagName);
			this.builder.PopTag();
		}

		//public override void WriteLine()
		//{
		//    base.WriteLine();
		//}

		public override void WriteLine(bool value)
		{
			this.Write("{0}", value);
		}

		public override void WriteLine(char value)
		{
#warning Need to remove this filter
			if (value != '>')
				this.Write("{0}", value);
		}

		public override void WriteLine(char[] buffer)
		{
			this.Write(buffer);
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			//base.WriteLine(buffer, index, count);
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

		public override void WriteLine(string format, params object[] arg)
		{
			this.Write(format, arg);
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
			// this calls WriteStyleAttribute(string name, string value, bool fEncode)
			//base.WriteStyleAttribute(name, value);
			this.builder.AddStyle(name, value);
		}

		public override void WriteStyleAttribute(string name, string value, bool fEncode)
		{
			//base.WriteStyleAttribute(name, value, fEncode);
			this.builder.AddStyle(name, value);
		}

		#endregion Methods

		#region Callback

		private void ParseLiteral(string value)
		{
			// Need to check for and parse for literal HTML here :(

			// Handles this:
			// <li><a class="AsyncLink" href="
			// <%=this.ResolveUrl("~/FAQ/") %>
			// ">FAQ</a></li>

			int start = 0;
			int end = value.IndexOf('<');
			while (end >= 0)
			{
				if (end-start > 0)
				{
					this.builder.AddLiteral(value.Substring(start, end-start));
				}

				// check if wasn't part of a tag
				start = value.IndexOf('>', end);
				if (start < 0)
				{
					start = end;
					break;
				}
				else
				{
					start++;
				}

				Match match = Regex_Tag.Match(value, end, start-end);
				if (match.Success)
				{
					string tagName = match.Groups["tagname"].Value;

					// open tag
					this.WriteBeginTag(tagName);

					// write out attributes
					int attribCount = Math.Min(
						match.Groups["attrname"].Captures.Count,
						match.Groups["attrval"].Captures.Count);
					for (int i=0; i<attribCount; i++)
					{
						this.WriteAttribute(
							match.Groups["attrname"].Captures[i].Value,
							match.Groups["attrval"].Captures[i].Value);
					}

					if (!String.IsNullOrEmpty(match.Groups["empty"].Value))
					{
						// empty tag so immediately close it
						this.WriteEndTag(tagName);
					}
				}
				else
				{
					match = Regex_EndTag.Match(value, end, start-end);
					if (match.Success)
					{
						// found a closing tag
						this.WriteEndTag(match.Groups["tagname"].Value);
					}
					else
					{
						match = Regex_Comment.Match(value, end, start-end);
						if (match.Success)
						{
							// was a comment or DOCTYPE, just exclude
						}
						else
						{
							// wasn't part of a valid tag, but output as text
							this.builder.AddLiteral(value.Substring(end, start-end));
						}
					}
				}

				end = value.IndexOf('<', start);
			}

			this.builder.AddLiteral(value.Substring(start));
		}

		#endregion Callback
	}
}
