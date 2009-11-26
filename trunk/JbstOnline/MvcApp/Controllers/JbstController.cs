using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;

using JbstOnline.Models;
using JsonFx.BuildTools;
using JsonFx.Handlers;
using JsonFx.UI.Jbst;

namespace JbstOnline.Controllers
{
	public class JbstController : AppControllerBase
    {
		public ActionResult Test(CompilationResult result)
		{
			string message = result != null ? "Awesome." : "Dang.";
			HttpStatusCode status = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

			return this.DataResult(message, status);
		}

		public ActionResult Compile()
        {
			// TODO: use ModelBinder or Ninject to get request data
			HttpRequest request = HttpContext.Current.Request;
			JbstCompiler compiler = new JbstCompiler();

			List<ParseException> compilationErrors = new List<ParseException>();
			List<ParseException> compactionErrors = new List<ParseException>();
			IOptimizedResult result;
			using (var reader = new StreamReader(request.InputStream))
			{
				using (var writer = new StringWriter())
				{
					result = compiler.Compile(reader.ReadToEnd(), null, compilationErrors, compactionErrors);
				}
			}

			HttpStatusCode statusCode = HttpStatusCode.OK;
			object data;
			if (compilationErrors.Count > 0)
			{
				statusCode = HttpStatusCode.BadRequest;
				List<object> foo = new List<object>(compilationErrors.Count);
				foreach (ParseException ex in compilationErrors)
				{
					foo.Add(new
					{
						Message = ex.Message,
						Line = ex.Line,
						Col = ex.Column
					});
				}
				data = new
				{
					key = result.Hash+result.FileExtension,
					source = result.Source,
					errors = foo
				};
			}
			else if (compactionErrors.Count > 0)
			{
				statusCode = HttpStatusCode.BadRequest;
				List<CompilationError> foo = new List<CompilationError>(compactionErrors.Count);
				foreach (ParseException ex in compactionErrors)
				{
					foo.Add(new CompilationError
					{
						Message = ex.Message,
						Line = ex.Line,
						Col = ex.Column
					});
				}
				data = new CompilationResult
				{
					key = result.Hash+result.FileExtension,
					source = result.PrettyPrinted,
					errors = foo
				};
			}
			else
			{
				data = new CompilationResult
				{
					key = result.Hash+result.FileExtension,
					pretty = result.PrettyPrinted,
					compacted = result.Compacted
				};
			}

			return this.DataResult(data, statusCode);
        }

		#region Scripts

		public ActionResult SupportScripts()
		{
			IOptimizedResult result = (IOptimizedResult)BuildManager.CreateInstanceFromVirtualPath("~/Scripts/JBST.Merge", typeof(IOptimizedResult));

			return new JavaScriptResult()
			{
				Script = result.PrettyPrinted
			};
		}

		public ActionResult ScriptsCompacted()
		{
			IOptimizedResult result = (IOptimizedResult)BuildManager.CreateInstanceFromVirtualPath("~/Scripts/JBST.Merge", typeof(IOptimizedResult));

			return new JavaScriptResult()
			{
				Script = result.Compacted
			};
		}

		#endregion Scripts
	}
}
