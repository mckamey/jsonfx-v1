using System;
using System.IO;
using System.Collections.Generic;

using BuildTools;
using BuildTools.IO;

namespace BuildTools.CssCompactor
{
	public class CssParser
	{
		#region Constants

		// this defines comments for CSS
		private static readonly ReadFilter[] ReadFilters = new ReadFilter[] { new ReadFilter("/*", "*/") };

		private object SyncLock = new object();
		LineReader reader;
		CssStyleSheet styleSheet = null;
		string filePath = null;

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public CssParser(String filePath)
		{
			if (String.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException("filePath");
			}

			this.filePath = filePath;
		}

		#endregion Init

		#region Properties

		public CssStyleSheet StyleSheet
		{
			get
			{
				if (this.styleSheet == null)
				{
					lock (this.SyncLock)
					{
						// check again in case race condition
						// so we don't parse twice
						if (this.styleSheet == null)
						{
							this.ParseStyleSheet();
						}
					}
				}
				return this.styleSheet;
			}
		}

		#endregion Properties

		#region Parse Methods

		#region StyleSheet

		protected void ParseStyleSheet()
		{
			using (this.reader = new LineReader(filePath, CssParser.ReadFilters))
			{
				this.styleSheet = new CssStyleSheet();

				char ch;
				while (this.Read(out ch))
				{
					switch (ch)
					{
						case '\uFEFF':
						case '\u0009': //TAB
						case '\u000A': //LF
						case '\u000D': //CR
						case '\u0020': //Space
						{
							// skip whitespace
							continue;
						}
						case '<':
						{
							// CDO (Char Data Open?)
							if (!this.Read(out ch) || ch != '-' ||
								!this.Read(out ch) || ch != '-')
							{
								throw new SyntaxError("Expected \"<!--\"", this.reader.FilePath, this.reader.Line, this.reader.Col);
							}
							continue;
						}
						case '-':
						{
							// CDC (Char Data Close?)
							if (!this.Read(out ch) || ch != '-' ||
								!this.Read(out ch) || ch != '>')
							{
								throw new SyntaxError("Expected \"<!--\"", this.reader.FilePath, this.reader.Line, this.reader.Col);
							}
							continue;
						}
						default:
						{
							CssStatement statement = this.ParseStatement(ch);
							this.styleSheet.Statements.Add(statement);
							continue;
						}
					}
				}
			}

			this.reader = null;
		}

		#endregion StyleSheet

		#region Statement

		protected CssStatement ParseStatement(char ch)
		{
			if (ch == '@')
			{
				return this.ParseAtRule();
			}
			else
			{
				return this.ParseRuleSet(ch);
			}
		}

		#endregion Statement

		#region At-Rule

		private CssAtRule ParseAtRule()
		{
			CssAtRule atRule = new CssAtRule();
			int start = this.Position+1;// start with first char of ident
			char ch;

			while (this.Read(out ch) && !Char.IsWhiteSpace(ch))
			{
				// continue consuming
			}

			atRule.Ident = this.Copy(start);

			start = this.Position;// start with current char
			while (this.Read(out ch))
			{
				switch (ch)
				{
					case '{': //Block Begin
					{
						CssBlock block = this.ParseBlock();
						atRule.Block = block;
						break;
					}
					case ';': //At-Rule End
					{
						atRule.Value = this.Copy(start);
						break;
					}
					default:
					{
						// consume until whitespace
						continue;
					}
				}
			}

			return atRule;
		}

		#endregion At-Rule

		#region Block

		private CssBlock ParseBlock()
		{
			CssBlock block = new CssBlock();
			int start = this.Position;// start with current char
			char ch;

			while (this.Read(out ch) && ch != '}')
			{
#warning This should parse block
				if (ch == '{')
				{
					CssBlock innerBlock = this.ParseBlock();
				}
			}

			block.Value = this.Copy(start);
			return block;
		}

		#endregion Block

		#region RuleSet

