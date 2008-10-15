#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion BuildTools License

#region JSLint License
/*---------------------------------------------------------------------------------*\

	Copyright (c) 2002 Douglas Crockford  (www.JSLint.com)

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

\*---------------------------------------------------------------------------------*/
#endregion JSLint License

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;

using BuildTools;

namespace BuildTools.ScriptCompactor
{
	/// <summary>
	/// Implements .NET wrapper for JSLint
	/// </summary>
	public class JSLint
	{
		#region JSLint.Options

		[ComVisible(true)]
		public class Options
		{
			/// <summary>
			/// true if use of some browser features should be restricted
			/// </summary>
			[Description("IsAdSafe")]
			public bool adsafe = false;

			/// <summary>
			/// true if bitwise operators should not be allowed
			/// </summary>
			[Description("DisallowBitwise")]
			public bool bitwise = false;

			/// <summary>
			/// true if the standard browser globals should be predefined
			/// </summary>
			[Description("IsBrowser")]
			public bool browser = true;

			/// <summary>
			/// true if upper case HTML should be allowed
			/// </summary>
			[Description("AllowUpperCaseHtml")]
			public bool cap = false;

			/// <summary>
			/// true if debugger statements should be allowed
			/// </summary>
			[Description("AllowDebugger")]
			public bool debug = false;

			/// <summary>
			/// true if === should be required
			/// </summary>
			[Description("RequireStrictEquals")]
			public bool eqeqeq = true;

			/// <summary>
			/// true if eval should be allowed
			/// </summary>
			[Description("AllowEval")]
			public bool evil = false;

			/// <summary>
			/// true if for...in statements must filter
			/// </summary>
			[Description("AllowForIn")]
			public bool forin = false;

			/// <summary>
			/// true if HTML fragments should be allowed
			/// </summary>
			[Description("AllowHtmlFragments")]
			public bool fragment = false;

			/// <summary>
			/// true if var should not be allowed to declare global variables
			/// </summary>
			[Description("DisallowGlobalVariables")]
			public bool glovar = false;

			/// <summary>
			/// true if line breaks should not be checked
			/// </summary>
			[Description("AllowLaxNewLines")]
			public bool laxbreak = false;

			/// <summary>
			/// true if names should be checked
			/// </summary>
			[Description("CheckIdentifiers")]
			public bool nomen = false;

			/// <summary>
			/// true if HTML event handlers should be allowed
			/// </summary>
			[Description("OnEventHandlers")]
			public bool on = true;

			/// <summary>
			/// true if the scan should stop on first error
			/// </summary>
			[Description("StopOnFirstError")]
			public bool passfail = false;

			/// <summary>
			/// true if increment/decrement should not be allowed
			/// </summary>
			[Description("NoIncDec")]
			public bool plusplus = false;

			/// <summary>
			/// true if the . should not be allowed in regexp literals
			/// </summary>
			[Description("CheckRegExp")]
			public bool regexp = false;

			/// <summary>
			/// true if the Rhino environment globals should be predefined
			/// </summary>
			[Description("IsRhino")]
			public bool rhino = false;

			/// <summary>
			/// true if the Windows Siderbar Gadget globals should be predefined
			/// </summary>
			[Description("IsSidebar")]
			public bool sidebar = false;

			/// <summary>
			/// true if variables should be declared before used
			/// </summary>
			[Description("NoUndefVars")]
			public bool undef = true;

			/// <summary>
			/// true if strict whitespace rules apply
			/// </summary>
			[Description("StrictWhitespace")]
			public bool white = false;

			/// <summary>
			/// true if the Yahoo Widgets globals should be predefined
			/// </summary>
			[Description("IsYahooWidget")]
			public bool widget = false;
		}

		#endregion JSLint.Options

		#region Constants

		private const string JSLintScript = "BuildTools.ScriptCompactor.jslint.js";

		#endregion Constants

		#region Fields

		private JSLint.Options options = new JSLint.Options();
		private List<ParseException> errors = new List<ParseException>();

		#endregion Fields

		#region Properties

