using System;
using System.IO;
using System.Text;
using System.Web.Compilation;

namespace JsonFx.Handlers
{
	internal abstract class ResourceHandlerInfo
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
			string dir = Path.GetDirectoryName(resourceName);
			if (!String.IsNullOrEmpty(dir))
			{
				dir += Path.DirectorySeparatorChar;
			}

			return dir +
				ResourceHandlerInfo.CompactedPath +
				Path.GetFileName(resourceName);
		}

		internal static ResourceHandlerInfo GetHandlerInfo(string virtualPath)
		{
			return BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(ResourceHandlerInfo)) as ResourceHandlerInfo;
		}

		#endregion Methods
	}
}
