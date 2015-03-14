# JsonFx.NET (v1): the JSON-savvy Ajax framework for ASP.NET

## JsonFx v2 can be found at http://github.com/jsonfx/jsonfx

This legacy repository is the source of JsonFx v1.

----

JsonFx is a fast Ajax framework for ASP.NET 2.0 and above:

  * Supports true Ajax via client-side templating with familiar ASP-style syntax
  * Supports JSON within all Ajax features
  * JsonML+Browser-Side Templating (JBST) allows data-binding JSON in the client
  * JSON-RPC 2.0 support with auto-generated JavaScript proxies
  * Build-time optimization of client libraries (compaction/Gzip/Deflate)
  * Build-time syntax checking of JavaScript & CSS
  * Easily emit data into page as JavaScript
  * Supports UserAgent-specific CSS (no more browser hacks)
  * Dynamic behavior binding features
  * Client-side globalization support
  * Ajax history (browser back button) support
  * Helps get a Grade A in YSlow

The JSON parser/serializer in JsonFx is able to be used outside of web apps:

  *  Functions similarly to the XmlSerializer in .NET
  *  Serializes and deserializes any `Dictionary<String, T>` and `IDictionary` with `String` keys directly as JSON objects
  *  Serializes and deserializes any `List<T>`, `ArrayList`, `LinkedList<T>`, `Queue<T>` and many other `IEnumerable` types directly as JSON arrays
  *  Serializes and deserializes `DateTime`, `Enum`, `Nullable<T>`, `Guid` and other common .NET Types directly as JSON primitives
  *  Serializes and deserializes strongly-typed custom classes (similarly to XML Serialization in .NET)
  *  Serializes C# 3.0 Anonymous Types directly as JSON objects
  *  Serializes C# 3.0 LINQ Queries as JSON arrays of objects (by enumerating the results)
  *  Optional ability to control serialization via attributes/interfaces
  *  Optional Type-Hinting improves deserializing to strongly-typed objects
  *  Optional PrettyPrint mode helps with debugging / human-readability
  *  Optional custom DateTime serialization override
  *  Optional native EcmaScript serialization of DateTime, Regex and identifiers
  *  Deserializes common non-JSON JavaScript literals such as "`Infinity`", "`NaN`", and "`undefined`"
  *  Ignores block and line comments when deserializing

The VSTemplates installer will add Visual Studio Project and Item Templates for the following:

  *  JsonFx-enabled ASP.NET MVC Application Project
  *  JsonFx-enabled ASP.NET Web Application Project
  *  JsonFx-enabled ASP.NET WebSite Project
  *  JBST Control Template
  *  JSON-RPC Service Template
  *  Merge File Template

**See <http://jsonfx.net> for details.**
