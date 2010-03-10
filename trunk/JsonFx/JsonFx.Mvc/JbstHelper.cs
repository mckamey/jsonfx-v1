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
using System.IO;
using System.Web.UI;

using JsonFx.Json;

namespace JsonFx.Mvc
{
	public static class Jbst
	{
		#region JBST Helper Methods

		/// <summary>
		/// Bind the JBST to the provided data.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="jbst"></param>
		/// <param name="data">named data to bind</param>
		/// <param name="dataItems">collection of data to emit</param>
		/// <returns></returns>
		public static string Bind(EcmaScriptIdentifier jbst, EcmaScriptIdentifier data, IDictionary<string, object> dataItems)
		{
			// build the control
			JsonFx.UI.Jbst.Control control = new JsonFx.UI.Jbst.Control();
			control.Name = jbst;
			control.Data = data;

			if (dataItems != null)
			{
				foreach (string key in dataItems.Keys)
				{
					control.DataItems[key] = dataItems[key];
				}
			}

			// render the control
			return Render(control);
		}

		/// <summary>
		/// Bind the JBST to the provided data.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="jbst"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string Bind(EcmaScriptIdentifier jbst, object data)
		{
			// build the control
			JsonFx.UI.Jbst.Control control = new JsonFx.UI.Jbst.Control();
			control.Name = jbst;
			control.InlineData = data;

			// render the control
			return Render(control);
		}

		/// <summary>
		/// Bind the JBST to the provided data.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="jbst"></param>
		/// <param name="data"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static string Bind(EcmaScriptIdentifier jbst, object data, int index, int count)
		{
			// build the control
			JsonFx.UI.Jbst.Control control = new JsonFx.UI.Jbst.Control();
			control.Name = jbst;
			control.InlineData = data;
			control.Index = index;
			control.Count = count;

			// render the control
			return Render(control);
		}

		#endregion JBST Helper Methods

		#region Script Data Helper Methods

		/// <summary>
		/// Emit the provided data as JavaScript variables.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="name"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string ScriptData(string name, object data)
		{
			// build the control
			JsonFx.Client.ScriptDataBlock dataBlock = new JsonFx.Client.ScriptDataBlock();
			dataBlock.DataItems[name] = dataBlock;

			// render the control
			return Render(dataBlock);
		}

		/// <summary>
		/// Emit the provided data as JavaScript variables.
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="name"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string ScriptData(IDictionary<string, object> dataItems)
		{
			// build the control
			JsonFx.Client.ScriptDataBlock dataBlock = new JsonFx.Client.ScriptDataBlock();
			foreach (string key in dataItems.Keys)
			{
				dataBlock.DataItems[key] = dataItems[key];
			}

			// render the control
			return Render(dataBlock);
		}

		#endregion Script Data Helper Methods

		#region Utility Methods

		internal static string Render(System.Web.UI.Control control)
		{
			HtmlTextWriter writer = new XhtmlTextWriter(new StringWriter());
			control.RenderControl(writer);
			writer.Flush();

			return writer.InnerWriter.ToString();
		}

		#endregion Utility Methods
	}
}
