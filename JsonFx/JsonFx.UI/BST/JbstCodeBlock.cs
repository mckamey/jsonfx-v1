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

		private const string EmbeddedFormat = "function($item){{{0}}}";
		private const string ExpressionFormat = "function($item){{return({0});}}";
#if DEBUG
		private const string CommentFormat = "/*{0}*/";
#endif

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

			for (int i=0; i<code.Length; i++)
			{
				if (Char.IsWhiteSpace(code[i]))
				{
					continue;
				}

				switch (code[i])
				{
					case '@':
					{
						this.type = JbstCodeBlockType.Directive;
						this.code = code.Substring(i+1).Trim();
						break;
					}
					case '!':
					{
						this.type = JbstCodeBlockType.Declaration;
						this.code = code.Substring(i+1).Trim();
						break;
					}
					case '-':
					{
						// look ahead one
						if (i+1 >= code.Length ||
							code[i+1] != '-')
						{
							// is general case
							goto default;
						}

						this.type = JbstCodeBlockType.Comment;
						this.code = code.Substring(i+2).TrimEnd('-').Trim();
						break;
					}
					case '=':
					{
						this.type = JbstCodeBlockType.Expression;
						this.code = code.Substring(i+1).Trim();
						break;
					}
					default:
					{
						this.type = JbstCodeBlockType.EmbeddedScript;
						this.code = code.Substring(i+1).Trim();
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
			switch (this.type)
			{
				case JbstCodeBlockType.Expression:
				{
					// output expressions are the core of the syntax
					writer.TextWriter.Write(ExpressionFormat, this.Code);
					break;
				}
				case JbstCodeBlockType.EmbeddedScript:
				case JbstCodeBlockType.Declaration:
				{
					// currently there isn't a scope difference between
					// JSP-style declarations and embedded code blocks
					writer.TextWriter.Write(EmbeddedFormat, this.Code);
					break;
				}
				case JbstCodeBlockType.Comment:
				{
#if DEBUG
					// comments are only emitted during debug builds
					writer.TextWriter.Write(CommentFormat, this.Code);
#endif
					break;
				}
				case JbstCodeBlockType.Directive:
				{
					// there is no output typically from directives
					break;
				}
				case JbstCodeBlockType.None:
				default:
				{
					break;
				}
			}
		}

		void JsonFx.Json.IJsonSerializable.ReadJson(JsonFx.Json.JsonReader reader)
		{
			throw new NotImplementedException("JbstCodeBlock deserialization is not yet implemented.");
		}

		#endregion IJsonSerializable Members
	}
}
