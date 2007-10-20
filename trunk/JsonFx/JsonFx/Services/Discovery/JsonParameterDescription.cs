using System;
using System.ComponentModel;
using System.Reflection;

using JsonFx.JSON;

namespace JsonFx.Services.Discovery
{
	public class JsonParameterDescription : JsonDescriptionBase
	{
		#region Fields

		private JsonParameterType type = JsonParameterType.Any;

		#endregion Fields

		#region Init

		public JsonParameterDescription() { }

		/// <summary>
		/// Ctor.
		/// </summary>
		internal JsonParameterDescription(ParameterInfo param)
		{
			if (param == null)
				return;

			this.Type = this.GetJsonParameterType(param.ParameterType);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets a summary of the purpose of the service.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that denotes the expected value type for the
		/// parameter. If this member is not supplied or is the Null value then
		/// the type is defined "any".
		/// </remarks>
		[JsonName("type")]
		public JsonParameterType Type
		{
			get { return this.type; }
			set { this.type = value; }
		}

		#endregion Properties

		#region Methods

		protected internal JsonParameterType GetJsonParameterType(Type type)
		{
			if (type == null)
				return JsonParameterType.None;

			if (type.IsEnum)
				return JsonParameterType.String;

			if (type.IsSubclassOf(typeof(System.Collections.IEnumerable)))
				return JsonParameterType.Array;

			switch (type.FullName)
			{
				case "System.String":
				case "System.Char":
				{
					return JsonParameterType.String;
				}
				case "System.Double":
				case "System.Single":
				case "System.Decimal":
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.UInt16":
				case "System.UInt32":
				case "System.UInt64":
				case "System.Byte":
				case "System.SByte":
				{
					return JsonParameterType.Number;
				}
				case "System.Object":
				{
					return JsonParameterType.Any;
				}
				case "System.Boolean":
				{
					return JsonParameterType.Boolean;
				}
				case "System.Void":
				{
					return JsonParameterType.None;
				}
				default:
				{
					return JsonParameterType.Object;
				}
			}
		}

		#endregion Methods
	}
}
