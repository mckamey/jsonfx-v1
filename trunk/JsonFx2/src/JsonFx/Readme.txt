JsonFx.Json v2.0

Improvements over v1.x
- ground up rewrite for better flexibiliy, performance?, memory consumption, code clarity
	- TextReader =(chars)=> Tokenizer<TokenType> =(tokens)=> Analyzer<TokenType> =(objects)=> DataReader
	- DataWriter =(objects)=> Walker<TokenType> =(tokens)=> Formatter<TokenType> =(chars)=> TextWriter
- stream based
- can support Comet-like deserialization of streamed objects
- cleaner / simpler interface for JsonReader/JsonWriter
- JsonTokenizer is essentially a token generator for JSON grammar
- more modular (separate tokenizer, analyzer, coercion, settings)
- even more flexible automatic type coercion
	- can deserialize common collection interfaces
	- IEnumerable, IList, ICollection, IDictionary, IEnumerable<T>, IList<T>, ICollection<T>, IDictionary<TKey, TVal>
- optionally supports unbounded nested objects
- interfaces extensible to other serialization formats/engines for building RESTful services
- allows continually be deserializing objects off an open streamed socket like Comet

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
- allow trailing tokens allows this to parse a JSON stream inside other structures for example <[CDATA[ ... ]]>
