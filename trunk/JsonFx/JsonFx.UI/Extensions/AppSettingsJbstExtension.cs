using System;
using System.Configuration;

using JsonFx.Json;

namespace JsonFx.Extensions
{
	public class AppSettingsJbstExtension : JbstExtension
	{
		#region JbstExtension Members

		protected override string Eval(string expression)
		{
			return JsonWriter.Serialize(ConfigurationManager.AppSettings[expression]);
		}

		#endregion JbstExtension Members
	}
}
