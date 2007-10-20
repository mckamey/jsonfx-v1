using System;

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
		public virtual JsonObject NamedParams
		{
			get { return this.Params as JsonObject; }
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