		private CssRuleSet ParseRuleSet(char ch)
		{
			CssRuleSet ruleSet = new CssRuleSet();
			int start = this.Position;// start with current char

			if (ch != '{')
			{
				while (this.Read(out ch) && ch != '{')
				{
					// continue consuming selector
				}
			}

			ruleSet.Selector.Value = this.Copy(start);
			if (ruleSet.Selector.Value != null)
			{
#warning shouldn't have to trim
				ruleSet.Selector.Value = ruleSet.Selector.Value.Trim();
			}

			while (true)
			{
				try
				{
					CssDeclaration declaration = this.ParseDeclaration();
					if (declaration == null)
					{
						break;
					}
					ruleSet.Declarations.Add(declaration);
				}
				catch (ParseError ex)
				{
					Console.Error.WriteLine(ex);

					while (this.Read(out ch) && ch != ';' && ch != '}')
					{
						// restabalize on next declaration
					}
				}
			}

			return ruleSet;
		}

		#endregion RuleSet

		#region Declaration

		private CssDeclaration ParseDeclaration()
		{
			CssDeclaration declaration = new CssDeclaration();
			char ch;

			while (this.Read(out ch) && (Char.IsWhiteSpace(ch) || ch == ';'))
			{
				// skip whitespace, and empty declarations
			}

			if (ch == '}')
			{
				// no more declarations
				return null;
			}
			if (ch == ':')
			{
				throw new SyntaxError("Declaration missing property", this.reader.FilePath, this.reader.Line, this.reader.Col);
			}

			// read property, starting with current char
			int start = this.Position;
			while (this.Read(out ch) && !Char.IsWhiteSpace(ch) && ch != ':')
			{
				// consume property name

				if (ch == ';' || ch == '}')
				{
					throw new SyntaxError("Error in declaration property", this.reader.FilePath, this.reader.Line, this.reader.Col);
				}
			}
			declaration.Property = this.Copy(start);

			if (Char.IsWhiteSpace(ch))
			{
				while (this.Read(out ch) && (Char.IsWhiteSpace(ch)))
				{
					// skip whitespace & delimiter
				}
			}

			if (ch != ':')
			{
				throw new SyntaxError("Declaration missing value", this.reader.FilePath, this.reader.Line, this.reader.Col);
			}

			CssValue value = this.ParseValue();
			declaration.Value = value;

			return declaration;
		}

		private CssValue ParseValue()
		{
			CssValue value = new CssValue();
			char ch;

			while (this.Read(out ch) && Char.IsWhiteSpace(ch))
			{
				// skip whitespace, and empty declarations
			}

			if (ch == '}' || ch == ';')
			{
				throw new SyntaxError("Declaration missing value", this.reader.FilePath, this.reader.Line, this.reader.Col);
			}

			// read value, starting with current char
			int start = this.Position;
			while (this.Read(out ch) && ch != ';' && ch != '}')
			{
				// consume declaration value
			}

#warning According to the grammar CssValue can be more complicated than a string
			value.Values.Add(new CssString(this.Copy(start)));
			return value;
		}

		#endregion Declaration

		#endregion Parse Methods

		#region Methods

		public void Write(TextWriter writer, CssOptions options)
		{
			this.StyleSheet.Write(writer, options);
		}

		#endregion Methods

		#region Reader Methods

		private int Position
		{
			get { return this.reader.Position; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ch"></param>
		/// <returns>Success</returns>
		private bool Read(out char ch)
		{
			if (this.reader.EndOfStream)
			{
				throw new UnexpectedEndOfFile("Reading past end of file", this.reader.FilePath, this.reader.Line, this.reader.Col);
			}

			int c = this.reader.Read();
			if (c < 0)
			{
				ch = '\0';
				return false;
			}
			ch = (char)c;
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ch"></param>
		/// <returns>Success</returns>
		private bool Peek(out char ch)
		{
			int c = this.reader.Peek();
			if (c < 0)
			{
				ch = '\0';
				return false;
			}
			ch = (char)c;
			return true;
		}

		/// <summary>
		/// Copies chars from start until the position before the current position
		/// </summary>
		/// <returns></returns>
		private string Copy(int start)
		{
			// read block
			return this.reader.Copy(start, this.reader.Position-1);
		}

		#endregion Reader Methods
	}
}
