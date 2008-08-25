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

namespace JsonFx.JsonML.BST
{
	/// <summary>
	/// Internal representation of a JsonML+BST code block.
	/// </summary>
	internal class JbstCodeBlock : IJbstControl, Json.IJsonSerializable
	{
		#region Constants

		// TODO: evaluate the usefulness of straight code output
		private const string CodeBlockFormat = "function($item){{return({0});}}";
		private const string FunctionFormat = "function($item){{return({0});}}";

		#endregion Constants

		#region Field

		private string code;

		#endregion Field

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		public JbstCodeBlock(string code)
		{
			this.code = (code != null) ?
				code.Trim() :
				String.Empty;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the code block content
		/// </summary>
		public string Code
		{
			get { return this.code; }
		}

		#endregion Properties

		#region IJsonSerializable Members

		void JsonFx.Json.IJsonSerializable.WriteJson(JsonFx.Json.JsonWriter writer)
		{
			if (this.Code.StartsWith("="))
			{
				writer.TextWriter.Write(FunctionFormat, this.Code.Substring(1));
			}
			else
			{
				writer.TextWriter.Write(CodeBlockFormat, this.Code);
			}
		}

		void JsonFx.Json.IJsonSerializable.ReadJson(JsonFx.Json.JsonReader reader)
		{
			throw new NotImplementedException("JbstCodeBlock deserialization is not yet implemented.");
		}

		#endregion IJsonSerializable Members
	}
}
