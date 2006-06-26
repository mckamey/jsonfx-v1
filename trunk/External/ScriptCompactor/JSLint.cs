using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

/*
	.NET Wrapper for running JSLint

	(C)2002 Douglas Crockford
	www.JSLint.com
	WSH Edition
*/

namespace ScriptCompactor
{
	public class JSLint
	{
		#region Init

		public JSLint()
		{
		}

		#endregion Init

		#region Methods

		public void Run(string inputFile)
		{
			if (!File.Exists(inputFile))
				throw new FileNotFoundException(String.Format("File (\"{0}\") not found.", inputFile), inputFile);

			string assemblyPath = (Assembly.GetAssembly(typeof(JSLint)).Location);
			assemblyPath = Path.GetDirectoryName(assemblyPath);

			string jslintPath = assemblyPath+"\\jslint_wsh.js";
			if (!File.Exists(jslintPath))
				throw new FileNotFoundException(String.Format("JSLint script not found at \"{0}\".", jslintPath));

			using (Process myProcess = new Process())
			{
				myProcess.StartInfo.FileName = "cscript";
				myProcess.StartInfo.Arguments = String.Format("\"{0}\" //NoLogo", jslintPath);
				//myProcess.StartInfo.CreateNoWindow = true;
				myProcess.StartInfo.UseShellExecute = false;
				myProcess.StartInfo.RedirectStandardInput = true;
				myProcess.StartInfo.RedirectStandardOutput = true;
				myProcess.StartInfo.RedirectStandardError = true;
				myProcess.Start();

				// read input file into memory
				string scriptText;
				using (StreamReader reader = new StreamReader(inputFile))
				{
					scriptText = reader.ReadToEnd();
				}

				// pipe input file to script
				myProcess.StandardInput.Write(scriptText);
				myProcess.StandardInput.Flush();
				myProcess.StandardInput.Close();

				string stdErr = String.Empty;
				string stdOut = String.Empty;
				bool exited = false;
				while (!exited)
				{
					exited = myProcess.WaitForExit(10000);
					stdOut += myProcess.StandardOutput.ReadToEnd();
					stdErr += myProcess.StandardError.ReadToEnd();
				}

				if (myProcess.ExitCode != 0)
					throw new ApplicationException("JSLint Error: "+stdOut+stdErr);
			}
		}

		#endregion Methods
	}
}
