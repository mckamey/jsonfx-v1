#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Collections.Generic;

namespace JsonFx.UI.Jbst.Extensions
{
	/// <summary>
	/// Base class for extending JBST with custom declarations
	/// </summary>
	public abstract class JbstExtension
	{
		#region Constants

		private static readonly char[] PrefixDelim = { ':' };

		private const string PrefixNotFoundError =
			@"if (window.confirm(""Error \""{0}\"" is an unknown JbstExtension prefix. Debug?"")) {{
				/*jslint evil:true */
				debugger;
				/* oringinal value = ""{1}"" */
			}}";

		#endregion Constants

		#region JbstExtension Members

		protected abstract string Eval(string value, string virtualPath);

		#endregion JbstExtension Members

		#region Methods

		internal static string Evaluate(string source, string virtualPath)
		{
			KeyValuePair<string, string> expr = JbstExtension.ParseExpression(source);

			JbstExtension extension = null;

			// lookup the corresponding extension evaluator
			switch (expr.Key)
			{
				case "AppSettings":
				{
					extension = new AppSettingsJbstExtension();
					break;
				}
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
				result = String.Format(JbstExtension.PrefixNotFoundError, expr.Value, source);
			}
			else
			{
				result = extension.Eval(expr.Value, virtualPath);
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
