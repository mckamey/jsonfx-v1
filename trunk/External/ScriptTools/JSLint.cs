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
				string resourceName = assembly.GetName().Name+"."+JSLintScript;
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

		public void Run(TextReader reader)
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

		public void Run(string inputFile)
		{
			if (!File.Exists(inputFile))
				throw new FileNotFoundException(String.Format("File not found: \"{0}\"", inputFile), inputFile);

			using (StreamReader reader = new StreamReader(inputFile))
			{
				this.Run(reader);
			}
		}

		public void Run(Stream input)
		{
			if (input == null)
				throw new NullReferenceException("Input Stream was null");

			// read input file into memory
			using (StreamReader reader = new StreamReader(input))
			{
				this.Run(reader);
			}
		}

		#endregion Methods
	}
}
