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
using System.Reflection;

namespace JsonFx
{
	public static class About
	{
		#region Fields

		public static readonly Version Version;
		public static readonly string FullName;
		public static readonly string Name;
		public static readonly string Configuration;
		public static readonly string Copyright;
		public static readonly string Title;
		public static readonly string Description;
		public static readonly string Company;

		#endregion Fields

		#region Init

		static About()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(About));
			AssemblyName name = assembly.GetName();

			About.FullName = assembly.FullName;
			About.Version = name.Version;
			About.Name = name.Name;

			AssemblyCopyrightAttribute copyright = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
			About.Copyright = (copyright != null) ? copyright.Copyright : String.Empty;

			AssemblyDescriptionAttribute description = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
			About.Description = (description != null) ? description.Description : String.Empty;

			AssemblyTitleAttribute title = Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
			About.Title = (title != null) ? title.Title : String.Empty;

			AssemblyCompanyAttribute company = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
			About.Company = (company != null) ? company.Company : String.Empty;

			AssemblyConfigurationAttribute config = Attribute.GetCustomAttribute(assembly, typeof(AssemblyConfigurationAttribute)) as AssemblyConfigurationAttribute;
			About.Configuration = (config != null) ? config.Configuration : String.Empty;
		}

		#endregion Init
	}
}
