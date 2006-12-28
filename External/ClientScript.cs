using System;

#region Web Resource Declarations

// Allow these resources to be web browsable from this Assembly
[assembly: System.Web.UI.WebResourceAttribute(JsonFx.Scripts.ClientScript.CoreScript, JsonFx.Scripts.ClientScript.JavaScriptContentType)]
[assembly: System.Web.UI.WebResourceAttribute(JsonFx.Scripts.ClientScript.IOScript, JsonFx.Scripts.ClientScript.JavaScriptContentType)]
[assembly: System.Web.UI.WebResourceAttribute(JsonFx.Scripts.ClientScript.UIScript, JsonFx.Scripts.ClientScript.JavaScriptContentType)]
[assembly: System.Web.UI.WebResourceAttribute(JsonFx.Scripts.ClientScript.JsonScript, JsonFx.Scripts.ClientScript.JavaScriptContentType)]
[assembly: System.Web.UI.WebResourceAttribute(JsonFx.Scripts.ClientScript.UtilityScript, JsonFx.Scripts.ClientScript.JavaScriptContentType)]
[assembly: System.Web.UI.WebResourceAttribute(JsonFx.Scripts.ClientScript.TraceScript, JsonFx.Scripts.ClientScript.JavaScriptContentType)]

#endregion Web Resource Declarations

namespace JsonFx.Scripts
{
	/// <summary>
	/// Serves as a central repository for resource metadata
	/// </summary>
	public sealed class ClientScript
	{
		#region Constants

		public const string JavaScriptContentType = "application/javascript";

		#endregion Constants

		#region Scripts

#if DEBUG
		// full source scripts: good for dev and debugging
		public const string ScriptPath = "JsonFx.Scripts.";
#else
		// compacted scripts: better for distribution and speed
		public const string ScriptPath = "JsonFx.Scripts.Compacted.";
#endif

		public const string CoreScript = ScriptPath + "Core.js";
		public const string IOScript = ScriptPath + "IO.js";
		public const string UIScript = ScriptPath + "UI.js";
		public const string JsonScript = ScriptPath + "JSON.js";
		public const string UtilityScript = ScriptPath + "Utility.js";
		public const string TraceScript = ScriptPath + "Trace.js";

		#endregion Scripts

		#region Methods

		public static void RegisterScript(System.Web.UI.Page page, string scriptResource)
		{
			if (page == null || String.IsNullOrEmpty(scriptResource))
				return;

			page.ClientScript.RegisterClientScriptResource(typeof(ClientScript), scriptResource);
		}

		public static string GetUrl(System.Web.UI.Page page, string resource)
		{
			if (page == null || String.IsNullOrEmpty(resource))
				return null;

			return page.ClientScript.GetWebResourceUrl(typeof(ClientScript), resource);
		}

		/// <summary>
		/// Gets a URL to the blank image in System.Web.dll
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public static string GetBlankImageUrl(System.Web.UI.Page page)
		{
			if (page == null)
				return null;

			return page.ClientScript.GetWebResourceUrl(typeof(System.Web.UI.Page), "Spacer.gif");
		}

		#endregion Methods
	}
}
