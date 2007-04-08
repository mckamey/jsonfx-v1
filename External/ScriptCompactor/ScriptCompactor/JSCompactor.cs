using System;
using System.Text.RegularExpressions;
using System.IO;

namespace ScriptCompactor
{
	public class JSCompactor
	{
		#region Fields

		private const string Pattern_Script = @"(?<Comment>([\/][\*].*?[\*][\/])|(\s*[\/]{2}.*?$))|(?<WhiteSpace>\s+)";
		private static readonly Regex Regex_Script = new Regex(Pattern_Script,
			RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase|RegexOptions.ExplicitCapture|RegexOptions.Multiline|RegexOptions.Singleline);

		#endregion Fields

		#region Methods

		public void Compact(string inputFile, string outputFile)
		{
			if (!File.Exists(inputFile))
				throw new FileNotFoundException(String.Format("File (\"{0}\") not found.", inputFile), inputFile);

			if (File.Exists(outputFile))
				throw new AccessViolationException(String.Format("File (\"{0}\") already exists.", outputFile));

			string scriptText;
			using (StreamReader reader = File.OpenText(inputFile))
			{
				scriptText = reader.ReadToEnd();
			}

			string outputText = Regex_Script.Replace(scriptText, new MatchEvaluator(ScriptReplacement));
			using (StreamWriter writer = File.CreateText(outputFile))
			{
				writer.Write(outputText);
			}
		}

		private static string ScriptReplacement(Match match)
		{
			if (match.Groups["WhiteSpace"].Success)
				return " ";
			else if (match.Groups["Comment"].Success)
				return String.Empty;
			else
				throw new InvalidOperationException("Unknown capture group.");
		}

		#endregion Methods
	}
}
