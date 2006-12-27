using System;
using System.IO;
using System.Web;
using System.Reflection;

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
			string script = Path.GetFileNameWithoutExtension(context.Request.FilePath)+".js";

			Assembly assembly = Assembly.GetAssembly(typeof(JsonFx.Scripts.ClientScript));
			using (Stream input = assembly.GetManifestResourceStream(JsonFx.Scripts.ClientScript.ScriptPath+script))
			{
			    if (input == null)
			        throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Invalid script name");

				context.Response.Clear();
				context.Response.ContentType = JsonFx.Scripts.ClientScript.JavaScriptContentType;
				context.Response.AddHeader("Content-Disposition", "inline;filename="+script);

#if DEBUG
				context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
#endif

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