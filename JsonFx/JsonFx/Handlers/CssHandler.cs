using System;
using System.IO;
using System.Web;
using System.Reflection;
using System.Text;
using System.Web.Compilation;

namespace JsonFx.Handlers
{
	public abstract class CssHandlerInfo
	{
		#region Constants

		private const string CompactedCssPath = "Compacted_";

		#endregion Constants

		#region Properties

		public abstract string ResourceName { get; }

		public string CompactResourceName
		{
			get { return CssHandlerInfo.GetCompactedResourceName(this.ResourceName); }
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
			return CssHandlerInfo.CompactedCssPath+Path.GetFileName(resourceName);
		}

		#endregion Methods
	}

	public class CssHandler : IHttpHandler
	{
		#region Constants

		private const int BufferSize = 1024;

		internal const string CssContentType = "text/css";
		internal const string CssExtension = ".css";

		#endregion Constants

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			bool isDebug = "debug".Equals(context.Request.QueryString[null], StringComparison.InvariantCultureIgnoreCase);

			context.Response.Clear();
			context.Response.ClearContent();
			context.Response.ClearHeaders();
			context.Response.BufferOutput = true;
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.ContentType = CssHandler.CssContentType;

			// this is causing issues? Transfer-Encoding: chunked
			context.Response.AddHeader("Content-Disposition",
				"inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+CssHandler.CssExtension);

			// is this causing issues? Transfer-Encoding: chunked
			context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			// specifying "DEBUG" in the query string gets the non-compacted form
			this.OutputResourceFile(context, isDebug);
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members

		#region CssHandler Members

		protected void OutputResourceFile(HttpContext context, bool isDebug)
		{
			string virtualPath = context.Request.FilePath;
			CssHandlerInfo info = CssHandler.GetEmbeddedResourceInfo(virtualPath);
			if (info == null)
			{
				//throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Invalid StyleSheet path");
				this.OutputTargetFile(context);
				return;
			}
			string resourcePath = isDebug ? info.ResourceName : info.CompactResourceName;

			Assembly assembly = BuildManager.GetCompiledAssembly(virtualPath);
			Stream input = assembly.GetManifestResourceStream(resourcePath);
			if (input == null)
			{
				//throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Invalid StyleSheet path");
				this.OutputTargetFile(context);
				return;
			}

			this.BufferedWrite(context, new StreamReader(input, System.Text.Encoding.UTF8));
		}

		protected void OutputTargetFile(HttpContext context)
		{
			context.Response.TransmitFile(context.Request.PhysicalPath);

			//using (StreamReader reader = File.OpenText(context.Request.PhysicalPath))
			//{
			//    this.BufferedWrite(context, reader);
			//}
		}

		protected void BufferedWrite(HttpContext context, TextReader reader)
		{
			if (reader == null)
				throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Input stream is null.");

			using (TextWriter writer = context.Response.Output)
			{
				// buffered write to response
				char[] buffer = new char[CssHandler.BufferSize];
				int count;
				do
				{
					count = reader.ReadBlock(buffer, 0, CssHandler.BufferSize);
					writer.Write(buffer, 0, count);
				} while (count > 0);
				writer.Flush();
				writer.Close();
			}
		}

		protected static CssHandlerInfo GetEmbeddedResourceInfo(string virtualPath)
		{
			return (CssHandlerInfo)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(CssHandlerInfo));
		}

		#endregion CssHandler Members
	}
}