using System;

namespace JsonFx.UI.JsonML
{
	public class JsonLiteral : JsonFx.UI.JsonML.IJsonControl, JsonFx.Json.IJsonSerializable
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

		void JsonFx.Json.IJsonSerializable.ReadJson(JsonFx.Json.JsonReader reader)
		{
			throw new NotImplementedException("IJsonSerializable.ReadJson is not implemented.");
		}

		void JsonFx.Json.IJsonSerializable.WriteJson(JsonFx.Json.JsonWriter writer)
		{
			writer.Write(this.Text);
		}

		#endregion IJsonSerializable Members
	}
}
