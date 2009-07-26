using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using EcmaScript.NET;

namespace JsonFx.BuildTools.ScriptCompactor
{
    public class BuildErrorReporter : ErrorReporter
	{
		#region Fields

		private readonly string sourceName;
		private readonly IList<ParseException> errors;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public BuildErrorReporter(string sourceName)
			: this(sourceName, new List<ParseException>())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <remarks></remarks>
		public BuildErrorReporter(string sourceName, IList<ParseException> errors)
		{
			this.sourceName = sourceName;
			this.errors = errors;
		}

		#endregion Init

		#region Properties

		public IList<ParseException> Errors
		{
			get { return this.errors; }
		}

		#endregion Properties

		#region ErrorReporter Members

		public virtual void Warning(
			string message,
            string sourceName,
            int line,
            string lineSource,
            int column)
        {
			this.errors.Add(new ParseWarning(message, sourceName??this.sourceName, line, column));
        }

        public virtual void Error(
			string message,
            string sourceName,
            int line,
            string lineSource,
            int column)
        {
			this.errors.Add(new ParseError(message, sourceName??this.sourceName, line, column));
        }

        public virtual EcmaScriptRuntimeException RuntimeError(
			string message,
            string sourceName, 
            int line,
            string lineSource, 
            int column)
        {
			return new EcmaScriptRuntimeException(message, sourceName, line, lineSource, column);
		}

		#endregion ErrorReporter Members
	}
}