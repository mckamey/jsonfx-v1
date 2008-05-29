using System;
using System.Web;

namespace JsonFx.JsonML
{
	/// <summary>
	/// HttpModule which augments the HttpRequest.Browser object to signal
	/// Request is being made from asynchronous JsonFx call.
	/// </summary>
	public class JsonMLBrowserModule : IHttpModule
	{
		#region Constants

		private const string JsonFxHeader = "X-JsonFx-Version";

		#endregion Constants

		#region IHttpModule Members

		/// <summary>
		/// Dispose
		/// </summary>
		void IHttpModule.Dispose()
		{
		}

		/// <summary>
		/// Attaches event handlers
		/// </summary>
		/// <param name="application"></param>
		void IHttpModule.Init(HttpApplication application)
		{
			application.BeginRequest += new EventHandler(this.application_BeginRequest);
		}

		#endregion IHttpModule Members

		#region Application Events

		/// <summary>
		/// Checks for JsonFx asynchronous calls
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void application_BeginRequest(object sender, EventArgs e)
		{
			HttpApplication application = sender as HttpApplication;
			if (application == null || application.Request == null)
			{
				return;
			}

			if (JsonFxBrowserCapabilities.IsJsonFxRequest(application.Request))
			{
				application.Request.Browser = new JsonFxBrowserCapabilities(application.Request.Browser);
			}

			application.Response.AddHeader(JsonFxHeader, JsonFx.About.Version.ToString());
		}

		#endregion Application Events
	}
}
