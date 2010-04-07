using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

namespace MyApp
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default",                                              // Route name
				"{controller}/{action}/{id}",                           // URL with parameters
				new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
			);
		}

		protected void Application_Start()
		{
			RegisterRoutes(RouteTable.Routes);
		}

		#region Stream Compression

		protected void Application_PreRequestHandlerExecute(object sender, System.EventArgs e)
		{
			Page page = this.Context.Handler as Page;
			if (page != null)
			{
				page.PreRenderComplete += new EventHandler(this.Page_PreRenderComplete);
			}
		}

		private void Page_PreRenderComplete(object sender, EventArgs e)
		{
			this.Error += new EventHandler(this.Page_Error);

			// improve the Yslow rating by compressing output
			JsonFx.Handlers.ResourceHandler.EnableStreamCompression(this.Context);
		}

		private void Page_Error(object sender, System.EventArgs e)
		{
			// remove compression in error conditions
			JsonFx.Handlers.ResourceHandler.DisableStreamCompression(this.Context);
		}

		#endregion Stream Compression
	}
}