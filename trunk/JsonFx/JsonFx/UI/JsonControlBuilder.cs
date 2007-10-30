using System;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;

using BuildTools.HtmlDistiller;
using BuildTools.HtmlDistiller.Filters;

namespace JsonFx.UI
{
	public class JsonControlBuilder : IDisposable, IHtmlFilter
	{
		#region Constants

		private static readonly Regex RegexWhitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);

		#endregion Constants

		#region Fields

		TextWriter writer;
		JsonControlCollection controls = new JsonControlCollection(null);
		JsonControl current = null;
		JsonControl next = null;
		bool allowLiteralsInRoot = false;
		private bool dirty = false;
		private bool disposed = false;

		private bool parsingHtml = false;
		private readonly HtmlDistiller parser = new HtmlDistiller();

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writer"></param>
		public JsonControlBuilder(TextWriter writer)
		{
			this.writer = writer;

			this.parser.EncodeNonAscii = false;
			this.parser.BalanceTags = false;
			this.parser.NormalizeWhitespace = false;
			this.parser.HtmlFilter = this;
			this.parser.BeginIncrementalParsing();
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

		#endregion Properties

		#region Methods

		public void AddLiteral(string text)
		{
			if (this.parsingHtml)
			{
				return;
			}

			try
			{
				this.parsingHtml = true;

				this.parser.Parse(text);
			}
			finally
			{
				this.parsingHtml = false;
			}
		}

		private void Flush()
		{
			if (this.parsingHtml)
			{
				return;
			}

			try
			{
				this.parsingHtml = true;

				// flush remaining
				this.parser.EndIncrementalParsing();

				// reset for additional
				this.parser.BeginIncrementalParsing();
			}
			finally
			{
				this.parsingHtml = false;
			}
		}

		protected void OutputLiteral(string text)
		{
			if (String.IsNullOrEmpty(text))
			{
				return;
			}

			// this allows HTML entities to be encoded as unicode
			text = HttpUtility.HtmlDecode(text);

			// normalize whitespace
			text = RegexWhitespace.Replace(text, " ");

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

		public void PushTag(string tagName)
		{
			// flush any accumulated literals
			this.Flush();

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
			this.Flush();

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
			{
				this.RenderControls();
			}
		}

		public void StoreAttribute(string name, string value)
		{
			if (this.next == null)
			{
				this.next = new JsonControl();
			}

			this.SetAttribute(this.next, name, value);
		}

		public void AddAttribute(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetAttribute(this.current, name, value);
		}

		public void SetAttribute(JsonControl target, string name, string value)
		{
			// flush any accumulated literals
			this.Flush();

			value = HttpUtility.HtmlDecode(value);
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
			{
				this.next = new JsonControl();
			}

			this.SetStyle(this.next, name, value);
		}

		public void AddStyle(string name, string value)
		{
			if (this.current == null)
			{
				throw new InvalidOperationException("Unexpected attribute");
			}

			this.SetStyle(this.current, name, value);
		}

		public void SetStyle(JsonControl target, string name, string value)
		{
			// flush any accumulated literals
			this.Flush();

			if (target == null)
			{
				throw new NullReferenceException("target is null");
			}

			if (target.Attributes["style"] != null)
			{
				target.Attributes["style"] = target.Attributes["style"]+";"+value;
			}
			else
			{
				target.Attributes["style"] = value;
			}
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

		#region IHtmlFilter Members

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public bool FilterTag(HtmlTag tag)
		{
			switch (tag.TagType)
			{
				case HtmlTagType.FullTag:
				case HtmlTagType.BeginTag:
				{
					this.PushTag(tag.TagName);

					if (tag.HasAttributes)
					{
						foreach (string key in tag.Attributes.Keys)
						{
							this.AddAttribute(key, tag.Attributes[key]);
						}
					}

					if (tag.HasStyles)
					{
						foreach (string key in tag.Styles.Keys)
						{
							this.AddStyle(key, tag.Styles[key]);
						}
					}

					if (tag.TagType == HtmlTagType.FullTag)
					{
						this.PopTag();
					}
					break;
				}
				case HtmlTagType.EndTag:
				{
					this.PopTag();
					break;
				}
				case HtmlTagType.Comment:
				default:
				{
					break;
				}
			}

			// suppress writing of HtmlDistiller output
			return false;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool FilterAttribute(string tag, string attribute, ref string value)
		{
			// suppress writing of HtmlDistiller output
			return false;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="style"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool FilterStyle(string tag, string style, ref string value)
		{
			// suppress writing of HtmlDistiller output
			return false;
		}

		/// <summary>
		/// Callback from literal parser
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public bool FilterLiteral(string source, int start, int end, out string replacement)
		{
			this.OutputLiteral(source.Substring(start, end-start));

			// suppress writing of HtmlDistiller output
			replacement = null;
			return true;
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
						this.Flush();

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
