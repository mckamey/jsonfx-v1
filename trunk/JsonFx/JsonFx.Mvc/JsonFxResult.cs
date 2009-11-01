using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using JsonFx.Json;

namespace JsonFx.Mvc
{
	public class JsonFxResult : ActionResult
	{
		#region Properties

		public Encoding ContentEncoding
		{
			get;
			set;
		}

		public string ContentType
		{
			get;
			set;
		}

		public object Data
		{
			get;
			set;
		}

		#endregion Properties

		#region Methods

		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			HttpResponseBase response = context.HttpContext.Response;

			if (!String.IsNullOrEmpty(this.ContentType))
			{
				response.ContentType = this.ContentType;
			}
			else
			{
				response.ContentType = "application/json";
			}

			if (this.ContentEncoding != null)
			{
				response.ContentEncoding = this.ContentEncoding;
			}

			if (this.Data != null)
			{
				response.Write(JsonWriter.Serialize(this.Data));
			}
		}

		#endregion Methods
	}
}
