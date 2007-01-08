using System;
using System.Web.Compilation;
using System.Security.Permissions;

namespace JsonFx.Compilation
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public class ForceCopyBuildProvider : System.Web.Compilation.BuildProvider
	{
		#region Init

		public ForceCopyBuildProvider()
		{
		}

		#endregion Init
	}
}
