using System;

namespace JsonFx.WapTemplate
{
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.PageData["Example.myData.renderTime"] = DateTime.Now;
			this.PageData["Example.myData.serverName"] = this.Server.MachineName;
			this.PageData["Example.myData.JsonFxVersion"] = JsonFx.About.Fx.Version;

			if (!this.Context.IsDebuggingEnabled)
			{
				// improve the Yslow rating
				JsonFx.Handlers.ResourceHandler.EnableStreamCompression(this.Context);
			}
		}
	}
}
