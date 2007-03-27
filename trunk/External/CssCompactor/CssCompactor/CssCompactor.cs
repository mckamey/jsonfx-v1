using System;
using System.IO;

namespace BuildTools.CssCompactor
{
	#region CssOptions

	[Flags]
	public enum CssOptions
	{
		None = 0x00,
		PrettyPrint = 0x01,
		Overwrite = 0x02
	}

	#endregion CssOptions

	public static class CssCompactor
	{
		#region Static Methods

		public static void Compact(string inputFile, string outputFile, string copyright, string timeStamp, CssOptions options)
		{
			if (!File.Exists(inputFile))
			{
				throw new FileNotFoundException(String.Format("File (\"{0}\") not found.", inputFile), inputFile);
			}

			if ((options&CssOptions.Overwrite) == 0x0 && File.Exists(outputFile))
			{
				throw new AccessViolationException(String.Format("File (\"{0}\") already exists.", outputFile));
			}

			if (inputFile.Equals(outputFile, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new ApplicationException("Input and output file are set to the same path.");
			}

			CssCompactor.PrepSavePath(outputFile);
			using (TextWriter output = File.CreateText(outputFile))
			{
				CssCompactor.Compact(inputFile, null, output, copyright, timeStamp, options);
			}
		}

		public static void Compact(string inputFile, string inputSource, TextWriter output, string copyright, string timeStamp, CssOptions options)
		{
			if (output == null)
			{
				throw new NullReferenceException("Output TextWriter was null.");
			}

			CssCompactor.WriteHeader(output, copyright, timeStamp);

			CssParser parser = new CssParser(inputFile, inputSource);
			parser.Write(output, options);
		}

		internal static void WriteHeader(TextWriter writer, string copyright, string timeStamp)
		{
			if (!String.IsNullOrEmpty(copyright) || !String.IsNullOrEmpty(timeStamp))
			{
				int width = 6;
				if (!String.IsNullOrEmpty(copyright))
				{
					copyright = copyright.Replace("*/", "");
					width = Math.Max(copyright.Length+6, width);
				}
				if (!String.IsNullOrEmpty(timeStamp))
				{
					timeStamp = DateTime.Now.ToString(timeStamp).Replace("*/", "");
					width = Math.Max(timeStamp.Length+6, width);
				}

				writer.WriteLine("/*".PadRight(width, '-')+"*\\");

				if (!String.IsNullOrEmpty(copyright))
				{
					writer.WriteLine("\t"+copyright);
				}

				if (!String.IsNullOrEmpty(timeStamp))
				{
					writer.WriteLine("\t"+timeStamp);
				}

				writer.WriteLine("\\*".PadRight(width, '-')+"*/");
			}
		}

		/// <summary>
		/// Makes sure directory exists and if file exists is not readonly.
		/// </summary>
		/// <param name="filename"></param>
		internal static void PrepSavePath(string filename)
		{
			if (File.Exists(filename))
			{
				// make sure not readonly
				FileAttributes attributes = File.GetAttributes(filename);
				attributes &= ~FileAttributes.ReadOnly;
				File.SetAttributes(filename, attributes);
			}
			else if (!Directory.Exists(Path.GetDirectoryName(filename)))
			{
				// make sure directories exist
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
			}
		}

		#endregion Static Methods
	}
}
