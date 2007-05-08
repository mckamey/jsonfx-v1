using System;
using System.IO;
using System.Web;
using System.Reflection;
using System.Text;
using System.Web.Compilation;

namespace JsonFx.Handlers
{
	public abstract class ResourceHandler : IHttpHandler
	{
		#region Constants

		private const int BufferSize = 1024;

		#endregion Constants

		#region Properties

		protected abstract string ResourceContentType { get; }
		protected abstract string ResourceExtension { get; }

		#endregion Properties

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			bool isDebug = "debug".Equals(context.Request.QueryString[null], StringComparison.InvariantCultureIgnoreCase);

			context.Response.Clear();
			context.Response.ClearContent();
			context.Response.ClearHeaders();
			context.Response.BufferOutput = true;
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.ContentType = this.ResourceContentType;

			// this is causing issues? Transfer-Encoding: chunked
			context.Response.AddHeader("Content-Disposition",
				"inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+this.ResourceExtension);

			if (isDebug)
			{
				// is this causing issues? Transfer-Encoding: chunked
				context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			}

			// specifying "DEBUG" in the query string gets the non-compacted form
			Stream input = this.GetResourceStream(context, isDebug);
			if (input == null)
			{
				//throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Invalid path");
				this.OutputTargetFile(context);
			}
			else
			{
				this.BufferedWrite(context, input);
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members

		#region ResourceHandler Members

		protected virtual Stream GetResourceStream(HttpContext context, bool isDebug)
		{
			string virtualPath = context.Request.FilePath;
			ResourceHandlerInfo info = ResourceHandlerInfo.GetHandlerInfo(virtualPath);
			if (info == null)
			{
				return null;
			}
			string resourcePath = isDebug ? info.ResourceName : info.CompactResourceName;

			Assembly assembly = BuildManager.GetCompiledAssembly(virtualPath);
			return assembly.GetManifestResourceStream(resourcePath);
		}

		protected virtual void OutputTargetFile(HttpContext context)
		{
			context.Response.TransmitFile(context.Request.PhysicalPath);

			//using (StreamReader reader = File.OpenText(context.Request.PhysicalPath))
			//{
			//    this.BufferedWrite(context, reader);
			//}
		}

		protected void BufferedWrite(HttpContext context, Stream input)
		{
			if (input == null)
			{
				throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Input stream is null.");
			}
			using (TextReader reader = new StreamReader(input, System.Text.Encoding.UTF8))
			{
				using (TextWriter writer = context.Response.Output)
				{
					// buffered write to response
					char[] buffer = new char[ResourceHandler.BufferSize];
					int count;
					do
					{
						count = reader.ReadBlock(buffer, 0, ResourceHandler.BufferSize);
						writer.Write(buffer, 0, count);
					} while (count > 0);
					writer.Flush();
					writer.Close();
				}
			}
		}

		#endregion ResourceHandler Members
	}
}