using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using JsonFx.Serialization;

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
		
		#region Ctor

		/// <summary>
		/// Ctor.  Uses iframe as tag name.
		/// </summary>
		public HistoryManager() : base(System.Web.UI.HtmlTextWriterTag.Iframe) { }

		#endregion Ctor

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

			string url = this.HistoryUrl;

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
				url += "?"+HttpUtility.UrlEncode(builder.ToString());
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveUrl(url));

			writer.AddAttribute("onload", "JsonFx.UI.History.changed(this)");

			if (!this.IsDebugMode)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
			}
		}

		#endregion Page Events
	}
}
