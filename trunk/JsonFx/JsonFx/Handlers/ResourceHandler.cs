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

			context.Response.ClearHeaders();
			context.Response.BufferOutput = true;

			// specifying "DEBUG" in the query string gets the non-compacted form
			Stream input = this.GetResourceStream(context, isDebug);
			if (input != Stream.Null)
			{
				context.Response.ContentEncoding = System.Text.Encoding.UTF8;
				context.Response.ContentType = this.ResourceContentType;

				context.Response.AppendHeader(
					"Content-Disposition",
					"inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+this.ResourceExtension);

				if (isDebug)
				{
					context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				}

				if (input == null)
				{
					//throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Invalid path");
					this.OutputTargetFile(context, isDebug);
				}
				else
				{
					this.BufferedWrite(context, input);
				}
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members

		#region ResourceHandler Members

		/// <summary>
		/// Determines the appropriate source stream for the incomming request
		/// </summary>
		/// <param name="context"></param>
		/// <param name="isDebug"></param>
		/// <returns></returns>
		protected virtual Stream GetResourceStream(HttpContext context, bool isDebug)
		{
			string virtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
			ResourceHandlerInfo info = ResourceHandlerInfo.GetHandlerInfo(virtualPath);
			if (info == null)
			{
				return null;
			}
			string resourcePath = isDebug ? info.ResourceName : info.CompactResourceName;

			Assembly assembly = BuildManager.GetCompiledAssembly(virtualPath);

			// check if client has cached copy
			ETag etag = new EmbeddedResourceETag(assembly, resourcePath);
			if (etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
			{
				return Stream.Null;
			}

			return assembly.GetManifestResourceStream(resourcePath);
		}

		protected void OutputTargetFile(HttpContext context, bool isDebug)
		{
			string fileName = context.Request.PhysicalPath;

			// check if client has cached copy
			ETag etag = new FileETag(fileName);
			if (!etag.HandleETag(context, HttpCacheability.ServerAndPrivate, isDebug))
			{
				context.Response.TransmitFile(fileName);

				//using (StreamReader reader = File.OpenText(fileName))
				//{
				//    this.BufferedWrite(context, reader);
				//}
			}
		}

		protected void BufferedWrite(HttpContext context, Stream input)
		{
			if (input == null)
			{
				throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Input stream is null.");
			}
			using (TextReader reader = new StreamReader(input, System.Text.Encoding.UTF8))
			{
				TextWriter writer = context.Response.Output;
				// buffered write to response
				char[] buffer = new char[ResourceHandler.BufferSize];
				int count;
				do
				{
					count = reader.ReadBlock(buffer, 0, ResourceHandler.BufferSize);
					writer.Write(buffer, 0, count);
				} while (count > 0);

				// flushing/closing the output causes "Transfer-Encoding: Chunked" which chokes IE6
			}
		}

		#endregion ResourceHandler Members
	}
}