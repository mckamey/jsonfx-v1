using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace JsonFx.Mvc
{
	/// <summary>
	/// A simple ActionResult for returning a custom HTTP status code
	/// </summary>
	public class HttpResult : ActionResult
	{
		#region Properties

		public virtual HttpStatusCode HttpStatus
		{
			get;
			set;
		}

		public virtual string ContentType
		{
			get;
			set;
		}

		public virtual string Message
		{
			get;
			set;
		}

		#endregion Properties

		#region ActionResult Members

		public override void ExecuteResult(ControllerContext context)
		{
			HttpResponseBase response = context.HttpContext.Response;

			try
			{
				response.ClearHeaders();
				response.ClearContent();
			}
			catch { }

			response.TrySkipIisCustomErrors = true;

			if (this.HttpStatus != default(HttpStatusCode))
			{
				response.StatusCode = (int)this.HttpStatus;
			}

			if (String.IsNullOrEmpty(this.ContentType))
			{
				response.ContentType = "text/plain";
			}
			else
			{
				response.ContentType = this.ContentType;
			}

			this.WriteMessage(response);
		}

		protected virtual void WriteMessage(HttpResponseBase response)
		{
			response.Write(response.Status);

			string message = this.Message;
			if (!String.IsNullOrEmpty(message))
			{
				response.Write(": ");
				response.Write(message);
			}
		}

		#endregion ActionResult Members
	}
}
