#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Net.Mime;
using System.Web;
using System.Web.Compilation;

using JsonFx.Json;
using JsonFx.JsonRpc;
using JsonFx.JsonRpc.Discovery;
using JsonFx.JsonRpc.Proxy;

namespace JsonFx.Handlers
{
	internal class JsonServiceHandler : IHttpHandler
	{
		#region Constants

		protected internal const string DescriptionMethodName = "system.describe";
		public const string JsonFileExtension = ".json";

		#endregion Constants

		#region Fields

		private readonly IJsonServiceInfo ServiceInfo;
		private object service = null;
		private string serviceUrl = null;
		private Exception error = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="serviceInfo"></param>
		/// <param name="serviceUrl"></param>
		public JsonServiceHandler(IJsonServiceInfo serviceInfo, string serviceUrl)
		{
			try
			{
				this.ServiceInfo = serviceInfo;
				this.serviceUrl = serviceUrl;
			}
			catch (Exception ex)
			{
				this.error = ex;
			}
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the service object servicing the request
		/// </summary>
		protected object Service
		{
			get
			{
				if (this.service == null)
				{
					this.service = this.ServiceInfo.CreateService();
				}
				return this.service;
			}
		}

		#endregion Properties

		#region Methods

		private JsonRequest BuildRequestFromGet(HttpContext context)
		{
			JsonRequest request = new JsonRequest();

			Dictionary<String, Object> parameters = new Dictionary<String, Object>();
			foreach (string key in context.Request.QueryString.Keys)
			{
				if (String.IsNullOrEmpty(key))
				{
					continue;
				}
				parameters[key] = context.Request.QueryString[key];
			}

			request.NamedParams = parameters;

			if (!String.IsNullOrEmpty(context.Request.PathInfo))
			{
				request.Method = context.Request.PathInfo.Substring(1);
				if (String.IsNullOrEmpty(request.Method) && request.NamedParams.Keys.Count < 1)
				{
					request.Method = JsonServiceHandler.DescriptionMethodName;
				}
			}

			return request;
		}

		private JsonRequest BuildRequestFromPost(HttpContext context)
		{
			JsonReader reader = new JsonReader(context.Request.InputStream);
			return (JsonRequest)reader.Deserialize(typeof(JsonRequest));
		}

		private void HandleRequest(HttpContext context, JsonRequest request, ref JsonResponse response)
		{
			context.Response.Clear();
			HttpBrowserCapabilities browser = context.Request.Browser;

			// this is a specific fix for Opera 8.x
			// Opera 8 requires "text/plain" or "text/html"
			// otherwise the content encoding is mangled
			bool isOpera8 = browser.IsBrowser("opera") && (browser.MajorVersion <= 8);
			context.Response.ContentType =
					(isOpera8) ?
					MediaTypeNames.Text.Plain :
					JsonFx.Json.JsonWriter.JsonMimeType;

			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.AddHeader("Content-Disposition", "inline;filename=JsonResponse"+JsonServiceHandler.JsonFileExtension);

			response.ID = request.ID;

			System.Reflection.MethodInfo method = this.ServiceInfo.ResolveMethodName(request.Method);

			if (JsonServiceHandler.DescriptionMethodName.Equals(request.Method, StringComparison.InvariantCulture))
			{
				response.Result = new JsonServiceDescription(this.ServiceInfo.ServiceType, this.serviceUrl);
			}
			else if (method != null)
			{
				Object[] positionalParams = null;
				if (request.NamedParams != null)
				{
					String[] paramMap = this.ServiceInfo.GetMethodParams(method.Name);
					positionalParams = new Object[paramMap.Length];
					for (int i=0; i<paramMap.Length; i++)
					{
						// initially map name to position
						positionalParams[i] = request.NamedParams[paramMap[i]];
						if (positionalParams[i] == null)
						{
							// try to get as named positional param
							positionalParams[i] = request.NamedParams[i.ToString()];
						}
					}
				}
				else if (request.PositionalParams != null)
				{
					positionalParams = new object[request.PositionalParams.Length];
					request.PositionalParams.CopyTo(positionalParams, 0);
				}

				try
				{
					if (positionalParams != null)
					{
						ParameterInfo[] paramInfo = method.GetParameters();
						for (int i=0; i<positionalParams.Length; i++)
						{
							// ensure type compatibility of parameters
							positionalParams[i] = JsonReader.CoerceType(paramInfo[i].ParameterType, positionalParams[i]);
						}
					}

					response.Result = method.Invoke(this.Service, positionalParams);
				}
				catch (TargetParameterCountException ex)
				{
					throw new InvalidParamsException(
						String.Format(
							"Method \"{0}\" expects {1} parameters, {2} provided",
							method.Name,
							method.GetParameters().Length, positionalParams.Length),
						ex);
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null)
					{
						throw new JsonServiceException(ex.InnerException.Message, ex.InnerException);
					}
					throw new JsonServiceException(ex.Message, ex);
				}
				catch (Exception ex)
				{
					throw new JsonServiceException(ex.Message, ex);
				}
			}
			else
			{
				throw new InvalidMethodException("Invalid method name: "+request.Method);
			}
		}

		#endregion Methods

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			context.Response.Cache.SetCacheability(HttpCacheability.Private);

			JsonResponse response = new JsonResponse();
			try
			{
				if (this.error != null)
					throw this.error;

				JsonRequest request = null;

				if ("GET".Equals(context.Request.HttpMethod, StringComparison.OrdinalIgnoreCase))
				{
					request = this.BuildRequestFromGet(context);
				}
				else
				{
					request = this.BuildRequestFromPost(context);
				}

				if (request == null)
				{
					throw new InvalidRequestException("The JSON-RPC Request was empty.");
				}

				this.HandleRequest(context, request, ref response);
			}
			catch (InvalidRequestException ex)
			{
				context.Response.ClearContent();
				response.Result = null;
				response.Error = new JsonError(ex, JsonRpcErrors.InvalidRequest);
			}
			catch (InvalidMethodException ex)
			{
				context.Response.ClearContent();
				response.Result = null;
				response.Error = new JsonError(ex, JsonRpcErrors.MethodNotFound);
			}
			catch (InvalidParamsException ex)
			{
				context.Response.ClearContent();
				response.Result = null;
				response.Error = new JsonError(ex, JsonRpcErrors.InvalidParams);
			}
			catch (JsonServiceException ex)
			{
				context.Response.ClearContent();
				response.Result = null;
				response.Error = new JsonError(ex, JsonRpcErrors.InvalidRequest);
			}
			catch (JsonDeserializationException ex)
			{
				context.Response.ClearContent();
				response.Result = null;
				response.Error = new JsonError(ex, JsonRpcErrors.ParseError);
			}
			catch (Exception ex)
			{
				context.Response.ClearContent();
				response.Result = null;
				response.Error = new JsonError(ex, JsonRpcErrors.InternalError);
			}
			finally
			{
				try
				{
					using (JsonWriter writer = new JsonWriter(context.Response.Output))
					{
						writer.Write(response);
					}
				}
				catch (TargetInvocationException ex)
				{
					context.Response.ClearContent();
					response.Result = null;
					response.Error = new JsonError(ex.InnerException);

					using (JsonWriter writer = new JsonWriter(context.Response.Output))
					{
						writer.Write(response);
					}
				}
				catch (Exception ex)
				{
					context.Response.ClearContent();
					response.Result = null;
					response.Error = new JsonError(ex);

					using (JsonWriter writer = new JsonWriter(context.Response.Output))
					{
						writer.Write(response);
					}
				}
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}