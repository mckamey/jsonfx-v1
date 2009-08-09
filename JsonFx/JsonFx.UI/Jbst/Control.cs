using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web.UI;

using JsonFx.Json;
using JsonFx.Client;
using JsonFx.Compilation;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Convenience control for combining JBST controls and JSON data on an ASP.NET page.
	/// </summary>
	//[ParseChildren(false), PersistChildren(false, true)]
	//[ControlBuilderAttribute(typeof(JbstControlBuilder))]
	[ToolboxData("<{0}:Control runat=\"server\" Name=\"\"></{0}:Control>")]
	public class Control : System.Web.UI.Control
	{
		#region Fields

		private bool isDebug;
		private string name;
		private string data;
		private object inlineData;
		//private object inlineJbst;
		private int? index;
		private int? count;
		private ScriptDataBlock dataBlock;
		//private List<string> g11nKeys;

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
		public virtual string Name
		{
			get
			{
				if (this.name == null)
				{
					return String.Empty;
				}
				return this.name;
			}
			set { this.name = EcmaScriptIdentifier.EnsureValidIdentifier(value, true); }
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
					this.Controls.Add(this.dataBlock);
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
				//if (String.IsNullOrEmpty(this.Name) && this.inlineJbst == null)
				//{
				//    this.ParseJbstContents();
				//}

				// generate an ID for controls which do not have explicit
				this.EnsureID();

				// render the placeholder hook
				writer.Write("<div id=\"");
				writer.Write(this.ClientID);
				writer.Write("\">");

				//if (this.inlineJbst == null)
				{
					// render out any children as loading/error markup
					base.RenderChildren(writer);
				}
				//else if (this.dataBlock != null)
				//{
				//    this.dataBlock.RenderControl(writer);
				//}

				// build the binding script
				StringBuilder builder = new StringBuilder();

				builder.Append("JsonFx.Bindings.replace(\"#");
				builder.Append(this.ClientID);
				builder.Append("\",");
				/*if (this.inlineJbst != null)
				{
					// serialize InlineJbst as a JavaScript literal
					EcmaScriptWriter jsWriter = new EcmaScriptWriter(builder);
					jsWriter.PrettyPrint = this.IsDebug;
					jsWriter.NewLine = Environment.NewLine;
					jsWriter.Tab = "\t";
					jsWriter.Write(this.inlineJbst);
				}
				else*/ if (!String.IsNullOrEmpty(this.Name))
				{
					builder.Append(this.Name);
				}
				else
				{
					throw new ArgumentNullException("jbst:Control Name and InlineJbst cannot both be empty.");
				}

				builder.Append(",");
				if (this.InlineData != null)
				{
					// serialize InlineData as a JavaScript literal
					EcmaScriptWriter jsWriter = new EcmaScriptWriter(builder);
					jsWriter.PrettyPrint = this.IsDebug;
					jsWriter.NewLine = Environment.NewLine;
					jsWriter.Tab = "\t";
					jsWriter.Write(this.InlineData);
				}
				else if (!String.IsNullOrEmpty(this.Data))
				{
					// assume Data is either a JavaScript literal or variable reference
					builder.Append('(');
					builder.Append(this.Data);
					builder.Append(')');
				}
				else
				{
					// smallest most innocuous default data
					builder.Append("{}");
				}

				if (this.Index >= 0)
				{
					builder.Append(",(");
					builder.Append(this.Index);
					builder.Append(')');
				}
				else
				{
					builder.Append(",NaN");
				}

				if (this.Count >= 0)
				{
					builder.Append(",(");
					builder.Append(this.Count);
					builder.Append(')');
				}
				builder.Append(");");

				if (this.Page != null && this.Page.Form != null)
				{
					// register the binding script
					this.Page.ClientScript.RegisterStartupScript(
						typeof(Control),
						this.ClientID + "_init",
						builder.ToString(),
						true);
				}
				else
				{
					// render the binding script
					writer.Write("<script type=\"text/javascript\">");
					writer.Write(builder.ToString());
					writer.Write("</script>");
				}

				writer.Write("</div>");
			}
			finally
			{
				writer.EndRender();
			}
		}

		//private void ParseJbstContents()
		//{
		//    StringWriter writer = new StringWriter();

		//    // render out any children as loading/error markup
		//    base.RenderChildren(new XhtmlTextWriter(writer, this.IsDebug ? "\t" : ""));

		//    JbstCompiler parser = new JbstCompiler(this.GetType().FullName);

		//    // parse JBST markup
		//    parser.Parse(writer.GetStringBuilder().ToString());

		//    this.inlineJbst = parser.Document;

		//    this.g11nKeys = new List<string>();
		//    JbstCodeProvider.ExtractGlobalizationKeys(parser.Document, this.g11nKeys);
		//}

		#endregion Page Event Handlers
	}
}
