using System;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace PseudoCode.Web.Controls
{
	/// <summary>
	/// Represents an inline frame element.
	/// http://www.w3.org/TR/xhtml-modularization/dtd_module_defs.html#a_module_Iframe
	/// http://www.w3.org/TR/REC-html40/present/frames.html#h-16.5
	/// </summary>
	public class InlineFrame : System.Web.UI.WebControls.WebControl
	{
		#region Fields

		private const string DefaultAltText = "Your browser does not support IFrames.";
		System.Web.UI.HtmlControls.HtmlAnchor altLink = new System.Web.UI.HtmlControls.HtmlAnchor();

		#endregion Fields

		#region Ctor

		/// <summary>
		/// Ctor.  Uses iframe as tag name.
		/// </summary>
		public InlineFrame() : base(System.Web.UI.HtmlTextWriterTag.Iframe) { }

		#endregion Ctor

		#region Page Events

		/// <summary>
		/// Sets up the controls for rendering.
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			this.AlternateText = InlineFrame.DefaultAltText;
			this.Controls.Add(this.altLink);
		}

		#endregion Page Events

		#region Properties

		/// <summary>
		/// This is the URL to be the displayed content.
		/// </summary>
		[Browsable(true)]
		[DefaultValue("")]
		[Category(" InlineFrame")]
		[Description("Gets and sets the source Url.")]
		public string SourceUrl
		{
			get { return this.Attributes["src"]; }
			set
			{
				this.EnsureChildControls();
				this.Attributes["src"] = this.altLink.HRef = this.ResolveUrl(value);
			}
		}

		/// <summary>
		/// This is the URL to be the displayed content.
		/// </summary>
		[Browsable(true)]
		[DefaultValue("")]
		[Category(" InlineFrame")]
		[Description("Gets and sets the log description Url.")]
		public string LongDescriptionUrl
		{
			get { return this.Attributes["longdesc"]; }
			set { this.Attributes["longdesc"] = this.ResolveUrl(value); }
		}

		/// <summary>
		/// Text shown if the browser does not support IFrames.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(InlineFrame.DefaultAltText)]
		[Category(" InlineFrame")]
		[Description("Gets and sets the text to be displayed if browser doesn't support the iframe tag.")]
		public string AlternateText
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
		[Category(" InlineFrame")]
		[Description("Gets and sets the height of the frame.")]
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
		[Category(" InlineFrame")]
		[Description("Gets and sets the width of the frame.")]
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
		[Category(" InlineFrame")]
		[Description("Gets and sets the state of frame border.")]
		public bool FrameBorder
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
		[Category(" InlineFrame")]
		[Description("Gets and sets the state of frame scrollbar.")]
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
		[Category(" InlineFrame")]
		[Description("Gets and sets the iframe alignment.")]
		public HorizontalAlign HorizontalAlign
		{
			get
			{
				try { return (HorizontalAlign)Enum.Parse(typeof(HorizontalAlign),this.Attributes["align"],true); }
				catch { return HorizontalAlign.NotSet; }
			}
			set
			{
				if (value == HorizontalAlign.NotSet ||
					value == HorizontalAlign.Justify)
				{
					this.Attributes.Remove("align");
					return;
				}

				this.Attributes["align"] = value.ToString("G");
			}
		}

		/// <summary>
		/// Margin around the content.
		/// </summary>
		[Browsable(true)]
		[Category(" InlineFrame")]
		[Description("Gets and sets the margin height in pixels.")]
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
		[Category(" InlineFrame")]
		[Description("Gets and sets the margin width in pixels.")]
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
		/// Gets and sets the AllowTransparency property.
		/// </summary>
		/// <remarks>Requires IE 5.5 or later.  Need to set the body color on the target document to transparent: <code>&gt;body style="background-color:transparent;"&lt;</code></remarks>
		[Browsable(true)]
		[DefaultValue(true)]
		[Category(" InlineFrame")]
		[Description("Gets and sets the transparency of the frame.")]
		public bool AllowTransparency
		{
			get { return (this.Attributes["allowtransparency"] == true.ToString().ToLower()); }
			set { this.Attributes["allowtransparency"] = value.ToString().ToLower(); }
		}

		/// <summary>
		/// Gets and sets the value of the z-index style property.
		/// </summary>
		/// <remarks>Requires IE 5.5 or later.</remarks>
		[Browsable(true)]
		[DefaultValue(true)]
		[Category(" InlineFrame")]
		[Description("Gets and sets the z-index of the frame.")]
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
					this.Style.Remove("z-index");
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
