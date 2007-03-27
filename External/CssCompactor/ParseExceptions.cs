/*-----------------------------------------------------------------------*\
	Copyright (c) 2007 Stephen M. McKamey

	CssCompactor is distributed under the terms of an MIT-style license
	http://www.opensource.org/licenses/mit-license.php
\*-----------------------------------------------------------------------*/

using System;
using System.Runtime.Serialization;

namespace BuildTools
{
	public enum ParseExceptionType
	{
		Warning,
		Error
	}

	[Serializable]
	public abstract class ParseException : ApplicationException
	{
		#region Fields

		private string file;
		private int line;
		private int column;

		#endregion Fields

		#region Init

		public ParseException(string message, string file, int line, int column)
			: base(message)
		{
			this.file = file;
			this.line = line;
			this.column = column;
		}

		public ParseException(string message, string file, int line, int column, Exception innerException)
			: base(message, innerException)
		{
			this.file = file;
			this.line = line;
			this.column = column;
		}

		#endregion Init

		#region Properties

		public abstract ParseExceptionType Type
		{
			get;
		}

		public virtual string ErrorCode
		{
			get { return null; }
		}

		public string File
		{
			get { return this.file; }
		}

		public int Line
		{
			get { return this.line; }
		}

		public int Column
		{
			get { return this.column; }
		}

		#endregion Properties

		#region Methods

		public virtual string GetCompilerMessage()
		{
			return this.GetCompilerMessage(this.Type == ParseExceptionType.Warning);
		}

		public virtual string GetCompilerMessage(bool isWarning)
		{
			string message = String.IsNullOrEmpty(this.ErrorCode) ?
				this.Message :
				String.Format("{0}: {1}", this.ErrorCode, this.Message);

			// format as a VS2005 error/warning
			return String.Format(
				"{0}({1},{2}): {4} {3}",
				this.File,
				this.Line,
				this.Column,
				message,
				isWarning ? "warning" : "error");
		}

		#endregion Methods
	}

	[Serializable]
	public class ParseWarning : ParseException
	{
		#region Init

		public ParseWarning(string message, string file, int line, int column)
			: base(message, file, line, column)
		{
		}

		public ParseWarning(string message, string file, int line, int column, Exception innerException)
			: base(message, file, line, column, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override ParseExceptionType Type
		{
			get { return ParseExceptionType.Warning; }
		}

		#endregion Properties
	}

	[Serializable]
	public class ParseError : ParseException
	{
		#region Init

		public ParseError(string message, string file, int line, int column)
			: base(message, file, line, column)
		{
		}

		public ParseError(string message, string file, int line, int column, Exception innerException)
			: base(message, file, line, column, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override ParseExceptionType Type
		{
			get { return ParseExceptionType.Error; }
		}

		#endregion Properties
	}

	[Serializable]
	public class UnexpectedEndOfFile : ParseError
	{
		#region Init

		public UnexpectedEndOfFile(string message, string file, int line, int column)
			: base(message, file, line, column)
		{
		}

		public UnexpectedEndOfFile(string message, string file, int line, int column, Exception innerException)
			: base(message, file, line, column, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override string ErrorCode
		{
			get { return "Unexpected End of File"; }
		}

		#endregion Properties
	}

	[Serializable]
	public class FileError : ParseWarning
	{
		#region Init

		public FileError(string message, string file, int line, int column)
			: base(message, file, line, column)
		{
		}

		public FileError(string message, string file, int line, int column, Exception innerException)
			: base(message, file, line, column, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override string ErrorCode
		{
			get { return "File Error"; }
		}

		#endregion Properties
	}

	[Serializable]
	public class SyntaxError : ParseError
	{
		#region Init

		public SyntaxError(string message, string file, int line, int column)
			: base(message, file, line, column)
		{
		}

		public SyntaxError(string message, string file, int line, int column, Exception innerException)
			: base(message, file, line, column, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override string ErrorCode
		{
			get { return "Syntax error"; }
		}

		#endregion Properties
	}
}
