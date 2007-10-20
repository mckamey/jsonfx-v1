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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace JsonFx.Json
{
	/// <summary>
	/// Writer for producing JSON data.
	/// </summary>
	public class JsonWriter : IDisposable
	{
		#region Constants

		protected const string IgnorePropertySuffix = "Specified";

		private const string TypeBoolean = "System.Boolean";
		private const string TypeChar = "System.Char";
		private const string TypeByte = "System.Byte";
		private const string TypeInt16 = "System.Int16";
		private const string TypeInt32 = "System.Int32";
		private const string TypeInt64 = "System.Int64";
		private const string TypeSByte = "System.SByte";
		private const string TypeUInt16 = "System.UInt16";
		private const string TypeUInt32 = "System.UInt32";
		private const string TypeUInt64 = "System.UInt64";
		private const string TypeSingle = "System.Single";
		private const string TypeDouble = "System.Double";
		private const string TypeDecimal = "System.Decimal";
		private const string TypeDateTime = "System.DateTime";
		private const string TypeGuid = "System.Guid";

		#endregion Constants
		
		#region Fields

		private bool strictConformance = true;
		private bool classHinting = false;
		private System.IO.TextWriter writer = null;

		#endregion Fields

		#region Init

		public JsonWriter(TextWriter output)
		{
			this.writer = output;
		}

		public JsonWriter(Stream output)
		{
			this.writer = new StreamWriter(output, Encoding.UTF8);
		}

		public JsonWriter(string outputFileName)
		{
			Stream stream = null;
			try
			{
				stream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
			}
			catch
			{
				if (stream != null)
					stream.Close();
				throw;
			}
			this.writer = new StreamWriter(stream, Encoding.UTF8);
		}

		public JsonWriter(StringBuilder output)
		{
			this.writer = new StringWriter(output, System.Globalization.CultureInfo.InvariantCulture);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets if should conform strictly to JSON spec.
		/// </summary>
		/// <remarks>
		/// Setting to true causes NaN, Infinity, -Infinity to serialize as null.
		/// </remarks>
		public bool StrictConformance
		{
			get { return this.strictConformance; }
			set { this.strictConformance = value; }
		}

		/// <summary>
		/// Gets and sets if should produce class hinting.
		/// </summary>
		/// <remarks>
		/// Class hinting is output as per JSON-RPC 1.1.
		/// </remarks>
		public bool ClassHinting
		{
			get { return this.classHinting; }
			set { this.classHinting = value; }
		}

		#endregion Properties

		#region Public Methods

		public virtual void Write(object value)
		{
			if (value == null)
			{
				this.writer.Write(JsonReader.LiteralNull);
				return;
			}

			if (value is IJsonSerializable)
			{
				((IJsonSerializable)value).WriteJson(this);
				return;
			}

			if (value is Enum)
			{
				this.Write((Enum)value);
				return;
			}

			if (value is JsonObject)
			{
				this.WriteObject((JsonObject)value);
				return;
			}

			if (value is String)
			{
				this.Write((String)value);
				return;
			}

			if (value is IEnumerable)
			{
				this.WriteArray((IEnumerable)value);
				return;
			}

			// these are ordered based on an intuitive sense of their
			// frequency of use for nominally better switch performance
			switch (value.GetType().FullName)
			{
				case JsonWriter.TypeDateTime:
				{
					this.Write((DateTime)value);
					return;
				}
				case JsonWriter.TypeGuid:
				{
					this.Write((Guid)value);
					return;
				}
				case JsonWriter.TypeDouble:
				{
					this.Write((Double)value);
					return;
				}
				case JsonWriter.TypeInt32:
				{
					this.Write((Int32)value);
					return;
				}
				case JsonWriter.TypeBoolean:
				{
					this.Write((Boolean)value);
					return;
				}
				case JsonWriter.TypeDecimal:
				{
					// Conversions from Char, SByte, Int16, Int32, Int64, Byte, UInt16, UInt32, and UInt64
					// to Decimal are widening conversions that never lose information or throw exceptions.
					// Conversions from Single or Double to Decimal throw an OverflowException
					// if the result of the conversion is not representable as a Decimal.
					this.Write((Decimal)value);
					return;
				}
				case JsonWriter.TypeByte:
				{
					this.Write((Byte)value);
					return;
				}
				case JsonWriter.TypeInt16:
				{
					this.Write((Int16)value);
					return;
				}
				case JsonWriter.TypeInt64:
				{
					this.Write((Int64)value);
					return;
				}
				case JsonWriter.TypeChar:
				{
					this.Write((Char)value);
					return;
				}
				case JsonWriter.TypeSingle:
				{
					this.Write((Single)value);
					return;
				}
				case JsonWriter.TypeUInt16:
				{
					this.Write((UInt16)value);
					return;
				}
				case JsonWriter.TypeUInt32:
				{
					this.Write((UInt32)value);
					return;
				}
				case JsonWriter.TypeUInt64:
				{
					this.Write((UInt64)value);
					return;
				}
				case JsonWriter.TypeSByte:
				{
					this.Write((SByte)value);
					return;
				}
				default:
				{
					// structs and classes
					this.WriteObject(value);
					return;
				}
			}
		}

		public virtual void WriteBase64(byte[] value)
		{
			this.Write(Convert.ToBase64String(value));
		}

		public virtual void Write(DateTime value)
		{
			this.Write(value.ToString("r"));
		}

		public virtual void Write(Guid value)
		{
			this.Write(value.ToString("D"));
		}

		public virtual void Write(Enum value)
		{
			string enumName = null;

			Type type = value.GetType();

			if (type.IsDefined(typeof(FlagsAttribute), true) && !Enum.IsDefined(type, value))
			{
				Enum[] flags = JsonWriter.GetFlagList(type, value);
				string[] flagNames = new string[flags.Length];
				for (int i=0; i<flags.Length; i++)
				{
					flagNames[i] = JsonNameAttribute.GetJsonName(flags[i]);
					if (String.IsNullOrEmpty(flagNames[i]))
						flagNames[i] = flags[i].ToString("f");
				}
				enumName = String.Join(", ", flagNames);
			}
			else
			{
				enumName = JsonNameAttribute.GetJsonName(value);
				if (String.IsNullOrEmpty(enumName))
					enumName = value.ToString("f");
			}

			this.Write(enumName);
		}

		public virtual void Write(string value)
		{
			if (value == null)
			{
				this.writer.Write(JsonReader.LiteralNull);
				return;
			}

			int length = value.Length;
			int start = 0;

			this.writer.Write(JsonReader.OperatorStringDelim);

			for (int i = start; i < length; i++)
			{
				if (value[i] <= '\u001F' ||
					value[i] >= '\u007F' ||
					value[i] == '<' ||
					value[i] == JsonReader.OperatorStringDelim ||
					value[i] == JsonReader.OperatorCharEscape)
				{
					this.writer.Write(value.Substring(start, i-start));
					start = i+1;

					switch (value[i])
					{
						case JsonReader.OperatorStringDelim:
						case JsonReader.OperatorCharEscape:
						{
							this.writer.Write(JsonReader.OperatorCharEscape);
							this.writer.Write(value[i]);
							continue;
						}
						case '\b':
						{
							this.writer.Write("\\b");
							continue;
						}
						case '\f':
						{
							this.writer.Write("\\f");
							continue;
						}
						case '\n':
						{
							this.writer.Write("\\n");
							continue;
						}
						case '\r':
						{
							this.writer.Write("\\r");
							continue;
						}
						case '\t':
						{
							this.writer.Write("\\t");
							continue;
						}
						default:
						{
							this.writer.Write("\\u{0:X4}", Char.ConvertToUtf32(value, i));
							continue;
						}
					}
				}
			}

			this.writer.Write(value.Substring(start, length-start));

			this.writer.Write(JsonReader.OperatorStringDelim);
		}

		#endregion Public Methods

		#region Primative Methods

		public virtual void Write(bool value)
		{
			this.writer.Write(value ? JsonReader.LiteralTrue : JsonReader.LiteralFalse);
		}

		public virtual void Write(byte value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(sbyte value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(short value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(ushort value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(int value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(uint value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(long value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(ulong value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(float value)
		{
			if (this.StrictConformance && (Single.IsNaN(value) || Single.IsInfinity(value)))
			{
				this.writer.Write(JsonReader.LiteralNull);
			}
			else
			{
				this.writer.Write("{0:g}", value);
			}
		}

		public virtual void Write(double value)
		{
			if (this.StrictConformance && (Double.IsNaN(value) || Double.IsInfinity(value)))
			{
				this.writer.Write(JsonReader.LiteralNull);
			}
			else
			{
				this.writer.Write("{0:g}", value);
			}
		}

		public virtual void Write(decimal value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(char value)
		{
			this.Write(Convert.ToString(value));
		}

		#endregion Primative Methods

		#region Private Methods

		protected internal virtual void WriteArray(IEnumerable value)
		{
			bool appendDelim = false;

			this.writer.Write(JsonReader.OperatorArrayStart);

			foreach (object item in value)
			{
				if (appendDelim)
					this.writer.Write(JsonReader.OperatorValueDelim);
				this.Write(item);

				appendDelim = true;
			}

			this.writer.Write(JsonReader.OperatorArrayEnd);
		}

		protected virtual void WriteObject(JsonObject value)
		{
			bool appendDelim = false;

			this.writer.Write(JsonReader.OperatorObjectStart);

			foreach (String name in value.Properties)
			{
				if (appendDelim)
					this.writer.Write(JsonReader.OperatorValueDelim);

				this.Write(name);
				this.writer.Write(JsonReader.OperatorNameDelim);
				this.Write(value[name]);

				appendDelim = true;
			}

			this.writer.Write(JsonReader.OperatorObjectEnd);
		}

		protected virtual void WriteObject(object value)
		{
			if (this.ClassHinting)
			{
				throw new NotImplementedException("Class Hinting is not yet implemented.");
			}

			bool appendDelim = false;

			this.writer.Write(JsonReader.OperatorObjectStart);

			Type objType = value.GetType();

			// serialize public properties
			PropertyInfo[] properties = objType.GetProperties();
			foreach (PropertyInfo property in properties)
			{
				if (!property.CanWrite || !property.CanRead)
					continue;

				if (JsonIgnoreAttribute.IsJsonIgnore(property))
					continue;

				PropertyInfo specProp = objType.GetProperty(property.Name+JsonWriter.IgnorePropertySuffix);
				if (specProp != null)
				{
					object isSpecified = specProp.GetValue(value, null);
					if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
						continue;
				}

				if (appendDelim)
					this.writer.Write(JsonReader.OperatorValueDelim);

				string propertyName = JsonNameAttribute.GetJsonName(property);
				if (String.IsNullOrEmpty(propertyName))
					propertyName = property.Name;

				this.Write(propertyName);
				this.writer.Write(JsonReader.OperatorNameDelim);
				object propertyValue = property.GetValue(value, null);
				this.Write(propertyValue);

				appendDelim = true;
			}

			// serialize public fields
			FieldInfo[] fields = objType.GetFields();
			foreach (FieldInfo field in fields)
			{
				if (!field.IsPublic || field.IsStatic)
					continue;
				
				if (JsonIgnoreAttribute.IsJsonIgnore(field))
					continue;

				PropertyInfo specProp = objType.GetProperty(field.Name+JsonWriter.IgnorePropertySuffix);
				if (specProp != null)
				{
					object isSpecified = specProp.GetValue(value, null);
					if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
						continue;
				}

				if (appendDelim)
					this.writer.Write(JsonReader.OperatorValueDelim);
				
				string fieldName = JsonNameAttribute.GetJsonName(field);
				if (String.IsNullOrEmpty(fieldName))
					fieldName = field.Name;

				// use Attributes here to control naming
				this.Write(fieldName);
				this.writer.Write(JsonReader.OperatorNameDelim);
				object fieldValue = field.GetValue(value);
				this.Write(fieldValue);

				appendDelim = true;
			}

			this.writer.Write(JsonReader.OperatorObjectEnd);
		}

		#region GetFlagList

		/// <summary>
		/// Splits a bitwise-OR'd set of enums into a list.
		/// </summary>
		/// <param name="enumType">the enum type</param>
		/// <param name="value">the combined value</param>
		/// <returns>list of flag enums</returns>
		/// <remarks>
		/// from PseudoCode.EnumHelper
		/// </remarks>
		private static Enum[] GetFlagList(Type enumType, object value)
		{
			ulong longVal = Convert.ToUInt64(value);
			string[] enumNames = Enum.GetNames(enumType);
			Array enumValues = Enum.GetValues(enumType);

			List<Enum> enums = new List<Enum>(enumValues.Length);

			// check for empty
			if (longVal == 0L)
			{
				// Return the value of empty, or zero if none exists
				if (Convert.ToUInt64(enumValues.GetValue(0)) == 0L)
					enums.Add(enumValues.GetValue(0) as Enum);
				else
					enums.Add(null);
				return enums.ToArray();
			}

			for (int i = enumValues.Length-1; i >= 0; i--)
			{
				ulong enumValue = Convert.ToUInt64(enumValues.GetValue(i));

				if ((i == 0) && (enumValue == 0L))
					continue;

				// matches a value in enumeration
				if ((longVal & enumValue) == enumValue)
				{
					// remove from val
					longVal -= enumValue;

					// add enum to list
					enums.Add(enumValues.GetValue(i) as Enum);
				}
			}

			if (longVal != 0x0L)
				enums.Add(Enum.ToObject(enumType, longVal) as Enum);

			return enums.ToArray();
		}

		#endregion GetFlagList

		#endregion Private Methods

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			if (this.writer != null)
				this.writer.Dispose();
		}

		#endregion IDisposable Members
	}
}
