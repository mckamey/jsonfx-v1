using System;
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

		#endregion Fields

		#region Init

		public JsonReader(TextReader input)
		{
			this.Source = input.ReadToEnd();
			this.SourceLength = this.Source.Length;
		}

		public JsonReader(Stream input)
		{
			using (StreamReader reader = new StreamReader(input, true))
			{
				this.Source = reader.ReadToEnd();
			}
			this.SourceLength = this.Source.Length;
		}

		public JsonReader(string input)
		{
			this.Source = input;
			this.SourceLength = this.Source.Length;
		}

		public JsonReader(StringBuilder input)
		{
			this.Source = input.ToString();
			this.SourceLength = this.Source.Length;
		}

		#endregion Init

		#region Parsing Methods

		public object Deserialize()
		{
			return this.Deserialize(null);
		}

		public object Deserialize(Type type)
		{
			// should this run through a preliminary test here?
			return this.Read(type);
		}

		private object Read(Type expectedType)
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
					return this.ReadObject(expectedType);
				}
				case JsonToken.ArrayStart:
				{
					return this.ReadArray(expectedType);
				}
				case JsonToken.String:
				{
					return this.ReadString();
				}
				case JsonToken.Number:
				{
					return this.ReadNumber();
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
				throw new JsonSerializationException("Not a valid JSON object.", this.index);

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
					throw new JsonSerializationException("Unterminated JSON object.", this.index);

				// get next token
				token = this.Tokenize();
				if (token != JsonToken.String)
					throw new JsonSerializationException("Invalid JSON object.", this.index);

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
					throw new JsonSerializationException("Invalid JSON object.", this.index);

				// consume delim
				this.index++;
				if (this.index >= this.SourceLength)
					throw new JsonSerializationException("Unterminated JSON object.", this.index);

				// parse object member value
				object value = this.Read(propertyType);
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
				else if (jsObject is JsonObject)
				{
					((JsonObject)jsObject)[name] = value;
				}

				// get next token
				token = this.Tokenize();
			} while (token == JsonToken.ValueDelim);

			if (token != JsonToken.ObjectEnd)
				throw new JsonSerializationException("Invalid JSON object.", this.index);

			// consume closing brace
			this.index++;

			return jsObject;
		}

		private object CoerceType(Type targetType, object value)
		{
			if (value == null || targetType.IsAssignableFrom(value.GetType()))
				return value;

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
					return Enum.Parse(targetType, (string)value);
				else
					return Convert.ChangeType(value, targetType);
			}

			return value;
		}

		private Array ReadArray(Type arrayType)
		{
			if (this.Source[this.index] != JsonReader.OperatorArrayStart)
				throw new JsonSerializationException("Not a valid JSON array.", this.index);

			bool isArrayTypeSet = (arrayType != null);
			if (isArrayTypeSet && arrayType.IsArray)
				arrayType = arrayType.GetElementType();

			ArrayList jsArray = new ArrayList();

			JsonToken token;
			do
			{
				// consume opening bracket or delim
				this.index++;
				if (this.index >= this.SourceLength)
					throw new JsonSerializationException("Unterminated JSON array.", this.index);

				// get next token
				token = this.Tokenize();
				if (token == JsonToken.ArrayEnd)
					break;

				// parse array item
				object value = this.Read(arrayType);
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
				throw new JsonSerializationException("Invalid JSON array.", this.index);

			// consume closing bracket
			this.index++;

			// check to see if all the same type and convert to that
			if (arrayType != null && arrayType != typeof(object))
				return jsArray.ToArray(arrayType);

			return jsArray.ToArray();
		}

		private string ReadString()
		{
			if (this.Source[this.index] != JsonReader.OperatorStringDelim &&
				this.Source[this.index] != JsonReader.OperatorStringDelimAlt)
				throw new JsonSerializationException("Not a valid JSON string.", this.index);

			char startStringDelim = this.Source[this.index];

			// consume opening quote
			this.index++;
			if (this.index >= this.SourceLength)
				throw new JsonSerializationException("Unterminated JSON string.", this.index);

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
						throw new JsonSerializationException("Unterminated JSON string.", this.index);

					// decode
					switch (this.Source[this.index])
					{
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
						throw new JsonSerializationException("Unterminated JSON string.", this.index);

					start = this.index;
				}
				else
				{
					// next char
					this.index++;
					if (this.index >= this.SourceLength)
						throw new JsonSerializationException("Unterminated JSON string.", this.index);
				}
			}

			// copy rest of string
			builder.Append(this.Source, start, this.index-start);

			// consume closing quote
			this.index++;

			return builder.ToString();
		}

		private ValueType ReadNumber()
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
					throw new JsonSerializationException("Unterminated JSON number.", this.index);

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
					throw new JsonSerializationException("Unterminated JSON number.", this.index);

				int expStart = this.index;

				// optional minus/plus part
				if (this.Source[this.index] == JsonReader.OperatorNegate || this.Source[this.index] == JsonReader.OperatorUnaryPlus)
				{
					// consume sign
					this.index++;
					if (this.index >= this.SourceLength || !Char.IsDigit(this.Source[this.index]))
						throw new JsonSerializationException("Unterminated JSON number.", this.index);
				}
				else
				{
					if (!Char.IsDigit(this.Source[this.index]))
						throw new JsonSerializationException("Unterminated JSON number.", this.index);
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

			string numberString = this.Source.Substring(start, this.index - start);
			if (!hasDecimal && !hasExponent && precision < 19)
			{
				// Integer value
				try
				{
					// try most common
					return Int32.Parse(
						numberString,
						NumberStyles.Integer,
						NumberFormatInfo.InvariantInfo);
				}
				catch (OverflowException)
				{
					try
					{
						// try more flexible
						return Int64.Parse(
							numberString,
							NumberStyles.Integer,
							NumberFormatInfo.InvariantInfo);
					}
					catch (OverflowException)
					{
						// try most flexible
						return Decimal.Parse(
							numberString,
							NumberStyles.Integer,
							NumberFormatInfo.InvariantInfo);
					}
				}
			}
			else
			{
				// Floating point
				if (precision > 15 && (exponent >= -28 && exponent <= 28))
				{
					// if better precision is needed than double can give
					// use decimal unless a larger exponent is needed
					try
					{
						// try most common
						return Decimal.Parse(
							numberString,
							NumberStyles.Float,
							NumberFormatInfo.InvariantInfo);
					}
					catch (OverflowException)
					{
						// try most flexible
						return Double.Parse(
							numberString,
							NumberStyles.Float,
							NumberFormatInfo.InvariantInfo);
					}
				}
				else
				{
					// try most flexible
					return Double.Parse(
						numberString,
						NumberStyles.Float,
						NumberFormatInfo.InvariantInfo);
				}
			}
		}

		#endregion Parsing Methods

		#region Tokenizing Methods

		private JsonToken Tokenize()
		{
			if (this.index >= this.SourceLength)
				return JsonToken.End;

			// skip whitespace
			while (Char.IsWhiteSpace(this.Source[this.index]))
			{
				this.index++;
				if (this.index >= this.SourceLength)
					return JsonToken.End;
			}

			// consume positive signing (as is extraneous)
			if (this.Source[this.index] == JsonReader.OperatorUnaryPlus)
			{
				this.index++;
				if (this.index >= this.SourceLength)
					return JsonToken.End;
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
			if (JsonReader.LiteralFalse.Length + this.index <= this.SourceLength &&
				JsonReader.LiteralFalse.Equals(this.Source.Substring(this.index, JsonReader.LiteralFalse.Length), StringComparison.InvariantCulture))
			{
				return JsonToken.False;
			}

			// "true" literal
			if (JsonReader.LiteralTrue.Length + this.index <= this.SourceLength &&
				JsonReader.LiteralTrue.Equals(this.Source.Substring(this.index, JsonReader.LiteralTrue.Length), StringComparison.InvariantCulture))
			{
				return JsonToken.True;
			}

			// "null" literal
			if (JsonReader.LiteralNull.Length + this.index <= this.SourceLength &&
				JsonReader.LiteralNull.Equals(this.Source.Substring(this.index, JsonReader.LiteralNull.Length), StringComparison.InvariantCulture))
			{
				return JsonToken.Null;
			}

			// "NaN" literal
			if (JsonReader.LiteralNotANumber.Length + this.index <= this.SourceLength &&
				JsonReader.LiteralNotANumber.Equals(this.Source.Substring(this.index, JsonReader.LiteralNotANumber.Length), StringComparison.InvariantCulture))
			{
				return JsonToken.NaN;
			}

			// "Infinity" literal
			if (JsonReader.LiteralPositiveInfinity.Length + this.index <= this.SourceLength &&
				JsonReader.LiteralPositiveInfinity.Equals(this.Source.Substring(this.index, JsonReader.LiteralPositiveInfinity.Length), StringComparison.InvariantCulture))
			{
				return JsonToken.PositiveInfinity;
			}

			// "-Infinity" literal
			if (JsonReader.LiteralNegativeInfinity.Length + this.index <= this.SourceLength &&
				JsonReader.LiteralNegativeInfinity.Equals(this.Source.Substring(this.index, JsonReader.LiteralNegativeInfinity.Length), StringComparison.InvariantCulture))
			{
				return JsonToken.NegativeInfinity;
			}

			throw new JsonSerializationException("Unrecognized JSON text sequence.", this.index);
		}

		#endregion Tokenizing Methods
	}
}
