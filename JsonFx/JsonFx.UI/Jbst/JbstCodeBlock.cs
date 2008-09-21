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

using BuildTools.ScriptCompactor;
using JsonFx.Json;
using JsonFx.UI.Jbst.Extensions;

namespace JsonFx.UI.Jbst
{
	#region JbstCodeBlockType

	/*
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
	*/

	#endregion JbstCodeBlockType

	/// <summary>
	/// Internal representation of a JsonML+BST code block.
	/// </summary>
	internal abstract class JbstCodeBlock : IJbstControl, IJsonSerializable
	{
		#region Fields

		private readonly string code;
		private readonly string path;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="type"></param>
		/// <param name="code"></param>
		/// <param name="path"></param>
		protected JbstCodeBlock(string code, string path)
		{
			this.code = (code == null) ? String.Empty : code;
			this.path = (path == null) ? String.Empty : path;
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

		#region Methods

		protected abstract string GetCodeBlock();

		#endregion Methods

		#region IJsonSerializable Members

		void JsonFx.Json.IJsonSerializable.WriteJson(JsonWriter writer)
		{
			string codeBlock = this.GetCodeBlock();

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

	internal class JbstCommentBlock : JbstCodeBlock
	{
		#region Constants

		private const string CommentFormat = "\"\"/* {0} */";

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		/// <param name="path"></param>
		public JbstCommentBlock(string code, string path)
			: base(code, path)
		{
		}

		#endregion Init

		#region JbstCodeBlock Members

		protected override string GetCodeBlock()
		{
			// comments are only emitted for pretty-print
			return String.Format(CommentFormat, this.Code.Replace("*/", "*\\/"));
		}

		#endregion JbstCodeBlock Members
	}

	internal class JbstExpressionBlock : JbstCodeBlock
	{
		#region Constants

		private const string ExpressionFormat =
			@"function() {{
				return {0};
			}}";

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		/// <param name="path"></param>
		public JbstExpressionBlock(string code, string path)
			: base(code, path)
		{
		}

		#endregion Init

		#region JbstCodeBlock Members

		protected override string GetCodeBlock()
		{
			// output expressions are the core of the syntax
			return String.Format(ExpressionFormat, this.Code);
		}

		#endregion JbstCodeBlock Members
	}

	internal class JbstStatementBlock : JbstCodeBlock
	{
		#region Constants

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

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		/// <param name="path"></param>
		public JbstStatementBlock(string code, string path)
			: base(code, path)
		{
		}

		#endregion Init

		#region JbstCodeBlock Members

		protected override string GetCodeBlock()
		{

			// analogous to instance code, or JSP scriptlets
			// executed each time template is bound

			// add legacy support for those coming from ASP/JSP
			if (this.Code.IndexOf(AspResponse+'.') >= 0)
			{
				// Response.write
				return String.Format(StatementLegacyFormat, this.Code, AspResponse);
			}
			else if (this.Code.IndexOf("System.out.") >= 0)
			{
				// System.out.print
				return String.Format(StatementLegacyFormat, this.Code, JspResponseVar);
			}
			else if (this.Code.IndexOf(JspResponse+'.') >= 0)
			{
				// out.print
				return String.Format(StatementLegacyFormat, this.Code, JspResponse);
			}
			else
			{
				return String.Format(StatementFormat, this.Code);
			}
		}

		#endregion JbstCodeBlock Members
	}

	internal class JbstExtensionBlock : JbstCodeBlock
	{
		#region Constants

		private static readonly char[] PrefixDelim = { ':' };

		#endregion Constants

		#region Fields

		private readonly JbstExtension extension = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		/// <param name="path"></param>
		public JbstExtensionBlock(string code, string path)
			: base(code, path)
		{
			KeyValuePair<string, string> expr = JbstExtensionBlock.ParseExpression(code);

			// TODO: lookup the corresponding extension evaluator from web.config
			switch (expr.Key)
			{
				case "AppSettings":
				{
					this.extension = new AppSettingsJbstExtension(expr.Value, path);
					break;
				}
				case "Resources":
				{
					this.extension = new ResourceJbstExtension(expr.Value, path);
					break;
				}
				default:
				{
					this.extension = new JbstExtension(code, path);
					break;
				}
			}
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the extension represented by this block
		/// </summary>
		public JbstExtension Extension
		{
			get { return this.extension; }
		}

		#endregion Properties

		#region JbstCodeBlock Members

		protected override string GetCodeBlock()
		{
			// execute the corresponding extension evaluator
			return this.Extension.Eval();
		}

		#endregion JbstCodeBlock Members

		#region Utility Methods

		private static KeyValuePair<string, string> ParseExpression(string extension)
		{
			string key = String.Empty;
			string value = String.Empty;

			if (!String.IsNullOrEmpty(extension))
			{
				// split on first ':'
				string[] parts = extension.Split(JbstExtensionBlock.PrefixDelim, 2, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length > 1)
				{
					value = parts[1].Trim();
				}

				key = parts[0].Trim();
			}

			return new KeyValuePair<string, string>(key, value);
		}

		#endregion Utility Methods
	}
}
