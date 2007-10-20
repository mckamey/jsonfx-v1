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

using JsonFx.Json;

namespace BuildTools.Json
{
	class Program
	{
		#region Constants

		private const string UnitTestsUrl = @"http://www.json.org/JSON_checker/test.zip";
		private const string UnitTestsFolder = @"UnitTests\";
		private const string UnitTestsFiles = @"*.json";
		private const string ErrorMessage =
			"No unit tests were found.\r\n\r\n"+
			"Any "+UnitTestsFiles+" file in the "+UnitTestsFolder+" folder will be processed.\r\n"+
			"Download "+UnitTestsUrl+" and place into the "+UnitTestsFolder+" folder.";

		#endregion Constants

		#region Program Entry

		static void Main(string[] args)
		{
			string[] unitTests = Directory.GetFiles(UnitTestsFolder, UnitTestsFiles, SearchOption.AllDirectories);
			if (unitTests.Length < 1)
			{
				Console.Error.WriteLine(ErrorMessage);
				Console.ReadLine();
				Environment.Exit(-1);
			}

			foreach (string unitTest in unitTests)
			{
				Console.Error.WriteLine("________________________________________");
				Console.Error.WriteLine();
				string source = File.ReadAllText(unitTest);
				JsonReader reader = new JsonReader(source);
				try
				{
					object obj = reader.Deserialize();
					Console.Error.WriteLine("UnitTest \"{0}\" passed producing {1}",
						unitTest,
						(obj == null) ? "null" : obj.GetType().Name);
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("UnitTest \"{0}\" failed with message:", unitTest);
					Console.Error.WriteLine("\t\"{0}\"", ex.Message);
				}
			}
		}

		#endregion Program Entry
	}
}
