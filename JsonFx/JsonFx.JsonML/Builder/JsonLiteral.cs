using System;

using JsonFx.Json;

namespace JsonFx.JsonML.Builder
{
	/// <summary>
	/// Represents literal text in a JsonML document
	/// </summary>
	internal class JsonLiteral : IJsonControl, IJsonSerializable
	{
		#region Fields

		private string text;

		#endregion Fields
		
		#region Init

		public JsonLiteral(string text)
		{
			this.text = text;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the text of this literal
		/// </summary>
		public string Text
		{
			get { return this.text; }
			set { this.text = value; }
		}

		#endregion Properties

		#region IJsonSerializable Members

		void IJsonSerializable.ReadJson(JsonReader reader)
		{
			throw new NotImplementedException("IJsonSerializable.ReadJson is not implemented.");
		}

		void IJsonSerializable.WriteJson(JsonWriter writer)
		{
			writer.Write(this.Text);
		}

		#endregion IJsonSerializable Members
	}
}
