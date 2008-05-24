using System;

namespace JsonFx.UI
{
	internal class NullTextWriter : System.IO.TextWriter
	{
		public override System.Text.Encoding Encoding
		{
			get { return System.Text.Encoding.UTF8; }
		}
	}
}
