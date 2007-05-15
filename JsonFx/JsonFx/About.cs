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
