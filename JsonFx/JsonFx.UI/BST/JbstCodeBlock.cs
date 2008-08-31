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

namespace JsonFx.JsonML.BST
{
	/// <summary>
	/// Internal representation of a JsonML+BST code block.
	/// </summary>
	internal class JbstCodeBlock : IJbstControl, Json.IJsonSerializable
	{
		#region JbstCodeBlockType

		private enum JbstCodeBlockType
		{
			None,
			Directive,
			Declaration,
			Comment,
			EmbeddedScript,
			Expression
		}

		#endregion JbstCodeBlockType

		#region Constants

		private const string EmbeddedFormat = "function() {{ {0} }}";
		private const string ExpressionFormat = "function() {{ return ( {0} ); }}";
		private const string CommentFormat = "/*{0}*/";

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
		public JbstCodeBlock(string code)
		{
			if (code == null)
			{
				code = String.Empty;
			}

			this.code = String.Empty;
			this.type = JbstCodeBlockType.None;

			bool done = false;
			for (int i=0; !done && i<code.Length; i++)
			{
				switch (code[i])
				{
					case '@':
					{
						this.type = JbstCodeBlockType.Directive;
						this.code = code.Substring(i+1);
						done = true;
						break;
					}
					case '!':
					{
						this.type = JbstCodeBlockType.Declaration;
						this.code = code.Substring(i+1);
						done = true;
						break;
					}
					case '-':
					{
						if (!code.StartsWith("--") ||
							!code.EndsWith("--"))
						{
							// is general case e.g. "--this.index;"
							goto default;
						}

						this.type = JbstCodeBlockType.Comment;
						this.code = code.Substring(i+2).TrimEnd('-');
						done = true;
						break;
					}
					case '=':
					{
						this.type = JbstCodeBlockType.Expression;
						this.code = code.Substring(i+1);
						done = true;
						break;
					}
					default:
					{
						this.type = JbstCodeBlockType.EmbeddedScript;
						this.code = code;
						done = true;
						break;
					}
				}
			}
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

		#endregion Properties

		#region IJsonSerializable Members

		void JsonFx.Json.IJsonSerializable.WriteJson(JsonFx.Json.JsonWriter writer)
		{
			string output;
			switch (this.type)
			{
				case JbstCodeBlockType.Expression:
				{
					// output expressions are the core of the syntax
					output = String.Format(ExpressionFormat, this.Code);
					break;
				}
				case JbstCodeBlockType.EmbeddedScript:
				case JbstCodeBlockType.Declaration:
				{
					// currently there isn't a scope difference between
					// JSP-style declarations and embedded code blocks
					output = String.Format(EmbeddedFormat, this.Code);
					break;
				}
				case JbstCodeBlockType.Comment:
				{
					// comments are only emitted during debug builds
					output = String.Format(CommentFormat, this.Code);
					break;
				}
				case JbstCodeBlockType.Directive:
				{
					// there is no output typically from directives
					output = null;
					break;
				}
				case JbstCodeBlockType.None:
				default:
				{
					output = null;
					break;
				}
			}

			if (String.IsNullOrEmpty(output))
			{
				return;
			}

			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(output);
			}
			else
			{
				// min the output for better compaction
				// signal to JSMin that isn't linted so
				// doesn't break users code if they leave
				// off semicolons, etc.
				new JSMin().Run(output, writer.TextWriter, false, true);
			}
		}

		void JsonFx.Json.IJsonSerializable.ReadJson(JsonFx.Json.JsonReader reader)
		{
			throw new NotImplementedException("JbstCodeBlock deserialization is not yet implemented.");
		}

		#endregion IJsonSerializable Members
	}
}
