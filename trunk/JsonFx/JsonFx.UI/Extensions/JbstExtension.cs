using System;
using System.Collections.Generic;

namespace JsonFx.Extensions
{
	/// <summary>
	/// Base class for extending JBST with custom declarations
	/// </summary>
	public abstract class JbstExtension
	{
		#region Constants

		private static readonly char[] PrefixDelim = new char[] { ':' };

		private const string PrefixNotFoundError =
			@"if (window.confirm(""Error \""{0}\"" is an unknown JbstExtension prefix. Debug?"")) {{
				/*jslint evil:true */
				debugger;
				/* oringinal value = ""{1}"" */
			}}";

		#endregion Constants

		#region JbstExtension Members

		protected abstract string Eval(string value);

		#endregion JbstExtension Members

		#region Methods

		internal static string Evaluate(string source)
		{
			KeyValuePair<string, string> kvp = JbstExtension.ParseExpression(source);

			JbstExtension extension = null;

			// lookup the corresponding extension evaluator
			switch (kvp.Key)
			{
				case "Resources":
				{
					extension = new ResourceJbstExtension();
					break;
				}
			}

			string result;
			if (extension == null)
			{
				// output error
				result = String.Format(JbstExtension.PrefixNotFoundError, kvp.Value, source);
			}
			else
			{
				result = extension.Eval(kvp.Value);
			}

			return result;
		}

		private static KeyValuePair<string, string> ParseExpression(string extension)
		{
			string key = String.Empty;
			string value = String.Empty;

			if (!String.IsNullOrEmpty(extension))
			{
				// split on first ':'
				string[] parts = extension.Split(JbstExtension.PrefixDelim, 2, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length > 1)
				{
					value = parts[1].Trim();
				}

				key = parts[0].Trim();
			}

			return new KeyValuePair<string, string>(key, value);
		}

		#endregion Methods
	}
}
