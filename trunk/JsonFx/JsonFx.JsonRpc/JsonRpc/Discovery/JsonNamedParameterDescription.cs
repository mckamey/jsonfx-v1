using System;
using System.ComponentModel;
using System.Reflection;

using JsonFx.Json;

namespace JsonFx.JsonRpc.Discovery
{
	public class JsonNamedParameterDescription : JsonParameterDescription
	{
		#region Fields

		private string name = null;

		#endregion Fields

		#region Init

		public JsonNamedParameterDescription() { }

		/// <summary>
		/// Ctor.
		/// </summary>
		internal JsonNamedParameterDescription(ParameterInfo param) : base(param)
		{
			if (param == null)
				return;

			this.Name = param.Name;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets a simple name for the parameter.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String value that provides a simple name for parameter.
		/// </remarks>
		[JsonName("name")]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		#endregion Properties
	}
}
