using System;
using System.Text;
using System.Web;
using System.Web.UI;

using JsonFx.Serialization;

namespace JsonFx.UI
{
	[ToolboxData("<{0}:HistoryManager runat=\"server\" />")]
	public class HistoryManager : PseudoCode.Web.Controls.InlineFrame
	{
		#region Field

		private object startState = null;

		#endregion Field

		#region Properties

		/// <summary>
		/// Gets and sets the initial state object which represents this page request
		/// </summary>
		public object StartState
		{
			get { return this.startState; }
			set { this.startState = value; }
		}

		/// <summary>
		/// Gets and sets the URL of the history echo handler
		/// </summary>
		public override string SourceUrl
		{
			get { return base.SourceUrl; }
			set
			{
				if (value != null)
				{
					int query = value.IndexOf('?');
					if (query >= 0)
					{
						value = value.Substring(0, query);
					}
				}
				base.SourceUrl = value;
			}
		}

		#endregion Properties

		#region Page Events

		protected override void Render(HtmlTextWriter writer)
		{
			// preserve the original history url
			string sourceUrl = base.SourceUrl;

			try
			{
				if (this.StartState != null)
				{
					// serialize the start state onto URL
					StringBuilder builder = new StringBuilder();
					using (JsonWriter jsonWriter = new JsonWriter(builder))
					{
						jsonWriter.Write(this.StartState);
					}
					base.SourceUrl += "?"+HttpUtility.UrlEncode(builder.ToString());
				}

				base.Render(writer);
			}
			finally
			{
				// restore the original history url
				base.SourceUrl = sourceUrl;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);

			writer.AddAttribute("onload", "JsonFx.UI.History.changed(this)");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
		}

		#endregion Page Events
	}
}
