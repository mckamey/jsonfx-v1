using System;

namespace JsonFx.Serialization
{
	public interface IJsonSerializable
	{
		void ReadJson(JsonReader reader);
		void WriteJson(JsonWriter writer);
	}
}
