using System;
using System.IO;
using System.Web;
using System.Reflection;
using System.Web.Compilation;

namespace JsonFx.Handlers
{
	public class ClientScriptHandler : IHttpHandler
	{
		#region Constants

		private const int BufferSize = 1024;

		#endregion Constants

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			context.Response.Clear();
			context.Response.ContentType = JsonFx.Scripts.ClientScript.JavaScriptContentType;
#if DEBUG
			context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
#endif

			string virtualPath = context.Request.FilePath;
			string compiledScript = BuildManager.GetCompiledCustomString(virtualPath);
			if (!String.IsNullOrEmpty(compiledScript))
			{
				using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
				{
					writer.Write(compiledScript);
					writer.Flush();
					writer.Close();
					return;
				}
			}

			// wasn't precompiled so just stream from original file
			using (Stream input = new FileStream(context.Request.PhysicalPath, FileMode.Open, FileAccess.Read))
			{
				if (input == null)
			        throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Invalid script name");

				// buffered write to response
				byte[] buffer = new byte[ClientScriptHandler.BufferSize];
				Stream output = context.Response.OutputStream;
				int count;
				do
				{
					count = input.Read(buffer, 0, ClientScriptHandler.BufferSize);
					output.Write(buffer, 0, count);
				} while (count > 0);
				output.Flush();
				output.Close();
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}