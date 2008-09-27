#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

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
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Web.Compilation;
using System.Globalization;
using System.Threading;

using JsonFx.Json;
using JsonFx.Compilation;

namespace JsonFx.Handlers
{
	public class GlobalizedResourceHandler : System.Web.IHttpHandler
	{
		#region Constants

		private const string JslintDirective = "/*global JsonFx */\r\n";
		private const string ResStart = "JsonFx.Lang.add(";
		private const string ResEnd = ");";
		private const string LookupStart = "JsonFx.Lang.get(";

		#endregion Constants

		#region Methods

		public static void ExtractGlobalizationKeys(string script, IList<string> globalizationKeys)
		{
			int i = 0;

			while ((i = script.IndexOf(GlobalizedResourceHandler.LookupStart, i)) >= 0)
			{
				i += GlobalizedResourceHandler.LookupStart.Length;

				try
				{
					string key = JsonReader.Deserialize(script, i) as string;
					if (String.IsNullOrEmpty(key))
					{
						continue;
					}

					globalizationKeys.Add(key);
				}
				catch
				{
					continue;
				}
			}
		}

		public static string GetKey(ResourceExpressionFields fields, string path)
		{
			if (fields == null)
			{
				return String.Empty;
			}
			if (String.IsNullOrEmpty(fields.ClassKey))
			{
				if (String.IsNullOrEmpty(path))
				{
					return fields.ResourceKey.ToLowerInvariant();
				}
				else if (path.StartsWith("~"))
				{
					path = path.Substring(1);
				}

				return path.ToLowerInvariant() + ',' + fields.ResourceKey.ToLowerInvariant();
			}

			return fields.ClassKey.ToLowerInvariant() + ',' + fields.ResourceKey.ToLowerInvariant();
		}

		protected virtual IDictionary<string, object> GetResourceStrings(IList<string> keys, string path)
		{
			if (keys == null)
			{
				return null;
			}

			Dictionary<string, object> res = new Dictionary<string, object>(keys.Count);
			foreach (string key in keys)
			{
				ResourceExpressionFields fields = ResourceExpressionBuilder.ParseExpression(key);
				if (fields == null)
				{
					continue;
				}

				object value;
				try
				{
					bool isLocal = String.IsNullOrEmpty(fields.ClassKey);
					if (!isLocal && fields.ClassKey.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
					{
						path = fields.ClassKey;
						isLocal = true;
					}

					if (isLocal)
					{
						if (path.StartsWith("~"))
						{
							path = path.Substring(1);
						}
						value = HttpContext.GetLocalResourceObject(path, fields.ResourceKey);
					}
					else
					{
						value = HttpContext.GetGlobalResourceObject(fields.ClassKey, fields.ResourceKey);
					}
				}
				catch (Exception ex)
				{
					value = ex.Message;
				}

				if (value == null)
				{
					continue;
				}
				res[GetKey(fields, path)] = value;
			}

			return res;
		}

		#endregion Methods

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			string setting = context.Request.QueryString[null];
			bool isDebug = ResourceHandler.DebugFlag.Equals(setting, StringComparison.InvariantCultureIgnoreCase);
			if (!isDebug)
			{
				// TODO: provide a mechanism for disabling compression?
				CompiledBuildResult.EnableStreamCompression(context);
			}

			string userCulture = context.Request.PathInfo;
			if (userCulture != null && userCulture.Length > 2)
			{
				userCulture = userCulture.Substring(1);
				try
				{
					Thread.CurrentThread.CurrentUICulture = new CultureInfo(userCulture);
				}
				catch { }
				try
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(userCulture);
				}
				catch { }
			}

			context.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);

			// get the target
			string targetPath = context.Request.FilePath;

			// TODO: provide mechanism for easily defining this target
			GlobalizedCompiledBuildResult target = CompiledBuildResult.Create(targetPath) as GlobalizedCompiledBuildResult;
			if (target == null)
			{
				// TODO: handle this more gracefully
				return;
			}

			IDictionary<string, object> res = this.GetResourceStrings(target.GlobalizationKeys, context.Request.FilePath);

			HttpResponse response = context.Response;
			response.ContentType = "text/javascript";

			response.AppendHeader(
				"Content-Disposition",
				"inline;filename="+Path.GetFileNameWithoutExtension(targetPath)+".js");

			if (isDebug)
			{
				response.Write(JslintDirective);
			}

			if (res.Count < 1)
			{
				// don't output call
				return;
			}

			response.Write(ResStart);

			JsonWriter writer = new JsonWriter(response.Output);
			writer.PrettyPrint = isDebug;
			writer.Write(res);

			response.Write(",");

			writer = new JsonWriter(response.Output);
			writer.PrettyPrint = isDebug;
			writer.Write(Thread.CurrentThread.CurrentCulture.Name);

			response.Write(ResEnd);
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}
