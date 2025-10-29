using System.Collections.Generic;

namespace SIL.Core.ClearShare
{
	/// <summary/>
	public class LicenseUtils
	{
		// Note: the only use of FromXmp is in Metadata, to create a license.
		public static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("license") &&
			    properties["license"].Contains("creativecommons"))
				return CreativeCommonsLicense.FromMetadata(properties);

			if (properties.ContainsKey("rights (en)"))
				return CustomLicense.FromMetadata(properties);
			return new NullLicense();
		}
	}
}