using System;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;

using JsonFx.Handlers;

namespace JbstOnline.Controllers
{
	[HandleError]
	public class HomeController : AppControllerBase
	{
		public ActionResult Index()
		{
			IOptimizedResult sample = (IOptimizedResult)BuildManager.CreateInstanceFromVirtualPath("~/Sample.jbst", typeof(IOptimizedResult));

			// populate data to be used directly in a JBST
			this.ViewData["jbst-source"] = HttpUtility.HtmlEncode(sample.Source);

			return new ViewResult
			{
				ViewData = this.ViewData
			};
		}
	}
}
