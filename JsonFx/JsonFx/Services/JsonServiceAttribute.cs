using System;

namespace JsonFx.Services
{
	/// <summary>
	/// Specifies the service information to use when serializing to JSON.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class JsonServiceAttribute : JsonFx.Services.JsonDocsAttribute
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonServiceAttribute() : base()
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="jsonName"></param>
		public JsonServiceAttribute(string jsonName) : base(jsonName)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="jsonName"></param>
		public JsonServiceAttribute(string jsonName, string helpUrl) : base(jsonName, helpUrl)
		{
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Gets the name specified for use in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsJsonService(object value)
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

			return JsonServiceAttribute.IsDefined(memberInfo, typeof(JsonServiceAttribute));
		}

		#endregion Methods
	}
}
