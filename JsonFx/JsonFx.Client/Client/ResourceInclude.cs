#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;

using JsonFx.Compilation;
using JsonFx.Handlers;

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
		private bool suppressLocalization;
		private StyleIncludeType styleFormat = StyleIncludeType.Link;
		private Dictionary<string, string> attributes;

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
		/// Gets and sets if will be manually emitting localization values
		/// </summary>
		[DefaultValue(false)]
		public bool SuppressLocalization
		{
			get { return this.suppressLocalization; }
			set { this.suppressLocalization = value; }
		}

		/// <summary>
		/// Gets and sets if page determines the culture or
		/// if uses CurrentUICulture
		/// </summary>
		[DefaultValue(StyleIncludeType.Link)]
		public StyleIncludeType StyleFormat
		{
			get { return this.styleFormat; }
			set { this.styleFormat = value; }
		}

		/// <summary>
		/// Gets the collection of custom attributes
		/// </summary>
		public Dictionary<string, string> Attributes
		{
			get
			{
				if (this.attributes == null)
				{
					this.attributes = new Dictionary<string, string>(2, StringComparer.OrdinalIgnoreCase);
				}
				return this.attributes;
			}
		}

		#endregion Properties

		#region Page Event Handlers

		protected override void Render(HtmlTextWriter writer)
		{
			string url = this.SourceUrl;

			bool isExternal = (url != null) && (url.IndexOf("://") > 0);

			IBuildResult info = isExternal ? null :
				this.GetResourceInfo(url);

			if (info == null && !isExternal)
			{
				throw new ArgumentException(String.Format(
					"Error loading resources for \"{0}\".\r\n"+
					"This can be caused by an invalid path, build errors, or incorrect configuration.\r\n"+
					"Check http://help.jsonfx.net/instructions for troubleshooting.",
					url));
			}

			url = ResourceHandler.GetResourceUrl(info, url, this.isDebug);
			url = this.ResolveUrl(url);

			string type =
				isExternal || String.IsNullOrEmpty(info.ContentType) ?
				String.Empty :
				info.ContentType.ToLowerInvariant();

			string format;
			switch (type)
			{
				case CssResourceCodeProvider.MimeType:
				{
					switch (this.StyleFormat)
					{
						case StyleIncludeType.Import:
						{
							format = ResourceInclude.StyleImport;
							break;
						}
						default:
						{
							format = ResourceInclude.StyleLink;
							if (!this.Attributes.ContainsKey("rel"))
							{
								this.Attributes["rel"] = "stylesheet";
							}
							break;
						}
					}
					break;
				}
				case "":
				{
					type = ScriptResourceCodeProvider.MimeType;
					goto default;
				}
				default:
				{
					format = ResourceInclude.ScriptInclude;
					break;
				}
			}

			string attrib = this.BuildCustomAttributes();
			writer.Write(format, type, url, attrib);

			if (!this.SuppressLocalization &&
				info is IGlobalizedBuildResult)
			{
				string culture = this.UsePageCulture ?
					Thread.CurrentThread.CurrentCulture.Name :
					String.Empty;

				url = ResourceHandler.GetLocalizationUrl(url, culture);
				url = this.ResolveUrl(url);
				writer.Write(ResourceInclude.ScriptInclude, ScriptResourceCodeProvider.MimeType, url, String.Empty);
			}
		}

		protected virtual IBuildResult GetResourceInfo(string url)
		{
			return ResourceHandler.Create<IBuildResult>(url);
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
			this.Attributes[key] = value;
		}

		#endregion IAttributeAccessor Members
	}
}
