using System;

using JsonFx.Serialization;

namespace JsonFx.Services
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
