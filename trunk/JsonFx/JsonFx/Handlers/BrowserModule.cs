using System;
using System.Collections;
using System.Web;

namespace JsonFx.Handlers
{
	/// <summary>
	/// HttpModule which augments HttpRequest.Browser object to show
	/// Request is being made from asynchronous JsonFx call.
	/// </summary>
	/// <remarks>
	/// Originally used ASP.NET .browser files to perform this, but
	/// inconsistencies and tighter control make this a preferred solution.
	/// The user only needing a web.config addition, instead of content files too.
	/// </remarks>
	public class BrowserModule : IHttpModule
	{
		#region IHttpModule Members

		void IHttpModule.Dispose()
		{
		}

		void IHttpModule.Init(HttpApplication application)
		{
			application.BeginRequest += new EventHandler(application_BeginRequest);
		}

		#endregion IHttpModule Members

		#region Application Events

		private void application_BeginRequest(object sender, EventArgs e)
		{
			HttpApplication application = sender as HttpApplication;
			if (application == null || application.Request == null)
			{
				return;
			}

			string accept = application.Request.Headers["Accept"];
			if (!String.IsNullOrEmpty(accept) &&
				accept.IndexOf("application/jsonml+json", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				HttpBrowserCapabilities browserCaps = application.Request.Browser;

				// clone current caps so as to not taint cached caps
				HttpBrowserCapabilities browser = new HttpBrowserCapabilities();
				browser.Capabilities = new Hashtable(browserCaps.Capabilities, StringComparer.OrdinalIgnoreCase);
				foreach (object key in browserCaps.Adapters.Keys)
				{
					browser.Adapters[key] = browserCaps.Adapters[key];
				}
				foreach (string b in browserCaps.Browsers)
				{
					browser.AddBrowser(b);
				}

				// Disable Browser "OptimizedCacheKey" for this request so
				// this UserAgent doesn't become permanently associated with JsonFx
				browser.DisableOptimizedCacheKey();

				// setup response characteristics for JsonML/JSON-RPC
				browser.Capabilities["tagWriter"] = browser.HtmlTextWriter = "JsonFx.UI.JsonMLTextWriter, JsonFx";
				browser.Capabilities["supportsJson"] = "true";
				browser.Capabilities["supportsJsonML"] = "true";
				browser.Capabilities["preferredRequestEncoding"] = "UTF-8";
				browser.Capabilities["preferredResponseEncoding"] = "UTF-8";
				browser.AddBrowser("JsonFx");

				// Opera 8 requires text/plain otherwise it mangles response
				bool isOpera8 = browser.IsBrowser("opera") && (browser.MajorVersion == 8);
				browser.Capabilities["preferredRenderingMime"] =
						(isOpera8) ? "text/plain" : "application/json";

				application.Request.Browser = browser;

				// don't cache JSON requests?
				//application.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			}
		}

		#endregion Application Events
	}
}
