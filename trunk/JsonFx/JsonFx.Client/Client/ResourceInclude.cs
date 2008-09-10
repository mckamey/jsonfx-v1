using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

using JsonFx.Handlers;
using JsonFx.Compilation;

namespace JsonFx.Client
{
	/// <summary>
	/// Base control for referencing a ResourceHandler
	/// </summary>
	public class ResourceInclude : Control
	{
		#region Constants

		private const string StyleImportDirective = "@import url({0});";

		#endregion Constants

		#region Fields

		private bool isDebug = false;
		private bool isLocalized = false;
		private string sourceUrl = String.Empty;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets and sets if should render a debuggable ("Pretty-Print") reference.
		/// </summary>
		[DefaultValue(false)]
		public bool IsDebug
		{
			get { return this.isDebug; }
			set { this.isDebug = value; }
		}

		/// <summary>
		/// Gets and sets if should render reference to a localized strings resource.
		/// </summary>
		[DefaultValue(false)]
		public bool IsLocalized
		{
			get { return this.isLocalized; }
			set { this.isLocalized = value; }
		}

		/// <summary>
		/// Gets and sets resource url.
		/// </summary>
		[DefaultValue("")]
		public string SourceUrl
		{
			get { return this.sourceUrl; }
			set { this.sourceUrl = value; }
		}

		#endregion Properties

		#region Page Event Handlers

		protected override void Render(HtmlTextWriter writer)
		{
			string url = this.SourceUrl;
			CompiledBuildResult info = CompiledBuildResult.Create(url);
			if (info == null)
			{
				throw new ArgumentException(String.Format(
					"Path \"{0}\" does not resolve to a ResourceHandler result.",
					url));
			}

			url = ResourceHandler.GetResourceUrl(url, this.isDebug);
			url = this.ResolveUrl(url);
			string type =
				String.IsNullOrEmpty(info.ContentType) ?
				String.Empty :
				info.ContentType.ToLowerInvariant();

			switch (type)
			{
				case CssResourceCodeProvider.MimeType:
				{
					this.RenderStyleImport(writer, url, info.ContentType);
					break;
				}
				default:
				{
					this.RenderScriptInclude(writer, url, info.ContentType);
					break;
				}
			}

			if (this.isLocalized)
			{
				string i18n = ResourceHandler.GetLocalizationUrl(this.SourceUrl, this.isDebug);
				this.RenderScriptInclude(writer, i18n, ScriptResourceCodeProvider.MimeType);
			}
		}

		private void RenderStyleImport(HtmlTextWriter writer, string url, string mimeType)
		{
			if (!String.IsNullOrEmpty(mimeType))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Type, mimeType);
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Style);
			writer.Write(StyleImportDirective, url);
			writer.RenderEndTag();
		}

		private void RenderScriptInclude(HtmlTextWriter writer, string url, string mimeType)
		{
			if (!String.IsNullOrEmpty(mimeType))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Type, mimeType);
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Src, url);
			writer.RenderBeginTag(HtmlTextWriterTag.Script);
			writer.RenderEndTag();
		}

		#endregion Page Event Handlers
	}
}
