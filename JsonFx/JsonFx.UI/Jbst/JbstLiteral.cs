#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

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
using System.Text.RegularExpressions;

using JsonFx.Json;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Represents literal text in a JsonML+BST document
	/// </summary>
	internal class JbstLiteral : IJbstControl, IJsonSerializable
	{
		#region Constants

		private const string Whitespace = " ";
		private static readonly Regex RegexWhitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);

		#endregion Constants

		#region Fields

		private string text;

		#endregion Fields
		
		#region Init

		public JbstLiteral(string text)
		{
			this.Text = text;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the text of this literal
		/// </summary>
		public string Text
		{
			get { return this.text; }
			set
			{
				// normalize whitespace
				this.text = RegexWhitespace.Replace(value, JbstLiteral.Whitespace);
			}
		}

		public bool IsWhitespace
		{
			get { return (this.Text == JbstLiteral.Whitespace); }
		}

		#endregion Properties

		#region IJsonSerializable Members

		void IJsonSerializable.WriteJson(JsonWriter writer)
		{
			writer.Write(this.Text);
		}

		void IJsonSerializable.ReadJson(JsonReader reader)
		{
			throw new NotImplementedException("IJsonSerializable.ReadJson is not implemented.");
		}

		#endregion IJsonSerializable Members
	}
}
