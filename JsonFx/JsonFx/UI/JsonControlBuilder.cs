using System;
using System.Web;
using System.Text.RegularExpressions;

using BuildTools.HtmlDistiller;
using BuildTools.HtmlDistiller.Filters;

namespace JsonFx.UI
{
	public class JsonControlBuilder : IDisposable, IHtmlFilter
	{
		#region Constants

		private const string Pattern_Comment = @"(<!DOCTYPE[^>]*>)|(<!--(.)*?-->)";
		private const string Pattern_Tag = "\\G<(?<tagname>[\\w:\\.]+)(\\s+(?<attrname>\\w[-\\w:]*)(\\s*=\\s*\"(?<attrval>[^\"]*)\"|\\s*=\\s*'(?<attrval>[^']*)'|\\s*=\\s*(?<attrval><%#.*?%>)|\\s*=\\s*(?<attrval>[^\\s=/>]*)|(?<attrval>\\s*?)))*\\s*(?<empty>/)?>";
		private const string Pattern_EndTag = @"\G</(?<tagname>[\w:\.]+)\s*>";

		private static readonly Regex Regex_Tag = new Regex(Pattern_Tag, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.Compiled);
		private static readonly Regex Regex_EndTag = new Regex(Pattern_EndTag, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.Compiled);
		private static readonly Regex Regex_Comment = new Regex(Pattern_Comment, RegexOptions.Singleline|RegexOptions.Multiline|RegexOptions.IgnoreCase);
		private static readonly Regex RegexWhitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);

		#endregion Constants

		#region Fields

		System.IO.TextWriter writer;
		JsonControlCollection controls = new JsonControlCollection(null);
		JsonControl current = null;
		JsonControl next = null;
		bool allowLiteralsInRoot = false;
		bool normalizeWhitespace = true;
		private bool dirty = false;
		private bool disposed = false;

		private string cachedLiteral = null;
		private bool processingLiteral = false;

		//private readonly HtmlDistiller parser = new HtmlDistiller(true);

		#endregion Fields

		#region Init

		public JsonControlBuilder(System.IO.TextWriter writer)
		{
			this.writer = writer;

			//this.parser.EncodeNonAscii = false;
			//this.parser.HtmlFilter = this;
		}

		#endregion Init

		#region Properties

		public JsonControlCollection Controls
		{
			get { return this.controls; }
		}

		public bool AllowLiteralsInRoot
		{
			get { return this.allowLiteralsInRoot; }
			set { this.allowLiteralsInRoot = value; }
		}

		public bool NormalizeWhitespace
		{
			get { return this.normalizeWhitespace; }
			set { this.normalizeWhitespace = value; }
		}

		#endregion Properties

		#region Methods

		private void ProcessLiteral()
		{
			if (this.processingLiteral)
			{
				return;
			}

			try
			{
				this.processingLiteral = true;

				string cached = this.cachedLiteral;
				this.cachedLiteral = null;
				if (!String.IsNullOrEmpty(cached))
				{
					// allow second-chance processing
					this.ParseLiteral(cached);
				}
			}
			finally
			{
				this.processingLiteral = false;
			}
		}

		private void FlushLiteralCache()
		{
			this.ProcessLiteral();

			// output anything remaining after second-chance processing
			string cached = this.cachedLiteral;
			this.cachedLiteral = null;
			if (!String.IsNullOrEmpty(cached))
			{
				this.OutputLiteral(cached);
			}
		}

		protected void OutputLiteral(string text)
		{
			if (String.IsNullOrEmpty(text))
			{
				return;
			}

			text = this.ScrubLiteral(text);

			if (this.current == null)
			{
				if (this.AllowLiteralsInRoot)
				{
					this.controls.Add(new JsonLiteral(text));
				}
			}
			else
			{
				this.current.ChildControls.Add(new JsonLiteral(text));
			}
		}

		/// <summary>
		/// Normalizes Whitespace, HtmlDecode content
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private string ScrubLiteral(string text)
		{
			if (text == null)
			{
				return null;
			}

			if (this.NormalizeWhitespace)
			{
				text = RegexWhitespace.Replace(text, " ");
			}

			// this allows HTML entities to be encoded as unicode
			text = System.Web.HttpUtility.HtmlDecode(text);
			return text;
		}

		public void AddLiteral(string text)
		{
			this.cachedLiteral += text;

			this.ProcessLiteral();
		}

		public void PushTag(string tagName)
		{
			// flush any accumulated literals
			this.FlushLiteralCache();

			JsonControl control = this.next;
			if (control == null)
			{
				control = new JsonControl(tagName);
			}
			else
			{
				control.TagName = tagName;
				this.next = null;
			}

			if (this.current == null)
			{
				this.controls.Add(control);
			}
			else
			{
				this.current.ChildControls.Add(control);
			}

			this.current = control;
		}

		public void PopTag()
		{
			// flush any accumulated literals
			this.FlushLiteralCache();

			if (this.next != null)
			{
				throw new InvalidOperationException("Pop mismatch? (Next is null)");
			}

			if (this.current == null)
			{
				throw new InvalidOperationException("Pop mismatch? (Current is null)");
			}

			this.current = this.current.Parent;

			if (this.current == null)
				this.RenderControls();
		}

		public void StoreAttribute(string name, string value)
		{
			if (this.next == null)
				this.next = new JsonControl();

			this.SetAttribute(this.next, name, value);
		}

		public void AddAttribute(string name, string value)
		{
			if (this.current == null)
				throw new InvalidOperationException("Unexpected attribute");

			this.SetAttribute(this.current, name, value);
		}

