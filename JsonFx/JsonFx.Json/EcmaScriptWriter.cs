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
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace JsonFx.Json
{
	/// <summary>
	/// Writes data as full ECMAScript objects, rather than the limited set of JSON objects.
	/// </summary>
	public class EcmaScriptWriter : JsonWriter
	{
		#region Constants

		private static readonly DateTime EcmaScriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		private const string EcmaScriptDateCtor = "new Date({0})";
		private const string EmptyRegExpLiteral = "(?:)";
		private const char RegExpLiteralDelim = '/';
		private const char OperatorCharEscape = '\\';

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">TextWriter for writing</param>
		public EcmaScriptWriter(TextWriter output)
			: base(output)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">Stream for writing</param>
		public EcmaScriptWriter(Stream output)
			: base(output)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">File name for writing</param>
		public EcmaScriptWriter(string outputFileName)
			: base(outputFileName)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">StringBuilder for appending</param>
		public EcmaScriptWriter(StringBuilder output)
			: base(output)
		{
		}

		#endregion Init

		#region Writer Methods

		/// <summary>
		/// Writes dates as
		/// </summary>
		/// <param name="value"></param>
		public override void Write(DateTime value)
		{
			EcmaScriptWriter.WriteEcmaScriptDate(this, value);
		}

		protected override void Write(object value, bool isProperty)
		{
			if (value is Regex)
			{
				if (isProperty && this.PrettyPrint)
				{
					this.TextWriter.Write(' ');
				}
				EcmaScriptWriter.WriteEcmaScriptRegExp(this, (Regex)value);
				return;
			}

			base.Write(value, isProperty);
		}

		public static void WriteEcmaScriptDate(JsonWriter writer, DateTime value)
		{
			// find the time since Jan 1, 1970
			TimeSpan duration = value.ToUniversalTime().Subtract(EcmaScriptEpoch);

			// get the total milliseconds
			long ticks = (long)duration.TotalMilliseconds;

			// write out as a Date constructor
			writer.TextWriter.Write(EcmaScriptDateCtor, ticks);
		}

		/// <summary>
		/// Outputs a .NET Regex as an ECMAScript RegExp literal.
		/// Defaults to global matching.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="regex"></param>
		/// <remarks>
		/// http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf
		/// </remarks>
		public static void WriteEcmaScriptRegExp(JsonWriter writer, Regex regex)
		{
			EcmaScriptWriter.WriteEcmaScriptRegExp(writer, regex, true);
		}

		/// <summary>
		/// Outputs a .NET Regex as an ECMAScript RegExp literal.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="regex"></param>
		/// <param name="isGlobal"></param>
		/// <remarks>
		/// http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf
		/// </remarks>
		public static void WriteEcmaScriptRegExp(JsonWriter writer, Regex regex, bool isGlobal)
		{
			if (regex == null)
			{
				writer.TextWriter.Write("null");
				return;
			}

			// Regex.ToString() returns the original pattern
			string pattern = regex.ToString();
			if (String.IsNullOrEmpty(pattern))
			{
				// must output something otherwise becomes a code comment
				pattern = EmptyRegExpLiteral;
			}

			string modifiers = isGlobal ? "g" : "";
			switch (regex.Options & (RegexOptions.IgnoreCase | RegexOptions.Multiline))
			{
				case RegexOptions.IgnoreCase:
					{
						modifiers += "i";
						break;
					}
				case RegexOptions.Multiline:
					{
						modifiers += "m";
						break;
					}
				case RegexOptions.IgnoreCase | RegexOptions.Multiline:
					{
						modifiers += "im";
						break;
					}
			}

			writer.TextWriter.Write(RegExpLiteralDelim);

			int length = pattern.Length;
			int start = 0;

			for (int i = start; i < length; i++)
			{
				switch (pattern[i])
				{
					case RegExpLiteralDelim:
						{
							writer.TextWriter.Write(pattern.Substring(start, i - start));
							start = i + 1;
							writer.TextWriter.Write(OperatorCharEscape);
							writer.TextWriter.Write(pattern[i]);
							break;
						}
				}
			}

			writer.TextWriter.Write(pattern.Substring(start, length - start));
			writer.TextWriter.Write(RegExpLiteralDelim);
			writer.TextWriter.Write(modifiers);
		}

		#endregion Writer Methods
	}
}
