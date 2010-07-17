﻿#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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

namespace JsonFx.Common
{
	/// <summary>
	/// Common Data Language tokens
	/// </summary>
	public enum CommonTokenType
	{
		/// <summary>
		/// No token
		/// </summary>
		/// <remarks>
		/// The token Value and Name must be null.
		/// Represents the absence of a token and is not represented in the document.
		/// </remarks>
		None,

		/// <summary>
		/// Begin object, the token contains the object Name
		/// </summary>
		/// <remarks>
		/// In the absence of a object Name, one may be automatically chosen.
		/// The object Name is often not represented directly in the output as some formats do not have the concept of named objects.
		/// Marks the beginning of an associative array of key-value pairs
		/// </remarks>
		ObjectBegin,

		/// <summary>
		/// End object
		/// </summary>
		/// <remarks>
		/// The token Value and Name must be null. The object Name is determined by the ObjectBegin token.
		/// Marks the end of the last object property as well as the object itself
		/// </remarks>
		ObjectEnd,

		/// <summary>
		/// Begin array, the token contains the array Name
		/// </summary>
		/// <remarks>
		/// In the absence of an array Name, one may be automatically chosen.
		/// The array Name is often not represented directly in the output as some formats do not have the concept of named arrays.
		/// Marks the beginning of a single-dimensional array of values
		/// </remarks>
		ArrayBegin,

		/// <summary>
		/// End array
		/// </summary>
		/// <remarks>
		/// The token Value and Name must be null. The array Name is determined by the ObjectBegin token.
		/// Marks the end of the last array item as well as the array itself
		/// </remarks>
		ArrayEnd,

		/// <summary>
		/// Begin object property, the token contains the property Name
		/// </summary>
		/// <remarks>
		/// The property Name is required as all formats name their properties.
		/// Marks the beginning of an object property.
		/// </remarks>
		Property,

		/// <summary>
		/// Simple primitive value, the token contains the primitive value
		/// </summary>
		/// <remarks>
		/// The token Value is optional, i.e. null is a valid primitive value.
		/// Typically token Value is serialized directly as a single primitive.
		/// If the token value is not a CLR primitive, then the value must implement IConvertable, IFormatable or be meaningful when cast to a string.
		/// </remarks>
		Primitive,

		/// <summary>
		/// Value delimiter
		/// </summary>
		/// <remarks>
		/// The token Value and Name must be null.
		/// Marks the end of an object property or array item
		/// </remarks>
		ValueDelim
	}
}