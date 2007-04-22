using System;

namespace JsonFx.Services
{
	/// <summary>
	/// Specifies the service information to use when serializing to JSON.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class JsonServiceAttribute : JsonFx.Services.JsonDocsAttribute
	{
		#region Fields

		private string nameSpace = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonServiceAttribute()
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets ans sets the namespace to be used when generating the service proxy
		/// </summary>
		public string Namespace
		{
			get { return this.nameSpace; }
			set { this.nameSpace = value; }
		}

		#endregion Properties

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

		/// <summary>
		/// Gets the namespace for use in JSON service proxy.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>proxy namespace</returns>
		public static string GetNamespace(object value)
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

			if (!JsonServiceAttribute.IsDefined(memberInfo, typeof(JsonServiceAttribute)))
				return null;
			JsonServiceAttribute attribute = (JsonServiceAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(JsonServiceAttribute));

			return attribute.Namespace;
		}

		#endregion Methods
	}
}
