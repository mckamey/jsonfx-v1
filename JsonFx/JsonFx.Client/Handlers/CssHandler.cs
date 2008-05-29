using System;

namespace JsonFx.Handlers
{
	public class CssHandler : JsonFx.Handlers.ResourceHandler
	{
		#region Properties

		protected override string ResourceContentType
		{
			get { return "text/css"; }
		}

		protected override string  ResourceExtension
		{
			get { return ".css"; }
		}

		#endregion Properties
	}
}