		/// <summary>
		/// List of warnings and errors found
		/// </summary>
		public List<ParseException> Errors
		{
			get { return this.errors; }
		}

		/// <summary>
		/// true if use of some browser features should be restricted
		/// </summary>
		public bool IsAdSafe
		{
			get { return this.options.adsafe; }
			set { this.options.adsafe = value; }
		}

		/// <summary>
		/// true if bitwise operators should not be allowed
		/// </summary>
		public bool DisallowBitwise
		{
			get { return this.options.bitwise; }
			set { this.options.bitwise = value; }
		}

		/// <summary>
		/// true if the standard browser globals should be predefined
		/// </summary>
		public bool IsBrowser
		{
			get { return this.options.browser; }
			set { this.options.browser = value; }
		}

		/// <summary>
		/// true if upper case HTML should be allowed
		/// </summary>
		public bool AllowUpperCaseHtml
		{
			get { return this.options.cap; }
			set { this.options.cap = value; }
		}

		/// <summary>
		/// true if debugger statements should be allowed
		/// </summary>
		public bool AllowDebugger
		{
			get { return this.options.debug; }
			set { this.options.debug = value; }
		}

		/// <summary>
		/// true if === should be required
		/// </summary>
		public bool RequireStrictEquals
		{
			get { return this.options.eqeqeq; }
			set { this.options.eqeqeq = value; }
		}

		/// <summary>
		/// true if eval should be allowed
		/// </summary>
		public bool AllowEval
		{
			get { return this.options.evil; }
			set { this.options.evil = value; }
		}

		/// <summary>
		/// true if HTML fragments should be allowed
		/// </summary>
		public bool AllowHtmlFragments
		{
			get { return this.options.fragment; }
			set { this.options.fragment = value; }
		}

		/// <summary>
		/// true if var should not be allowed to declare global variables
		/// </summary>
		public bool DisallowGlobalVariables
		{
			get { return this.options.glovar; }
			set { this.options.glovar = value; }
		}

		/// <summary>
		/// true if line breaks should not be checked
		/// </summary>
		public bool AllowLaxNewLines
		{
			get { return this.options.laxbreak; }
			set { this.options.laxbreak = value; }
		}

		/// <summary>
		/// true if names should be checked
		/// </summary>
		public bool CheckIdentifiers
		{
			get { return this.options.nomen; }
			set { this.options.nomen = value; }
		}

		/// <summary>
		/// true if the scan should stop on first error
		/// </summary>
		public bool StopOnFirstError
		{
			get { return this.options.passfail; }
			set { this.options.passfail = value; }
		}

		/// <summary>
		/// true if increment/decrement should not be allowed
		/// </summary>
		public bool NoIncDec
		{
			get { return this.options.plusplus; }
			set { this.options.plusplus = value; }
		}

		/// <summary>
		/// true if the Rhino environment globals should be predefined
		/// </summary>
		public bool IsRhino
		{
			get { return this.options.rhino; }
			set { this.options.rhino = value; }
		}

		/// <summary>
		/// true if undefined variables are errors
		/// </summary>
		public bool NoUndefVars
		{
			get { return this.options.undef; }
			set { this.options.undef = value; }
		}

		/// <summary>
		/// true if strict whitespace rules apply
		/// </summary>
		public bool StrictWhitespace
		{
			get { return this.options.white; }
			set { this.options.white = value; }
		}

		/// <summary>
		/// true if the Yahoo Widgets globals should be predefined
		/// </summary>
		public bool IsYahooWidget
		{
			get { return this.options.widget; }
			set { this.options.widget = value; }
		}

		#endregion Properties

		#region Methods

