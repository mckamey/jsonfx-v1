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

using JsonFx.Json;

namespace JsonFx.JsonRpc
{
	/// <summary>
	/// The message that frames the result of a successful call or the error in the case of a failed call.
	/// </summary>
	public class JsonResponse : JsonMessage
	{
		#region Constants

		private object result = null;
		private object error = null;

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonResponse()
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the answer for a successful call.
		/// </summary>
		/// <remarks>
		/// REQUIRED on success. The value that was returned by the procedure upon a
		/// successful invocation. This member MUST be entirely omitted in case there
		/// was an error invoking the procedure.
		/// </remarks>
		[JsonName("result")]
		public virtual object Result
		{
			get { return this.result; }
			set { this.result = value; }
		}

		/// <summary>
		/// Gets and sets the answer for a failed call.
		/// </summary>
		/// <remarks>
		/// REQUIRED on error. An Object containing error information about the fault
		/// that occured before, during or after the call. This member MUST be entirely
		/// omitted if there was no error.
		/// </remarks>
		[JsonName("error")]
		public virtual object Error
		{
			get { return this.error; }
			set { this.error = value; }
		}

		#endregion Properties
	}
}
