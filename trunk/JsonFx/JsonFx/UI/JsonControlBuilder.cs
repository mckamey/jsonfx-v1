using System;
using System.Text.RegularExpressions;

namespace JsonFx.UI
{
	public class JsonControlBuilder : IDisposable
	{
		#region Constants

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

		#endregion Fields

		#region Init

		public JsonControlBuilder(System.IO.TextWriter writer)
		{
			this.writer = writer;
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

		public void AddLiteral(string text)
		{
			if (text == null || text.Trim().Length == 0)
				return;

			if (this.NormalizeWhitespace)
			{
				text = RegexWhitespace.Replace(text, " ");
			}

			text = System.Web.HttpUtility.HtmlDecode(text);

			if (this.current == null)
			{
				if (this.AllowLiteralsInRoot)
				{
					this.AddOrAppendLiteral(this.controls, text);
				}
			}
			else
			{
				this.AddOrAppendLiteral(this.current.ChildControls, text);
			}
		}

		protected void AddOrAppendLiteral(JsonControlCollection controls, string text)
		{
			JsonLiteral literal = controls.Last as JsonLiteral;
			if (literal == null)
			{
				controls.Add(new JsonLiteral(text));
			}
			else
			{
				literal.Text += text;
			}
		}

		public void PushTag(string tagName)
		{
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
			if (this.next != null)
				throw new InvalidOperationException("Pop mismatch? (Next is null)");

			if (this.current == null)
				throw new InvalidOperationException("Pop mismatch? (Current is null)");

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
				return "floatStyle";

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
			using (JsonFx.Serialization.JsonWriter jw = new JsonFx.Serialization.JsonWriter(this.writer))
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
