using System;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Specifies the naming to use when serializing to JSON.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple=false)]
	public sealed class JsonIgnoreAttribute : Attribute
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonIgnoreAttribute()
		{
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Gets a value which indicates if should be ignored in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsJsonIgnore(object value)
		{
			if (value == null)
				return false;

			Type type = value.GetType();
			System.Reflection.MemberInfo memberInfo = null;

			if (type.IsEnum)
			{
				memberInfo = type.GetField(Enum.GetName(type, value));
			}
			else
			{
				memberInfo = value as System.Reflection.MemberInfo;
			}

			if (memberInfo == null)
			{
				throw new NotImplementedException();
			}

			return JsonIgnoreAttribute.IsDefined(memberInfo, typeof(JsonIgnoreAttribute));
		}

		#endregion Methods
	}
}
