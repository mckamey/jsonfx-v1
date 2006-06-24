using System;
using System.IO;

/* Originally written in 'C', this code has been converted to the C# language.
 * The author's copyright message is reproduced below.
 * All modifications from the original to C# are placed in the public domain.
 */

/* jsmin.c
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
	public class JavaScriptMinifier
	{
		#region Constants

		private const int EOF = -1;

		#endregion Constants

		#region Fields

		private StreamReader sr;
		private StreamWriter sw;
		private int theA;
		private int theB;
		private int theLookahead = JavaScriptMinifier.EOF;

		#endregion Fields

		#region Public Methods

		public void Minify(string src, string dst)
		{
			if (!File.Exists(src))
				throw new FileNotFoundException(String.Format("File (\"{0}\") not found.", src), src);

			if (File.Exists(dst))
				throw new AccessViolationException(String.Format("File (\"{0}\") already exists.", dst));

			using (this.sr = new StreamReader(src))
			{
				using (this.sw = new StreamWriter(dst))
				{
					this.JSMin();
				}
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
			while (this.theA != JavaScriptMinifier.EOF)
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
								throw new Exception(string.Format("Error: JSMIN unterminated string literal: {0}\n", this.theA));

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
								throw new Exception(string.Format("Error: JSMIN unterminated Regular Expression literal : {0}.\n", this.theA));
							}
							this.Put(this.theA);
						}
						this.theB = this.Next();
					}
					break;
				}
				default:
				{
					throw new Exception("Unknown action command.");
				}
			}
		}

		/// <summary>
		/// Get the next character, excluding comments. Peek() is used to see
		///		if a '/' is followed by a '/' or '*'.
		/// </summary>
		/// <returns></returns>
		private int Next()
		{
			int c = this.Get();
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
								case JavaScriptMinifier.EOF:
								{
									throw new Exception("Error: JSMIN Unterminated comment.\n");
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
		private int Peek()
		{
			this.theLookahead = this.Get();
			return this.theLookahead;
		}

		/// <summary>
		/// Return the next character from stdin. Watch out for lookahead. If
		///		the character is a control character, translate it to a space or
		///		linefeed.
		/// </summary>
		/// <returns>the next character from stdin</returns>
		private int Get()
		{
			int c = this.theLookahead;
			this.theLookahead = JavaScriptMinifier.EOF;

			if (c == JavaScriptMinifier.EOF)
				c = this.sr.Read();

			if (c >= ' ' || c == '\n' || c == JavaScriptMinifier.EOF)
				return c;

			if (c == '\r')
				return '\n';

			return ' ';
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="c"></param>
		private void Put(int c)
		{
			this.sw.Write((char)c);
		}

		/// <summary>
		/// Return true if the character is a letter, digit, underscore,
		///		dollar sign, or non-ASCII character.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool IsAlphaNum(int c)
		{
			return ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
				(c >= 'A' && c <= 'Z') || c == '_' || c == '$' || c == '\\' ||
				c > 126);
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