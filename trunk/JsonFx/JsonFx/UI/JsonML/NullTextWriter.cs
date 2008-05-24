using System;

namespace JsonFx.UI.JsonML
{
	internal class NullTextWriter : System.IO.TextWriter
	{
		public override System.Text.Encoding Encoding
		{
			get { return System.Text.Encoding.UTF8; }
		}
	}
}
