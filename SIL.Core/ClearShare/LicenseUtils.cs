using System.Collections.Generic;

namespace SIL.Core.ClearShare
{
	/// <summary/>
	public static class LicenseUtils
	{
		/// <summary>
		/// Creates a <see cref="LicenseInfo"/> instance from XMP metadata properties.
		/// XMP (Extensible Metadata Platform) is a standard for embedding structured metadata in digital files.
		/// This method examines known metadata keys and returns the appropriate concrete license type.
		/// Returns a <see cref="NullLicense"/> if no recognized license is found.
		/// </summary>
		// Note: the only use of FromXmp is in Metadata, to create a license.
		public static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("license") &&
			    properties["license"].Contains("creativecommons"))
				return CreativeCommonsLicenseInfo.CCLicenseInfoFromMetadata(properties);

			if (properties.ContainsKey("rights (en)"))
				return CustomLicenseInfo.CustomLicenseInfoFromMetadata(properties);
			return new NullLicense();
		}
	}
}