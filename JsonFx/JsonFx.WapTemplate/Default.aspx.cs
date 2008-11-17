using System;

namespace JsonFx.WapTemplate
{
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.PageData["Example.renderTime"] = DateTime.Now;
			this.PageData["Example.serverName"] = this.Server.MachineName;

#if !DEBUG
			// improve the Yslow rating
			JsonFx.Handlers.CompiledBuildResult.EnableStreamCompression(this.Context);
#endif
		}
	}
}
