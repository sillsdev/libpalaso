using System;
using System.Reflection;

namespace SIL
{
	public static class EntryAssembly
	{
		public static string Location
		{
			get
			{
				var assembly = Assembly.GetEntryAssembly();
				return assembly == null ? "" : assembly.Location;
			}
		}

		public static string CompanyName
		{
			get
			{
				var assembly = Assembly.GetEntryAssembly();
				if(assembly==null)
				{
					return "";
				}
				var attributes = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
				return attributes.Company;
			}
		}

		public static string ProductName
		{
			get
			{
				var assembly = Assembly.GetEntryAssembly();
				if (assembly == null)
				{
					return "";
				}
				var attributes = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
				return attributes.Product;
			}
		}
	}
}
