JsonFx.Json v2.0

Improvements over v1.x
- ground up rewrite for better flexibiliy, performance?, memory consumption, code clarity
	- TextReader =(chars)=> ITokenizer<TokenType> =(tokens)=> IParser<TokenType> =(objects)=> IDataReader
	- IDataWriter =(objects)=> IGenerator<TokenType> =(tokens)=> IFormatter<TokenType> =(chars)=> TextWriter
- stream based
- cleaner / simpler interface for JsonReader/JsonWriter
- JsonTokenizer is essentially a SAX-like token generator for JSON grammar
- more modular (separate tokenizer, parser, coercion, settings)
- even more flexible automatic type coercion
	- can deserialize common collection interfaces
	- IEnumerable, IList, ICollection, IDictionary, IEnumerable<T>, IList<T>, ICollection<T>, IDictionary<TKey, TVal>
- optionally supports unbounded nested objects
- interfaces extensible to other serialization formats/engines for building RESTful services

TODO:
- find best intermediate format to be able to cross-serialize between formats
- split DataReaderSettings into type coercion and settings
- evaluate ctor vs. property dependency injection for each class
- check for graph cycles in JsonWriter using a stack while walking: [check stack, push, visit children, pop]
- ensure JsonFx.Json.JsonReader is threadsafe

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
