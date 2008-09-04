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

using JsonFx.Json;

namespace JsonFx.Handlers
{
	public class ResxHandler : System.Web.IHttpHandler
	{
		#region Constants

		private const string JslintDirective = "/*global JsonFx */\r\n";

		private const string ResxStart = "JsonFx.ResX.add(";

		private const string ResxEnd = ");";

		#endregion Constants

		#region Methods

		public static string GetKey(ResourceExpressionFields fields)
		{
			if (fields == null)
			{
				return String.Empty;
			}
			if (String.IsNullOrEmpty(fields.ClassKey))
			{
				return fields.ResourceKey.ToLowerInvariant();
			}

			return fields.ClassKey.ToLowerInvariant() + ',' + fields.ResourceKey.ToLowerInvariant();
		}

		protected virtual IDictionary<string, object> GetResourceStrings(IList<string> keys, string path)
		{
			if (keys == null)
			{
				return null;
			}

			Dictionary<string, object> resx = new Dictionary<string, object>(keys.Count);
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
					if (String.IsNullOrEmpty(fields.ClassKey))
					{
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
				resx[GetKey(fields)] = value;
			}

			return resx;
		}

		#endregion Methods

		#region IHttpHandler Members

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			string cacheKey = context.Request.QueryString[null];
			bool isDebug = "debug".Equals(cacheKey, StringComparison.InvariantCultureIgnoreCase);

			// TODO: provide mechanism for easily defining this list
			string[] keys = {
				"thismustbelocal",
				"global, test",
				"global, copyright",
				"global, foo",
				"global, charsinalphabet",
				"global, trueorfalse",
				"global, randomguid",
				"global, pi",
				"global, empty"
			};

			IDictionary<string, object> resx = this.GetResourceStrings(keys, context.Request.AppRelativeCurrentExecutionFilePath);

			HttpResponse response = context.Response;
			response.ContentType = "text/javascript";

			response.AppendHeader(
				"Content-Disposition",
				"inline;filename="+Path.GetFileNameWithoutExtension(context.Request.FilePath)+".js");

			if (isDebug)
			{
				response.Write(JslintDirective);
			}

			response.Write(ResxStart);

			JsonWriter writer = new JsonWriter(response.Output);
			writer.PrettyPrint = isDebug;
			writer.Write(resx);

			response.Write(ResxEnd);
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}
