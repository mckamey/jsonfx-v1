JsonFx.NET

JSON Serializer functions like XML Serialization in .NET Framework

Serializes and deserializes IDictionary<String, T> and IDictionary with
String keys directly as a JSON object

Serializes and deserializes List<T>, ArrayList, LinkedList<T>, Queue<T> and
many other IEnumerable types directly as JSON arrays

Serializes and deserializes DateTime, Enum, Nullable<T>, Guid and other
common .NET Types directly as JSON primitives

Serializes and deserializes strongly-typed classes (similarly to XML
Serialization in .NET Framework)

Optional ability to control serialization via attributes/interfaces:

	IJsonSerializable interface:
	Allows classes to control their own JSON serialization

	JsonIgnore attribute:
	Designates a property or field to not be serialized

	DefaultValue attribute:
	Member does not serialize if the value matches the DefaultValue attribute

	JsonName attribute:
	Specifies the naming to use for a property or field when serializing

	JsonSpecifiedProperty attribute:
	Specifies the name of the property which specifies if member should be serialized

Optional Type-Hinting improves deserializing to strongly-typed objects

	JsonWriter.TypeHintName & JsonReader.TypeHintName:
	Designates the name of the type hint property (e.g. "__type") and enables type hinting

Optional PrettyPrint mode helps with debugging
