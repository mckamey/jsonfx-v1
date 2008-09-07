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
	public abstract class JsonMessage
	{
		#region Constants

		public const string JsonRpcVersion = "2.0";

		#endregion Constants

		#region Fields

		private object id = null;
		private string version = JsonMessage.JsonRpcVersion;

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonMessage()
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets an identifier which may be used to correlate a response with its request.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A request identifier that can be of any JSON type. This member is essentially
		/// maintained for backward compatibility with version 1.0 of the specification where it was
		/// used to correlate a response with its request. If this member is present then the server
		/// MUST repeat it verbatim on the response.
		/// </remarks>
		[JsonName("id")]
		public virtual object ID
		{
			get { return this.id; }
			set { this.id = value; }
		}

		/// <summary>
		/// Gets and sets the version of the JSON-RPC specification to which this conforms.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String specifying the version of the JSON-RPC protocol to which the client
		/// conforms. An implementation conforming to this specification MUST use the exact String
		/// value of "1.1" for this member. The absence of this member can effectively be taken to
		/// mean that the remote server implement version 1.0 of the JSON-RPC protocol.
		/// </remarks>
		[JsonName("version")]
		public virtual string Version
		{
			get { return this.version; }
			set { this.version = value; }
		}

		#endregion Properties
	}
}
