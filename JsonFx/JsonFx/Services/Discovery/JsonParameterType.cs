using System;

using JsonFx.Serialization;

namespace JsonFx.Services.Discovery
{
	public enum JsonParameterType
	{
		[JsonName("nil")]
		None,

		[JsonName("bit")]
		Boolean,

		[JsonName("num")]
		Number,

		[JsonName("str")]
		String,

		[JsonName("arr")]
		Array,

		[JsonName("obj")]
		Object,

		[JsonName("any")]
		Any
	}
}
