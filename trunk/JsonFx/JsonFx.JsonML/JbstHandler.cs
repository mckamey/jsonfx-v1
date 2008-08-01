using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Compilation;

using JsonFx.JsonML.Builder;

namespace JsonFx.JsonML
{
	public class JbstHandler : JsonFx.Handlers.ResourceHandler
	{
		#region Properties

		protected override string ResourceContentType
		{
			get { return "text/plain"; }
		}

		protected override string ResourceExtension
		{
			get { return ".jbst"; }
		}

		#endregion Properties
	}
}