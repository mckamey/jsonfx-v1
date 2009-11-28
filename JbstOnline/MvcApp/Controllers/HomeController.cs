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
			IOptimizedResult sample = (IOptimizedResult)BuildManager.CreateInstanceFromVirtualPath(
				"~/Views/Example/Foo.MyZebraList.jbst", typeof(IOptimizedResult));

			// populate data to be used directly in a JBST
			this.ViewData["jbst-source"] = HttpUtility.HtmlEncode(sample.Source).Trim();

			IOptimizedResult support = JbstController.GetSupportScripts();
			this.ViewData["compact-size"] = FormatFileSize(support.Compacted.Length);
			this.ViewData["full-size"] = FormatFileSize(support.PrettyPrinted.Length);
			this.ViewData["gzip-size"] = FormatFileSize(support.Gzipped.Length);
			this.ViewData["deflate-size"] = FormatFileSize(support.Deflated.Length);

			return new ViewResult
			{
				ViewData = this.ViewData
			};
		}

		private string FormatFileSize(int bytes)
		{
			const decimal BytesPerKilo = 1024m;
			const decimal BytesPerMega = 1024m * BytesPerKilo;
			const decimal BytesPerGiga = 1024m * BytesPerMega;
			const decimal BytesPerTera = 1024m * BytesPerGiga;

			if (bytes < BytesPerMega)
			{
				return (bytes / BytesPerKilo).ToString("0.0")+" KB";
			}

			if (bytes < BytesPerGiga)
			{
				return (bytes / BytesPerMega).ToString("0.0")+" MB";
			}

			if (bytes < BytesPerTera)
			{
				return (bytes / BytesPerGiga).ToString("0.0")+" GB";
			}

			return (bytes / BytesPerTera).ToString("0.0")+" TB";
		}
	}
}
