using System;

using JsonFx.JSON;

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
