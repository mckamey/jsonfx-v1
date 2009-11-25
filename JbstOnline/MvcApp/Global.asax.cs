//#define GZIP

using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

using Ninject;
using Ninject.Web.Mvc;
using JbstOnline.Mvc.IoC;

namespace JbstOnline
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : NinjectHttpApplication
	{
		protected virtual void RegisterRoutes(RouteCollection routes)
		{
			routes.MapRoute(
				"Compile",
				"compiler",
				new { controller = "Jbst", action = "Compile" }
			);

			routes.MapRoute(
				"PrettyPrinted",
				"compiler/scripts",
				new { controller = "Jbst", action = "SupportScripts" }
			);

			routes.MapRoute(
				"Compacted",
				"compiler/compacted",
				new { controller = "Jbst", action = "ScriptsCompacted" }
			);

			routes.MapRoute(
				"Default",
				"{controller}",
				new { controller = "Home", action = "Index", id = "" }
			);
		}

		#region Ninject

		protected override IKernel CreateKernel()
		{
			return new StandardKernel(new AppIocModule());
		}

		protected override void OnApplicationStarted()
		{
			this.RegisterAllControllersIn(Assembly.GetExecutingAssembly());
			this.RegisterRoutes(RouteTable.Routes);

			MvcHandler.DisableMvcResponseHeader = true;
		}

		#endregion Ninject

		#region Stream Compression

#if GZIP
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
#endif
		#endregion Stream Compression
	}
}