using System;

namespace JsonFx.Serialization
{
	public class JsonSerializationException : InvalidOperationException
	{
		#region Fields

		private int index = -1;

		#endregion Fields

		#region Init

		public JsonSerializationException() : base() { }

		public JsonSerializationException(string message) : base(message) { }

		public JsonSerializationException(string message, int index) : base(message)
		{
			this.index = index;
		}

		public JsonSerializationException(string message, Exception innerException) : base(message, innerException) { }

		public JsonSerializationException(string message, Exception innerException, int index) : base(message, innerException)
		{
			this.index = index;
		}

		public JsonSerializationException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		#endregion Init
	}
}
