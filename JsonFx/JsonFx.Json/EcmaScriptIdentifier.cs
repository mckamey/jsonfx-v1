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

namespace JsonFx.Json
{
	/// <summary>
	/// Represents an ECMAScript identifier for serialization.
	/// </summary>
	public class EcmaScriptIdentifier : IJsonSerializable
	{
		#region Fields

		private readonly string identifier;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public EcmaScriptIdentifier()
			: this(String.Empty)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public EcmaScriptIdentifier(string identifier)
		{
			this.identifier = String.IsNullOrEmpty(identifier) ?
				"null" :
				EcmaScriptIdentifier.EnsureValidIdentifier(identifier, true);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the ECMAScript identifier represented by this instance.
		/// </summary>
		public string Identifier
		{
			get { return this.identifier; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Verifies is a valid EcmaScript variable expression.
		/// </summary>
		/// <param name="varExpr">the variable expression</param>
		/// <returns>varExpr</returns>
		public static string EnsureValidIdentifier(string varExpr, bool nested)
		{
			return EcmaScriptIdentifier.EnsureValidIdentifier(varExpr, nested, true);
		}

		/// <summary>
		/// Verifies is a valid EcmaScript variable expression.
		/// </summary>
		/// <param name="varExpr">the variable expression</param>
		/// <returns>varExpr</returns>
		/// <remarks>
		/// http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf
		/// 
		/// IdentifierName =
		///		IdentifierStart | IdentifierName IdentifierPart
		/// IdentifierStart =
		///		Letter | '$' | '_'
		/// IdentifierPart =
		///		IdentifierStart | Digit
		/// </remarks>
		public static string EnsureValidIdentifier(string varExpr, bool nested, bool throwOnEmpty)
		{
			if (String.IsNullOrEmpty(varExpr))
			{
				if (throwOnEmpty)
				{
					throw new ArgumentException("Variable expression is empty.");
				}
				return String.Empty;
			}

			varExpr = varExpr.Replace(" ", "");

			bool indentPart = false;

			// TODO: ensure is not a keyword
			foreach (char ch in varExpr)
			{
				if (indentPart)
				{
					if (ch == '.' && nested)
					{
						// reset to IndentifierStart
						indentPart = false;
						continue;
					}

					if (Char.IsDigit(ch))
					{
						continue;
					}
				}

				// can be start or part
				if (Char.IsLetter(ch) || ch == '_' || ch == '$')
				{
					indentPart = true;
					continue;
				}

				throw new ArgumentException("Variable expression \""+varExpr+"\" is not supported.");
			}

			return varExpr;
		}

		#endregion Methods

		#region IJsonSerializable Members

		void IJsonSerializable.ReadJson(JsonReader reader)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		void IJsonSerializable.WriteJson(JsonWriter writer)
		{
			writer.TextWriter.Write(this.identifier);
		}

		#endregion IJsonSerializable Members
	}
}
