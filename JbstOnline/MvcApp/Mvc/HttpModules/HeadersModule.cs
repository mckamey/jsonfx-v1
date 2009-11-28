using System;
using System.Reflection;
using System.Web;

namespace JbstOnline.Mvc.HttpModules
{
	public class HeadersModule : IHttpModule
	{
		#region Fields

		private static readonly JsonFx.About Info = new JsonFx.About(Assembly.GetExecutingAssembly());
		private string ServerAgent;

		#endregion Fields

		#region IHttpModule Members

		void IHttpModule.Init(HttpApplication context)
		{
			this.ServerAgent = String.Concat(
				HeadersModule.Info.Name,
				"/",
				HeadersModule.Info.Version.ToString());

			context.EndRequest += this.OnEndRequest;
		}

		void IHttpModule.Dispose() {}

		#endregion IHttpModule Members

		#region Request Events

		private void OnEndRequest(object sender, EventArgs e)
		{
			HttpResponse response = HttpContext.Current.Response;

			// customize the "Server" HTTP Header
			try
			{
				response.Headers.Remove("Server");
			}
			catch (PlatformNotSupportedException) {}

			response.AppendHeader("Server", this.ServerAgent);
		}

		#endregion Request Events
	}
}