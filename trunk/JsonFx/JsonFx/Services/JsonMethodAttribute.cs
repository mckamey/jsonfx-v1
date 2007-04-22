using System;

namespace JsonFx.Services
{
	/// <summary>
	/// Specifies the method information to use when serializing to JSON.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
	public sealed class JsonMethodAttribute : JsonFx.Services.JsonDocsAttribute
	{
		#region Fields

		private bool idempotent = false;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonMethodAttribute()
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="jsonName"></param>
		public JsonMethodAttribute(string jsonName) : base(jsonName)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public bool Idempotent
		{
			get { return this.idempotent; }
			set { this.idempotent = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets the name specified for use in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsJsonMethod(object value)
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

			return JsonMethodAttribute.IsDefined(memberInfo, typeof(JsonMethodAttribute));
		}

		/// <summary>
		/// Gets the name specified for use in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsIdempotent(object value)
		{
			if (value == null)
				return false;

			Type type = value.GetType();
			System.Reflection.MemberInfo memberInfo = null;

			if (type.IsEnum)
			{
				string name = Enum.GetName(type, value);
				if (String.IsNullOrEmpty(name))
					return false;
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

			if (!JsonMethodAttribute.IsDefined(memberInfo, typeof(JsonMethodAttribute)))
				return false;
			JsonMethodAttribute attribute = (JsonMethodAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(JsonMethodAttribute));

			return attribute.Idempotent;
		}

		#endregion Methods
	}
}
