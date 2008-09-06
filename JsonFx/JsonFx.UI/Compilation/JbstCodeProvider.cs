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
using JsonFx.UI.Jbst.Extensions;

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
			get { return typeof(GlobalizedCompiledBuildResult); }
		}

		#endregion ResourceCodeProvider Properties

		#region CodeDom Methods

		protected override void GenerateCodeExtensions(CodeTypeDeclaration resourceType)
		{
			#region private static readonly string[] g11nKeys

			CodeMemberField field = new CodeMemberField();
			field.Name = "g11nKeys";
			field.Type = new CodeTypeReference(typeof(string[]));
			field.Attributes = MemberAttributes.Private|MemberAttributes.Static|MemberAttributes.Final;

			CodeArrayCreateExpression arrayInit = new CodeArrayCreateExpression(field.Type, this.g11nKeys.Count);
			foreach (string key in this.g11nKeys)
			{
				arrayInit.Initializers.Add(new CodePrimitiveExpression(key));
			}
			field.InitExpression = arrayInit;

			resourceType.Members.Add(field);

			#endregion private static readonly string[] g11nKeys

			#region public override string[] GlobalizationKeys { get; }

			// add a readonly property returning the static data
			CodeMemberProperty property = new CodeMemberProperty();
			property.Name = "GlobalizationKeys";
			property.Type = field.Type;
			property.Attributes = MemberAttributes.Public|MemberAttributes.Override;
			property.HasGet = true;
			// get { return g11nKeys; }
			property.GetStatements.Add(new CodeMethodReturnStatement(
				new CodeFieldReferenceExpression(
					new CodeTypeReferenceExpression(resourceType.Name),
					field.Name)));
			resourceType.Members.Add(property);

			#endregion public override string[] GlobalizationKeys { get; }
		}

		#endregion CodeDom Methods

		#region Compilation Methods

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

			this.ExtractGlobalizationKeys(parser.Document);

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

		private void ExtractGlobalizationKeys(JbstControl root)
		{
			List<IJbstControl> queue = new List<IJbstControl>();
			queue.Add(root);

			while (queue.Count > 0)
			{
				// pop and cast
				IJbstControl control = queue[0];
				queue.RemoveAt(0);

				// queue up children
				JbstControl container = control as JbstControl;
				if (container != null && container.ChildControlsSpecified)
				{
					queue.AddRange(container.ChildControls);
					continue;
				}

				// queue up children
				JbstExtensionBlock extension = control as JbstExtensionBlock;
				if (extension == null)
				{
					continue;
				}

				ResourceJbstExtension resx = extension.Extension as ResourceJbstExtension;
				if (resx == null)
				{
					continue;
				}

				this.g11nKeys.Add(resx.GlobalizationKey);
			}
		}

		#endregion Compilation Methods
	}
}
