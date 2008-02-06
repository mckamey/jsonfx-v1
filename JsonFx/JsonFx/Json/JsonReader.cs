#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion BuildTools License

using System;
using System.ComponentModel;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace JsonFx.Json
{
	/// <summary>
	/// Reader for consuming JSON data
	/// </summary>
	public class JsonReader
	{
		#region Constants

		internal const string LiteralFalse = "false";
		internal const string LiteralTrue = "true";
		internal const string LiteralNull = "null";
		internal const string LiteralNotANumber = "NaN";
		internal const string LiteralPositiveInfinity = "Infinity";
		internal const string LiteralNegativeInfinity = "-Infinity";

		internal const char OperatorNegate = '-';
		internal const char OperatorUnaryPlus = '+';
		internal const char OperatorArrayStart = '[';
		internal const char OperatorArrayEnd = ']';
		internal const char OperatorObjectStart = '{';
		internal const char OperatorObjectEnd = '}';
		internal const char OperatorStringDelim = '"';
		internal const char OperatorStringDelimAlt = '\'';
		internal const char OperatorValueDelim = ',';
		internal const char OperatorNameDelim = ':';
		internal const char OperatorCharEscape = '\\';

		#endregion Constants
		
		#region Fields

		private readonly string Source = null;
		private readonly int SourceLength = 0;
		private int index = 0;
		private bool allowNullValueTypes = false;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="input">TextReader containing source</param>
		public JsonReader(TextReader input)
		{
			this.Source = input.ReadToEnd();
			this.SourceLength = this.Source.Length;
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="input">Stream containing source</param>
		public JsonReader(Stream input)
		{
			using (StreamReader reader = new StreamReader(input, true))
			{
				this.Source = reader.ReadToEnd();
			}
			this.SourceLength = this.Source.Length;
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="input">string containing source</param>
		public JsonReader(string input)
		{
			this.Source = input;
			this.SourceLength = this.Source.Length;
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="input">StringBuilder containing source</param>
		public JsonReader(StringBuilder input)
		{
			this.Source = input.ToString();
			this.SourceLength = this.Source.Length;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets if ValueTypes can accept values of null
		/// </summary>
		/// <remarks>
		/// Only affects deserialization: if a ValueType is assigned the
		/// value of null, it will receive the value default(TheType).
		/// Setting this to false, throws an exception if null is
		/// specified for a ValueType member.
		/// </remarks>
		public bool AllowNullValueTypes
		{
			get { return this.allowNullValueTypes; }
			set { this.allowNullValueTypes = value; }
		}

		#endregion Properties

		#region Parsing Methods

		/// <summary>
		/// Convert from JSON string to Object graph
		/// </summary>
		/// <returns></returns>
		public object Deserialize()
		{
			return this.Deserialize(null);
		}

		/// <summary>
		/// Convert from JSON string to Object graph of specific Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public object Deserialize(Type type)
		{
			// should this run through a preliminary test here?
			return this.Read(type, false);
		}

		private object Read(Type expectedType, bool typeIsHint)
		{
			if (expectedType == typeof(Object))
			{
				expectedType = null;
			}

			JsonToken token = this.Tokenize();

			switch (token)
			{
				case JsonToken.ObjectStart:
				{
					return this.ReadObject(typeIsHint ? null : expectedType);
				}
				case JsonToken.ArrayStart:
				{
					return this.ReadArray(typeIsHint ? null : expectedType);
				}
				case JsonToken.String:
				{
					return this.ReadString();
				}
				case JsonToken.Number:
				{
					return this.ReadNumber(typeIsHint ? null : expectedType);
				}
				case JsonToken.False:
				{
					this.index += JsonReader.LiteralFalse.Length;
					return false;
				}
				case JsonToken.True:
				{
					this.index += JsonReader.LiteralTrue.Length;
					return true;
				}
				case JsonToken.Null:
				{
					this.index += JsonReader.LiteralNull.Length;
					return null;
				}
				case JsonToken.NaN:
				{
					this.index += JsonReader.LiteralNotANumber.Length;
					return Double.NaN;
				}
				case JsonToken.PositiveInfinity:
				{
					this.index += JsonReader.LiteralPositiveInfinity.Length;
					return Double.PositiveInfinity;
				}
				case JsonToken.NegativeInfinity:
				{
					this.index += JsonReader.LiteralNegativeInfinity.Length;
					return Double.NegativeInfinity;
				}
				case JsonToken.End:
				default:
				{
					return null;
				}
			}
		}

		private object ReadObject(Type objectType)
		{
			if (this.Source[this.index] != JsonReader.OperatorObjectStart)
			{
				throw new JsonSerializationException("Not a valid JSON object.", this.index);
			}

			Dictionary<string, MemberInfo> propertyMap = new Dictionary<string, MemberInfo>();
			Object jsObject;
			if (objectType != null)
			{
				ConstructorInfo ctor = objectType.GetConstructor(Type.EmptyTypes);
				if (ctor == null)
				{
					throw new JsonSerializationException("Only objects with default constructors can be deserialized.", this.index);
				}
				jsObject = ctor.Invoke(null);

				// load properties into property map
				PropertyInfo[] propertyInfos = objectType.GetProperties();

				foreach (PropertyInfo info in propertyInfos)
				{
					if (!info.CanRead || !info.CanWrite)
						continue;

					if (JsonIgnoreAttribute.IsJsonIgnore(info))
						continue;

					string jsonName = JsonNameAttribute.GetJsonName(info);
					if (String.IsNullOrEmpty(jsonName))
					{
						propertyMap[info.Name] = info;
					}
					else
					{
						propertyMap[jsonName] = info;
					}
				}

				// load public fields into property map
				FieldInfo[] fieldInfos = objectType.GetFields();
				foreach (FieldInfo info in fieldInfos)
				{
					if (!info.IsPublic)
					{
						continue;
					}

					if (JsonIgnoreAttribute.IsJsonIgnore(info))
					{
						continue;
					}

					string jsonName = JsonNameAttribute.GetJsonName(info);
					if (String.IsNullOrEmpty(jsonName))
					{
						propertyMap[info.Name] = info;
					}
					else
					{
						propertyMap[jsonName] = info;
					}
				}
			}
			else
			{
				jsObject = new JsonObject();
			}

			JsonToken token;
			do
			{
				MemberInfo propertyInfo = null;

				Type propertyType = null;

				// consume opening brace or delim
				this.index++;
				if (this.index >= this.SourceLength)
				{
					throw new JsonSerializationException("Unterminated JSON object.", this.index);
				}

				// get next token
				token = this.Tokenize();
				if (token == JsonToken.ObjectEnd)
				{
					break;
				}

				if (token != JsonToken.String)
				{
					throw new JsonSerializationException("Expected JSON object property name.", this.index);
				}

				// parse object member value
				string name = this.ReadString();

				// determine the type of the property/field
				if (objectType != null && propertyMap.ContainsKey(name))
				{
					// Check properties for object member
					propertyInfo = propertyMap[name];

					if (propertyInfo is PropertyInfo)
					{
						// maps to public property
						propertyType = ((PropertyInfo)propertyInfo).PropertyType;
					}
					else if (propertyInfo is FieldInfo)
					{
						// maps to public field
						propertyType = ((FieldInfo)propertyInfo).FieldType;
					}
					else
					{
						// none found
						propertyType = null;
					}
				}
				else
				{
					// none found
					propertyType = null;
				}

				// get next token
				token = this.Tokenize();
				if (token != JsonToken.NameDelim)
				{
					throw new JsonSerializationException("Missing JSON object property name delimiter.", this.index);
				}

				// consume delim
				this.index++;
				if (this.index >= this.SourceLength)
				{
					throw new JsonSerializationException("Unterminated JSON object.", this.index);
				}

				// parse object member value
				object value = this.Read(propertyType, false);
				if (propertyInfo is PropertyInfo)
				{
					// set value of public property
					((PropertyInfo)propertyInfo).SetValue(jsObject, this.CoerceType(propertyType, value), null);
				}
				else if (propertyInfo is FieldInfo)
				{
					// set value of public field
					((FieldInfo)propertyInfo).SetValue(jsObject, this.CoerceType(propertyType, value));
				}
				else if (jsObject is IDictionary)
				{
					((IDictionary)jsObject)[name] = value;
				}

				// get next token
				token = this.Tokenize();
			} while (token == JsonToken.ValueDelim);

			if (token != JsonToken.ObjectEnd)
			{
				throw new JsonSerializationException("Unterminated JSON object.", this.index);
			}

			// consume closing brace
			this.index++;

			return jsObject;
		}

		private object CoerceType(Type targetType, object value)
		{
			if (value == null)
			{
				if (this.AllowNullValueTypes &&
					targetType.IsValueType &&
					!this.IsNullable(targetType))
				{
					throw new JsonSerializationException(targetType.Name+" does not accept null as a value", this.index);
				}
				return value;
			}

			Type actualType = value.GetType();
			if (targetType.IsAssignableFrom(actualType))
			{
				return value;
			}

			if (targetType.IsEnum)
			{
				if (!Enum.IsDefined(targetType, value))
				{
					// if isn't a defined value perhaps it is the JsonName
					foreach (FieldInfo field in targetType.GetFields())
					{
						string jsonName = JsonNameAttribute.GetJsonName(field);
						if (((string)value).Equals(jsonName))
						{
							value = field.Name;
							break;
						}
					}
				}

				if (value is String)
				{
					return Enum.Parse(targetType, (string)value);
				}
				else
				{
					return Convert.ChangeType(value, targetType);
				}
			}

			if (actualType.IsArray && !targetType.IsArray)
			{
				// targetType serializes as a JSON array but is not an array
				// assume is an ICollection / IEnumerable with AddRange, Add,
				// or custom Constructor with which we can populate it

				ConstructorInfo ctor = targetType.GetConstructor(Type.EmptyTypes);
				if (ctor == null)
				{
					throw new JsonSerializationException("Only objects with default constructors can be deserialized.", this.index);
				}
				object collection = ctor.Invoke(null);

				Array arrayValue = (Array)value;

				// many ICollection types have an AddRange method
				// which adds all items at once
				MethodInfo method = targetType.GetMethod("AddRange");
				ParameterInfo[] parameters = (method == null) ?
					null : method.GetParameters();
				Type paramType = (parameters == null || parameters.Length != 1) ?
					null : parameters[0].ParameterType;
				if (paramType != null &&
					paramType.IsAssignableFrom(actualType))
				{
					// add all members in one method
					method.Invoke(
						collection,
						new object[] { arrayValue });
					return collection;
				}
				else
				{
					// many ICollection types have an Add method
					// which adds items one at a time
					method = targetType.GetMethod("Add");
					parameters = (method == null) ?
						null : method.GetParameters();
					paramType = (parameters == null || parameters.Length != 1) ?
						null : parameters[0].ParameterType;
					if (paramType != null)
					{
						// loop through adding items to collection
						foreach (object item in arrayValue)
						{
							method.Invoke(
								collection,
								new object[] {
									this.CoerceType(paramType, item)
								});
						}
						return collection;
					}
				}

				// many ICollection types take an IEnumerable or ICollection
				// as a constructor argument.  look through constructors for
				// a compatible match.
				ConstructorInfo[] ctors = targetType.GetConstructors();
				foreach (ConstructorInfo ctor2 in ctors)
				{
					ParameterInfo[] paramList = ctor2.GetParameters();
					if (paramList.Length == 1 &&
						paramList[0].ParameterType.IsAssignableFrom(actualType))
					{
						try
						{
							// invoke first constructor that can take this value as an argument
							return ctor2.Invoke(
									new object[] { value }
								);
						}
						catch { }
					}
				}
			}

			if (value is String)
			{
				if (targetType == typeof(DateTime))
				{
					DateTime date;
					if (DateTime.TryParse(
						(string)value,
						DateTimeFormatInfo.InvariantInfo,
						DateTimeStyles.RoundtripKind|DateTimeStyles.AllowWhiteSpaces|DateTimeStyles.NoCurrentDateDefault,
						out date))
					{
						return date;
					}
				}
				else if (targetType == typeof(Guid))
				{
					// try-catch is pointless since will throw upon generic conversion
					return new Guid((string)value);
				}
				else if (targetType == typeof(Uri))
				{
					Uri uri;
					if (Uri.TryCreate((string)value, UriKind.RelativeOrAbsolute, out uri))
					{
						return uri;
					}
				}
				else if (targetType == typeof(Version))
				{
					// try-catch is pointless since will throw upon generic conversion
					return new Version((string)value);
				}
			}

			else if (value is Int64 && targetType == typeof(TimeSpan))
			{
				return new TimeSpan((long)value);
			}

			return Convert.ChangeType(value, targetType);
		}

		private bool IsNullable(Type type)
		{
			return type.IsGenericType && (typeof(Nullable<>) == type.GetGenericTypeDefinition());
		}

		private Array ReadArray(Type arrayType)
		{
			if (this.Source[this.index] != JsonReader.OperatorArrayStart)
			{
				throw new JsonSerializationException("Not a valid JSON array.", this.index);
			}

			bool isArrayTypeSet = (arrayType != null);
			bool isArrayTypeAHint = !isArrayTypeSet;

			if (isArrayTypeSet)
			{
				if (arrayType.HasElementType)
				{
					arrayType = arrayType.GetElementType();
				}
				else if (arrayType.IsGenericType)
				{
					Type[] generics = arrayType.GetGenericArguments();
					if (generics.Length != 1)
					{
						// could use the first or last, but this more correct
						arrayType = null;
					}
					else
					{
						arrayType = generics[0];
					}
				}
				else
				{
					arrayType = null;
				}
			}

			ArrayList jsArray = new ArrayList();

			JsonToken token;
			do
			{
				// consume opening bracket or delim
				this.index++;
				if (this.index >= this.SourceLength)
				{
					throw new JsonSerializationException("Unterminated JSON array.", this.index);
				}

				// get next token
				token = this.Tokenize();
				if (token == JsonToken.ArrayEnd)
				{
					break;
				}

				// parse array item
				object value = this.Read(arrayType, isArrayTypeAHint);
				jsArray.Add(value);

				// establish if array is of common type
				if (value == null)
				{
					if (arrayType != null && arrayType.IsValueType)
					{
						// use plain object to hold null
						arrayType = null;
					}
					isArrayTypeSet = true;
				}
				else if (arrayType != null && !arrayType.IsAssignableFrom(value.GetType()))
				{
					// use plain object to hold value
					arrayType = null;
					isArrayTypeSet = true;
				}
				else if (!isArrayTypeSet)
				{
					// try out special type
					// if hasn't been set before
					arrayType = value.GetType();
					isArrayTypeSet = true;
				}

				// get next token
				token = this.Tokenize();
			} while (token == JsonToken.ValueDelim);

			if (token != JsonToken.ArrayEnd)
			{
				throw new JsonSerializationException("Unterminated JSON array.", this.index);
			}

			// consume closing bracket
			this.index++;

			// check to see if all the same type and convert to that
			if (arrayType != null && arrayType != typeof(object))
			{
				return jsArray.ToArray(arrayType);
			}

			return jsArray.ToArray();
		}

		private string ReadString()
		{
			if (this.Source[this.index] != JsonReader.OperatorStringDelim &&
				this.Source[this.index] != JsonReader.OperatorStringDelimAlt)
			{
				throw new JsonSerializationException("Not a valid JSON string.", this.index);
			}

			char startStringDelim = this.Source[this.index];

			// consume opening quote
			this.index++;
			if (this.index >= this.SourceLength)
			{
				throw new JsonSerializationException("Unterminated JSON string.", this.index);
			}

			int start = this.index;
			StringBuilder builder = new StringBuilder();

			while (this.Source[this.index] != startStringDelim)
			{
				if (this.Source[this.index] == JsonReader.OperatorCharEscape)
				{
					// copy chunk before decoding
					builder.Append(this.Source, start, this.index - start);

					// consume escape char
					this.index++;
					if (this.index >= this.SourceLength)
					{
						throw new JsonSerializationException("Unterminated JSON string.", this.index);
					}

					// decode
					switch (this.Source[this.index])
					{
						case '0':
						{
							// don't allow NULL char '\0'
							// causes CStrings to terminate
							break;
						}
						case 'b':
						{
							// backspace
							builder.Append('\b');
							break;
						}
						case 'f':
						{
							// formfeed
							builder.Append('\f');
							break;
						}
						case 'n':
						{
							// newline
							builder.Append('\n');
							break;
						}
						case 'r':
						{
							// carriage return
							builder.Append('\r');
							break;
						}
						case 't':
						{
							// tab
							builder.Append('\t');
							break;
						}
						case 'u':
						{
							// Unicode escape sequence
							// e.g. Copyright: "\u00A9"

							// unicode ordinal
							int utf16;
							if (this.index+4 < this.SourceLength &&
								Int32.TryParse(
									this.Source.Substring(this.index+1, 4),
									NumberStyles.AllowHexSpecifier,
									NumberFormatInfo.InvariantInfo,
									out utf16))
							{
								builder.Append(Char.ConvertFromUtf32(utf16));
								this.index += 4;
							}
							else
							{
								// using FireFox style recovery, if not a valid hex
								// escape sequence then treat as single escaped 'u'
								// followed by rest of string
								builder.Append(this.Source[this.index]);
							}
							break;
						}
						default:
						{
							builder.Append(this.Source[this.index]);
							break;
						}
					}

					this.index++;
					if (this.index >= this.SourceLength)
					{
						throw new JsonSerializationException("Unterminated JSON string.", this.index);
					}

					start = this.index;
				}
				else
				{
					// next char
					this.index++;
					if (this.index >= this.SourceLength)
					{
						throw new JsonSerializationException("Unterminated JSON string.", this.index);
					}
				}
			}

			// copy rest of string
			builder.Append(this.Source, start, this.index-start);

			// consume closing quote
			this.index++;

			return builder.ToString();
		}

		private ValueType ReadNumber(Type expectedType)
		{
			bool hasDecimal = false;
			bool hasExponent = false;
			int start = this.index;
			int precision = 0;
			int exponent = 0;

			// optional minus part
			if (this.Source[this.index] == JsonReader.OperatorNegate)
			{
				// consume sign
				this.index++;
				if (this.index >= this.SourceLength || !Char.IsDigit(this.Source[this.index]))
					throw new JsonSerializationException("Unterminated JSON number.", this.index);
			}

			// integer part
			while ((this.index < this.SourceLength) && Char.IsDigit(this.Source[this.index]))
			{
				// consume digit
				this.index++;
			}

			// optional decimal part
			if ((this.index < this.SourceLength) && (this.Source[this.index] == '.'))
			{
				hasDecimal = true;

				// consume decimal
				this.index++;
				if (this.index >= this.SourceLength || !Char.IsDigit(this.Source[this.index]))
				{
					throw new JsonSerializationException("Unterminated JSON number.", this.index);
				}

				// fraction part
				while (this.index < this.SourceLength && Char.IsDigit(this.Source[this.index]))
				{
					// consume digit
					this.index++;
				}
			}

			// note the number of significant digits
			precision = this.index-start - (hasDecimal ? 1 : 0);

			// optional exponent part
			if (this.index < this.SourceLength && (this.Source[this.index] == 'e' || this.Source[this.index] == 'E'))
			{
				hasExponent = true;

				// consume 'e'
				this.index++;
				if (this.index >= this.SourceLength)
				{
					throw new JsonSerializationException("Unterminated JSON number.", this.index);
				}

				int expStart = this.index;

				// optional minus/plus part
				if (this.Source[this.index] == JsonReader.OperatorNegate || this.Source[this.index] == JsonReader.OperatorUnaryPlus)
				{
					// consume sign
					this.index++;
					if (this.index >= this.SourceLength || !Char.IsDigit(this.Source[this.index]))
					{
						throw new JsonSerializationException("Unterminated JSON number.", this.index);
					}
				}
				else
				{
					if (!Char.IsDigit(this.Source[this.index]))
					{
						throw new JsonSerializationException("Unterminated JSON number.", this.index);
					}
				}

				// exp part
				while (this.index < this.SourceLength && Char.IsDigit(this.Source[this.index]))
				{
					// consume digit
					this.index++;
				}

				Int32.TryParse(this.Source.Substring(expStart, this.index-expStart), NumberStyles.Integer,
					NumberFormatInfo.InvariantInfo, out exponent);
			}

			// at this point, we have the full number string and know its characteristics
			string numberString = this.Source.Substring(start, this.index - start);

			if (!hasDecimal && !hasExponent && precision < 19)
			{
				// is Integer value

				// parse as most flexible
				decimal number = Decimal.Parse(
					numberString,
					NumberStyles.Integer,
					NumberFormatInfo.InvariantInfo);

				if (expectedType != null)
				{
					TypeConverter converter = TypeDescriptor.GetConverter(typeof(Decimal));
					if (converter.CanConvertTo(expectedType))
					{
						return (ValueType)converter.ConvertTo(number, expectedType);
					}
				}

				if (number >= Int32.MinValue && number <= Int32.MaxValue)
				{
					// use most common
					return (int)number;
				}
				if (number >= Int64.MinValue && number <= Int64.MaxValue)
				{
					// use more flexible
					return (long)number;
				}

				// use most flexible
				return number;
			}
			else
			{
				// is Floating Point value

				if (expectedType == typeof(Decimal))
				{
					// special case since Double does not convert to Decimal
					return Decimal.Parse(
						numberString,
						NumberStyles.Float,
						NumberFormatInfo.InvariantInfo);
				}

				// use native EcmaScript number (IEEE 754)
				double number = Double.Parse(
					numberString,
					NumberStyles.Float,
					NumberFormatInfo.InvariantInfo);

				if (expectedType != null)
				{
					TypeConverter converter = TypeDescriptor.GetConverter(typeof(Double));
					if (converter.CanConvertTo(expectedType))
					{
						return (ValueType)converter.ConvertTo(number, expectedType);
					}
				}

				return number;
			}
		}

		#endregion Parsing Methods

		#region Tokenizing Methods

		private JsonToken Tokenize()
		{
			if (this.index >= this.SourceLength)
			{
				return JsonToken.End;
			}

			// skip whitespace
			while (Char.IsWhiteSpace(this.Source[this.index]))
			{
				this.index++;
				if (this.index >= this.SourceLength)
				{
					return JsonToken.End;
				}
			}

			// consume positive signing (as is extraneous)
			if (this.Source[this.index] == JsonReader.OperatorUnaryPlus)
			{
				this.index++;
				if (this.index >= this.SourceLength)
				{
					return JsonToken.End;
				}
			}

			switch (this.Source[this.index])
			{
				case JsonReader.OperatorArrayStart:
				{
					return JsonToken.ArrayStart;
				}
				case JsonReader.OperatorArrayEnd:
				{
					return JsonToken.ArrayEnd;
				}
				case JsonReader.OperatorObjectStart:
				{
					return JsonToken.ObjectStart;
				}
				case JsonReader.OperatorObjectEnd:
				{
					return JsonToken.ObjectEnd;
				}
				case JsonReader.OperatorStringDelim:
				case JsonReader.OperatorStringDelimAlt:
				{
					return JsonToken.String;
				}
				case JsonReader.OperatorValueDelim:
				{
					return JsonToken.ValueDelim;
				}
				case JsonReader.OperatorNameDelim:
				{
					return JsonToken.NameDelim;
				}
				default:
				{
					break;
				}
			}

			// number
			if (Char.IsDigit(this.Source[this.index]) ||
				((this.Source[this.index] == JsonReader.OperatorNegate) && (this.index+1 < this.SourceLength) && Char.IsDigit(this.Source[this.index+1])))
			{
				return JsonToken.Number;
			}

			// "false" literal
			if (this.MatchLiteral(JsonReader.LiteralFalse))
			{
				return JsonToken.False;
			}

			// "true" literal
			if (this.MatchLiteral(JsonReader.LiteralTrue))
			{
				return JsonToken.True;
			}

			// "null" literal
			if (this.MatchLiteral(JsonReader.LiteralNull))
			{
				return JsonToken.Null;
			}

			// "NaN" literal
			if (this.MatchLiteral(JsonReader.LiteralNotANumber))
			{
				return JsonToken.NaN;
			}

			// "Infinity" literal
			if (this.MatchLiteral(JsonReader.LiteralPositiveInfinity))
			{
				return JsonToken.PositiveInfinity;
			}

			// "-Infinity" literal
			if (this.MatchLiteral(JsonReader.LiteralNegativeInfinity))
			{
				return JsonToken.NegativeInfinity;
			}

			throw new JsonSerializationException("Illegal JSON sequence.", this.index);
		}

		/// <summary>
		/// Determines if the next token is the given literal
		/// </summary>
		/// <param name="literal"></param>
		/// <returns></returns>
		private bool MatchLiteral(string literal)
		{
			int literalLength = literal.Length;
			for (int i=0, j=this.index; i<literalLength && j<this.SourceLength; i++, j++)
			{
				if (literal[i] != this.Source[j])
				{
					return false;
				}
			}

			return true;
		}

		#endregion Tokenizing Methods
	}
}
