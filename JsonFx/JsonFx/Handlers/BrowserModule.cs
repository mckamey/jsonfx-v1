using System;
using System.Web;

using JsonFx.UI;

namespace JsonFx.Handlers
{
	/// <summary>
	/// HttpModule which augments HttpRequest.Browser object to show
	/// Request is being made from asynchronous JsonFx call.
	/// </summary>
	public class BrowserModule : IHttpModule
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

			if (JsonFxBrowserCapabilities.IsJsonFx(application.Request))
			{
				application.Request.Browser = new JsonFxBrowserCapabilities(application.Request.Browser);
			}

			application.Response.AddHeader(JsonFxHeader, JsonFx.About.Version.ToString());
		}

		#endregion Application Events
	}
}
