#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BuildTools;
using BuildTools.ScriptCompactor;
using JsonFx.Json;
using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	public class ScriptResourceCodeProvider : JsonFx.Compilation.ResourceCodeProvider
	{
		#region Constants

		public const string MimeType = "text/javascript";

		#endregion Constants

		#region ResourceCodeProvider Properties

		public override string ContentType
		{
			get { return MimeType; }
		}

		public override string FileExtension
		{
			get { return "js"; }
		}

		#endregion ResourceCodeProvider Properties

		#region ResourceCodeProvider Methods

		protected override void ResetCodeProvider()
		{
			// no state is actually stored
		}

		protected internal override void ProcessResource(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			out string resource,
			out string compacted,
			List<ParseException> errors)
		{
			using (StringWriter writer = new StringWriter())
			{
				IList<ParseException> parseErrors;
				try
				{
					parseErrors = ScriptCompactor.Compact(
						virtualPath,
						sourceText,
						writer,
						null,
						null,
						ScriptCompactor.Options.None);
				}
				catch (Exception ex)
				{
					errors.Add(new ParseError(ex.Message, virtualPath, 0, 0, ex));
					parseErrors = null;
				}

				if (parseErrors != null && parseErrors.Count > 0)
				{
					errors.AddRange(parseErrors);
				}

				writer.Flush();

				resource = sourceText;
				compacted = writer.ToString();

				this.ExtractGlobalizationKeys(compacted);
			}
		}

		private void ExtractGlobalizationKeys(string compacted)
		{
			GlobalizedResourceHandler.ExtractGlobalizationKeys(compacted, this.GlobalizationKeys);
		}

		#endregion ResourceCodeProvider Methods
	}
}
