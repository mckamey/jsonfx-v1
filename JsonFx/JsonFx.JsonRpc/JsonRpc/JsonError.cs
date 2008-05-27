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
