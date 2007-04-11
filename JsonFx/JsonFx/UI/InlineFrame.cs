using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace PseudoCode.Web.Controls
{
	/// <summary>
	/// Represents an inline frame element.
	/// http://www.w3.org/TR/xhtml-modularization/dtd_module_defs.html#a_module_Iframe
	/// http://www.w3.org/TR/REC-html40/present/frames.html#h-16.5
	/// </summary>
	[ToolboxData("<{0}:InlineFrame runat=\"server\">\n\t\n<{0}:InlineFrame>")]
	public class InlineFrame : System.Web.UI.WebControls.WebControl
	{
		#region Fields

		private string sourceUrl = null;
		private string longDescriptionUrl = null;
		private string alternateText = null;
		//private const string DefaultAltText = "Your browser does not support IFrames.";

		#endregion Fields

		#region Ctor

		/// <summary>
		/// Ctor.  Uses iframe as tag name.
		/// </summary>
		public InlineFrame() : base(System.Web.UI.HtmlTextWriterTag.Iframe) { }

		#endregion Ctor

		#region Properties

		/// <summary>
		/// This is the URL to be the displayed content.
		/// </summary>
		[Browsable(true)]
		[DefaultValue("")]
		[Category(" InlineFrame")]
		[Description("Gets and sets the source Url.")]
		public virtual string SourceUrl
		{
			get { return this.sourceUrl; }
			set { this.sourceUrl = value; }
		}

		/// <summary>
		/// This is the URL to be the displayed content.
		/// </summary>
		[Browsable(true)]
		[DefaultValue("")]
		[Category(" InlineFrame")]
		[Description("Gets and sets the log description Url.")]
		public virtual string LongDescriptionUrl
		{
			get { return this.longDescriptionUrl; }
			set { this.longDescriptionUrl = value; }
		}

		/// <summary>
		/// Text shown if the browser does not support IFrames.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(null)]
		[Category(" InlineFrame")]
		[Description("Gets and sets the text to be displayed if browser doesn't support the iframe tag.")]
		public virtual string AlternateText
		{
			get { return this.alternateText; }
			set { this.alternateText = value; }
		}

		/// <summary>
		/// Gets and sets the Height of the frame
		/// </summary>
		[Browsable(true)]
		[Category(" InlineFrame")]
		[Description("Gets and sets the height of the frame.")]
		public override Unit Height
		{
			get
			{
				try { return new Unit(this.Attributes["height"]); }
				catch { return Unit.Empty; }
			}
			set { this.Attributes["height"] = value.ToString(); }
		}

		/// <summary>
		/// Gets and sets the Width of the frame
		/// </summary>
		[Browsable(true)]
		[Category(" InlineFrame")]
		[Description("Gets and sets the width of the frame.")]
		public override Unit Width
		{
			get
			{
				try { return new Unit(this.Attributes["width"]); }
				catch { return Unit.Empty; }
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
		public virtual bool FrameBorder
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
		public virtual InlineFrameScrollingType Scrolling
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
		public virtual HorizontalAlign HorizontalAlign
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
		public virtual int MarginHeight
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
		public virtual int MarginWidth
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
		public virtual bool AllowTransparency
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
		public virtual int ZIndex
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

		#region Page Events

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);

			if (!String.IsNullOrEmpty(this.sourceUrl))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveUrl(this.sourceUrl));
			}

			if (!String.IsNullOrEmpty(this.longDescriptionUrl))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Longdesc, this.ResolveUrl(this.longDescriptionUrl));
			}
		}

		/// <summary>
		/// Sets up the controls for rendering.
		/// </summary>
		protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
		{
			if (String.IsNullOrEmpty(this.AlternateText))
			{
				base.RenderContents(writer);
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, this.SourceUrl);
				writer.RenderBeginTag(HtmlTextWriterTag.A);
				writer.WriteEncodedText(this.AlternateText);
				writer.RenderEndTag();
			}
		}

		#endregion Page Events
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
