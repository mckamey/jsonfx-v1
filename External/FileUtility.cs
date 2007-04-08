using System;
using System.IO;

namespace BuildTools.IO
{
	internal class FileUtility
	{
		#region Methods

		/// <summary>
		/// Makes sure directory exists and if file exists is not readonly.
		/// </summary>
		/// <param name="filename"></param>
		public static void PrepSavePath(string filename)
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

		#endregion Methods
	}
}
