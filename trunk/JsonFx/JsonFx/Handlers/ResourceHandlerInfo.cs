using System;
using System.IO;
using System.Text;
using System.Web.Compilation;

namespace JsonFx.Handlers
{
	public abstract class ResourceHandlerInfo
	{
		#region Constants

		protected internal const string GeneratedNamespace = "JsonFx._Generated";

		private const string CompactedPath = "Compacted_";

		#endregion Constants

		#region Properties

		public abstract string ResourceName { get; }

		public string CompactResourceName
		{
			get { return ResourceHandlerInfo.GetCompactedResourceName(this.ResourceName); }
		}

		#endregion Properties

		#region Methods

		internal static string GetEmbeddedResourceName(string virtualPath)
		{
			if (String.IsNullOrEmpty(virtualPath))
			{
				return virtualPath;
			}

			StringBuilder builder = new StringBuilder(virtualPath);
			builder.Replace('/', '.');
			builder.Replace('\\', '.');
			builder.Replace('?', '.');
			builder.Replace('*', '.');
			builder.Replace(':', '.');
			return builder.ToString().TrimStart('.');
		}

		internal static string GetCompactedResourceName(string resourceName)
		{
			return ResourceHandlerInfo.CompactedPath+Path.GetFileName(resourceName);
		}

		internal static ResourceHandlerInfo GetHandlerInfo(string virtualPath)
		{
			try
			{
				return BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(ResourceHandlerInfo)) as ResourceHandlerInfo;
			}
			catch
			{
#warning Need to figure out why this fails sometimes
				return null;
			}
		}

		#endregion Methods
	}
}
