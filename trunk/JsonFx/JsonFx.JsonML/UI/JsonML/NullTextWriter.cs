using System;
using System.Text;

namespace JsonFx.UI.JsonML
{
	internal class NullTextWriter : System.IO.TextWriter
	{
		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}
	}
}
