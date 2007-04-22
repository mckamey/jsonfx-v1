using System;

namespace JsonFx.Services
{
	/// <summary>
	/// Gets the help url for use in Json service description.
	/// </summary>
	public abstract class JsonDocsAttribute : JsonFx.Serialization.JsonNameAttribute
	{
		#region Fields

		private string helpUrl = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonDocsAttribute()
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="jsonName"></param>
		public JsonDocsAttribute(string jsonName) : base(jsonName)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the URL which points to help documentation
		/// </summary>
		public string HelpUrl
		{
			get { return this.helpUrl; }
			set { this.helpUrl = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets the help url for use in Json service description.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetHelpUrl(object value)
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

			if (!JsonDocsAttribute.IsDefined(memberInfo, typeof(JsonDocsAttribute)))
				return null;
			JsonDocsAttribute attribute = (JsonDocsAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(JsonDocsAttribute));

			return attribute.HelpUrl;
		}

		#endregion Methods
	}
}
