using System;
using System.IO;

/*
	Originally written in C and then converted to C#.
	jsmin.c
	2003-04-21

Copyright (c) 2002 Douglas Crockford  (www.crockford.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

The Software shall be used for Good, not Evil.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace ScriptCompactor
{
	public class JSMinifier
	{
		#region Constants

		private const char EOF = Char.MinValue;

		#endregion Constants

		#region Fields

		private StreamReader reader;
		private StreamWriter writer;
		private char theA;
		private char theB;
		private char theLookahead = JSMinifier.EOF;
		private char lastWritten = JSMinifier.EOF;

		#endregion Fields

		#region Public Methods

		public void Minify(string inputFile, string outputFile, bool overwrite)
		{
			this.Minify(inputFile, outputFile, overwrite, null);
		}

		public void Minify(string inputFile, string outputFile, bool overwrite, string copyright)
		{
			if (!File.Exists(inputFile))
				throw new FileNotFoundException(String.Format("File (\"{0}\") not found.", inputFile), inputFile);

			if (!overwrite && File.Exists(outputFile))
				throw new AccessViolationException(String.Format("File (\"{0}\") already exists.", outputFile));

			using (this.reader = new StreamReader(inputFile))
			{
				JSMinifier.PrepSavePath(outputFile);
				using (this.writer = new StreamWriter(outputFile, false))
				{
					if (!String.IsNullOrEmpty(copyright))
					{
						this.writer.WriteLine(copyright);
					}

					this.JSMin();
				}
			}
		}

		/// <summary>
		/// Makes sure directory exists and if file exists is not readonly.
		/// </summary>
		/// <param name="filename"></param>
		protected static void PrepSavePath(string filename)
		{
			if (File.Exists(filename))
			{
				// make sure not readonly
				FileAttributes attributes = File.GetAttributes(filename);
				attributes &= ~FileAttributes.ReadOnly;
				File.SetAttributes(filename, attributes);
			}
			else if (!Directory.Exists(Path.GetDirectoryName(filename)))
			{
				// make sure directories exist
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
			}
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Copy the input to the output, deleting the characters which are
		///		insignificant to JavaScript. Comments will be removed. Tabs will be
		///		replaced with spaces. Carriage returns will be replaced with linefeeds.
		///		Most spaces and linefeeds will be removed. 
		/// </summary>
		private void JSMin()
		{
			this.theA = '\n';
			this.Do(Action.Read);
			while (this.theA != JSMinifier.EOF)
			{
				switch (this.theA)
				{
					case ' ':
					{
						if (this.IsAlphaNum(this.theB))
						{
							this.Do(Action.Output);
						}
						else
						{
							this.Do(Action.Shift);
						}
						break;
					}
					case '\n':
					{
						switch (this.theB)
						{
							case '{':
							case '[':
							case '(':
							case '+':
							case '-':
							{
								this.Do(Action.Output);
								break;
							}
							case ' ':
							{
								this.Do(Action.Read);
								break;
							}
							default:
							{
								if (this.IsAlphaNum(this.theB))
								{
									this.Do(Action.Output);
								}
								else
								{
									this.Do(Action.Shift);
								}
								break;
							}
						}
						break;
					}
					default:
					{
						switch (this.theB)
						{
							case ' ':
							{
								if (this.IsAlphaNum(this.theA))
								{
									this.Do(Action.Output);
									break;
								}
								this.Do(Action.Read);
								break;
							}
							case '\n':
							{
								switch (this.theA)
								{
									case '}':
									case ']':
									case ')':
									case '+':
									case '-':
									case '"':
									case '\'':
									{
										this.Do(Action.Output);
										break;
									}
									default:
									{
										if (this.IsAlphaNum(this.theA))
										{
											this.Do(Action.Output);
										}
										else
										{
											this.Do(Action.Read);
										}
										break;
									}
								}
								break;
							}
							default:
							{
								this.Do(Action.Output);
								break;
							}
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// Do something! What to do is determined by the argument:
		///		1   Output A. Copy B to A. Get the next B.
		///		2   Copy B to A. Get the next B. (Delete A).
		///		3   Get the next B. (Delete B).
		///   Treats a string as a single character. Wow!
		///   Recognizes a regular expression if it is preceded by ( or , or =. 
		/// </summary>
		/// <param name="d"></param>
		private void Do(Action action)
		{
			switch (action)
			{
				case Action.Output:
				{
					this.Put(this.theA);
					this.Do(Action.Shift);
					break;
				}
				case Action.Shift:
				{
					this.theA = this.theB;
					if (this.theA == '\'' || this.theA == '"')
					{
						while (true)
						{
							this.Put(this.theA);
							this.theA = this.Get();
							if (this.theA == this.theB)
								break;

							if (this.theA <= '\n')
								throw new Exception(String.Format("Unterminated string literal: {0}", (int)this.theA));

							if (this.theA == '\\')
							{
								this.Put(this.theA);
								this.theA = this.Get();
							}
						}
					}
					this.Do(Action.Read);
					break;
				}
				case Action.Read:
				{
					this.theB = this.Next();
					if (this.theB == '/' && (this.theA == '(' || this.theA == ',' || this.theA == '='))
					{
						this.Put(this.theA);
						this.Put(this.theB);
						while (true)
						{
							this.theA = this.Get();
							if (this.theA == '/')
							{
								break;
							}
							else if (this.theA == '\\')
							{
								this.Put(this.theA);
								this.theA = this.Get();
							}
							else if (this.theA <= '\n')
							{
								throw new Exception(String.Format("Unterminated Regular Expression literal : {0}.", (int)this.theA));
							}
							this.Put(this.theA);
						}
						this.theB = this.Next();
					}
					break;
				}
				default:
				{
					throw new Exception("Unknown Action.");
				}
			}
		}

		/// <summary>
		/// Get the next character, excluding comments. Peek() is used to see
		///		if a '/' is followed by a '/' or '*'.
		/// </summary>
		/// <returns></returns>
		private char Next()
		{
			char c = this.Get();
			if (c == '/')
			{
				switch (this.Peek())
				{
					case '/':
					{
						while (true)
						{
							c = this.Get();
							if (c <= '\n')
								return c;
						}
					}
					case '*':
					{
						this.Get();
						while (true)
						{
							switch (this.Get())
							{
								case '*':
								{
									if (this.Peek() == '/')
									{
										this.Get();
										return ' ';
									}
									break;
								}
								case JSMinifier.EOF:
								{
									throw new Exception("Unterminated comment.");
								}
							}
						}
					}
					default:
					{
						return c;
					}
				}
			}
			return c;
		}

		/// <summary>
		/// Get the next character without getting it.
		/// </summary>
		/// <returns>the next character without getting it</returns>
		private char Peek()
		{
			this.theLookahead = this.Get();
			return this.theLookahead;
		}

		/// <summary>
		/// Return the next character from input. Watch out for lookahead. If
		///		the character is a control character, translate it to a space or
		///		linefeed.
		/// </summary>
		/// <returns>the next character from input</returns>
		private char Get()
		{
			// shift char from look ahead
			char c = this.theLookahead;
			this.theLookahead = JSMinifier.EOF;

			if (c == JSMinifier.EOF)
			{
				// this is the only place where we use int for char
				// StreamReader.Read returns -1 for EOF
				// we are using Char.MinValue since it usually isn't valid anyway
				// so must first check if we actually found our EOF and replace it
				int ch = this.reader.Read();
				if (ch == JSMinifier.EOF)
					ch = ' ';
				c = (ch == -1) ? JSMinifier.EOF : (char)ch;
			}

			if (c >= ' ' || c == '\n' || c == JSMinifier.EOF)
				return c;

			if (c == '\r')
				return '\n';

			return ' ';
		}

		/// <summary>
		/// Writes a character to output
		///		- writes '\n' as platform StreamWriter.NewLine
		///		- removes unnecessary line endings
		/// </summary>
		/// <param name="c"></param>
		private void Put(char c)
		{
			if (c == '\n')
			{
				switch (this.lastWritten)
				{
					case JSMinifier.EOF:
					case ',':
					case '.':
					case ';':
					case ':':
					case '{':
					case '}':
					case '(':
					case '[':
					case '=':
					case '<':
					case '>':
					case '?':
					case '!':
					case '+':
					case '-':
					case '*':
					case '/':
					case '%':
					case '~':
					case '^':
					case '|':
					case '&':
					{
						// safely suppress NewLine
						// (as per JSLint http://www.jslint.com)
						break;
					}
					default:
					{
						this.writer.WriteLine();
						break;
					}
				}
			}
			else
			{
				this.writer.Write(c);
			}

			this.lastWritten = c;
		}

		/// <summary>
		/// Return true if the character is a letter, digit, underscore,
		///		dollar sign, or non-ASCII character.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool IsAlphaNum(char c)
		{
			return (Char.IsLetterOrDigit(c) || c == '_' || c == '$' || c == '\\' || c > 126);
		}

		#endregion Private Methods

		#region Enums

		private enum Action
		{
			/// <summary>
			/// Output A. Copy B to A. Get the next B.
			/// </summary>
			Output = 1,

			/// <summary>
			/// Copy B to A. Get the next B. (Delete A)
			/// </summary>
			Shift = 2,

			/// <summary>
			/// Get the next B. (Delete B)
			/// </summary>
			Read = 3
		}

		#endregion Enums
	}
}