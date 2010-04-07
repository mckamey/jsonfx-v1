using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;

using JbstOnline.Models;
using JsonFx.BuildTools;
using JsonFx.Handlers;
using JsonFx.UI.Jbst;

namespace JbstOnline.Controllers
{
	public class JbstController : AppControllerBase
	{
		#region Constants

		public const string SupportScriptPath = "~/Scripts/JBST.Merge";

		#endregion Constants

		#region Controller Actions

		public ActionResult Compile(TextReader source)
        {
			List<ParseException> compilationErrors = new List<ParseException>();
			List<ParseException> compactionErrors = new List<ParseException>();

			IOptimizedResult result = new JbstCompiler().Compile(source, null, compilationErrors, compactionErrors);

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
					key = result.Hash,
					source = result.PrettyPrinted,
					errors = foo
				};
			}
			else
			{
				data = new CompilationResult
				{
					key = result.Hash,
					pretty = result.PrettyPrinted,
					compacted = result.Compacted
				};
			}

			return this.DataResult(data, statusCode);
        }

#if DEBUG
		public ActionResult Test(CompilationResult result)
		{
			return this.DataResult(result);
		}
#endif

		public ActionResult SupportScripts()
		{
			IOptimizedResult result = ResourceHandler.Create<IOptimizedResult>(JbstController.SupportScriptPath);

			return new JavaScriptResult()
			{
				Script = result.PrettyPrinted
			};
		}

		public ActionResult ScriptsCompacted()
		{
			IOptimizedResult result = ResourceHandler.Create<IOptimizedResult>(JbstController.SupportScriptPath);

			return new JavaScriptResult()
			{
				Script = result.Compacted
			};
		}

		#endregion Controller Actions

	}
}
