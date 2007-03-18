using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

/*
	.NET Wrapper for running JSLint

	(C)2002 Douglas Crockford
	www.JSLint.com
	WSH Edition
*/

namespace ScriptTools
{
	public class JSLint
	{
		#region Constants

		private const string JSLintScript = "jslint_wsh.js";

		#endregion Constants

		#region Fields

		private string jsLintPath = null;

		#endregion Fields

		#region Init

		public JSLint()
		{
			// look next to assembly for script
			Assembly assembly = Assembly.GetAssembly(typeof(JSLint));
			string assemblyPath = Path.GetDirectoryName(assembly.Location);
			this.jsLintPath = Path.Combine(assemblyPath, JSLintScript);

			// generate if does not exist
			if (!File.Exists(this.JSLintPath))
			{
				JSMinifier.PrepSavePath(this.JSLintPath);

				// stored as a resource file
				string resourceName = "ScriptTools"/*assembly.GetName().Name*/+"."+JSLintScript;
				if (assembly.GetManifestResourceInfo(resourceName) == null)
				{
					throw new FileNotFoundException("Cannot find the JSLint script file.", JSLintScript);
				}

				// output next to assembly
				using (Stream input = assembly.GetManifestResourceStream(resourceName))
				{
					using (StreamReader reader = new StreamReader(input))
					{
						File.WriteAllText(this.JSLintPath, reader.ReadToEnd());
					}
				}
			}
		}

		#endregion Init

		#region Properties

		public string JSLintPath
		{
			get { return this.jsLintPath; }
		}

		#endregion Properties

		#region Methods

		//protected static void RunScript(String script)
		//{
		//    // Microsoft Script Control 1.0
		//    // http://msdn2.microsoft.com/en-us/library/ms974577.aspx
		//    // http://msdn.microsoft.com/msdnmag/issues/02/08/VisualStudioforApplications/

		//    MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControl();
		//    //sc.AddObject
		//    //sc.AddCode(script);
		//    sc.Language = "JavaScript";
		//    Object result = sc.Eval("JSLINT(source, option);");
		//    Console.WriteLine("result: {0}", (string)result);
		//    Console.ReadLine();
		//}

		public void Run(TextReader reader, string filename)
		{
			if (reader == null)
				throw new NullReferenceException("Input StreamReader was null");

			// read input file into memory
			string scriptText = reader.ReadToEnd();

			using (Process myProcess = new Process())
			{
				myProcess.StartInfo.FileName = "cscript";
				myProcess.StartInfo.Arguments = String.Format("\"{0}\" //NoLogo", this.JSLintPath);
				myProcess.StartInfo.CreateNoWindow = true;
				myProcess.StartInfo.UseShellExecute = false;
				myProcess.StartInfo.RedirectStandardInput = true;
				myProcess.StartInfo.RedirectStandardOutput = true;
				myProcess.StartInfo.RedirectStandardError = true;
				myProcess.Start();

				// pipe input file to script
				myProcess.StandardInput.Write(scriptText);
				myProcess.StandardInput.Flush();
				myProcess.StandardInput.Close();

				string stdErr = String.Empty;
				string stdOut = String.Empty;

				int attempts = 0;
				while (!myProcess.WaitForExit(5000))
				{
					attempts++;
					if (attempts > 6)
					{
						throw new ParseException("JSLint Error: External script is timing out (30 sec).", null, this.JSLintPath, scriptText, 0, 0);
					}
				}
				stdOut += myProcess.StandardOutput.ReadToEnd();
				stdErr += myProcess.StandardError.ReadToEnd();

				if (myProcess.ExitCode != 0)
				{
					string message = stdOut+stdErr;
					int line = 0, column = 0;
					Match match = Regex.Match(message, "Lint at line (?<Line>[\\d]+) character (?<Char>[\\d]+)[:]\\s*(?<Error>[^\\n\\r]*)\\s*(?<Source>.*)$", RegexOptions.ExplicitCapture|RegexOptions.Compiled|RegexOptions.Singleline);
					if (match.Success)
					{
						string lineStr = match.Groups["Line"].Value;
						Int32.TryParse(lineStr, out line);
						string columnStr = match.Groups["Char"].Value;
						Int32.TryParse(columnStr, out column);
						string error = match.Groups["Error"].Value;
						if (!String.IsNullOrEmpty(error))
							message = error;
						string source = match.Groups["Source"].Value;
						if (!String.IsNullOrEmpty(source))
							scriptText = source;
					}

					throw new ParseException(message, null, filename, scriptText, line, column);
				}
			}
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
