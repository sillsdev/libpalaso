using System.Collections.Generic;
using System;
using System.Drawing;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	public class CustomLicense : CustomLicenseBare, ILicenseWithImage
	{
		public Image GetImage()
		{
			return null;
		}

		public static LicenseInfo FromMetadata(Dictionary<string, string> properties)
		{
			if (!properties.ContainsKey("rights (en)"))
				throw new ApplicationException("A license property is required in order to make a  Custom License from metadata.");

			var license = new CustomLicense();
			license.RightsStatement = properties["rights (en)"];
			return license;
		}
	}
}