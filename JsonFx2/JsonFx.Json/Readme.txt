JsonFx.Json v2.0

Improvements over v1.x
- ground up rewrite for better flexibiliy, performance, memory consumption, code clarity
	- TextReader =(chars)=> ITokenizer<TokenType> =(tokens)=> IParser<TokenType> =(objects)=> IDataReader
	- IDataWriter =(objects)=> IGenerator<TokenType> =(tokens)=> IFormatter<TokenType> =(chars)=> TextWriter
- stream based
- cleaner / simpler interface for JsonReader/JsonWriter
- more modular (separate tokenizer, parser, coercion, settings)
- JsonFx.Json.JsonReader is threadsafe
- supports unbounded nested objects
- interfaces extensible to other serialization formats for REST
- even more flexible automatic type coercion
	- can deserialize common collection interfaces
	- IEnumerable, IList, ICollection, IDictionary, IEnumerable<T>, IList<T>, ICollection<T>, IDictionary<TKey, TVal>

TODO:
- split DataReaderSettings into type coercion and settings
- evaluate ctor vs. property dependency injection
- check for cycles in JsonWriter (using a stack?)

Same as v1.x
- fast
- supports .NET Framework 2.0 and above
- most flexible JSON serializer for .NET Framework
	- supports POCO classes
	- supports C# 3.0 Anonymous objects
	- supports serializing Linq queries
- JSON is deserialized to standard BCL types (not JSON-specific types)
	- Object => System.Collections.Generic.Dictionary<string, object>
	- Array => System.Array
	- Boolean => System.Boolean
	- Number => System.Int32, System.Int64, System.Double, System.Decimal
	- null, undefined => null
- follows Postel's Law by being less rigid about JSON input
	- ignores block and line comments
	- supports single quoted String
	- supports tabs in String
	- supports unquoted keys in Object
	- supports NaN, Infinity, undefined
	- supports Numbers beyond IEEE-754
