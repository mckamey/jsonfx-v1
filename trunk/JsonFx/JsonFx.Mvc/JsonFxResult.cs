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
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

using JsonFx.Json;

namespace JsonFx.Mvc
{
	/// <summary>
	/// ActionResult which uses JsonFx to serialize result data as JSON
	/// </summary>
	public class JsonFxResult : ActionResult
	{
		#region Properties

		/// <summary>
		/// Gets and sets the content encoding of the response
		/// </summary>
		public Encoding ContentEncoding
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets the content type of the response
		/// </summary>
		public string ContentType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets the response data to be serialized
		/// </summary>
		public object Data
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets the HTTP status code of the response
		/// </summary>
		public HttpStatusCode HttpStatusCode
		{
			get;
			set;
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Renders the actual response
		/// </summary>
		/// <param name="context"></param>
		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			HttpResponseBase response = context.HttpContext.Response;

			if (!String.IsNullOrEmpty(this.ContentType))
			{
				response.ContentType = this.ContentType;
			}
			else
			{
				response.ContentType = "application/json";
			}

			if (this.HttpStatusCode != default(HttpStatusCode))
			{
				response.StatusCode = (int)this.HttpStatusCode;
			}

			if (this.ContentEncoding != null)
			{
				response.ContentEncoding = this.ContentEncoding;
			}

			if (this.Data != null)
			{
				JsonWriter writer = new JsonWriter(response.Output);
				writer.PrettyPrint = context.HttpContext != null && context.HttpContext.IsDebuggingEnabled;
				writer.Write(this.Data);
			}
		}

		#endregion Methods
	}
}
