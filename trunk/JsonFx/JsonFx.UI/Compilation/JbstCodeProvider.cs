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
using System.IO;
using System.Collections.Generic;
using System.Web.Compilation;
using System.CodeDom;

using BuildTools;
using JsonFx.UI.Jbst;

namespace JsonFx.Compilation
{
	public class JbstCodeProvider : JsonFx.Compilation.ResourceCodeProvider
	{
		#region Fields

		private List<string> g11nKeys = new List<string>();

		#endregion Fields

		#region ResourceCodeProvider Properties

		public override string ContentType
		{
			get { return "text/javascript"; }
		}

		public override string FileExtension
		{
			get { return "js"; }
		}

		protected override Type CompiledBuildResultType
		{
			get { return typeof(JbstCompiledBuildResult); }
		}

		#endregion ResourceCodeProvider Properties

		#region ResourceCodeProvider Methods

		protected override void GenerateCodeExtensions(CodeTypeDeclaration resourceType)
		{
		}

		protected override void ProcessResource(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			out string resource,
			out string compacted,
			List<ParseException> errors)
		{
			// parse JBST markup
			JbstCompiler parser = new JbstCompiler(virtualPath, true);
			parser.Parse(sourceText);

			using (StringWriter sw = new StringWriter())
			{
				// render the pretty-printed version
				parser.Render(sw, true);
				sw.Flush();
				resource = sw.ToString();
			}

			using (StringWriter sw = new StringWriter())
			{
				// render the compacted version
				parser.Render(sw, false);
				sw.Flush();
				compacted = sw.ToString();
			}
		}

		#endregion ResourceCodeProvider Methods
	}
}
