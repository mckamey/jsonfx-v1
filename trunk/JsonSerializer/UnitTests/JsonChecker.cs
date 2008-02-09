using System;
using System.IO;

using JsonFx.Json;

namespace BuildTools.Json.UnitTests
{
	public class JsonChecker
	{
		#region Constants

		private const string UnitTestsUrl = "http://www.json.org/JSON_checker/test.zip";
		private const string UnitTestsFiles = "*.json";
		private const string Separator = "________________________________________\r\n";
		private const string ErrorMessage =
			Separator+"\r\n"+
			"No unit tests were found.\r\n\r\n"+
			"Any "+UnitTestsFiles+" file in the {0} folder will be processed.\r\n"+
			"Download "+UnitTestsUrl+" and place contents into the {0} folder.";

		#endregion Constants

		#region Methods

		public static void RunTest(TextWriter writer, string unitTestsFolder, string outputFolder)
		{
			string[] unitTests = Directory.GetFiles(unitTestsFolder, UnitTestsFiles, SearchOption.AllDirectories);
			if (unitTests.Length > 0)
			{
				foreach (string unitTest in unitTests)
				{
					writer.WriteLine(Separator);

					string source = File.ReadAllText(unitTest);
					JsonReader jsonReader = new JsonReader(source);
					object obj = null;
					try
					{
						bool hasRead = false;
						jsonReader.TypeHintName = StronglyTyped.MyTypeHintName;
						while (true)
						{
							obj = jsonReader.Deserialize();

							if (hasRead && obj == null)
							{
								break;
							}
							else
							{
								hasRead = true;
							}
							writer.WriteLine("READ: {0}", unitTest.Replace(unitTestsFolder, ""));
							writer.WriteLine("Result: {0}", (obj == null) ? "null" : obj.GetType().FullName);
						}

						string outputFile = unitTest.Replace(unitTestsFolder, outputFolder);
						string outputDir = Path.GetDirectoryName(outputFile);
						if (!Directory.Exists(outputDir))
						{
							Directory.CreateDirectory(outputDir);
						}
						using (JsonWriter jsonWriter = new JsonWriter(outputFile))
						{
							jsonWriter.TypeHintName = StronglyTyped.MyTypeHintName;
							jsonWriter.PrettyPrint = true;
							jsonWriter.Write(obj);
						}
					}
					catch (JsonDeserializationException ex)
					{
						int col, line;
						ex.GetLineAndColumn(source, out line, out col);

						writer.WriteLine("ERROR: {0}", unitTest.Replace(unitTestsFolder, ""));
						writer.WriteLine("\"{0}\" ({1}, {2})", ex.Message, line, col);
						continue;
					}
					catch (JsonSerializationException ex)
					{
						writer.WriteLine("ERROR: {0}", unitTest.Replace(unitTestsFolder, ""));
						writer.WriteLine("\"{0}\"", ex.Message);
						continue;
					}
				}
			}
			else
			{
				writer.WriteLine(ErrorMessage);
			}
		}

		#endregion Methods
	}
}
