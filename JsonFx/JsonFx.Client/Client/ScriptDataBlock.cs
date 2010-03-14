#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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
using System.IO;
using System.Web;
using System.Web.UI;

using JsonFx.Json;

namespace JsonFx.Client
{
	/// <summary>
	/// Specifically for emitting runtime data to the page as JavaScript variables.
	/// </summary>
	[ToolboxData("<{0}:ScriptDataBlock runat=\"server\"></{0}:ScriptDataBlock>")]
	public class ScriptDataBlock : Control
	{
		#region Constants

		private const string ScriptOpen = "<script type=\"text/javascript\">";
		private const string ScriptClose = "</script>";
		private const string NoScriptOpen = "<noscript>";
		private const string NoScriptClose = "</noscript>";
		private const string VarAssignmentDebug = "{0} = ";
		private const string VarAssignment = "{0}=";
		private const string VarAssignmentEnd = ";";

		#endregion Constants

		#region Fields

		private bool isDebug;
		private AutoMarkupType autoMarkup;
		private readonly Dictionary<string, object> Data = new Dictionary<string, object>(StringComparer.Ordinal);

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public ScriptDataBlock()
		{
			this.isDebug = this.Context.IsDebuggingEnabled;
		}

		#endregion Init

		#region Properties

		// TODO: create a custom TypeConverter to allow simple declarative assignment
		/// <summary>
		/// Gets and sets values in the JavaScript global namespace.
		/// </summary>
		/// <param name="varName"></param>
		/// <returns></returns>
		public object this[string varName]
		{
			get
			{
				if (!this.Data.ContainsKey(varName))
				{
					return null;
				}
				return this.Data[varName];
			}
			set
			{
				varName = EcmaScriptIdentifier.EnsureValidIdentifier(varName, true);
				this.Data[varName] = value;
			}
		}

		/// <summary>
		/// Gets access to data items as dictionary
		/// </summary>
		public IDictionary<string, object> DataItems
		{
			get { return this.Data; }
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
		[DefaultValue(AutoMarkupType.Off)]
		public AutoMarkupType AutoMarkup
		{
			get { return this.autoMarkup; }
			set { this.autoMarkup = value; }
		}

		#endregion Properties

		#region Page Event Handlers

		/// <summary>
		/// Renders the data items as a block of JavaScript
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			if (this.Data.Count < 1)
			{
				// emit nothing when empty
				return;
			}

			writer.BeginRender();
			try
			{
				List<string> namespaces = new List<string>();

				StringWriter markup;
				EcmaScriptWriter jsWriter;
				if (this.AutoMarkup == AutoMarkupType.Off)
				{
					markup = null;
					jsWriter = new EcmaScriptWriter(writer);
				}
				else
				{
					markup = new StringWriter();
					jsWriter = new JsonMarkupWriter(writer, markup);
				}

				if (this.IsDebug)
				{
					jsWriter.Settings.PrettyPrint = true;
					jsWriter.Settings.NewLine = Environment.NewLine;
					jsWriter.Settings.Tab = "\t";
				}

				writer.Write(ScriptDataBlock.ScriptOpen);

				foreach (string key in this.Data.Keys)
				{
					if (markup != null)
					{
						if (this.IsDebug)
						{
							markup.WriteLine();
						}
						markup.Write("<div title=\"");
						HttpUtility.HtmlAttributeEncode(key, markup);
						markup.Write("\">");
					}

					string declaration;
					if (!EcmaScriptWriter.WriteNamespaceDeclaration(writer, key, namespaces, this.IsDebug))
					{
						declaration = "var "+key;
					}
					else
					{
						declaration = key;
					}

					if (this.IsDebug)
					{
						writer.Indent += 3;
						writer.Write(ScriptDataBlock.VarAssignmentDebug, declaration);
						writer.Indent -= 3;
						if (this.Data[key] != null &&
							this.Data[key].GetType().IsClass)
						{
							writer.WriteLine();
						}
					}
					else
					{
						writer.Write(ScriptDataBlock.VarAssignment, declaration);
					}

					// emit the value as JSON
					jsWriter.Write(this.Data[key]);
					writer.Write(ScriptDataBlock.VarAssignmentEnd);

					if (markup != null)
					{
						markup.Write("</div>");
					}

					if (this.IsDebug)
					{
						writer.WriteLine();
					}
				}

				writer.Write(ScriptDataBlock.ScriptClose);

				if (markup != null && this.Data.Count > 0)
				{
					writer.Write(ScriptDataBlock.NoScriptOpen);
					writer.Write(markup.ToString());
					writer.Write(ScriptDataBlock.NoScriptClose);
				}
			}
			finally
			{
				writer.EndRender();
			}
		}

		#endregion Page Event Handlers
	}
}
