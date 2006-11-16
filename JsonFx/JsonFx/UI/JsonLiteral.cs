using System;

namespace JsonFx.UI
{
	public class JsonLiteral : JsonFx.UI.IJsonControl, JsonFx.Serialization.IJsonSerializable
	{
		#region Fields

		string text;

		#endregion Fields
		
		#region Init

		public JsonLiteral(string text)
		{
			this.text = text;
		}

		#endregion Init

		#region Properties

		public string Text
		{
			get { return this.text; }
			set { this.text = value; }
		}

		#endregion Properties

		#region IJsonSerializable Members

		void JsonFx.Serialization.IJsonSerializable.ReadJson(JsonFx.Serialization.JsonReader reader)
		{
			throw new NotImplementedException("JsonFx.Serialization.IJsonSerializable.ReadJson is not implemented.");
		}

		void JsonFx.Serialization.IJsonSerializable.WriteJson(JsonFx.Serialization.JsonWriter writer)
		{
			writer.Write(this.Text);
		}

		#endregion IJsonSerializable Members
	}
}
