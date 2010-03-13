#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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
using System.CodeDom;
using System.Collections.Generic;
using System.IO;

using JsonFx.BuildTools;
using JsonFx.BuildTools.HtmlDistiller;
using JsonFx.BuildTools.HtmlDistiller.Filters;
using JsonFx.Client;
using JsonFx.Handlers;
using JsonFx.Json;
using JsonFx.UI.Jbst;
using JsonFx.UI.Jbst.Extensions;

namespace JsonFx.Compilation
{
	public class JbstCodeProvider : JsonFx.Compilation.ResourceCodeProvider
	{
		#region Fields

		private JbstWriter jbstWriter;

		#endregion Fields

		#region ResourceCodeProvider Properties

		public override string ContentType
		{
			get { return ScriptResourceCodeProvider.MimeType; }
		}

		public override string FileExtension
		{
			get { return ScriptResourceCodeProvider.FileExt; }
		}

		#endregion ResourceCodeProvider Properties

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
			this.jbstWriter = new JbstWriter(virtualPath);

			try
			{
				HtmlDistiller parser = new HtmlDistiller();
				parser.EncodeNonAscii = false;
				parser.BalanceTags = false;
				parser.NormalizeWhitespace = false;
				parser.HtmlWriter = this.jbstWriter;
				parser.HtmlFilter = new NullHtmlFilter();
				parser.Parse(sourceText);

				// determine which globalization keys were used
				JbstCodeProvider.ExtractGlobalizationKeys(this.jbstWriter.JbstParseTree, this.GlobalizationKeys);
			}
			catch (ParseException ex)
			{
				errors.Add(ex);
			}
			catch (Exception ex)
			{
				errors.Add(new ParseError(ex.Message, virtualPath, 0, 0, ex));
			}

			string renderedTemplate;
			using (StringWriter sw = new StringWriter())
			{
				// render the pretty-printed version
				this.jbstWriter.Render(sw);
				sw.Flush();
				renderedTemplate = sw.ToString();

				resource = ScriptResourceCodeProvider.FirewallScript(virtualPath, renderedTemplate, false);
			}

			// min the output for better compaction
			compacted = ScriptCompactionAdapter.Compact(virtualPath, renderedTemplate, errors);
			compacted = ScriptResourceCodeProvider.FirewallScript(virtualPath, compacted, true);
		}

		protected override void ProcessExternalResource(
			IResourceBuildHelper helper,
			string url,
			out string preProcessed,
			out string compacted,
			List<ParseException> errors)
		{
			compacted = preProcessed = String.Format(ScriptResourceCodeProvider.ExternalImport, url);

			preProcessed = ScriptResourceCodeProvider.FirewallScript(url, preProcessed, true);
			compacted = ScriptResourceCodeProvider.FirewallScript(url, compacted, true);
		}

		#endregion Compilation Methods

		#region ResourceCodeProvider Methods

		protected override void ResetCodeProvider()
		{
			this.jbstWriter = null;

			base.ResetCodeProvider();
		}

		protected override void GenerateCodeExtensions(CodeTypeDeclaration resourceType)
		{
			base.GenerateCodeExtensions(resourceType);

			if (this.jbstWriter == null)
			{
				throw new InvalidOperationException("JbstCodeProvider: JbstWriter is missing");
			}

			string jbstName = this.jbstWriter.JbstName;
			AutoMarkupType autoMarkup = this.jbstWriter.AutoMarkup;

			resourceType.BaseTypes.Add(typeof(IJbstBuildResult));

			#region private static readonly EcmaScriptIdentifier jbstName

			CodeMemberField field = new CodeMemberField();
			field.Name = "jbstName";
			field.Type = new CodeTypeReference(typeof(EcmaScriptIdentifier));
			field.Attributes = MemberAttributes.Private|MemberAttributes.Static|MemberAttributes.Final;

			field.InitExpression = new CodePrimitiveExpression(jbstName);

			resourceType.Members.Add(field);

			#endregion private static readonly EcmaScriptIdentifier jbstName

			#region EcmaScriptIdentifier IJbstBuildResult.JbstName { get; }

			// add a readonly property returning the static data
			CodeMemberProperty property = new CodeMemberProperty();
			property.Name = "JbstName";
			property.Type = new CodeTypeReference(typeof(EcmaScriptIdentifier));
			property.PrivateImplementationType = new CodeTypeReference(typeof(IJbstBuildResult));
			property.HasGet = true;
			// get { return jbstName; }
			property.GetStatements.Add(new CodeMethodReturnStatement(
				new CodeFieldReferenceExpression(
					new CodeTypeReferenceExpression(resourceType.Name),
					field.Name)));
			resourceType.Members.Add(property);

			#endregion EcmaScriptIdentifier IJbstBuildResult.JbstName { get; }

			#region AutoMarkupType IJbstBuildResult.AutoMarkup { get; }

			// add a readonly property returning the static data
			property = new CodeMemberProperty();
			property.Name = "AutoMarkup";
			property.Type = new CodeTypeReference(typeof(AutoMarkupType));
			property.PrivateImplementationType = new CodeTypeReference(typeof(IJbstBuildResult));
			property.HasGet = true;
			// get { return autoMarkup; }
			property.GetStatements.Add(new CodeMethodReturnStatement(
				new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(AutoMarkupType)), autoMarkup.ToString())));
			resourceType.Members.Add(property);

			#endregion AutoMarkupType IJbstBuildResult.AutoMarkup { get; }
		}

		#endregion ResourceCodeProvider Methods

		#region Globalization Methods

		internal static void ExtractGlobalizationKeys(object root, List<string> globalizationKeys)
		{
			List<JbstControl> queue = new List<JbstControl>();
			queue.Add(root as JbstControl);

			while (queue.Count > 0)
			{
				// pop and cast
				JbstControl control = queue[0];
				queue.RemoveAt(0);
				if (control == null)
				{
					continue;
				}

				// queue up children
				JbstContainerControl container = control as JbstContainerControl;
				if (container != null)
				{
					if (container.ChildControlsSpecified)
					{
						queue.AddRange(container.ChildControls);
					}
					if (container.AttributesSpecified)
					{
						foreach (object value in container.Attributes.Values)
						{
							if (value is JbstControl)
							{
								queue.Add((JbstControl)value);
							}
						}
					}
					continue;
				}

				// parse expression blocks
				if (control is JbstExpressionBlock)
				{
					GlobalizedResourceHandler.ExtractGlobalizationKeys(((JbstExpressionBlock)control).Code, globalizationKeys);
					continue;
				}

				// parse statement blocks
				if (control is JbstStatementBlock)
				{
					GlobalizedResourceHandler.ExtractGlobalizationKeys(((JbstStatementBlock)control).Code, globalizationKeys);
					continue;
				}

				// look up declarative resource string expressions
				JbstExtensionBlock extension = control as JbstExtensionBlock;
				if (extension == null)
				{
					continue;
				}

				ResourceJbstExtension res = extension.Extension as ResourceJbstExtension;
				if (res == null)
				{
					continue;
				}

				string key = res.GlobalizationKey;
				if (!globalizationKeys.Contains(key))
				{
					globalizationKeys.Add(key);
				}
			}
		}

		#endregion Globalization Methods
	}
}
