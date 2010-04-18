#region License
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
using System.IO;
using System.Text;

namespace JsonFx.Json
{
	/// <summary>
	/// JSON serializer
	/// </summary>
	public class JsonWriter : DataWriter<JsonTokenType>
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public JsonWriter(DataWriterSettings settings)
			: base(settings)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the content encoding for the serialized data
		/// </summary>
		public override Encoding ContentEncoding
		{
			get { return Encoding.UTF8; }
		}

		/// <summary>
		/// Gets the supported content type for the serialized data
		/// </summary>
		public override string ContentType
		{
			get { return "application/json"; }
		}

		/// <summary>
		/// Gets the supported file extension for the serialized data
		/// </summary>
		public override string FileExtension
		{
			get { return ".json"; }
		}

		#endregion Properties

		#region DataWriter<JsonTokenType> Methods

		protected override IGenerator<JsonTokenType> GetGenerator(DataWriterSettings settings)
		{
			return new JsonGenerator(settings);
		}

		protected override IFormatter<JsonTokenType> GetFormatter(DataWriterSettings settings)
		{
			return new JsonFormatter(settings);
		}

		#endregion DataWriter<JsonTokenType> Methods
	}
}
