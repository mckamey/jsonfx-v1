using System;
using System.Web.UI;

namespace JsonFx.UI
{
	[ToolboxData("<{0}:HistoryManager runat=\"server\" />")]
	public class HistoryManager : PseudoCode.Web.Controls.InlineFrame
	{
		#region Page Events

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);

			writer.AddAttribute("onload", "JsonFx.UI.History.changed(this)");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
		}

		#endregion Page Events
	}
}
