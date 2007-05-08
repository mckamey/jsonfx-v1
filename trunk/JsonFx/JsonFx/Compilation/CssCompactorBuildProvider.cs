using System;
using System.IO;
using System.Collections.Generic;

using BuildTools;
using BuildTools.CssCompactor;

namespace JsonFx.Compilation
{
	public class CssCompactorCodeProvider : JsonFx.Compilation.CompactorCodeProvider
	{
		#region CompactorCodeProvider Members

		protected override List<ParseException> Compact(string virtualPath, string sourceText, TextWriter writer)
		{
			return CssCompactor.Compact(virtualPath, sourceText, writer, null, null, CssCompactor.Options.None);
		}

		#endregion CompactorCodeProvider Members
	}
}
