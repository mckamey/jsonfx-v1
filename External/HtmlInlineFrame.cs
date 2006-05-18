using System;
using System.ComponentModel;

namespace PseudoCode.Web.UI.WebControls
{
	/// <summary>
	/// Represents an inline frame element.
	/// http://www.w3.org/TR/REC-html40/present/frames.html#h-16.5
	/// </summary>
	public class HtmlInlineFrame : System.Web.UI.WebControls.WebControl
	{
		#region Fields

		private const string DefaultAltText = "[ Your browser does not support IFrames. ]";
		System.Web.UI.HtmlControls.HtmlAnchor altLink;

		#endregion Fields

		#region Ctor

		/// <summary>
		/// CTor.  Uses iframe as tag name.
		/// </summary>
		public HtmlInlineFrame() : base(System.Web.UI.HtmlTextWriterTag.Iframe) { }

		#endregion Ctor

		#region Page Events

		/// <summary>
		/// Sets up the controls for rendering.
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls ();

			this.altLink = new System.Web.UI.HtmlControls.HtmlAnchor();
			this.altLink.InnerText = this.AltText;
			this.Controls.Add(this.altLink);
		}

		#endregion Page Events

		#region Properties

		/// <summary>
		/// This is the URL to be the displayed content.
		/// </summary>
		[Browsable(true)]
		[DefaultValue("")]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the source Url.")]
		public string SourceUrl
		{
			get { return this.Attributes["src"]; }
			set
			{
				this.EnsureChildControls();
				this.Attributes["src"] = this.altLink.HRef = WebHelper.ResolveAppRelative(value);
			}
		}

		/// <summary>
		/// Text shown if the browser does not support IFrames.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(HtmlInlineFrame.DefaultAltText)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the text to be displayed if browser doesn't support the iframe tag.")]
		public string AltText
		{
			get { return System.Web.HttpUtility.HtmlDecode(this.altLink.InnerText); }
			set
			{
				this.EnsureChildControls();
				this.altLink.InnerText = System.Web.HttpUtility.HtmlEncode(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[Browsable(true)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the height of the frame.")]
		public override System.Web.UI.WebControls.Unit Height
		{
			get
			{
				try { return new System.Web.UI.WebControls.Unit(this.Attributes["height"]); }
				catch { return System.Web.UI.WebControls.Unit.Empty; }
			}
			set { this.Attributes["height"] = value.ToString(); }
		}

		/// <summary>
		/// 
		/// </summary>
		[Browsable(true)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the width of the frame.")]
		public override System.Web.UI.WebControls.Unit Width
		{
			get
			{
				try { return new System.Web.UI.WebControls.Unit(this.Attributes["width"]); }
				catch { return System.Web.UI.WebControls.Unit.Empty; }
			}
			set { this.Attributes["width"] = value.ToString(); }
		}

		/// <summary>
		/// Show/hide a border around the content.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(true)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the state of frame border.")]
		public bool FrameBorders
		{
			get
			{
				try { return Convert.ToBoolean(this.Attributes["frameborder"]); }
				catch { return true; }
			}
			set { this.Attributes["frameborder"] = Convert.ToInt32(value).ToString(); }
		}

		/// <summary>
		/// Visibility of the scroll bars.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(InlineFrameScrollingType.Auto)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the state of frame scrollbar.")]
		public InlineFrameScrollingType Scrolling
		{
			get
			{
				try { return (InlineFrameScrollingType)Enum.Parse(typeof(InlineFrameScrollingType), this.Attributes["scrolling"], true); }
				catch { return InlineFrameScrollingType.Auto; }
			}
			set { this.Attributes["scrolling"] = value.ToString(); }
		}

		/// <summary>
		/// Default alignment of content within the IFrame.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(null)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the iframe alignment.")]
		public string Align
		{
			get { return this.Attributes["align"]; }
			set { this.Attributes["align"] = value; }
		}

		/// <summary>
		/// Margin around the content.
		/// </summary>
		[Browsable(true)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the margin height in pixels.")]
		public int MarginHeight
		{
			get
			{
				try { return Convert.ToInt32(this.Attributes["marginheight"]); }
				catch { return 0; }
			}
			set { this.Attributes["marginheight"] = value.ToString(); }
		}

		/// <summary>
		/// Margin around the content.
		/// </summary>
		[Browsable(true)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the margin width in pixels.")]
		public int MarginWidth
		{
			get
			{
				try { return Convert.ToInt32(this.Attributes["marginwidth"]); }
				catch { return 0; }
			}
			set { this.Attributes["marginwidth"] = value.ToString(); }
		}

		/// <summary>
		/// Sets and gets the AllowTransparency property.
		/// </summary>
		/// <remarks>Requires IE 5.5 or later.  Need to set the body color on the target document to transparent: <code>&gt;body style="background-color:transparent;"&lt;</code></remarks>
		[Browsable(true)]
		[DefaultValue(true)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the transparency of the frame.")]
		public bool AllowTransparency
		{
			get { return (this.Attributes["allowtransparency"] == true.ToString().ToLower()); }
			set { this.Attributes["allowtransparency"] = value.ToString().ToLower(); }
		}

		/// <summary>
		/// Sets and gets the value of the z-index style property.
		/// </summary>
		/// <remarks>Requires IE 5.5 or later.</remarks>
		[Browsable(true)]
		[DefaultValue(true)]
		[Category(" HtmlInlineFrame")]
		[Description("Determines the z-index of the frame.")]
		public int ZIndex
		{
			get
			{
				try { return Convert.ToInt32(this.Style["z-index"]); }
				catch { return 0; }
			}
			set
			{
				if (value == 0)
					this.Style["z-index"] = "auto";
				else
					this.Style["z-index"] = value.ToString();
			}
		}

		#endregion Properties
	}

	#region InlineFrameScrollingType

	/// <summary>
	/// The three types of Scrolling options for an iframe
	/// </summary>
	public enum InlineFrameScrollingType
	{
		/// <summary>
		/// Scrollbars hidden. Content is clipped.
		/// </summary>
		No,

		/// <summary>
		/// Scrollbars always shown.
		/// </summary>
		Yes,

		/// <summary>
		/// Scrollbars are shown if needed.
		/// </summary>
		Auto
	}

	#endregion InlineFrameScrollingType
}
