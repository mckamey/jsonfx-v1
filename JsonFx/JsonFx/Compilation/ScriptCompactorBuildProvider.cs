using System;
using System.IO;
using System.Collections.Generic;

using BuildTools;
using BuildTools.ScriptCompactor;

namespace JsonFx.Compilation
{
	public class ScriptCompactorCodeProvider : JsonFx.Compilation.CompactorCodeProvider
	{
		#region CompactorCodeProvider Methods

		protected override List<BuildTools.ParseException> Compact(string virtualPath, string sourceText, TextWriter writer)
		{
			return ScriptCompactor.Compact(virtualPath, sourceText, writer, null, null, ScriptCompactor.Options.None);
		}

		#endregion CompactorCodeProvider Methods
	}
}
