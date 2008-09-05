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
using System.Collections;
using System.Web;
using System.Net.Mime;

namespace JsonFx.UI
{
	/// <summary>
	/// Manually augments Browser object to produce strongly-typed HttpBrowserCapabilities
	/// </summary>
	/// <remarks>
	/// Originally JsonFx used ASP.NET .browser files to perform this, but
	/// inconsistencies and tighter control make this a preferred solution.
	/// The user only needs to add a web.config setting, instead of content files too.
	/// </remarks>
	public class JsonFxBrowserCapabilities : HttpBrowserCapabilities
	{
		#region Fields

		private bool haveSupportsJSON = false;
		private bool supportsJSON = false;
		private bool haveSupportsJsonML = false;
		private bool supportsJsonML = false;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="browser"></param>
		protected internal JsonFxBrowserCapabilities(HttpBrowserCapabilities browser)
		{
			// clone current caps so as to not taint HttpBrowserCapabilities cached
			this.Capabilities = new Hashtable(browser.Capabilities, StringComparer.OrdinalIgnoreCase);

			// clone adapters
			foreach (object key in browser.Adapters.Keys)
			{
				this.Adapters[key] = browser.Adapters[key];
			}

			// clone browser list
			foreach (string b in browser.Browsers)
			{
				this.AddBrowser(b);
			}

			this.Init();
		}

		#endregion Init

		#region Properties

		/// <summary>Gets a value indicating whether the browser supports JSON.</summary>
		/// <returns>true if the browser supports JSON; otherwise, false. The default is false.</returns>
		public bool SupportsJSON
		{
			get
			{
				if (!this.haveSupportsJSON)
				{
					this.supportsJSON = this.ParseBoolean("supportsJSON", false);
					this.haveSupportsJSON = true;
				}
				return this.supportsJSON;
			}
		}

		/// <summary>Gets a value indicating whether the browser supports JSON.</summary>
		/// <returns>true if the browser supports JSON; otherwise, false. The default is false.</returns>
		public bool SupportsJsonML
		{
			get
			{
				if (!this.haveSupportsJsonML)
				{
					this.supportsJsonML = this.ParseBoolean("supportsJsonML", false);
					this.haveSupportsJsonML = true;
				}
				return this.supportsJsonML;
			}
		}

		#endregion Properties

		#region Static Methods

		/// <summary>
		/// Determines if JsonFx is asynchronously making the request
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		protected internal static bool IsJsonFxRequest(HttpRequest request)
		{
			if (request == null)
			{
				return false;
			}

			string accept = request.Headers["Accept"];
			return !String.IsNullOrEmpty(accept) &&
				(accept.IndexOf(JsonMLTextWriter.JsonMLMimeType, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		#endregion Static Methods

		#region Methods

		protected override void Init()
		{
			base.Init();

			// Disable Browser "OptimizedCacheKey" for this request so
			// this UserAgent doesn't become permanently associated with JsonFx
			this.DisableOptimizedCacheKey();

			// setup response characteristics for JsonML/JSON-RPC
			this.Capabilities["tagWriter"] = this.HtmlTextWriter = typeof(JsonMLTextWriter).AssemblyQualifiedName;
			this.Capabilities["supportsJSON"] = "true";
			this.Capabilities["supportsJsonML"] = "true";
			this.Capabilities["preferredRequestEncoding"] = "UTF-8";
			this.Capabilities["preferredResponseEncoding"] = "UTF-8";
			this.AddBrowser("JsonFx");

			// this is a specific fix for Opera 8.x
			// Opera 8 requires "text/plain" or "text/html"
			// otherwise the content encoding is mangled
			bool isOpera8 = this.IsBrowser("opera") && (this.MajorVersion <= 8);

			this.Capabilities["preferredRenderingMime"] =
				(isOpera8) ?
				MediaTypeNames.Text.Plain :
				JsonFx.Json.JsonWriter.JsonMimeType;
		}

		/// <summary>
		/// Parses a capability and attempts to convert to boolean.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private bool ParseBoolean(string key, bool defaultValue)
		{
			bool value;
			if (Boolean.TryParse(this[key], out value))
			{
				return value;
			}
			return defaultValue;
		}

		#endregion Methods
	}
}
