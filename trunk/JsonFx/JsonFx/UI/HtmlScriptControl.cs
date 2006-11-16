using System;
using System.Web.UI;

namespace JsonFx.UI
{
	[ParseChildren(false), PersistChildren(true)]
	[ToolboxData("<{0}:ClientScriptBlock runat=\"server\">\n\t\n<{0}:ClientScriptBlock>")]
	public class ClientScriptBlock : System.Web.UI.WebControls.WebControl
	{
		#region Init

		public ClientScriptBlock() : base("script")
		{
		}

		#endregion Init

		#region Methods

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
		}

		#endregion Methods
	}
}
