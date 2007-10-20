using System;

namespace JsonFx.JsonRpc
{
	public class JsonServiceException : InvalidOperationException
	{
		#region Init

		public JsonServiceException() : base() { }

		public JsonServiceException(string message) : base(message) { }

		public JsonServiceException(string message, Exception innerException) : base(message, innerException) { }

		public JsonServiceException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }

		#endregion Init
	}
}
