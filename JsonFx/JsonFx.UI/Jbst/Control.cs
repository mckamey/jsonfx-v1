using System;
using System.ComponentModel;
using System.Web.UI;

namespace JsonFx.UI.Jbst
{
	[ToolboxData("<{0}:Control runat=\"server\"></{0}:Control>")]
	public class Control : System.Web.UI.Control
	{
		#region Fields

		private string name;
		private string data;
		private string index;

		#endregion Fields

		#region Properties

		public virtual string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		public virtual string Data
		{
			get { return this.data; }
			set { this.data = value; }
		}

		public virtual string Index
		{
			get { return this.index; }
			set { this.index = value; }
		}

		#endregion Properties

		#region Page Event Handlers

		protected override void Render(HtmlTextWriter writer)
		{
		}

		#endregion Page Event Handlers
	}
}
