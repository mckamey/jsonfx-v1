using System;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;

using JsonFx.Handlers;

namespace JbstOnline.Controllers
{
	[HandleError]
	public class ExampleController : AppControllerBase
	{
		public ActionResult Index()
		{
			return new ViewResult();
		}
	}
}
