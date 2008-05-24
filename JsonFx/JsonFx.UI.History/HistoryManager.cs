#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

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
#endregion BuildTools License

using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using JsonFx.Json;

namespace JsonFx.UI
{
	[ToolboxData("<{0}:HistoryManager runat=\"server\" />")]
	public class HistoryManager : System.Web.UI.WebControls.WebControl
	{
		#region Constants

		private const string DefaultHistoryUrl = "/robots.txt";

		#endregion Constants

		#region Fields

		private object startState = null;
		private string historyUrl = null;
		private bool isDebugMode = false;

		#endregion Fields
		
		#region Init

		/// <summary>
		/// Ctor.  Uses iframe as tag name.
		/// </summary>
		public HistoryManager() : base(System.Web.UI.HtmlTextWriterTag.Iframe) { }

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the initial state object which represents this page request
		/// </summary>
		public object StartState
		{
			get { return this.startState; }
			set { this.startState = value; }
		}

		/// <summary>
		/// Gets and sets the URL used for storing information in the browser history.
		/// </summary>
		/// <remarks>
		/// When debugging history, it is often useful to point the HistoryUrl at a handler which
		/// simply echos back the query string. e.g. JsonFx.Handlers.EchoHandler
		/// </remarks>
		[Browsable(true)]
		[DefaultValue(DefaultHistoryUrl)]
		[Description("Gets and sets the URL used for storing information in the browser history.")]
		public virtual string HistoryUrl
		{
			get
			{
				if (String.IsNullOrEmpty(this.historyUrl))
				{
					return DefaultHistoryUrl;
				}
				return this.historyUrl;
			}
			set { this.historyUrl = value; }
		}

		/// <summary>
		/// Gets and sets a value which shows or hides the history iframe.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(false)]
		[Description("Gets and sets a value which shows or hides the history iframe.")]
		public bool IsDebugMode
		{
			get { return this.isDebugMode; }
			set { this.isDebugMode = value; }
		}

		#endregion Properties

		#region Page Events

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);

			string url = this.ResolveUrl(this.HistoryUrl);

			if (this.StartState != null)
			{
				int query = url.IndexOf('?');
				if (query >= 0)
				{
					url = url.Substring(0, query);
				}

				// serialize the start state onto URL query string
				StringBuilder builder = new StringBuilder();
				using (JsonWriter jsonWriter = new JsonWriter(builder))
				{
					jsonWriter.Write(this.StartState);
				}
				url += "?"+HttpUtility.UrlPathEncode(builder.ToString());
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveUrl(url));

			writer.AddAttribute("onload", "JsonFx.History.changed(this)");

			if (!this.IsDebugMode)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");

				// it is rhumored that display:none breaks some browsers but I haven't seen it
				// this could be used instead to not affect the layout
				//writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
				//writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
			}
		}

		#endregion Page Events
	}
}
