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
						case '\uFEFF': // UTF marker?
						case '\t': //TAB
						case '\n': //LF
						case '\r': //CR
						case ' ': //Space
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
							try
							{
								CssStatement statement = this.ParseStatement(ch);
								this.styleSheet.Statements.Add(statement);
							}
							catch (ParseError ex)
							{
								Console.Error.WriteLine(ex);

								while (this.Read(out ch) && ch != '}')
								{
									// restabalize on next statement
								}
							}
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

			while (this.Read(out ch) && Char.IsWhiteSpace(ch))
			{
				// consuming whitespace
			}

			start = this.Position;// start with current char
			while (this.Read(out ch))
			{
				switch (ch)
				{
					case '{': //Block Begin
					{
						atRule.Value = this.Copy(start);
						CssBlock block = this.ParseBlock();
						atRule.Block = block;
						return atRule;
					}
					case ';': //At-Rule End
					{
						atRule.Value = this.Copy(start);
						return atRule;
					}
				}
			}
			throw new UnexpectedEndOfFile("Unclosed At-Rule", this.reader.FilePath, this.reader.Line, this.reader.Col);
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
#warning shouldn't have to trim, should parse individual selectors splitting on comma
				ruleSet.Selector.Value = ruleSet.Selector.Value.Trim();
			}

ParseDeclarations:
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

					while (this.Read(out ch))
					{
						// restabalize on next declaration
						switch (ch)
						{
							case '{':
							{
								throw new SyntaxError("Error parsing RuleSet", this.reader.FilePath, this.reader.Line, this.reader.Col);
							}
							//case ':':// keep going
							case ';':
							{
								// continue parsing rest of delcarations
								goto ParseDeclarations;
							}
							case '}':
							{
								// no more declarations
								return ruleSet;
							}
						}
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

			// consume property name
			switch (ch)
			{
				case '{':
				case ':':
				//case ';':
				{
					throw new SyntaxError("Declaration missing property name", this.reader.FilePath, this.reader.Line, this.reader.Col);
				}
				case '}':
				{
					// no more declarations
					return null;
				}
			}

			// read property, starting with current char
			int start = this.Position;
			while (this.Read(out ch) && !Char.IsWhiteSpace(ch) && ch != ':')
			{
				// consume property name
				switch (ch)
				{
					case '{':
					//case ':':
					case ';':
					case '}':
					{
						throw new SyntaxError("Invalid Property name: "+this.Copy(start), this.reader.FilePath, this.reader.Line, this.reader.Col);
					}
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
				throw new SyntaxError("Expected ':'", this.reader.FilePath, this.reader.Line, this.reader.Col);
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

			switch (ch)
			{
				case '{':
				case ':':
				case ';':
				case '}':
				{
					throw new SyntaxError("Invalid char in property value: '"+ch+"'", this.reader.FilePath, this.reader.Line, this.reader.Col);
				}
			}

			// read value, starting with current char
			int start = this.Position;
			while (this.Read(out ch))
			{
				// consume declaration value

				switch (ch)
				{
					case '{':
					//case ':':// leave in for "filter: progid:DXImageTransform.Microsoft..."
					{
						throw new SyntaxError("Invalid property value: "+this.Copy(start), this.reader.FilePath, this.reader.Line, this.reader.Col);
					}
					case '}':
					case ';':
					{
#warning According to the grammar CssValue can be more complicated than a string
						value.Values.Add(new CssString(this.Copy(start)));
						if (ch == '}')
						{
							this.reader.PutBack(1);
						}
						return value;
					}
				}
			}
			throw new UnexpectedEndOfFile("Unclosed declaration", this.reader.FilePath, this.reader.Line, this.reader.Col);
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
