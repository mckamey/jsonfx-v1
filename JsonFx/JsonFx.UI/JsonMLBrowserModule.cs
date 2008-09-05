#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Web;

namespace JsonFx.UI
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
