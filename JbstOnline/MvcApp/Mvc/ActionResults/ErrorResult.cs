using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace JbstOnline.Mvc.ActionResults
{
	public class ErrorResult : ActionResult
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public ErrorResult(Exception ex)
		{
			this.Error = ex;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the associated error
		/// </summary>
		public Exception Error
		{
			get;
			protected set;
		}

		#endregion Properties

		#region ActionResult Members

		public override void ExecuteResult(ControllerContext context)
		{
			int statusCode;
			string message;

			HttpException ex = this.Error as HttpException;
			if (ex == null)
			{
				statusCode = (int)HttpStatusCode.InternalServerError;
			}
			else
			{
				statusCode = ex.GetHttpCode();
			}
#if DEBUG
			message = this.Error.ToString();
#else
			message = this.Error.Message;
#endif

			context.HttpContext.Response.StatusCode = statusCode;
			context.HttpContext.Response.ContentType = "text/plain";

			context.HttpContext.Response.Write(message);
		}

		#endregion ActionResult Members
	}
}
