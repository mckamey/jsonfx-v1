using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

using JsonFx.Json;

namespace JsonFx.Client
{
	/// <summary>
	/// Specifically for emitting runtime data to the page as JavaScript variables.
	/// </summary>
	[ToolboxData("<{0}:DataBlock runat=\"server\"></{0}:DataBlock>")]
	public class ScriptDataBlock : Control
	{
		#region Constants

		private static readonly char[] VariableDelims = new char[] { '.' };
		private const string ScriptOpen = "<script type=\"text/javascript\">";
		private const string ScriptClose = "</script>";
		private const string NamespaceCheck = @"if(""undefined""===typeof {0}){{{0}={{}};}}";
		private const string NamespaceCheckDebug = @"
			if (""undefined"" === typeof {0}) {{
				{0} = {{}};
			}}";
		private const string VarDeclarationDebug = "{0} = ";
		private const string VarDeclaration = "{0}=";
		private const string VarDeclarationEnd = ";";

		#endregion Constants

		#region Fields

		private bool isDebug;
		private readonly Dictionary<string, object> data = new Dictionary<string, object>(StringComparer.Ordinal);

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

		/// <summary>
		/// Gets and sets values in the JavaScript global namespace.
		/// </summary>
		/// <param name="varName"></param>
		/// <returns></returns>
		public object this[string varName]
		{
			get
			{
				if (!this.data.ContainsKey(varName))
				{
					return null;
				}
				return this.data[varName];
			}
			set
			{
				varName = this.EnsureEcmaScriptVar(varName);
				this.data[varName] = value;
			}
		}

		/// <summary>
		/// Gets and sets if should render a debuggable ("Pretty-Print") block.
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
		/// Renders the dictionary as a block of JavaScript
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			if (this.data.Count < 1)
			{
				// emit nothing if empty
				return;
			}

			writer.BeginRender();
			try
			{
				List<string> namespaces = new List<string>();

				JsonWriter jsonWriter = new JsonWriter(writer);
				jsonWriter.PrettyPrint = this.IsDebug;
				jsonWriter.NewLine = Environment.NewLine;
				jsonWriter.Tab = "\t";
				jsonWriter.DateTimeSerializer = JsonWriter.WriteEcmaScriptDate;

				writer.Write(ScriptOpen);

				foreach (string key in this.data.Keys)
				{
					string[] nsParts = key.Split(VariableDelims, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i<nsParts.Length-1; i++)
					{
						string ns = String.Join(".", nsParts, 0, i+1);
						if (namespaces.Contains(ns))
						{
							// don't emit multiple checks for same namespace
							continue;
						}

						// make note that we've emitted this namespace before
						namespaces.Add(ns);

						if (i == 0)
						{
							ns = "window."+ns;
						}

						if (this.IsDebug)
						{
							writer.WriteLine(NamespaceCheckDebug, ns);
						}
						else
						{
							writer.Write(NamespaceCheck, ns);
						}
					}

					string varName = key;
					if (nsParts.Length == 1)
					{
						varName = "var "+varName;
					}

					if (this.IsDebug)
					{
						writer.Indent += 3;
						writer.Write(VarDeclarationDebug, key);
						writer.Indent -= 3;
						if (this.data[key] == null ||
							!this.data[key].GetType().IsClass)
						{
							writer.WriteLine();
						}
					}
					else
					{
						writer.Write(VarDeclaration, key);
					}

					// emit the value as JSON
					jsonWriter.Write(this.data[key]);
					writer.Write(VarDeclarationEnd);

					if (this.IsDebug)
					{
						writer.WriteLine();
					}
				}

				writer.Write(ScriptClose);
			}
			finally
			{
				writer.EndRender();
			}
		}

		#endregion Page Event Handlers

		#region Methods

		/// <summary>
		/// Verifies the validity of the EcmaScript variable name.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private string EnsureEcmaScriptVar(string varName)
		{
			if (String.IsNullOrEmpty(varName))
			{
				throw new ArgumentException("Variable name is empty.");
			}

			// TODO: validate is valid EcmaScript var

			//throw new ArgumentException("Variable name is invalid in JavaScript.");
			return varName.Trim();
		}

		#endregion Methods
	}
}
