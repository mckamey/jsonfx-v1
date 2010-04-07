using System;
using System.Web.Mvc;

namespace MyApp.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			// populate data to be used directly in a JBST
			this.ViewData["renderTime"] = DateTime.Now;
			this.ViewData["serverName"] = this.Server.MachineName;
			this.ViewData["JsonFxVersion"] = JsonFx.About.Fx.Version;

			return View();
		}
	}
}