		public void SetAttribute(JsonControl target, string name, string value)
		{
			// flush any accumulated literals
			this.FlushLiteralCache();

			value = System.Web.HttpUtility.HtmlDecode(value);
			if ("style".Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				this.SetStyle(target, null, value);
			}
			else
			{
				target.Attributes[name] = value;
			}
		}

		public void StoreStyle(string name, string value)
		{
			if (this.next == null)
				this.next = new JsonControl();

			this.SetStyle(this.next, name, value);
		}

		public void AddStyle(string name, string value)
		{
			if (this.current == null)
				throw new InvalidOperationException("Unexpected attribute");

			this.SetStyle(this.current, name, value);
		}

		public void SetStyle(JsonControl target, string name, string value)
		{
			// flush any accumulated literals
			this.FlushLiteralCache();

			if (target == null)
				throw new NullReferenceException("target is null");

			if (target.Attributes["style"] != null)
			{
				target.Attributes["style"] = target.Attributes["style"]+";"+value;
			}
			else
			{
				target.Attributes["style"] = value;
			}

//            if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(name))
//            {
//                string[] rules = value.Split(';');
//                foreach (string rule in rules)
//                {
//                    if (String.IsNullOrEmpty(rule))
//                        continue;

//#warning parsing doesn't handle IE filters
//                    string[] parts = rule.Split(':');
//                    if (parts.Length != 2)
//                        throw new Exception("Bug in the Style parsing");

//                    parts[0] = parts[0].Trim();
//                    parts[1] = parts[1].Trim();
//                    if (String.IsNullOrEmpty(parts[0]) || String.IsNullOrEmpty(parts[1]))
//                        throw new Exception("Bug in the Style parsing");

//                    if (target.Style == null)
//                        throw new NullReferenceException("target.Style is null");

//                    parts[0] = this.ToJavaScriptStyle(parts[0]);

//                    target.Style[parts[0]] = parts[1];
//                }
//            }
//            else
//            {
//                target.Style[name] = value;
//            }
		}

		private string ToJavaScriptStyle(string property)
		{
			if ("float".Equals(property, StringComparison.OrdinalIgnoreCase))
				return "styleFloat";

			// break into words
			string[] parts = property.Split('-');

			if (parts.Length == 1)
				return property;

			for (int i=1; i<parts.Length; i++)
			{
				// make camel case
				parts[i] = Char.ToUpperInvariant(parts[i][0])+parts[i].Substring(1).ToLowerInvariant();
			}
			return String.Join("", parts);
		}

		#endregion Methods

		#region Methods

		private void RenderControls()
		{
			if (this.dirty)
			{
				// hacky but seems to work:
				// if this builder is dirty it means it has already written out a control graph
				// adding another back to back will not be valid JSON
				// for some reason Crockford's parseJSON method will treat multiple graphs
				// as if they are in an array if delimited by commas.
				this.writer.Write(",");
			}
			using (JsonFx.Json.JsonWriter jw = new JsonFx.Json.JsonWriter(this.writer))
			{
				if (this.Controls.Count > 1)
				{
					jw.Write(this.Controls);
				}
				else if (this.Controls.Count > 0)
				{
					jw.Write(this.Controls[0]);
				}
			}
			this.Controls.Clear();
			this.dirty = true;
		}

		#endregion Methods

		#region Parse Methods

		/// <summary>
		/// Converts literal HTML into actual tags/attributes/etc.
		/// </summary>
		/// <param name="value"></param>
		private void ParseLiteral(string value)
		{
			// Need to check for and parse for literal HTML here :(

			// Handles this:
			// <li><a class="AsyncLink" href="
			// <%=this.ResolveUrl("~/FAQ/") %>
			// ">FAQ</a></li>

#warning Need to remove this filter
			if (value == ">")
			{
				return;
			}

			// filter comments and DOCTYPEs
			value = Regex_Comment.Replace(value, "");

			// find until start of a tag
			int end = value.IndexOf('<');
			int start = 0;
			while (end >= 0)
			{
				if (end-start > 0)
				{
					this.AddLiteral(value.Substring(start, end-start));
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
					this.PushTag(tagName);

					// write out attributes
					int attribCount = Math.Min(
						match.Groups["attrname"].Captures.Count,
						match.Groups["attrval"].Captures.Count);
					for (int i=0; i<attribCount; i++)
					{
						this.AddAttribute(
							match.Groups["attrname"].Captures[i].Value,
							match.Groups["attrval"].Captures[i].Value);
					}

					if (!String.IsNullOrEmpty(match.Groups["empty"].Value))
					{
						// empty tag so immediately close it
						this.PopTag();
					}
				}
				else
				{
					match = Regex_EndTag.Match(value, end, start-end);
					if (match.Success)
					{
						// found a closing tag
						this.PopTag();
					}
					else
					{
						// wasn't part of a valid tag, but output as text
						this.AddLiteral(value.Substring(end, start-end));
					}
				}

				end = value.IndexOf('<', start);
			}

			// put rest back
			this.AddLiteral(value.Substring(start));
		}

		#endregion Parse Methods

		#region IHtmlFilter Members

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterTag(HtmlTag tag)
		{
			return true;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterAttribute(string tag, string attribute, ref string value)
		{
			return true;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="style"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterStyle(string tag, string style, ref string value)
		{
			return true;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		bool IHtmlFilter.FilterLiteral(string source, int start, int end, out string replacement)
		{
			replacement = null;
			return false;
		}

		#endregion IHtmlFilter Members

		#region IDisposable Members

		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				if (this.writer != null)
				{
					using (this.writer)
					{
						// flush any accumulated literals
						this.FlushLiteralCache();

						while (this.current != null)
						{
							this.PopTag();
						}

						this.writer.Flush();
						this.writer.Close();
						this.writer = null;
					}
				}
			}
		}

		#endregion IDisposable Members
	}
}
