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

		private const string ReportPath = "Report.txt";
		private const string UnitTestsUrl = "http://www.json.org/JSON_checker/test.zip";
		private const string UnitTestsFolder = @".\UnitTests\";
		private const string OutputFolder = @".\Output\";
		private const string UnitTestsFiles = "*.json";
		private const string Separator = "________________________________________\r\n";
		private const string HeaderMessage =
			"NOTE: JsonFx.Json accepts valid JSON and can recover from many minor errors.\r\n\r\n"+
			"Unit Test Report ({0:yyyy-MM-dd @ HH:mm:ss})";
		private const string ErrorMessage =
			Separator+"\r\n"+
			"No unit tests were found.\r\n\r\n"+
			"Any "+UnitTestsFiles+" file in the "+UnitTestsFolder+" folder will be processed.\r\n"+
			"Download "+UnitTestsUrl+" and place contents into the "+UnitTestsFolder+" folder.";

		#endregion Constants

		#region Program Entry

		static void Main(string[] args)
		{
			using (StreamWriter writer = new StreamWriter(ReportPath, false, Encoding.UTF8))
			{
				writer.WriteLine(HeaderMessage, DateTime.Now);

				string[] unitTests = Directory.GetFiles(UnitTestsFolder, UnitTestsFiles, SearchOption.AllDirectories);
				if (unitTests.Length > 0)
				{
					if (!Directory.Exists(OutputFolder))
					{
						Directory.CreateDirectory(OutputFolder);
					}

					foreach (string unitTest in unitTests)
					{
						writer.WriteLine(Separator);

						string source = File.ReadAllText(unitTest);
						JsonReader jsonReader = new JsonReader(source);
						object obj = null;
						try
						{
							obj = jsonReader.Deserialize();
							writer.WriteLine("PASSED: {0}", unitTest.Replace(UnitTestsFolder, ""));
							writer.WriteLine("Result: {0}", (obj == null) ? "null" : obj.GetType().Name);
						}
						catch (JsonSerializationException ex)
						{
							bool foundLF = false;
							int col = 1, line = 1;
							for (int i=ex.Index; i>0; i--)
							{
								if (!foundLF)
								{
									col++;
								}
								if (source[i-1] == '\n')
								{
									line++;
									foundLF = true;
								}
							}

							writer.WriteLine("FAILED: {0}", unitTest.Replace(UnitTestsFolder, ""));
							writer.WriteLine("\"{0}\" ({1}, {2})", ex.Message, line, col);
							continue;
						}

						string outputFile = unitTest.Replace(UnitTestsFolder, OutputFolder);
						using (JsonWriter jsonWriter = new JsonWriter(outputFile))
						{
							jsonWriter.Write(obj);
						}
					}
				}
				else
				{
					writer.WriteLine(ErrorMessage);
				}
			}

			Process process = new Process();
			process.StartInfo.FileName = "notepad.exe";
			process.StartInfo.Arguments = ReportPath;
			process.Start();
		}

		#endregion Program Entry
	}
}
