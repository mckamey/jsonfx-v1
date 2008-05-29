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
using System.Collections.Generic;

using JsonFx.Json;

namespace JsonFx.JsonRpc
{
	public class JsonError
	{
		#region Constants

		private string name = "JSONRPCError";
		private int code = 0x0;
		private string message = null;
		private object error = null;

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonError()
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="ex"></param>
		public JsonError(Exception ex) : this()
		{
			if (ex != null)
			{
				this.Message = ex.Message;
				Dictionary<String, Object> innerError = new Dictionary<String, Object>();
				innerError["Type"] = ex.GetType().Name;
#if DEBUG
			innerError["StackTrace"] = ex.StackTrace;
#endif
				this.Error = innerError;
			}
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the JSON-RPC error name.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String value that MUST read "JSONRPCError".
		/// </remarks>
		[JsonName("name")]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Gets and sets a Number value that indicates the actual error that occurred.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A Number value that indicates the actual error
		/// that occurred. This MUST be an integer between 100 and 999.
		/// </remarks>
		[JsonName("code")]
		public int Code
		{
			get { return this.code; }
			set { this.code = value; }
		}

		/// <summary>
		/// Gets and sets a short description of the error.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String value that provides a short description of the
		/// error. The message SHOULD be limited to a single sentence.
		/// </remarks>
		[JsonName("message")]
		public string Message
		{
			get { return this.message; }
			set { this.message = value; }
		}

		/// <summary>
		/// Gets and sets a short description of the error.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A JSON Null, Number, String or Object value that carries
		/// custom and application-specific error information. Error objects
		/// MAY be nested using this property.
		/// </remarks>
		[JsonName("error")]
		public object Error
		{
			get { return this.error; }
			set { this.error = value; }
		}

		#endregion Properties
	}
}
