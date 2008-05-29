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
using System.Collections.Generic;
using System.Reflection;

namespace JsonFx.JsonRpc
{
	public abstract class JsonServiceInfo
	{
		#region JsonServiceInfo Properties

		public abstract Type ServiceType { get; }

		public abstract String Proxy { get; }

		public abstract String DebugProxy { get; }

		#endregion JsonServiceInfo Properties

		#region JsonServiceInfo Methods

		public abstract Object CreateService();

		public abstract MethodInfo ResolveMethodName(string name);

		public abstract String[] GetMethodParams(string name);

		#endregion JsonServiceInfo Methods

		#region Static Methods

		/// <summary>
		/// Gets a mapping of parameter position to parameter name for a given method.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		internal static String[] CreateParamMap(MethodInfo method)
		{
			ParameterInfo[] parameters = method.GetParameters();
			string[] paramMap = new string[parameters.Length];
			for (int i=0; i<parameters.Length; i++)
			{
				// map name to position
				paramMap[i] = parameters[i].Name;
			}
			return paramMap;
		}

		/// <summary>
		/// Gets a mapping of method JsonName to MethodInfo for a given type.
		/// </summary>
		/// <param name="serviceType"></param>
		/// <returns></returns>
		internal static Dictionary<String, MethodInfo> CreateMethodMap(Type serviceType)
		{
			Dictionary<string, MethodInfo> methodMap = new Dictionary<String, MethodInfo>();

			// load methods into method map
			foreach (MethodInfo info in serviceType.GetMethods())
			{
				if (!info.IsPublic)
					continue;

				if (!JsonMethodAttribute.IsJsonMethod(info))
					continue;

				string jsonName = JsonMethodAttribute.GetJsonName(info);
				if (String.IsNullOrEmpty(jsonName))
				{
					methodMap[info.Name] = info;
				}
				else
				{
					methodMap[jsonName] = info;
				}
			}

			return methodMap;
		}

		#endregion Static Methods
	}
}
