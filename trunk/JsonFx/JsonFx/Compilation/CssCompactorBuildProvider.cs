using System;
using System.Reflection;
using System.Web.Compilation;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text;
using System.IO;
using System.Collections.Generic;

using BuildTools;
using BuildTools.CssCompactor;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public class CssCompactorBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Init

		public CssCompactorBuildProvider()
		{
		}

		#endregion Init

		#region BuildProvider Methods

		public override string GetCustomString(CompilerResults results)
		{
			string source = null;
			try
			{
				using (TextReader reader = this.OpenCssSource())
				{
					source = reader.ReadToEnd();
				}
				StringWriter writer = new StringWriter(new StringBuilder(source.Length));
				List<ParseException> errors = CssCompactor.Compact(base.VirtualPath, source, writer, null, null, CssOptions.None);

				foreach (ParseException ex in errors)
				{
					throw new System.Web.HttpParseException(ex.Message, ex, base.VirtualPath, source, ex.Line);
					//results.Errors.Add(new CompilerError(base.VirtualPath, ex.Line, ex.Column, ex.ErrorCode, ex.Message));
				}

				return writer.ToString();
			}
			catch (BuildTools.ParseException ex)
			{
				throw new System.Web.HttpParseException(ex.Message, ex, base.VirtualPath, source, ex.Line);
			}
		}

		#endregion BuildProvider Methods

		#region CssCompactorBuildProvider Methods

		protected virtual TextReader OpenCssSource()
		{
			return base.OpenReader();
		}

		#endregion CssCompactorBuildProvider Methods
	}
}
