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

using BuildTools.ScriptCompactor;
using JsonFx.Extensions;

namespace JsonFx.JsonML.BST
{
	#region JbstCodeBlockType

	internal enum JbstCodeBlockType
	{
		None,
		Comment,
		Directive,
		Declaration,
		Expression,
		Statement,
		Extension
	}

	#endregion JbstCodeBlockType

	/// <summary>
	/// Internal representation of a JsonML+BST code block.
	/// </summary>
	internal class JbstCodeBlock : IJbstControl, Json.IJsonSerializable
	{
		#region Constants

		private const string CommentFormat = "/* {0} */ \"\"";

		private const string ExpressionFormat =
			@"function() {{
				return ({0});
			}}";

		private const string StatementFormat =
			@"function() {{
				{0}
			}}";

		private const string StatementLegacyFormat =
			@"function() {{
				var {1} = new JsonML.Response();
				{0}
				return {1}.render();
			}}";

		private const string AspResponse = "Reponse";
		private const string JspResponse = "out";
		private const string JspResponseVar = "System = {}; System.out";

		#endregion Constants

		#region Field

		private readonly string code;
		private readonly JbstCodeBlockType type;

		#endregion Field

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		/// <param name="type"></param>
		public JbstCodeBlock(string code, JbstCodeBlockType type)
		{
			this.code = (code == null) ? String.Empty : code;
			this.type = type;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the code block content
		/// </summary>
		public string Code
		{
			get { return this.code; }
		}

		/// <summary>
		/// Gets the code block type
		/// </summary>
		public JbstCodeBlockType Type
		{
			get { return this.type; }
		}

		#endregion Properties

		#region IJsonSerializable Members

		void JsonFx.Json.IJsonSerializable.WriteJson(JsonFx.Json.JsonWriter writer)
		{
			string codeBlock;
			switch (this.type)
			{
				case JbstCodeBlockType.Comment:
				{
					// comments are only emitted for pretty-print
					codeBlock = String.Format(CommentFormat, this.Code.Replace("*/", "*\\/"));
					break;
				}
				case JbstCodeBlockType.Expression:
				{
					// output expressions are the core of the syntax
					codeBlock = String.Format(ExpressionFormat, this.Code);
					break;
				}
				case JbstCodeBlockType.Statement:
				{
					// analogous to instance code, or JSP scriptlets
					// executed each time template is bound

					// add legacy support for those coming from ASP/JSP
					if (this.Code.IndexOf(AspResponse+'.') >= 0)
					{
						// Response.write
						codeBlock = String.Format(StatementLegacyFormat, this.Code, AspResponse);
					}
					else if (this.Code.IndexOf("System.out.") >= 0)
					{
						// System.out.print
						codeBlock = String.Format(StatementLegacyFormat, this.Code, JspResponseVar);
					}
					else if (this.Code.IndexOf(JspResponse+'.') >= 0)
					{
						// out.print
						codeBlock = String.Format(StatementLegacyFormat, this.Code, JspResponse);
					}
					else
					{
						codeBlock = String.Format(StatementFormat, this.Code);
					}
					break;
				}
				case JbstCodeBlockType.Extension:
				{
					// execute the corresponding extension evaluator
					codeBlock = JbstExtension.Evaluate(this.Code);
					break;
				}
				default:
				{
					// others not supported inline
					return;
				}
			}

			if (String.IsNullOrEmpty(codeBlock))
			{
				return;
			}

			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(codeBlock);
			}
			else
			{
				// min the output for better compaction
				// signal to JSMin that isn't linted so
				// doesn't break users code if they leave
				// off semicolons, etc.
				new JSMin().Run(codeBlock, writer.TextWriter);
			}
		}

		void JsonFx.Json.IJsonSerializable.ReadJson(JsonFx.Json.JsonReader reader)
		{
			throw new NotImplementedException("JbstCodeBlock deserialization is not yet implemented.");
		}

		#endregion IJsonSerializable Members
	}
}
