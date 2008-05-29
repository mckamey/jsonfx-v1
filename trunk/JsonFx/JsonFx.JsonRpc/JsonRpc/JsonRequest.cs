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
	/// <summary>
	/// The message that frames a call and its parameters.
	/// </summary>
	public class JsonRequest : JsonMessage
	{
		#region Fields

		private string method = null;
		private Object parameters = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonRequest()
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the named operation on a service that is the target of this remote procedure call.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String containing the name of the procedure to be invoked. Procedure names that
		/// begin with the word system followed by a period character (U+002E or ASCII 46) are reserved.
		/// In other words, a procedure named system.foobar is considered to have reserved semantics.
		/// </remarks>
		[JsonName("method")]
		public virtual string Method
		{
			get { return this.method; }
			set { this.method = value; }
		}

		[JsonName("params")]
		public virtual Object Params
		{
			get { return this.parameters; }
			set { this.parameters = value; }
		}

		/// <summary>
		/// Gets and sets the named parameters for this remote procedure call.
		/// Mutually exclusive with <see cref="PositionalParams">PositionalParams</see>.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. An Array or Object that holds the actual parameter values for the invocation of the procedure.
		/// </remarks>
		[JsonIgnore]
		public virtual Dictionary<String, Object> NamedParams
		{
			get { return this.Params as Dictionary<String, Object>; }
			set { this.Params = value; }
		}

		/// <summary>
		/// Gets and sets the positional parameters for this remote procedure call.
		/// Mutually exclusive with <see cref="NamedParams">NamedParams</see>.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. An Array or Object that holds the actual parameter values for the invocation of the procedure.
		/// </remarks>
		[JsonIgnore]
		public virtual Array PositionalParams
		{
			get { return this.Params as Array; }
			set { this.Params = value; }
		}

		#endregion Properties
	}
}
