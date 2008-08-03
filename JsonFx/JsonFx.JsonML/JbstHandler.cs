using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Compilation;

using JsonFx.JsonML.BST;

namespace JsonFx.JsonML
{
	public class JbstHandler : JsonFx.Handlers.ResourceHandler
	{
		#region Properties

		protected override string ResourceContentType
		{
			get { return "text/plain"; }
		}

		protected override string ResourceExtension
		{
			get { return ".jbst"; }
		}

		#endregion Properties

		#region Methods

		protected override void Render(HttpContext context, bool isDebug, Stream input)
		{
			// jsonp(...); style
			string jsonp = context.Request.QueryString["jsonp"];

			if (!String.IsNullOrEmpty(jsonp))
			{
				context.Response.Output.Write(jsonp);
				context.Response.Output.Write("(");
			}

			// let JsonFx.Handlers.ResourceHandler serve the request
			base.Render(context, isDebug, input);

			if (!String.IsNullOrEmpty(jsonp))
			{
				context.Response.Output.Write(");");
			}
		}

		#endregion Methods
	}
}