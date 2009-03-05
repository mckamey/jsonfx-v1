#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
#endregion License

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
		internal const string LiteralUndefined = "undefined";
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

		private const string CommentStart = "/*";
		private const string CommentEnd = "*/";
		private const string CommentLine = "//";
		private const string LineEndings = "\r\n";

		internal const string TypeGenericIDictionary = "System.Collections.Generic.IDictionary`2";

		private const string ErrorUnrecognizedToken = "Illegal JSON sequence.";
		private const string ErrorUnterminatedComment = "Unterminated comment block.";
		private const string ErrorUnterminatedObject = "Unterminated JSON object.";
		private const string ErrorUnterminatedArray = "Unterminated JSON array.";
		private const string ErrorUnterminatedString = "Unterminated JSON string.";
		private const string ErrorIllegalNumber = "Illegal JSON number.";
		private const string ErrorExpectedString = "Expected JSON string.";
		private const string ErrorExpectedObject = "Expected JSON object.";
		private const string ErrorExpectedArray = "Expected JSON array.";
		private const string ErrorExpectedPropertyName = "Expected JSON object property name.";
		private const string ErrorExpectedPropertyNameDelim = "Expected JSON object property name delimiter.";
		internal const string ErrorGenericIDictionary = "Types which implement Generic IDictionary<TKey, TValue> also need to implement IDictionary to be deserialized. ({0})";
		private const string ErrorGenericIDictionaryKeys = "Types which implement Generic IDictionary<TKey, TValue> need to have string keys to be deserialized. ({0})";

		#endregion Constants

		#region ReaderState

		private class ReaderState
		{
			#region Fields

			private Dictionary<Type, Dictionary<string, MemberInfo>> memberMapCache;

			internal bool AllowNullValueTypes;
			internal string TypeHintName;
			internal int Index;

			#endregion Fields

			#region Properties

			internal Dictionary<Type, Dictionary<string, MemberInfo>> MemberMapCache
			{
				get
				{
					if (this.memberMapCache == null)
					{
						// instantiate space for cache
						this.memberMapCache = new Dictionary<Type, Dictionary<string, MemberInfo>>();
					}
					return this.memberMapCache;
				}
			}

			#endregion Properties
		}

		#endregion ReaderState

		#region Fields

		private readonly ReaderState State = new ReaderState();
		private readonly TypeCoercionUtility Coercion;
		private readonly string Source = null;
		private readonly int SourceLength = 0;

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
			this.Coercion = new TypeCoercionUtility(this.State);
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
			this.Coercion = new TypeCoercionUtility(this.State);
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="input">string containing source</param>
		public JsonReader(string input)
		{
			this.Source = input;
			this.SourceLength = this.Source.Length;
			this.Coercion = new TypeCoercionUtility(this.State);
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="input">StringBuilder containing source</param>
		public JsonReader(StringBuilder input)
		{
			this.Source = input.ToString();
			this.SourceLength = this.Source.Length;
			this.Coercion = new TypeCoercionUtility(this.State);
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
			get { return this.State.AllowNullValueTypes; }
			set { this.State.AllowNullValueTypes = value; }
		}

		/// <summary>
		/// Gets and sets the property name used for type hinting.
		/// </summary>
		public string TypeHintName
		{
			get { return this.State.TypeHintName; }
			set { this.State.TypeHintName = value; }
		}

		#endregion Properties

		#region Parsing Methods

		/// <summary>
		/// Convert from JSON string to Object graph
		/// </summary>
		/// <returns></returns>
		public object Deserialize()
		{
			return this.Deserialize((Type)null);
		}

		/// <summary>
		/// Convert from JSON string to Object graph
		/// </summary>
		/// <returns></returns>
		public object Deserialize(int start)
		{
			this.State.Index = start;
			return this.Deserialize((Type)null);
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

		/// <summary>
		/// Convert from JSON string to Object graph of specific Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public T Deserialize<T>()
		{
			// should this run through a preliminary test here?
			return (T)this.Read(typeof(T), false);
		}

		/// <summary>
		/// Convert from JSON string to Object graph of specific Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public object Deserialize(int start, Type type)
		{
			this.State.Index = start;

			// should this run through a preliminary test here?
			return this.Read(type, false);
		}

		/// <summary>
		/// Convert from JSON string to Object graph of specific Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public T Deserialize<T>(int start)
		{
			this.State.Index = start;

			// should this run through a preliminary test here?
			return (T)this.Read(typeof(T), false);
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
					return this.ReadString(typeIsHint ? null : expectedType);
				}
				case JsonToken.Number:
				{
					return this.ReadNumber(typeIsHint ? null : expectedType);
				}
				case JsonToken.False:
				{
					this.State.Index += JsonReader.LiteralFalse.Length;
					return false;
				}
				case JsonToken.True:
				{
					this.State.Index += JsonReader.LiteralTrue.Length;
					return true;
				}
				case JsonToken.Null:
				{
					this.State.Index += JsonReader.LiteralNull.Length;
					return null;
				}
				case JsonToken.NaN:
				{
					this.State.Index += JsonReader.LiteralNotANumber.Length;
					return Double.NaN;
				}
				case JsonToken.PositiveInfinity:
				{
					this.State.Index += JsonReader.LiteralPositiveInfinity.Length;
					return Double.PositiveInfinity;
				}
				case JsonToken.NegativeInfinity:
				{
					this.State.Index += JsonReader.LiteralNegativeInfinity.Length;
					return Double.NegativeInfinity;
				}
				case JsonToken.Undefined:
				{
					this.State.Index += JsonReader.LiteralUndefined.Length;
					return null;
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
			if (this.Source[this.State.Index] != JsonReader.OperatorObjectStart)
			{
				throw new JsonDeserializationException(JsonReader.ErrorExpectedObject, this.State.Index);
			}

			Type genericDictionaryType = null;
			Dictionary<string, MemberInfo> memberMap = null;
			Object result;
			if (objectType != null)
			{
				result = this.Coercion.InstantiateObject(objectType, out memberMap);

				if (memberMap == null)
				{
					// this allows specific IDictionary<string, T> to deserialize T
					Type genericDictionary = objectType.GetInterface(JsonReader.TypeGenericIDictionary);
					if (genericDictionary != null)
					{
						Type[] genericArgs = genericDictionary.GetGenericArguments();
						if (genericArgs.Length == 2)
						{
							if (genericArgs[0] != typeof(String))
							{
								throw new JsonDeserializationException(
									String.Format(JsonReader.ErrorGenericIDictionaryKeys, objectType),
									this.State.Index);
							}

							if (genericArgs[1] != typeof(Object))
							{
								genericDictionaryType = genericArgs[1];
							}
						}
					}
				}
			}
			else
			{
				result = new Dictionary<String, Object>();
			}

			JsonToken token;
			do
			{
				Type memberType;
				MemberInfo memberInfo;

				// consume opening brace or delim
				this.State.Index++;
				if (this.State.Index >= this.SourceLength)
				{
					throw new JsonDeserializationException(JsonReader.ErrorUnterminatedObject, this.State.Index);
				}

				// get next token
				token = this.Tokenize();
				if (token == JsonToken.ObjectEnd)
				{
					break;
				}

				if (token != JsonToken.String)
				{
					throw new JsonDeserializationException(JsonReader.ErrorExpectedPropertyName, this.State.Index);
				}

				// parse object member value
				string memberName = (String)this.ReadString(null);

				if (genericDictionaryType == null && memberMap != null)
				{
					// determine the type of the property/field
					TypeCoercionUtility.GetMemberInfo(memberMap, memberName, out memberType, out memberInfo);
				}
				else
				{
					memberType = genericDictionaryType;
					memberInfo = null;
				}

				// get next token
				token = this.Tokenize();
				if (token != JsonToken.NameDelim)
				{
					throw new JsonDeserializationException(JsonReader.ErrorExpectedPropertyNameDelim, this.State.Index);
				}

				// consume delim
				this.State.Index++;
				if (this.State.Index >= this.SourceLength)
				{
					throw new JsonDeserializationException(JsonReader.ErrorUnterminatedObject, this.State.Index);
				}

				// parse object member value
				object value = this.Read(memberType, false);

				if (result is IDictionary)
				{
					if (objectType == null &&
						!String.IsNullOrEmpty(this.State.TypeHintName) &&
						this.State.TypeHintName.Equals(memberName, StringComparison.InvariantCulture))
					{
						result = this.Coercion.ProcessTypeHint((IDictionary)result, value as string, this.State, out objectType, out memberMap);
					}
					else
					{
						((IDictionary)result)[memberName] = value;
					}
				}
				else if (objectType.GetInterface(JsonReader.TypeGenericIDictionary) != null)
				{
					throw new JsonDeserializationException(
						String.Format(JsonReader.ErrorGenericIDictionary, objectType),
						this.State.Index);
				}
				else
				{
					this.Coercion.SetMemberValue(result, memberType, memberInfo, value);
				}

				// get next token
				token = this.Tokenize();
			} while (token == JsonToken.ValueDelim);

			if (token != JsonToken.ObjectEnd)
			{
				throw new JsonDeserializationException(JsonReader.ErrorUnterminatedObject, this.State.Index);
			}

			// consume closing brace
			this.State.Index++;

			return result;
		}

		private IEnumerable ReadArray(Type arrayType)
		{
			if (this.Source[this.State.Index] != JsonReader.OperatorArrayStart)
			{
				throw new JsonDeserializationException(JsonReader.ErrorExpectedArray, this.State.Index);
			}

			bool isArrayItemTypeSet = (arrayType != null);
			bool isArrayTypeAHint = !isArrayItemTypeSet;
			Type arrayItemType = null;

			if (isArrayItemTypeSet)
			{
				if (arrayType.HasElementType)
				{
					arrayItemType = arrayType.GetElementType();
				}
				else if (arrayType.IsGenericType)
				{
					Type[] generics = arrayType.GetGenericArguments();
					if (generics.Length == 1)
					{
						// could use the first or last, but this more correct
						arrayItemType = generics[0];
					}
				}
			}

			// using ArrayList since has .ToArray(Type) method
			// cannot create generic list at runtime
			ArrayList jsArray = new ArrayList();

			JsonToken token;
			do
			{
				// consume opening bracket or delim
				this.State.Index++;
				if (this.State.Index >= this.SourceLength)
				{
					throw new JsonDeserializationException(JsonReader.ErrorUnterminatedArray, this.State.Index);
				}

				// get next token
				token = this.Tokenize();
				if (token == JsonToken.ArrayEnd)
				{
					break;
				}

				// parse array item
				object value = this.Read(arrayItemType, isArrayTypeAHint);
				jsArray.Add(value);

				// establish if array is of common type
				if (value == null)
				{
					if (arrayItemType != null && arrayItemType.IsValueType)
					{
						// use plain object to hold null
						arrayItemType = null;
					}
					isArrayItemTypeSet = true;
				}
				else if (arrayItemType != null && !arrayItemType.IsAssignableFrom(value.GetType()))
				{
					if (value.GetType().IsAssignableFrom(arrayItemType))
					{
						// attempt to use the more general type
						arrayItemType = value.GetType();
					}
					else
					{
						// use plain object to hold value
						arrayItemType = null;
						isArrayItemTypeSet = true;
					}
				}
				else if (!isArrayItemTypeSet)
				{
					// try out a hint type
					// if hasn't been set before
					arrayItemType = value.GetType();
					isArrayItemTypeSet = true;
				}

				// get next token
				token = this.Tokenize();
			} while (token == JsonToken.ValueDelim);

			if (token != JsonToken.ArrayEnd)
			{
				throw new JsonDeserializationException(JsonReader.ErrorUnterminatedArray, this.State.Index);
			}

			// consume closing bracket
			this.State.Index++;

			// TODO: optimize to reduce number of conversions on lists

			if (arrayItemType != null && arrayItemType != typeof(object))
			{
				// if all items are of same type then convert to array of that type
				return jsArray.ToArray(arrayItemType);
			}

			// convert to an object array for consistency
			return jsArray.ToArray();
		}

		/// <summary>
		/// Reads a JSON string
		/// </summary>
		/// <param name="expectedType"></param>
		/// <returns>string or value which is represented as a string in JSON</returns>
		private object ReadString(Type expectedType)
		{
			if (this.Source[this.State.Index] != JsonReader.OperatorStringDelim &&
				this.Source[this.State.Index] != JsonReader.OperatorStringDelimAlt)
			{
				throw new JsonDeserializationException(JsonReader.ErrorExpectedString, this.State.Index);
			}

			char startStringDelim = this.Source[this.State.Index];

			// consume opening quote
			this.State.Index++;
			if (this.State.Index >= this.SourceLength)
			{
				throw new JsonDeserializationException(JsonReader.ErrorUnterminatedString, this.State.Index);
			}

			int start = this.State.Index;
			StringBuilder builder = new StringBuilder();

			while (this.Source[this.State.Index] != startStringDelim)
			{
				if (this.Source[this.State.Index] == JsonReader.OperatorCharEscape)
				{
					// copy chunk before decoding
					builder.Append(this.Source, start, this.State.Index - start);

					// consume escape char
					this.State.Index++;
					if (this.State.Index >= this.SourceLength)
					{
						throw new JsonDeserializationException(JsonReader.ErrorUnterminatedString, this.State.Index);
					}

					// decode
					switch (this.Source[this.State.Index])
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
							if (this.State.Index+4 < this.SourceLength &&
								Int32.TryParse(
									this.Source.Substring(this.State.Index+1, 4),
									NumberStyles.AllowHexSpecifier,
									NumberFormatInfo.InvariantInfo,
									out utf16))
							{
								builder.Append(Char.ConvertFromUtf32(utf16));
								this.State.Index += 4;
							}
							else
							{
								// using FireFox style recovery, if not a valid hex
								// escape sequence then treat as single escaped 'u'
								// followed by rest of string
								builder.Append(this.Source[this.State.Index]);
							}
							break;
						}
						default:
						{
							builder.Append(this.Source[this.State.Index]);
							break;
						}
					}

					this.State.Index++;
					if (this.State.Index >= this.SourceLength)
					{
						throw new JsonDeserializationException(JsonReader.ErrorUnterminatedString, this.State.Index);
					}

					start = this.State.Index;
				}
				else
				{
					// next char
					this.State.Index++;
					if (this.State.Index >= this.SourceLength)
					{
						throw new JsonDeserializationException(JsonReader.ErrorUnterminatedString, this.State.Index);
					}
				}
			}

			// copy rest of string
			builder.Append(this.Source, start, this.State.Index-start);

			// consume closing quote
			this.State.Index++;

			if (expectedType != null && expectedType != typeof(String))
			{
				return this.Coercion.CoerceType(expectedType, builder.ToString());
			}

			return builder.ToString();
		}

		private object ReadNumber(Type expectedType)
		{
			bool hasDecimal = false;
			bool hasExponent = false;
			int start = this.State.Index;
			int precision = 0;
			int exponent = 0;

			// optional minus part
			if (this.Source[this.State.Index] == JsonReader.OperatorNegate)
			{
				// consume sign
				this.State.Index++;
				if (this.State.Index >= this.SourceLength || !Char.IsDigit(this.Source[this.State.Index]))
					throw new JsonDeserializationException(JsonReader.ErrorIllegalNumber, this.State.Index);
			}

			// integer part
			while ((this.State.Index < this.SourceLength) && Char.IsDigit(this.Source[this.State.Index]))
			{
				// consume digit
				this.State.Index++;
			}

			// optional decimal part
			if ((this.State.Index < this.SourceLength) && (this.Source[this.State.Index] == '.'))
			{
				hasDecimal = true;

				// consume decimal
				this.State.Index++;
				if (this.State.Index >= this.SourceLength || !Char.IsDigit(this.Source[this.State.Index]))
				{
					throw new JsonDeserializationException(JsonReader.ErrorIllegalNumber, this.State.Index);
				}

				// fraction part
				while (this.State.Index < this.SourceLength && Char.IsDigit(this.Source[this.State.Index]))
				{
					// consume digit
					this.State.Index++;
				}
			}

			// note the number of significant digits
			precision = this.State.Index-start - (hasDecimal ? 1 : 0);

			// optional exponent part
			if (this.State.Index < this.SourceLength && (this.Source[this.State.Index] == 'e' || this.Source[this.State.Index] == 'E'))
			{
				hasExponent = true;

				// consume 'e'
				this.State.Index++;
				if (this.State.Index >= this.SourceLength)
				{
					throw new JsonDeserializationException(JsonReader.ErrorIllegalNumber, this.State.Index);
				}

				int expStart = this.State.Index;

				// optional minus/plus part
				if (this.Source[this.State.Index] == JsonReader.OperatorNegate || this.Source[this.State.Index] == JsonReader.OperatorUnaryPlus)
				{
					// consume sign
					this.State.Index++;
					if (this.State.Index >= this.SourceLength || !Char.IsDigit(this.Source[this.State.Index]))
					{
						throw new JsonDeserializationException(JsonReader.ErrorIllegalNumber, this.State.Index);
					}
				}
				else
				{
					if (!Char.IsDigit(this.Source[this.State.Index]))
					{
						throw new JsonDeserializationException(JsonReader.ErrorIllegalNumber, this.State.Index);
					}
				}

				// exp part
				while (this.State.Index < this.SourceLength && Char.IsDigit(this.Source[this.State.Index]))
				{
					// consume digit
					this.State.Index++;
				}

				Int32.TryParse(this.Source.Substring(expStart, this.State.Index-expStart), NumberStyles.Integer,
					NumberFormatInfo.InvariantInfo, out exponent);
			}

			// at this point, we have the full number string and know its characteristics
			string numberString = this.Source.Substring(start, this.State.Index - start);

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
					return this.Coercion.CoerceType(expectedType, number);
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
					return this.Coercion.CoerceType(expectedType, number);
				}

				return number;
			}
		}

		#endregion Parsing Methods

		#region Static Methods

		/// <summary>
		/// A fast method for deserializing an object from JSON
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static object Deserialize(string value)
		{
			return JsonReader.Deserialize(value, 0, null);
		}

		/// <summary>
		/// A fast method for deserializing an object from JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static T Deserialize<T>(string value)
		{
			return (T)JsonReader.Deserialize(value, 0, typeof(T));
		}

		/// <summary>
		/// A fast method for deserializing an object from JSON
		/// </summary>
		/// <param name="value"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		public static object Deserialize(string value, int start)
		{
			return JsonReader.Deserialize(value, start, null);
		}

		/// <summary>
		/// A fast method for deserializing an object from JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		public static T Deserialize<T>(string value, int start)
		{
			return (T)JsonReader.Deserialize(value, start, typeof(T));
		}

		/// <summary>
		/// A fast method for deserializing an object from JSON
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object Deserialize(string value, Type type)
		{
			return JsonReader.Deserialize(value, 0, type);
		}

		/// <summary>
		/// A fast method for deserializing an object from JSON
		/// </summary>
		/// <param name="value">source text</param>
		/// <param name="start">starting position</param>
		/// <param name="type">expected type</param>
		/// <returns></returns>
		public static object Deserialize(string value, int start, Type type)
		{
			return (new JsonReader(value)).Deserialize(start, type);
		}

		#endregion Static Methods

		#region Tokenizing Methods

		private JsonToken Tokenize()
		{
			if (this.State.Index >= this.SourceLength)
			{
				return JsonToken.End;
			}

			// skip whitespace
			while (Char.IsWhiteSpace(this.Source[this.State.Index]))
			{
				this.State.Index++;
				if (this.State.Index >= this.SourceLength)
				{
					return JsonToken.End;
				}
			}

			#region Skip Comments

			// skip block and line comments
			if (this.Source[this.State.Index] == JsonReader.CommentStart[0])
			{
				if (this.State.Index+1 >= this.SourceLength)
				{
					throw new JsonDeserializationException(JsonReader.ErrorUnrecognizedToken, this.State.Index);
				}

				// skip over first char of comment start
				this.State.Index++;

				bool isBlockComment = false;
				if (this.Source[this.State.Index] == JsonReader.CommentStart[1])
				{
					isBlockComment = true;
				}
				else if (this.Source[this.State.Index] != JsonReader.CommentLine[1])
				{
					throw new JsonDeserializationException(JsonReader.ErrorUnrecognizedToken, this.State.Index);
				}
				// skip over second char of comment start
				this.State.Index++;

				if (isBlockComment)
				{
					// store index for unterminated case
					int commentStart = this.State.Index-2;

					if (this.State.Index+1 >= this.SourceLength)
					{
						throw new JsonDeserializationException(JsonReader.ErrorUnterminatedComment, commentStart);
					}

					// skip over everything until reach block comment ending
					while (this.Source[this.State.Index] != JsonReader.CommentEnd[0] ||
						this.Source[this.State.Index+1] != JsonReader.CommentEnd[1])
					{
						this.State.Index++;
						if (this.State.Index+1 >= this.SourceLength)
						{
							throw new JsonDeserializationException(JsonReader.ErrorUnterminatedComment, commentStart);
						}
					}

					// skip block comment end token
					this.State.Index += 2;
					if (this.State.Index >= this.SourceLength)
					{
						return JsonToken.End;
					}
				}
				else
				{
					// skip over everything until reach line ending
					while (JsonReader.LineEndings.IndexOf(this.Source[this.State.Index]) < 0)
					{
						this.State.Index++;
						if (this.State.Index >= this.SourceLength)
						{
							return JsonToken.End;
						}
					}
				}

				// skip whitespace again
				while (Char.IsWhiteSpace(this.Source[this.State.Index]))
				{
					this.State.Index++;
					if (this.State.Index >= this.SourceLength)
					{
						return JsonToken.End;
					}
				}
			}

			#endregion Skip Comments

			// consume positive signing (as is extraneous)
			if (this.Source[this.State.Index] == JsonReader.OperatorUnaryPlus)
			{
				this.State.Index++;
				if (this.State.Index >= this.SourceLength)
				{
					return JsonToken.End;
				}
			}

			switch (this.Source[this.State.Index])
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
			if (Char.IsDigit(this.Source[this.State.Index]) ||
				((this.Source[this.State.Index] == JsonReader.OperatorNegate) && (this.State.Index+1 < this.SourceLength) && Char.IsDigit(this.Source[this.State.Index+1])))
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

			// "undefined" literal
			if (this.MatchLiteral(JsonReader.LiteralUndefined))
			{
				return JsonToken.Undefined;
			}

			throw new JsonDeserializationException(JsonReader.ErrorUnrecognizedToken, this.State.Index);
		}

		/// <summary>
		/// Determines if the next token is the given literal
		/// </summary>
		/// <param name="literal"></param>
		/// <returns></returns>
		private bool MatchLiteral(string literal)
		{
			int literalLength = literal.Length;
			for (int i=0, j=this.State.Index; i<literalLength && j<this.SourceLength; i++, j++)
			{
				if (literal[i] != this.Source[j])
				{
					return false;
				}
			}

			return true;
		}

		#endregion Tokenizing Methods

		#region Type Methods

		/// <summary>
		/// Converts a value into the specified type using type inference.
		/// </summary>
		/// <typeparam name="T">target type</typeparam>
		/// <param name="value">value to convert</param>
		/// <param name="typeToMatch">example object to get the type from</param>
		/// <returns></returns>
		public static T CoerceType<T>(object value, T typeToMatch)
		{
			return (T)new TypeCoercionUtility(new ReaderState()).CoerceType(typeof(T), value);
		}

		/// <summary>
		/// Converts a value into the specified type.
		/// </summary>
		/// <typeparam name="T">target type</typeparam>
		/// <param name="value">value to convert</param>
		/// <returns></returns>
		public static T CoerceType<T>(object value)
		{
			return (T)new TypeCoercionUtility().CoerceType(typeof(T), value);
		}

		/// <summary>
		/// Converts a value into the specified type.
		/// </summary>
		/// <param name="targetType">target type</param>
		/// <param name="value">value to convert</param>
		/// <returns></returns>
		public static object CoerceType(Type targetType, object value)
		{
			return new TypeCoercionUtility().CoerceType(targetType, value);
		}

		#endregion Type Methods

		#region TypeHelper

		/// <summary>
		/// Utility for forcing conversion between types
		/// </summary>
		private class TypeCoercionUtility
		{
			#region Constants

			private const string ErrorNullValueType = "{0} does not accept null as a value";
			private const string ErrorDefaultCtor = "Only objects with default constructors can be deserialized. ({0})";
			private const string ErrorCannotInstantiate = "Interfaces, Abstract classes, and unsupported ValueTypes cannot be deserialized. ({0})";

			#endregion Constants

			#region Fields

			private readonly ReaderState State;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			internal TypeCoercionUtility() : this(new ReaderState())
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="state"></param>
			internal TypeCoercionUtility(ReaderState state)
			{
				this.State = state;
			}

			#endregion Init

			#region Object Methods

			/// <summary>
			/// If a Type Hint is present then this method attempts to
			/// use it and move any previously parsed data over.
			/// </summary>
			/// <param name="result">the previous result</param>
			/// <param name="typeInfo">the type info string to use</param>
			/// <param name="objectType">reference to the objectType</param>
			/// <param name="memberMap">reference to the memberMap</param>
			/// <returns></returns>
			internal object ProcessTypeHint(
				IDictionary result,
				string typeInfo,
				ReaderState state,
				out Type objectType,
				out Dictionary<string, MemberInfo> memberMap)
			{
				if (String.IsNullOrEmpty(typeInfo))
				{
					objectType = null;
					memberMap = null;
					return result;
				}

				Type hintedType = Type.GetType(typeInfo, false);
				if (hintedType == null)
				{
					objectType = null;
					memberMap = null;
					return result;
				}

				objectType = hintedType;
				return this.CoerceType(hintedType, result, out memberMap);
			}

			internal Object InstantiateObject(Type objectType, out Dictionary<string, MemberInfo> memberMap)
			{
				if (objectType.IsInterface || objectType.IsAbstract || objectType.IsValueType)
				{
					throw new JsonDeserializationException(
						String.Format(TypeCoercionUtility.ErrorCannotInstantiate, objectType.FullName),
						this.State.Index);
				}

				ConstructorInfo ctor = objectType.GetConstructor(Type.EmptyTypes);
				if (ctor == null)
				{
					throw new JsonDeserializationException(
						String.Format(TypeCoercionUtility.ErrorDefaultCtor, objectType.FullName),
						this.State.Index);
				}
				Object result;
				try
				{
					// always try-catch Invoke() to expose real exception
					result = ctor.Invoke(null);
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null)
					{
						throw new JsonDeserializationException(ex.InnerException.Message, ex.InnerException, this.State.Index);
					}
					throw new JsonDeserializationException("Error instantiating " + objectType.FullName, ex, this.State.Index);
				}

				// don't incurr the cost of member map for dictionaries
				if (typeof(IDictionary).IsAssignableFrom(objectType))
				{
					memberMap = null;
				}
				else
				{
					memberMap = CreateMemberMap(objectType);
				}
				return result;
			}

			private Dictionary<string, MemberInfo> CreateMemberMap(Type objectType)
			{
				if (this.State.MemberMapCache.ContainsKey(objectType))
				{
					// map was stored in cache
					return this.State.MemberMapCache[objectType];
				}

				// create a new map
				Dictionary<string, MemberInfo> memberMap = new Dictionary<string, MemberInfo>();

				// load properties into property map
				PropertyInfo[] properties = objectType.GetProperties();
				foreach (PropertyInfo info in properties)
				{
					if (!info.CanRead || !info.CanWrite)
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
						memberMap[info.Name] = info;
					}
					else
					{
						memberMap[jsonName] = info;
					}
				}

				// load public fields into property map
				FieldInfo[] fields = objectType.GetFields();
				foreach (FieldInfo info in fields)
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
						memberMap[info.Name] = info;
					}
					else
					{
						memberMap[jsonName] = info;
					}
				}

				// store in cache for repeated usage
				this.State.MemberMapCache[objectType] = memberMap;

				return memberMap;
			}

			internal static void GetMemberInfo(
				Dictionary<string, MemberInfo> memberMap,
				string memberName,
				out Type memberType,
				out MemberInfo memberInfo)
			{
				memberType = null;
				memberInfo = null;

				if (memberMap != null &&
					memberMap.ContainsKey(memberName))
				{
					// Check properties for object member
					memberInfo = memberMap[memberName];

					if (memberInfo is PropertyInfo)
					{
						// maps to public property
						memberType = ((PropertyInfo)memberInfo).PropertyType;
					}
					else if (memberInfo is FieldInfo)
					{
						// maps to public field
						memberType = ((FieldInfo)memberInfo).FieldType;
					}
				}
			}

			/// <summary>
			/// Helper method to set value of either property or field
			/// </summary>
			/// <param name="result"></param>
			/// <param name="memberType"></param>
			/// <param name="memberInfo"></param>
			/// <param name="value"></param>
			internal void SetMemberValue(Object result, Type memberType, MemberInfo memberInfo, object value)
			{
				if (memberInfo is PropertyInfo)
				{
					// set value of public property
					((PropertyInfo)memberInfo).SetValue(
						result,
						this.CoerceType(memberType, value),
						null);
				}
				else if (memberInfo is FieldInfo)
				{
					// set value of public field
					((FieldInfo)memberInfo).SetValue(
						result,
						this.CoerceType(memberType, value));
				}

				// all other values are ignored
			}

			#endregion Object Methods

			#region Type Methods

			internal object CoerceType(Type targetType, object value)
			{
				bool isNullable = TypeCoercionUtility.IsNullable(targetType);
				if (value == null)
				{
					if (this.State.AllowNullValueTypes &&
						targetType.IsValueType &&
						!isNullable)
					{
						throw new JsonDeserializationException(String.Format(TypeCoercionUtility.ErrorNullValueType, targetType.FullName), this.State.Index);
					}
					return value;
				}

				if (isNullable)
				{
					// nullable types have a real underlying struct
					Type[] genericArgs = targetType.GetGenericArguments();
					if (genericArgs.Length == 1)
					{
						targetType = genericArgs[0];
					}
				}

				Type actualType = value.GetType();
				if (targetType.IsAssignableFrom(actualType))
				{
					return value;
				}

				if (targetType.IsEnum)
				{
					if (value is String)
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

						return Enum.Parse(targetType, (string)value);
					}
					else
					{
						value = this.CoerceType(Enum.GetUnderlyingType(targetType), value);
						return Enum.ToObject(targetType, value);
					}
				}

				if (value is IDictionary)
				{
					Dictionary<string, MemberInfo> memberMap;
					return this.CoerceType(targetType, (IDictionary)value, out memberMap);
				}

				if (typeof(IEnumerable).IsAssignableFrom(targetType) &&
					typeof(IEnumerable).IsAssignableFrom(actualType))
				{
					return this.CoerceArray(targetType, actualType, value);
				}

				if (value is String)
				{
					if (targetType == typeof(DateTime))
					{
						DateTime date;
						if (DateTime.TryParse(
							(string)value,
							DateTimeFormatInfo.InvariantInfo,
							DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
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
					else if (targetType == typeof(Char))
					{
						if (((string)value).Length == 1)
						{
							return ((string)value)[0];
						}
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
				else if (targetType == typeof(TimeSpan))
				{
					return new TimeSpan((long)this.CoerceType(typeof(Int64), value));
				}

				TypeConverter converter = TypeDescriptor.GetConverter(targetType);
				if (converter.CanConvertFrom(actualType))
				{
					return converter.ConvertFrom(value);
				}

				converter = TypeDescriptor.GetConverter(actualType);
				if (converter.CanConvertTo(targetType))
				{
					return converter.ConvertTo(value, targetType);
				}

				try
				{
					// fall back to basics
					return Convert.ChangeType(value, targetType);
				}
				catch (Exception ex)
				{
					throw new JsonDeserializationException(String.Format("Error converting {0} to {1}", value.GetType().FullName, targetType.FullName), ex, this.State.Index);
				}
			}

			private object CoerceType(Type targetType, IDictionary value, out Dictionary<string, MemberInfo> memberMap)
			{
				object newValue = this.InstantiateObject(targetType, out memberMap);
				if (memberMap != null)
				{
					Type memberType;
					MemberInfo memberInfo;

					// copy any values into new object
					foreach (object key in value.Keys)
					{
						TypeCoercionUtility.GetMemberInfo(memberMap, key as String, out memberType, out memberInfo);
						this.SetMemberValue(newValue, memberType, memberInfo, value[key]);
					}
				}
				return newValue;
			}

			private object CoerceArray(Type targetType, Type arrayType, object value)
			{
				// targetType serializes as a JSON array but is not an array
				// assume is an ICollection / IEnumerable with AddRange, Add,
				// or custom Constructor with which we can populate it

				// many ICollection types take an IEnumerable or ICollection
				// as a constructor argument.  look through constructors for
				// a compatible match.
				ConstructorInfo[] ctors = targetType.GetConstructors();
				ConstructorInfo defaultCtor = null;
				foreach (ConstructorInfo ctor in ctors)
				{
					ParameterInfo[] paramList = ctor.GetParameters();
					if (paramList.Length == 0)
					{
						// save for in case cannot find closer match
						defaultCtor = ctor;
						continue;
					}

					if (paramList.Length == 1 &&
						paramList[0].ParameterType.IsAssignableFrom(arrayType))
					{
						try
						{
							// invoke first constructor that can take this value as an argument
							return ctor.Invoke(
									new object[] { value }
								);
						}
						catch
						{
							// there might exist a better match
							continue;
						}
					}
				}

				if (defaultCtor == null)
				{
					throw new JsonDeserializationException(
						String.Format(TypeCoercionUtility.ErrorDefaultCtor, targetType.FullName),
						this.State.Index);
				}
				object collection;
				try
				{
					// always try-catch Invoke() to expose real exception
					collection = defaultCtor.Invoke(null);
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null)
					{
						throw new JsonDeserializationException(ex.InnerException.Message, ex.InnerException, this.State.Index);
					}
					throw new JsonDeserializationException("Error instantiating " + targetType.FullName, ex, this.State.Index);
				}

				// many ICollection types have an AddRange method
				// which adds all items at once
				MethodInfo method = targetType.GetMethod("AddRange");
				ParameterInfo[] parameters = (method == null) ?
						null : method.GetParameters();
				Type paramType = (parameters == null || parameters.Length != 1) ?
						null : parameters[0].ParameterType;
				if (paramType != null &&
					paramType.IsAssignableFrom(arrayType))
				{
					try
					{
						// always try-catch Invoke() to expose real exception
						// add all members in one method
						method.Invoke(
							collection,
							new object[] { value });
					}
					catch (TargetInvocationException ex)
					{
						if (ex.InnerException != null)
						{
							throw new JsonDeserializationException(ex.InnerException.Message, ex.InnerException, this.State.Index);
						}
						throw new JsonDeserializationException("Error calling AddRange on " + targetType.FullName, ex, this.State.Index);
					}
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
						foreach (object item in (IEnumerable)value)
						{
							try
							{
								// always try-catch Invoke() to expose real exception
								method.Invoke(
									collection,
									new object[] {
									this.CoerceType(paramType, item)
								});
							}
							catch (TargetInvocationException ex)
							{
								if (ex.InnerException != null)
								{
									throw new JsonDeserializationException(ex.InnerException.Message, ex.InnerException, this.State.Index);
								}
								throw new JsonDeserializationException("Error calling Add on " + targetType.FullName, ex, this.State.Index);
							}
						}
						return collection;
					}
				}

				try
				{
					// fall back to basics
					return Convert.ChangeType(value, targetType);
				}
				catch (Exception ex)
				{
					throw new JsonDeserializationException(String.Format("Error converting {0} to {1}", value.GetType().FullName, targetType.FullName), ex, this.State.Index);
				}
			}

			private static bool IsNullable(Type type)
			{
				return type.IsGenericType && (typeof(Nullable<>) == type.GetGenericTypeDefinition());
			}

			#endregion Type Methods
		}

		#endregion TypeHelper
	}
}
