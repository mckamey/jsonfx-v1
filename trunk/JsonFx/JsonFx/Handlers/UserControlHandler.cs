using System;
using System.Web.UI;

using JsonFx.UI;

namespace JsonFx.Handlers
{
	public class UserControlHandler : System.Web.UI.Page
	{
		#region Fields

		private Control userControl = null;

		#endregion Fields

		#region Init

		public UserControlHandler()
		{
		}

		#endregion Init

		#region Properties

		public virtual string UserControlID
		{
			get
			{
				this.EnsureChildControls();
				return this.userControl.ID;
			}
			set
			{
				this.EnsureChildControls();
				this.userControl.ID = value;
			}
		}

		#endregion Properties

		#region Page Events

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			this.EnsureChildControls();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			System.Web.UI.HtmlControls.HtmlForm form = new System.Web.UI.HtmlControls.HtmlForm();
			form.ID = "F";
			this.Controls.Add(form);

			this.EnableViewState = false;
			this.MaintainScrollPositionOnPostBack = false;

			// instantiate the usercontrol
			Control control = this.LoadControl(this.Request.Path);
			if (control == null)
			{
				throw new System.Web.HttpException(500, "Error creating UserControl.");
			}

			// add the control to the hosted page
			this.userControl = control;
			this.Form.Controls.Add(this.userControl);

			// if is a cached usercontrol then need to get reference to actual usercontrol
			PartialCachingControl cachedControl = control as PartialCachingControl;
			if (cachedControl != null)
			{
				control = cachedControl.CachedControl;
			}

			// check the security on user control class
			if (!HostableUserControlAttribute.IsHostable(control))
			{
				throw new System.Web.HttpException(403, String.Format("UserControl \"{0}\" is forbidden.  In order to enable direct access, mark with a {1}.", this.Request.Path, typeof(HostableUserControlAttribute).FullName));
			}
			this.userControl.ID = HostableUserControlAttribute.GetUserControlID(control);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			try
			{
				if (this.userControl != null)
				{
					if (this.userControl.Controls.Count > 1)
					{
						writer.RenderBeginTag(HtmlTextWriterTag.Div);

						this.userControl.RenderControl(writer);

						writer.RenderEndTag();//Div
					}
					else
					{
						this.userControl.RenderControl(writer);
					}
				}
			}
			finally
			{
				writer.Close();
			}
		}

		public override void VerifyRenderingInServerForm(Control control)
		{
			// suppress this check since we aren't actually rendering the form
			//base.VerifyRenderingInServerForm(control);
		}

		#endregion Page Events
	}
}