using System;

namespace BuildTools
{
	public abstract class BaseParseException : Exception
	{
		#region Constants

		protected const string ErrorType = "error";
		protected const string WarningType = "error";

		#endregion Constants

		#region Fields

		private string file;
		private int line;
		private int col;

		#endregion Fields

		#region Init

		public BaseParseException(string message, string file, int line, int col)
			: base(message)
		{
			this.file = file;
			this.line = line;
			this.col = col;
		}

		public BaseParseException(string message, string file, int line, int col, Exception innerException)
			: base(message, innerException)
		{
			this.file = file;
			this.line = line;
			this.col = col;
		}

		#endregion Init

		#region Properties

		public abstract string Type
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

		public int Col
		{
			get { return this.col; }
		}

		#endregion Properties

		#region Object Overrides

		public override string ToString()
		{
			string message = String.IsNullOrEmpty(this.ErrorCode) ?
				this.Message :
				String.Format("{0}: {1}", this.ErrorCode, this.Message);

			// format as a VS2005 error/warning
			return String.Format(
				"{0}({1},{2}): {3} {4}",
				this.File,
				this.Line,
				this.Col,
				this.Type,
				message);
		}

		#endregion Object Overrides
	}

	public class ParseWarning : BaseParseException
	{
		#region Init

		public ParseWarning(string message, string file, int line, int col)
			: base(message, file, line, col)
		{
		}

		public ParseWarning(string message, string file, int line, int col, Exception innerException)
			: base(message, file, line, col, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override string Type
		{
			get { return BaseParseException.WarningType; }
		}

		#endregion Properties
	}

	public class ParseError : BaseParseException
	{
		#region Init

		public ParseError(string message, string file, int line, int col)
			: base(message, file, line, col)
		{
		}

		public ParseError(string message, string file, int line, int col, Exception innerException)
			: base(message, file, line, col, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override string Type
		{
			get { return BaseParseException.ErrorType; }
		}

		#endregion Properties
	}

	public class UnexpectedEndOfFile : ParseError
	{
		#region Init

		public UnexpectedEndOfFile(string message, string file, int line, int col)
			: base(message, file, line, col)
		{
		}

		public UnexpectedEndOfFile(string message, string file, int line, int col, Exception innerException)
			: base(message, file, line, col, innerException)
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

	public class FileError : ParseWarning
	{
		#region Init

		public FileError(string message, string file, int line, int col)
			: base(message, file, line, col)
		{
		}

		public FileError(string message, string file, int line, int col, Exception innerException)
			: base(message, file, line, col, innerException)
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

	public class SyntaxError : ParseError
	{
		#region Init

		public SyntaxError(string message, string file, int line, int col)
			: base(message, file, line, col)
		{
		}

		public SyntaxError(string message, string file, int line, int col, Exception innerException)
			: base(message, file, line, col, innerException)
		{
		}

		#endregion Init

		#region Properties

		public override string ErrorCode
		{
			get { return "Syntax"; }
		}

		#endregion Properties
	}
}