		public void Run(string filename, string script)
		{
			#region Microsoft Script Control References

			// Microsoft Script Control 1.0
			// http://weblogs.asp.net/rosherove/pages/DotNetScripting.aspx
			// http://msdn2.microsoft.com/en-us/library/ms974577.aspx
			// http://msdn.microsoft.com/msdnmag/issues/02/08/VisualStudioforApplications/
			// http://msdn2.microsoft.com/en-us/library/ms974586.aspx
			// http://msdn2.microsoft.com/en-us/library/ms950396.aspx
			// Must be built with x86 as Target (not Any CPU/x64/Itanium)
			// http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=1406892&SiteID=1
			// http://www.devarticles.com/c/a/C-Sharp/Using-Late-Bound-COM-Objects/2/

			#endregion Microsoft Script Control References

			if (String.IsNullOrEmpty(script))
			{
				if (String.IsNullOrEmpty(filename) || !File.Exists(filename))
				{
					throw new FileNotFoundException("Cannot find the script file.", filename);
				}
				script = File.ReadAllText(filename);
			}

			string jsLintSource = null;

			Assembly assembly = Assembly.GetAssembly(typeof(JSLint));

			// JSLint.js stored as a resource file
			string resourceName = /*assembly.GetName().Name+"."+*/JSLintScript;
			if (assembly.GetManifestResourceInfo(resourceName) == null)
			{
				throw new FileNotFoundException("Cannot find the JSLint script file.", resourceName);
			}

			// output next to assembly
			using (Stream input = assembly.GetManifestResourceStream(resourceName))
			{
				using (StreamReader reader = new StreamReader(input))
				{
					jsLintSource = reader.ReadToEnd();
				}
			}

			MSScriptControl.ScriptControlClass sc;
			try
			{
				sc = new MSScriptControl.ScriptControlClass();
			}
			catch (COMException ex)
			{
				try
				{
					// could just add to errors collection here since this
					// takes a performance hit to throw, but throwing sets stack dump too
					throw new ParseWarning("JSLint cannot be run. Must run as x86 and install MSScriptControl COM component (http://www.microsoft.com/downloads/details.aspx?FamilyId=D7E31492-2595-49E6-8C02-1426FEC693AC)", filename, -1, -1, ex);
				}
				catch (ParseException ex2)
				{
					this.errors.Add(ex2);
				}
				return;
			}
			sc.Language = "JScript";
			sc.AddCode(jsLintSource);

			this.IsBrowser = true;
			this.AllowDebugger = true;
			this.RequireStrictEquals = true;
			this.NoUndefVars = true;

			object[] p = new object[] { script, this.options };
			bool result = false;
			try
			{
				result = (bool)sc.Run("JSLINT", ref p);
			}
			catch (Exception ex)
			{
				this.errors.Add(new ParseError(ex.Message, filename, -1, -1, ex));
			}
			if (!result)
			{
				// Alternatively this could also import JSON.js
				// but then it would need to parse on the C# side
				// Just looping through with Eval is simpler
				int length = (int)sc.Eval("JSLINT.errors.length");
				for (int i=0; i<length; i++)
				{
					// if JSLint finds a fatal error it adds null to the end
					bool fatal = (i == length-1) && (bool)sc.Eval("!JSLINT.errors["+i+"]");
					if (!fatal)
					{
						int line = 1+(int)sc.Eval("JSLINT.errors["+i+"].line");
						int col = 1+(int)sc.Eval("JSLINT.errors["+i+"].character");
						string reason = sc.Eval("JSLINT.errors["+i+"].reason") as string;
						//string evidence = sc.Eval("JSLINT.errors["+i+"].evidence") as string;

						// could throw to set stack dump too but
						// just adding to errors collection here since
						// throwing adds a performance hit
						this.errors.Add(new ParseWarning(reason, filename, line, col));
					}
				}
			}
		}

		public void Run(TextReader reader, string filename)
		{
			if (reader == null)
				throw new NullReferenceException("Input StreamReader was null");

			// read input file into memory
			string scriptText = reader.ReadToEnd();

			this.Run(filename, scriptText);
		}

		public void Run(string filename)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException(String.Format("File not found: \"{0}\"", filename), filename);

			using (StreamReader reader = new StreamReader(filename))
			{
				this.Run(reader, filename);
			}
		}

		public void Run(Stream input, string filename)
		{
			if (input == null)
				throw new NullReferenceException("Input Stream was null");

			// read input file into memory
			using (StreamReader reader = new StreamReader(input))
			{
				this.Run(reader, filename);
			}
		}

		#endregion Methods
	}
}
