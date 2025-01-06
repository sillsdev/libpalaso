using System.IO;
using System.Reflection;
using System.Resources;

namespace SIL.Archiving.Resources
{
	internal static class Resources
	{
		public enum Name
		{
			AccessProtocols_json,
			CustomAccessProtocols_json,
			EmptyMets_xml,
		}

		public static string GetResource(Name namedResource)
		{
			return GetResource(namedResource.ToString().Replace("_", "."));
		}

		public static string GetResource(string filename)
		{
			using (var stream = Assembly.GetExecutingAssembly()
				       .GetManifestResourceStream($"SIL.Archiving.Resources.{filename}"))
			{
				if (stream == null)
				{
					throw new MissingManifestResourceException(
						$"{filename} resource not found");
				}

				var reader = new StreamReader(stream);
				return reader.ReadToEnd();
			}
		}
	}
}
