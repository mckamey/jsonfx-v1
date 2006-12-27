using System;

namespace JsonFx.UI
{
	/// <summary>
	/// Specifies the UserControl may be accessed directly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public sealed class HostableUserControlAttribute : Attribute
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public HostableUserControlAttribute()
		{
		}

		#endregion Init

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

		#endregion Methods
	}
}
