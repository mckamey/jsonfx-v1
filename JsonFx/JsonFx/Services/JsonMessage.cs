using System;

using JsonFx.Serialization;

namespace JsonFx.Services
{
	public abstract class JsonMessage
	{
		#region Constants

		public const string JsonRpcVersion = "1.1";

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
