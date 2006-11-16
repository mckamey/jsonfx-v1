using System;
using System.Collections.Generic;
using System.Reflection;

namespace JsonFx.Services
{
	public abstract class JsonServiceInfo
	{
		#region JsonServiceInfo Properties

		public abstract Type ServiceType { get; }

		public abstract String Proxy { get; }

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
