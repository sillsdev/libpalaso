using System;
using System.Collections.Generic;
using System.Drawing;
using SIL.Core.ClearShare;
using JetBrains.Annotations;
using L10NSharp;

namespace SIL.Windows.Forms.ClearShare
{
	public class CustomLicense : CustomLicenseWithoutImage, ILicenseWithImage
	{
		public new static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("license") && properties["license"].Contains("creativecommons"))
				return CreativeCommonsLicense.FromMetadata(properties);

			if (properties.ContainsKey("rights (en)"))
				return FromMetadata(properties);
			return new NullLicense();
		}

		public new static LicenseInfo FromToken(string abbr)
		{
			switch (abbr)
			{
				case "ask": return new NullLicense();
				case "custom": return new CustomLicense();
				default:
					return CreativeCommonsLicense.FromToken(abbr);
			}
		}
		public Image GetImage()
		{
			return null;
		}
	}
}