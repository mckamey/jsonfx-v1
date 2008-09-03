using System;

using JsonFx.Json;

namespace JsonFx.Extensions
{
	public class ResourceJbstExtension : JbstExtension
	{
		#region Constants

		private const string ResourceLookupFormat =
			@"function() {{
				return (JsonFx.ResX && JsonFx.ResX[{0}]) || {1};
			}}";

		#endregion Constants

		#region JbstExtension Members

		protected override string Eval(string expression)
		{
			return String.Format(
				ResourceLookupFormat,
				JsonWriter.Serialize(expression),
				JsonWriter.Serialize("$$"+expression+"$$"));
		}

		#endregion JbstExtension Members
	}
}
