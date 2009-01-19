using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.Threading;
using System.Text;

using JsonFx.Handlers;
using JsonFx.Compilation;

namespace JsonFx.Client
{
	public enum StyleIncludeType
	{
		Link,
		Import
	}

	/// <summary>
	/// Base control for referencing a ResourceHandler
	/// </summary>
	[ToolboxData("<{0}:ResourceInclude runat=\"server\"></{0}:ResourceInclude>")]
	public class ResourceInclude : Control, IAttributeAccessor
	{
		#region Constants

		private const string StyleLink = "<link type=\"{0}\" href=\"{1}\"{2} />";
		private const string StyleImport = "<style type=\"{0}\"{2}>@import url({1});</style>";
		private const string ScriptInclude = "<script type=\"{0}\" src=\"{1}\"{2}></script>";

		#endregion Constants

		#region Fields

		private bool isDebug;
		private string sourceUrl;
		private bool usePageCulture = true;
		private StyleIncludeType styleFormat = StyleIncludeType.Import;
		private Dictionary<string, string> attributes = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public ResourceInclude()
		{
			this.isDebug = this.Context.IsDebuggingEnabled;
		}

		#endregion Init

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
		/// Gets and sets resource url.
		/// </summary>
		[DefaultValue("")]
		public string SourceUrl
		{
			get
			{
				if (this.sourceUrl == null)
				{
					return String.Empty;
				}
				return this.sourceUrl;
			}
			set { this.sourceUrl = value; }
		}

		/// <summary>
		/// Gets and sets if page determines the culture or
		/// if uses CurrentUICulture
		/// </summary>
		[DefaultValue(true)]
		public bool UsePageCulture
		{
			get { return this.usePageCulture; }
			set { this.usePageCulture = value; }
		}

		/// <summary>
		/// Gets and sets if page determines the culture or
		/// if uses CurrentUICulture
		/// </summary>
		[DefaultValue(StyleIncludeType.Import)]
		public StyleIncludeType StyleFormat
		{
			get { return this.styleFormat; }
			set { this.styleFormat = value; }
		}

		#endregion Properties

		#region Page Event Handlers

		protected override void Render(HtmlTextWriter writer)
		{
			string url = this.SourceUrl;
			CompiledBuildResult info = CompiledBuildResult.Create(url, true);
			if (info == null)
			{
				throw new ArgumentException(String.Format(
					"Error loading resources for \"{0}\".\r\n"+
					"This can be caused by an invalid path, build errors, or incorrect configuration.\r\n"+
					"Check http://help.jsonfx.net/instructions for troubleshooting.",
					url));
			}

			url = ResourceHandler.GetResourceUrl(url, this.isDebug);
			url = this.ResolveUrl(url);
			string type =
				String.IsNullOrEmpty(info.ContentType) ?
				String.Empty :
				info.ContentType.ToLowerInvariant();
			string attrib = this.BuildCustomAttributes();
			string format;

			switch (type)
			{
				case CssResourceCodeProvider.MimeType:
				{
					if (this.styleFormat == StyleIncludeType.Import)
					{
						format = ResourceInclude.StyleImport;
					}
					else
					{
						format = ResourceInclude.StyleLink;
					}
					break;
				}
				default:
				{
					format = ResourceInclude.ScriptInclude;
					break;
				}
			}
			writer.Write(format, type, url, attrib);

			if (info is GlobalizedCompiledBuildResult)
			{
				string culture = this.UsePageCulture ?
					Thread.CurrentThread.CurrentCulture.Name :
					String.Empty;

				url = ResourceHandler.GetLocalizationUrl(this.SourceUrl, this.isDebug, culture);
				url = this.ResolveUrl(url);
				writer.Write(ResourceInclude.ScriptInclude, ScriptResourceCodeProvider.MimeType, url, String.Empty);
			}
		}

		#endregion Page Event Handlers

		#region Attribute Methods

		private string BuildCustomAttributes()
		{
			if (this.attributes == null || this.attributes.Count == 0)
			{
				return String.Empty;
			}

			StringBuilder builder = new StringBuilder();
			foreach (string key in this.attributes.Keys)
			{
				builder.Append(' ');
				builder.Append(HttpUtility.HtmlAttributeEncode(key.ToLowerInvariant()));
				builder.Append("=\"");
				builder.Append(HttpUtility.HtmlAttributeEncode(this.attributes[key]));
				builder.Append('"');
			}
			return builder.ToString();
		}

		#endregion Attribute Methods

		#region IAttributeAccessor Members

		string IAttributeAccessor.GetAttribute(string key)
		{
			if (this.attributes == null || !this.attributes.ContainsKey(key))
			{
				return null;
			}

			return this.attributes[key];
		}

		void IAttributeAccessor.SetAttribute(string key, string value)
		{
			if (this.attributes == null)
			{
				this.attributes = new Dictionary<string,string>(2, StringComparer.OrdinalIgnoreCase);
			}

			this.attributes[key] = value;
		}

		#endregion IAttributeAccessor Members
	}
}
