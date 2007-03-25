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

		/// <summary>
		/// (BNF) stylesheet : [ CDO | CDC | S | statement ]*;
		/// </summary>
		private void ParseStyleSheet()
		{
			using (this.reader = new LineReader(filePath, CssParser.ReadFilters))
			{
				this.reader.NormalizeWhiteSpace = true;
				this.styleSheet = new CssStyleSheet();

				char ch;
				while (this.Read(out ch))
				{
					switch (ch)
					{
						case '\uFEFF': // BOM (UTF byte order mark)
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
								CssStatement statement = this.ParseStatement();
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

		/// <summary>
		/// (BNF) statement : ruleset | at-rule;
		/// </summary>
		/// <returns></returns>
		private CssStatement ParseStatement()
		{
			if (this.reader.Current == '@')
			{
				return this.ParseAtRule();
			}
			else
			{
				this.PutBack();
				return this.ParseRuleSet();
			}
		}

		#endregion Statement

		#region At-Rule

		/// <summary>
		/// (BNF) at-rule : ATKEYWORD S* any* [ block | ';' S* ];
		/// </summary>
		/// <returns></returns>
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
						//CssBlock block = this.ParseBlock();
						//atRule.Block = block;

						while (true)
						{
							while (this.Read(out ch) && Char.IsWhiteSpace(ch))
							{
								// consume whitespace
							}

							if (ch != '}')
							{
								CssStatement statement = this.ParseStatement();
								atRule.Block.Add(statement);
								continue;
							}

							return atRule;
						}
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

		/// <summary>
		/// NBF block : '{' S* [ any | block | ATKEYWORD S* | ';' S* ]* '}' S*;
		/// </summary>
		/// <returns></returns>
		private CssBlock ParseBlock()
		{
			CssBlock block = new CssBlock();
			int start = this.Position;// start with current char
			char ch;

			while (this.Read(out ch))
			{
				switch (ch)
				{
					case '@':
					{
						// copy anything before
						string value = this.Copy(start);
						if (value != null && !String.IsNullOrEmpty(value = value.Trim()))
						{
							CssString any = new CssString();
							any.Value = value;
							block.Values.Add(any);
						}

						// parse inner block
						CssAtRule atRule = this.ParseAtRule();
						block.Values.Add(atRule);

						// reset start with current char
						start = this.Position;
						break;
					}
					case '{':
					{
						// copy anything before
						string value = this.Copy(start);
						if (value != null && !String.IsNullOrEmpty(value = value.Trim()))
						{
							CssString any = new CssString();
							any.Value = value;
							block.Values.Add(any);
						}

						// parse inner block
						CssBlock innerBlock = this.ParseBlock();
						block.Values.Add(innerBlock);

						// reset start with current char
						start = this.Position;
						break;
					}
					case '}':
					{
						// copy anything before
						string value = this.Copy(start);
						if (value != null && !String.IsNullOrEmpty(value = value.Trim()))
						{
							CssString any = new CssString();
							any.Value = value;
							block.Values.Add(any);
						}

						return block;
					}
				}
			}

			throw new UnexpectedEndOfFile("Unclosed block", this.reader.FilePath, this.reader.Line, this.reader.Col);
		}

		#endregion Block

		#region RuleSet

		/// <summary>
		/// (BNF) ruleset : selector? '{' S* declaration? [ ';' S* declaration? ]* '}' S*;
		/// </summary>
		/// <returns></returns>
		private CssRuleSet ParseRuleSet()
		{
			char ch;
			CssRuleSet ruleSet = new CssRuleSet();

		ParseSelectors:
			while (true)
			{
				try
				{
					CssSelector selector = this.ParseSelector();
					if (selector == null)
					{
						break;
					}
					ruleSet.Selectors.Add(selector);
				}
				catch (ParseError ex)
				{
					Console.Error.WriteLine(ex);

					while (this.Read(out ch))
					{
						// restabalize on next rulset
						switch (ch)
						{
							case ',':
							{
								// continue parsing rest of Selectors
								goto ParseSelectors;
							}
							case '{':
							{
								goto ParseDeclarations;
							}
							//case ':':// keep going
							case ';':
							case '}':
							{
								throw new SyntaxError("Error parsing Selectors", this.reader.FilePath, this.reader.Line, this.reader.Col);
							}
						}
					}
				}
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

		#region Selector

		/// <summary>
		/// (BNF) selector: any+;
		/// </summary>
		/// <returns></returns>
		private CssSelector ParseSelector()
		{
			CssSelector selector = new CssSelector();
			char ch;

			while (this.Read(out ch) && (Char.IsWhiteSpace(ch) || ch == ','))
			{
				// skip whitespace, and empty selectors
			}

			// consume property name
			switch (ch)
			{
				case '{':
				{
					// no more declarations
					return null;
				}
				//case ':':// pseudoclass
				case ';':
				case '}':
				{
					throw new SyntaxError("Invalid chars in Selector", this.reader.FilePath, this.reader.Line, this.reader.Col);
				}
			}

			int start = this.Position;// start with current char

			while (this.Read(out ch))
			{
				// continue consuming selector
				switch (ch)
				{
					case ',':
					case '{':
					{
						selector.Value = this.Copy(start);
						if (ch == '{')
						{
							this.PutBack();
						}
						return selector;
					}
					//case ':':// pseudoclass
					case ';':
					case '}':
					{
						throw new SyntaxError("Error parsing Selector", this.reader.FilePath, this.reader.Line, this.reader.Col);
					}
				}
			}
			throw new UnexpectedEndOfFile("Unclosed Selector", this.reader.FilePath, this.reader.Line, this.reader.Col);
		}

		#endregion Selector

		#region Declaration

		/// <summary>
		/// (BNF) declaration : property ':' S* value;
		/// (BNF) property    : IDENT S*;
		/// </summary>
		/// <returns></returns>
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
					{
						throw new SyntaxError("Invalid Property name: "+this.Copy(start), this.reader.FilePath, this.reader.Line, this.reader.Col);
					}
					case '}':
					{
						this.PutBack();
						goto case ';';
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

			CssValueList value = this.ParseValue();
			declaration.Value = value;

			return declaration;
		}
		
		#endregion Declaration

		#region Value

		/// <summary>
		/// (BNF) value :	[ any | block | ATKEYWORD S* ]+;
		/// (BNF) any :		[ IDENT | NUMBER | PERCENTAGE | DIMENSION | STRING
		///					| DELIM | URI | HASH | UNICODE-RANGE | INCLUDES
		///					| FUNCTION S* any* ')' | DASHMATCH | '(' S* any* ')'
		///					| '[' S* any* ']' ] S*;
		/// </summary>
		/// <returns></returns>
		private CssValueList ParseValue()
		{
			CssValueList value = new CssValueList();
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
#warning Should this parse value further?

						CssString any = new CssString();
						any.Value = this.Copy(start);
						value.Values.Add(any);
						if (ch == '}')
						{
							this.PutBack();
						}
						return value;
					}
				}
			}
			throw new UnexpectedEndOfFile("Unclosed declaration", this.reader.FilePath, this.reader.Line, this.reader.Col);
		}

		#endregion Value

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

		/// <summary>
		/// Put one character back
		/// </summary>
		private void PutBack()
		{
			this.reader.PutBack();
		}

		#endregion Reader Methods
	}
}
