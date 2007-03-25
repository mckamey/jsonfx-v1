using System;
using System.IO;

using BuildTools.Collections;

namespace BuildTools.IO
{
	public class LineReader : System.IO.TextReader
	{
		#region Fields

		private int line = 0;
		private int col = 0;
		private int position = -1;
		private string filePath;
		private string styles;
		private readonly ReadFilter[] filters;
		private readonly FilterTrie trie;

		#endregion Fields

		#region Init

		public LineReader(string filePath, ReadFilter[] filters)
		{
			if (!File.Exists(filePath))
			{
				throw new FileError("File Not Found", filePath, 0, 0);
			}
			this.filePath = filePath;

			this.styles = File.ReadAllText(filePath);
			if (String.IsNullOrEmpty(this.styles))
			{
				throw new FileError("The stylesheet was empty", filePath, 0, 0);
			}

			this.filters = filters;
			this.trie = new FilterTrie(filters);
		}

		public LineReader(string filePath) : this(filePath, new ReadFilter[0])
		{
		}

		#endregion Init

		#region Properties

		public string FilePath
		{
			get { return this.filePath; }
		}

		public int Line
		{
			get { return this.line; }
		}

		public int Col
		{
			get { return this.col; }
		}

		public int Position
		{
			get { return this.position; }
		}

		public bool EndOfStream
		{
			get { return this.position == this.styles.Length; }
		}

		#endregion Properties

		#region TextReader Members

		public override int Peek()
		{
			return this.Peek(1);
		}

		/// <summary>
		/// Performs a filtered read of the source.  Counters are incremented.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// NewLine sequences (CR/LF, LF, CR) are normalized to LF.
		/// </remarks>
		public override int Read()
		{
			return this.Read(true);
		}

		#endregion TextReader Members

		#region Utility Methods

		public void PutBack(int count)
		{
			if (count > this.position)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			this.position -= count;
		}

		/// <summary>
		/// Copies a range from the source
		/// </summary>
		/// <param name="start">starting position, inclusive</param>
		/// <param name="end">ending position, inclusive</param>
		/// <returns></returns>
		public string Copy(int start, int end)
		{
			if (start < 0)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (end < 0)
			{
				throw new ArgumentOutOfRangeException("end");
			}
			if (end < 1)
			{
				return null;
			}

			int savedPos = this.position;
			try
			{
				// set to just before read, next char is start
				this.position = start-1;

				// allocate the full range but may not use due to filtering
				char[] buffer = new char[end-start+1];

				int count = 0;
				do
				{
					int ch = this.Read(true);
					if (ch == -1)
					{
						throw new UnexpectedEndOfFile("Read past end of file", this.FilePath, this.Line, this.Col);
					}
					buffer[count++] = (char)ch;
				}
				while (this.position < end);

				return new String(buffer, 0, count);
			}
			finally
			{
				this.position = savedPos;
			}
		}

		#endregion Utility Methods

		#region Filter Methods

		/// <summary>
		/// Peeks with n chars of lookahead.
		/// </summary>
		/// <param name="lookahead"></param>
		/// <returns>unfiltered read</returns>
		protected int Peek(int lookahead)
		{
			int pos = this.position+lookahead;
			if (pos >= this.styles.Length)
			{
				return -1;
			}
			return this.styles[pos];
		}

		protected int Read(bool filter)
		{
			if (this.position+1 >= this.styles.Length)
			{
				return -1;
			}

			this.position++;
			this.col++;
			char ch = this.styles[this.position];

			// increment counters
			switch (ch)
			{
				case '\r': //CR
				case '\n': //LF
				{
					this.col = 0;
					this.line++;

					if ((ch == '\r') && (this.Peek() == '\n'))
					{
						this.position++;
					}
					ch = '\n';
					break;
				}
				default:
				{
					break;
				}
			}
			return filter ? this.Filter(ch) : ch;
		}

		private int Filter(char ch)
		{
			int lookAhead = 0;
			TrieNode<char, string> node = this.trie[ch];

			while (node != null)
			{
				if (node.HasValue)
				{
					// found StartToken
					string endToken = node.Value;
					int length = endToken.Length;

					// move to end of StartToken
					this.position += lookAhead;

					for (int i=0; i<length; i++)
					{
						int ch2 = this.Read(false);
						if (ch < 0)
						{
							throw new UnexpectedEndOfFile("Expected "+endToken, this.FilePath, this.Line, this.Col);
						}
						if (ch2 != endToken[i])
						{
							// reset search
							i = -1;
						}
					}
					return this.Read(true);
				}
				else
				{
					lookAhead++;
					int pk = this.Peek(lookAhead);
					if (pk < 0)
					{
						return ch;
					}
					node = node[(char)pk];
				}
			}

			return ch;
		}

		private int Filter1(char ch)
		{
			foreach (ReadFilter filter in this.filters)
			{
				// check for start token
				int length = filter.StartToken.Length;
				bool match = true;
				for (int i=0; i<length; i++)
				{
					if (this.Peek(i) != filter.StartToken[i])
					{
						match = false;
						break;
					}
				}
				if (match)
				{
					// move to end of StartToken
					this.position += length;

					length = filter.EndToken.Length;

					while (!this.EndOfStream)
					{
						if (this.Read(false) == filter.EndToken[0])
						{
							match = true;
							for (int i=1; i<length; i++)
							{
								// peek until completed filter.EndToken
								if (this.Peek(i) != filter.EndToken[i])
								{
									match = false;
									break;
								}
							}
							if (match)
							{
								// move to end of EndToken
								this.position += length-1;

								// return the next filtered char
								return this.Read(true);
							}
						}
					}
					if (this.EndOfStream)
					{
						throw new UnexpectedEndOfFile("Unclosed "+filter.StartToken+"..."+filter.EndToken, this.FilePath, this.Line, this.Col);
					}
				}
			}
			return ch;
		}

		private int Filter0(char ch)
		{
			// skip C-Style comments 
			if (ch == '/' && this.Peek() == (int)'*')
			{
				int c = this.Read(false);// consume opening star

				while ((c = this.Read(false)) >= 0)
				{
					if (c == (int)'*' && this.Peek() == (int)'/')
					{
						this.Read(false);// consume closing slash
						break;
					}
				}
				if (this.EndOfStream || (c = this.Read(false)) < 0)
				{
					throw new UnexpectedEndOfFile("Unclosed comment", this.FilePath, this.Line, this.Col);
				}
				ch = (char)c;
			}
			return ch;
		}

		#endregion Filter Methods

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			this.styles = null;
		}

		#endregion IDisposable Members
	}
}
