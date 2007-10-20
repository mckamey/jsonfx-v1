using System;
using System.Collections.Generic;

namespace JsonFx.JSON
{
	/// <summary>
	/// Specifies the naming to use when serializing to JSON.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple=false)]
	public class JsonNameAttribute : Attribute
	{
		#region Fields

		private string jsonName = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonNameAttribute()
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="jsonName"></param>
		public JsonNameAttribute(string jsonName)
		{
			this.jsonName = jsonName;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the name to be used in JSON
		/// </summary>
		public string Name
		{
			get { return this.jsonName; }
			set { this.jsonName = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets the name specified for use in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetJsonName(object value)
		{
			if (value == null)
				return null;

			Type type = value.GetType();
			System.Reflection.MemberInfo memberInfo = null;

			if (type.IsEnum)
			{
				string name = Enum.GetName(type, value);
				if (String.IsNullOrEmpty(name))
					return null;
				memberInfo = type.GetField(name);
			}
			else
			{
				memberInfo = value as System.Reflection.MemberInfo;
			}

			if (memberInfo == null)
			{
				throw new NotImplementedException();
			}

			if (!JsonNameAttribute.IsDefined(memberInfo, typeof(JsonNameAttribute)))
				return null;
			JsonNameAttribute attribute = (JsonNameAttribute)JsonNameAttribute.GetCustomAttribute(memberInfo, typeof(JsonNameAttribute));

			return attribute.Name;
		}

		#endregion Methods
	}
}
