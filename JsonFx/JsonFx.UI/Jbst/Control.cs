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
using System.Web.UI;

using JsonFx.Client;
using JsonFx.Json;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Convenience control for combining JBST controls and JSON data on an ASP.NET page.
	/// </summary>
	[ToolboxData("<{0}:Control runat=\"server\" Name=\"\"></{0}:Control>")]
	public class Control : AutoDataBindControl
	{
		#region Fields

		private bool isDebug;
		private bool autoMarkup;
		private EcmaScriptIdentifier name;
		private string data;
		private object inlineData;
		private int? index;
		private int? count;
		private ScriptDataBlock dataBlock;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public Control()
		{
			this.isDebug = this.Context.IsDebuggingEnabled;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the script variable name of the JBST control to be bound.
		/// </summary>
		[DefaultValue("")]
		public virtual EcmaScriptIdentifier Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Gets and sets data to be bound as JavaScript literal or variable reference.
		/// </summary>
		[DefaultValue("")]
		public virtual string Data
		{
			get
			{
				if (this.data == null)
				{
					return String.Empty;
				}
				return this.data;
			}
			set { this.data = value; }
		}

		/// <summary>
		/// Gets and sets data to be bound as an object which will be serialized.
		/// </summary>
		[DefaultValue(null)]
		public virtual object InlineData
		{
			get { return this.inlineData; }
			set { this.inlineData = value; }
		}

		/// <summary>
		/// Gets and sets the data index, passed in when binding the data and JBST.
		/// </summary>
		[DefaultValue(-1)]
		public virtual int Index
		{
			get
			{
				if (!this.index.HasValue)
				{
					return -1;
				}
				return this.index.Value;
			}
			set
			{
				if (value < 0)
				{
					this.index = null;
					return;
				}
				this.index = value;
			}
		}

		/// <summary>
		/// Gets and sets the total data count, passed in when binding the data and JBST.
		/// </summary>
		[DefaultValue(-1)]
		public virtual int Count
		{
			get
			{
				if (!this.count.HasValue)
				{
					return -1;
				}
				return this.count.Value;
			}
			set
			{
				if (value < 0)
				{
					this.count = null;
					return;
				}
				this.count = value;
			}
		}

		/// <summary>
		/// Gets a dictionary of Data to emit to the page.
		/// </summary>
		public IDictionary<string, object> DataItems
		{
			get
			{
				if (this.dataBlock == null)
				{
					this.dataBlock = new ScriptDataBlock();
				}
				return this.dataBlock.DataItems;
			}
		}

		/// <summary>
		/// Gets and sets if should render as a debuggable ("Pretty-Print") block.
		/// </summary>
		[DefaultValue(false)]
		public bool IsDebug
		{
			get { return this.isDebug; }
			set { this.isDebug = value; }
		}

		/// <summary>
		/// Gets and sets if should also render the data as markup for noscript clients.
		/// </summary>
		[DefaultValue(false)]
		public bool AutoMarkup
		{
			get { return this.autoMarkup; }
			set { this.autoMarkup = value; }
		}

		#endregion Properties

		#region Page Event Handlers

		/// <summary>
		/// Renders the JBST control reference and any stored data to be used.
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			writer.BeginRender();
			try
			{
				// render any named data items
				if (this.dataBlock != null)
				{
					this.dataBlock.AutoMarkup = this.AutoMarkup;
					this.dataBlock.RenderControl(writer);
				}

				// generate an ID for controls which do not have explicit
				this.EnsureID();
				if (String.IsNullOrEmpty(this.ClientID))
				{
					// happens with no parents
					this.ID = "_"+Guid.NewGuid().ToString("n");
				}

				bool hasControls = this.HasControls();
				string placeholder = hasControls ? "div" : "noscript";

				// render the placeholder hook
				writer.Write('<');
				writer.Write(placeholder);
				writer.Write(" id=\"");
				writer.Write(this.ClientID);
				writer.Write("\">");

				// render any children as loading/error markup
				base.RenderChildren(writer);

				string inlineData = null;
				if (this.AutoMarkup && this.InlineData != null)
				{
					if (hasControls)
					{
						writer.Write("<noscript>");
					}

					// serialize InlineData as a JavaScript literal
					StringBuilder builder = new StringBuilder();

					JsonMarkupWriter jsWriter = new JsonMarkupWriter(builder, writer);
					if (this.IsDebug)
					{
						jsWriter.Settings.PrettyPrint = true;
						jsWriter.Settings.NewLine = Environment.NewLine;
						jsWriter.Settings.Tab = "\t";
					}
					jsWriter.Write(this.InlineData);

					if (hasControls)
					{
						writer.Write("</noscript>");
					}

					inlineData = builder.ToString();
				}

				writer.Write("</");
				writer.Write(placeholder);
				writer.Write('>');

				// render the binding script
				writer.Write("<script type=\"text/javascript\">");

				if (!String.IsNullOrEmpty(this.Name))
				{
					writer.Write(this.Name);
				}
				else
				{
					throw new ArgumentNullException("jbst:Control Name must be specified.");
				}
				writer.Write(".replace(\"");
				writer.Write(this.ClientID);
				writer.Write("\",");

				if (!String.IsNullOrEmpty(inlineData))
				{
					writer.Write(inlineData);
				}
				else if (this.InlineData != null)
				{
					// serialize InlineData as a JavaScript literal
					EcmaScriptWriter jsWriter = new EcmaScriptWriter(writer);
					if (this.IsDebug)
					{
						jsWriter.Settings.PrettyPrint = true;
						jsWriter.Settings.NewLine = Environment.NewLine;
						jsWriter.Settings.Tab = "\t";
					}
					jsWriter.Write(this.InlineData);
				}
				else if (!String.IsNullOrEmpty(this.Data))
				{
					// assume Data is either a JavaScript literal or variable reference
					writer.Write('(');
					writer.Write(this.Data);
					writer.Write(')');
				}
				else
				{
					// smallest most innocuous default data
					writer.Write("{}");
				}

				if (this.Index >= 0)
				{
					writer.Write(",(");
					writer.Write(this.Index);
					writer.Write(')');

					if (this.Count >= 0)
					{
						writer.Write(",(");
						writer.Write(this.Count);
						writer.Write(')');
					}
				}
				writer.Write(");");

				writer.Write("</script>");
			}
			finally
			{
				writer.EndRender();
			}
		}

		#endregion Page Event Handlers
	}
}
