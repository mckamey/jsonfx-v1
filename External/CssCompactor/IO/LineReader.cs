using System;
using System.IO;
using System.Collections.Generic;

using BuildTools.Collections;

namespace BuildTools.IO
{
	public class LineReader : System.IO.TextReader
	{
		#region Fields

		private int line = 1;
		private int col = 0;
		private int position = -1;

		private string filePath;
		private string css;

		private readonly FilterTrie trie;

		private bool normalizeWhiteSpace = false;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="filters"></param>
		public LineReader(string filePath, IEnumerable<ReadFilter> filters)
		{
			if (!File.Exists(filePath))
			{
				throw new FileError("File Not Found", filePath, 0, 0);
			}
			this.filePath = filePath;

			this.css = File.ReadAllText(filePath);
			if (String.IsNullOrEmpty(this.css))
			{
				throw new FileError("The stylesheet was empty", filePath, 0, 0);
			}

			this.trie = new FilterTrie(filters);
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="filePath"></param>
		public LineReader(string filePath) : this(filePath, new ReadFilter[0])
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the path to the source file
		/// </summary>
		public string FilePath
		{
			get { return this.filePath; }
		}

		/// <summary>
		/// Gets the size of source file in chars
		/// </summary>
		public int Length
		{
			get { return this.css.Length; }
		}

		/// <summary>
		/// Gets the current line number
		/// </summary>
		public int Line
		{
			get { return this.line; }
		}

		/// <summary>
		/// Gets the current col number
		/// </summary>
		public int Col
		{
			get { return this.col; }
		}

		/// <summary>
		/// Gets the current char position
		/// </summary>
		public int Position
		{
			get { return this.position; }
		}

		/// <summary>
		/// Gets if at end the end of file
		/// </summary>
		public bool EndOfFile
		{
			get { return this.position >= this.css.Length; }
		}

		/// <summary>
		/// Gets and sets if whitespace is normalized while reading
		/// </summary>
		public bool NormalizeWhiteSpace
		{
			get { return this.normalizeWhiteSpace; }
			set { this.normalizeWhiteSpace = value; }
		}

		/// <summary>
		/// Gets the current char
		/// </summary>
		public int Current
		{
			get
			{
				if (this.EndOfFile)
				{
					return -1;
				}
				return this.css[this.position];
			}
		}

		#endregion Properties

		#region TextReader Members

		/// <summary>
		/// Unfiltered look ahead
		/// </summary>
		/// <returns></returns>
		public override int Peek()
		{
			return this.Peek(1);
		}

		/// <summary>
		/// Filtered read of the next source char.  Counters are incremented.
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

		/// <summary>
		/// Backs the current position up one.
		/// </summary>
		public void PutBack()
		{
			if (this.position < 0)
			{
				throw new InvalidOperationException("Already at start of source");
			}
			switch (this.css[this.position])
			{
				case '\r':
				case '\n':
				{
					this.line--;
					break;
				}
			}
			this.col--;
			this.position--;
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

			// set to just before read, next char is start
			int copyPosition = start-1;

			// allocate the full range but may not use due to filtering
			char[] buffer = new char[end-start+1];

			int count = 0;
			while (copyPosition < end)
			{
				int ch = this.CopyRead(ref copyPosition);
				if (ch == -1)
				{
					throw new UnexpectedEndOfFile("Read past end of file", this.FilePath, this.Line, this.Col);
				}
				buffer[count] = (char)ch;
				count++;
			}

			if (count < 1)
			{
				return null;
			}
			return new String(buffer, 0, count).Trim();
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
			if (pos >= this.css.Length)
			{
				return -1;
			}
			return this.css[pos];
		}

		/// <summary>
		/// Reads the next char 
		/// </summary>
		/// <param name="filter">if filtering</param>
		/// <returns>the next char, or -1 if at EOF</returns>
		protected int Read(bool filter)
		{
			if (this.position+1 >= this.css.Length)
			{
				this.position = this.css.Length;
				return -1;
			}

			// increment counters
			this.position++;
			this.col++;
			char ch = this.css[this.position];

			if (Char.IsWhiteSpace(ch))
			{
				ch = this.NormalizeSpaces(ch, ref this.position, ref this.line, ref this.col);
			}

			return filter ? this.Filter(ch) : ch;
		}

		/// <summary>
		/// Normalized CR/CRLF/LF/FF to LF, or all whitespace to SPACE if NormalizeWhiteSpace is true
		/// </summary>
		/// <param name="ch"></param>
		/// <param name="pos"></param>
		/// <param name="line"></param>
		/// <param name="col"></param>
		/// <returns></returns>
		private char NormalizeSpaces(char ch, ref int pos, ref int line, ref int col)
		{
			int length = this.css.Length;
			if (this.normalizeWhiteSpace)
			{
				// normalize runs of WhiteSpace to ' '
				while ((pos+1 < length) && Char.IsWhiteSpace(this.css, pos+1))
				{
					pos++;
					col++;

					// increment line count
					switch (this.css[pos])
					{
						case '\r': //CR
						{
							if ((pos+1 < length) && this.css[pos+1] == '\n')
							{
								pos++;
							}
							goto case '\n';
						}
						case '\n': //LF
						case '\f': //FF
						{
							line++;
							col = 0;
							break;
						}
					}
				}
				ch = ' ';
			}
			else
			{
				// normalize NewLines to '\n', increment line count
				switch (ch)
				{
					case '\r': //CR
					{
						if ((pos+1 < length) && this.css[pos+1] == '\n')
						{
							pos++;
						}
						goto case '\n';
					}
					case '\n': //LF
					case '\f': //FF
					{
						line++;
						col = 0;
						ch = '\n';
						break;
					}
				}
			}
			return ch;
		}

		/// <summary>
		/// Read for Copying (doesn't reset line.col counters)
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		protected int CopyRead(ref int copyPosition)
		{
			if (copyPosition+1 >= this.css.Length)
			{
				return -1;
			}

			// increment counters
			copyPosition++;
			char ch = this.css[copyPosition];

			if (Char.IsWhiteSpace(ch))
			{
				int dummyLine = 0, dummyCol = 0;
				ch = this.NormalizeSpaces(ch, ref copyPosition, ref dummyLine, ref dummyCol);
			}

			return this.Filter(ch);
		}

		/// <summary>
		/// Filters based upon an internal Trie
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
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

		///// <summary>
		///// This filter method is twice as fast as the Trie but hard-coded for C-Style comments.
		///// If there are two filters (e.g. "/*"..."*/" && "//"..."LF") this would be slower.
		///// </summary>
		///// <param name="ch"></param>
		///// <returns></returns>
		//private int Filter(char ch)
		//{
		//    // skip C-Style comments 
		//    if (ch == '/' && this.Peek() == (int)'*')
		//    {
		//        int c = this.Read(false);// consume opening star

		//        while ((c = this.Read(false)) >= 0)
		//        {
		//            if (c == (int)'*' && this.Peek() == (int)'/')
		//            {
		//                this.Read(false);// consume closing slash
		//                break;
		//            }
		//        }
		//        if (this.EndOfStream || (c = this.Read(false)) < 0)
		//        {
		//            throw new UnexpectedEndOfFile("Unclosed comment", this.FilePath, this.Line, this.Col);
		//        }
		//        ch = (char)c;
		//    }
		//    return ch;
		//}

		#endregion Filter Methods

		#region IDisposable Members

		/// <summary>
		/// Free source resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			this.css = null;
		}

		#endregion IDisposable Members
	}
}
