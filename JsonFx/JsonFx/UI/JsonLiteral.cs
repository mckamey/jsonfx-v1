using System;

namespace JsonFx.UI
{
	public class JsonLiteral : JsonFx.UI.IJsonControl, JsonFx.JSON.IJsonSerializable
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

		void JsonFx.JSON.IJsonSerializable.ReadJson(JsonFx.JSON.JsonReader reader)
		{
			throw new NotImplementedException("JsonFx.Serialization.IJsonSerializable.ReadJson is not implemented.");
		}

		void JsonFx.JSON.IJsonSerializable.WriteJson(JsonFx.JSON.JsonWriter writer)
		{
			writer.Write(this.Text);
		}

		#endregion IJsonSerializable Members
	}
}
