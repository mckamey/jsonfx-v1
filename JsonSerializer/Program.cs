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

using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using JsonFx.Json;

namespace BuildTools.Json
{
	class Program
	{
		#region Constants

		private const string Separator = "________________________________________\r\n";
		private const string ReportPath = "Report.txt";
		private const string UnitTestsFolder = @".\UnitTests\";
		private const string OutputFolder = @".\Output\";
		private const string HeaderMessage =
			"NOTE: JsonFx.Json accepts all valid JSON and can recover from many minor errors.\r\n\r\n"+
			"Unit Test Report ({0:yyyy-MM-dd @ HH:mm:ss})";

		#endregion Constants

		#region Program Entry

		static void Main(string[] args)
		{
			using (StreamWriter writer = new StreamWriter(ReportPath, false, Encoding.UTF8))
			{
				writer.WriteLine(HeaderMessage, DateTime.Now);
				writer.WriteLine(Separator);

				Stopwatch watch = Stopwatch.StartNew();

				UnitTests.StronglyTyped.RunTest(writer, UnitTestsFolder, OutputFolder);

				UnitTests.JsonChecker.RunTest(writer, UnitTestsFolder, OutputFolder);

				watch.Stop();

				writer.WriteLine(Separator);
				writer.WriteLine("Elapsed: {0} ms", watch.Elapsed.TotalMilliseconds);
			}

			Process process = new Process();
			process.StartInfo.FileName = "notepad.exe";
			process.StartInfo.Arguments = ReportPath;
			process.Start();
		}

		#endregion Program Entry
	}
}
