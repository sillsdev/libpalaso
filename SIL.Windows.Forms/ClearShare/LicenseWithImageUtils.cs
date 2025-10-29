using System.Collections.Generic;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	/// <summary/>
	public class LicenseWithImageUtils
	{
		// Note: the only use of FromXmp is in Metadata, to create a license.
		public static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("license") &&
			    properties["license"].Contains("creativecommons"))
				return CreativeCommonsLicenseWithImage.FromMetadata(properties);

			if (properties.ContainsKey("rights (en)"))
				return CustomLicenseWithImage.FromMetadata(properties);
			return new NullLicense();
		}
	}
}