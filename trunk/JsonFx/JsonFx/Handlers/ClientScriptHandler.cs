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
			context.Response.ClearContent();
			context.Response.ClearHeaders();
			context.Response.BufferOutput = true;
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.ContentType = JsonFx.Scripts.ClientScript.JavaScriptContentType;

			// this is causing issues? Transfer-Encoding: chunked
			context.Response.AddHeader("Content-Disposition", "inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+".js");

			// specifying "DEBUG" in the query string gets the non-compacted form
			if (//String.IsNullOrEmpty(context.Request.QueryString["debug"]) &&
				!"debug".Equals(context.Request.QueryString[null], StringComparison.InvariantCultureIgnoreCase))
			{
				if (this.OutputCompiledFile(context))
				{
					return;
				}
			}

			// this is causing issues? Transfer-Encoding: chunked
			context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			if (context.Request.FilePath.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase))
			{
				// wasn't precompiled so just stream from original file
				this.OutputTargetFile(context);
				return;
			}

			this.OutputResourceFile(context);
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members

		#region ClientScriptHandler Members

		protected bool OutputCompiledFile(HttpContext context)
		{
			string virtualPath = context.Request.FilePath;
			string compiledScript = BuildManager.GetCompiledCustomString(virtualPath);
			if (String.IsNullOrEmpty(compiledScript))
				return false;

			TextWriter writer = context.Response.Output;
			writer.Write(compiledScript);
			writer.Flush();
			writer.Close();
			return true;
		}

		protected void OutputResourceFile(HttpContext context)
		{
			string virtualPath = context.Request.FilePath;
			string script = Path.GetFileNameWithoutExtension(virtualPath)+".js";
			Assembly assembly = Assembly.GetAssembly(typeof(JsonFx.Scripts.ClientScript));
			Stream input = assembly.GetManifestResourceStream(JsonFx.Scripts.ClientScript.ScriptPath+script);
			if (input == null)
				throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Invalid script name");

			this.BufferedWrite(context, new StreamReader(input, System.Text.Encoding.UTF8));
		}

		protected void OutputTargetFile(HttpContext context)
		{
			context.Response.TransmitFile(context.Request.PhysicalPath);

			//StreamReader reader = File.OpenText(context.Request.PhysicalPath);

			//this.BufferedWrite(context, reader);
		}

		protected void BufferedWrite(HttpContext context, TextReader reader)
		{
			if (reader == null)
				throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Input stream is null.");

			using (reader)
			{
				using (TextWriter writer = context.Response.Output)
				{
					// buffered write to response
					char[] buffer = new char[ClientScriptHandler.BufferSize];
					int count;
					do
					{
						count = reader.ReadBlock(buffer, 0, ClientScriptHandler.BufferSize);
						writer.Write(buffer);
					} while (count > 0);
				}
			}
		}

		#endregion ClientScriptHandler Members
	}
}