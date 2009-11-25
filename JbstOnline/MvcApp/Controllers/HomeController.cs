using System;
using System.Web;
using System.Web.Mvc;

namespace JbstOnline.Controllers
{
	[HandleError]
	public class HomeController : AppControllerBase
	{
		public ActionResult Index()
		{
			// TODO: use ModelBinder or DI to map path

			// populate data to be used directly in a JBST
			this.ViewData["jbst-source"] = HttpUtility.HtmlEncode(
				System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Sample.txt")));

			return new ViewResult
			{
				ViewData = this.ViewData
			};
		}
	}
}
