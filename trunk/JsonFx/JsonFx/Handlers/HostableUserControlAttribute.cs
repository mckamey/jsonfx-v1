using System;

namespace JsonFx.UI
{
	/// <summary>
	/// Specifies the UserControl may be accessed directly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public sealed class HostableUserControlAttribute : Attribute
	{
		#region Constants

		private const string DefaultID = "_";

		#endregion Constants

		#region Fields

		private string userControlID; 

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public HostableUserControlAttribute() : this(HostableUserControlAttribute.DefaultID)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		public HostableUserControlAttribute(string userControlID)
		{
			if (String.IsNullOrEmpty(userControlID))
				this.userControlID = HostableUserControlAttribute.DefaultID;
			else
				this.userControlID = userControlID;
		}

		#endregion Init

		#region Properties

		public string UserControlID
		{
			get { return this.userControlID; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets a value which indicates if should be ignored in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsHostable(System.Web.UI.Control control)
		{
			if (control == null)
				return false;

			return HostableUserControlAttribute.IsDefined(control.GetType(), typeof(HostableUserControlAttribute));
		}

		/// <summary>
		/// Gets a value which indicates if should be ignored in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetUserControlID(System.Web.UI.Control control)
		{
			if (control == null)
				return HostableUserControlAttribute.DefaultID;

			HostableUserControlAttribute attribute = HostableUserControlAttribute.GetCustomAttribute(control.GetType(), typeof(HostableUserControlAttribute)) as HostableUserControlAttribute;
			if (attribute == null)
				return HostableUserControlAttribute.DefaultID;

			return attribute.UserControlID;
		}

		#endregion Methods
	}
}
