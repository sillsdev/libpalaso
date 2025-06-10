using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	public class CreativeCommonsLicense : CreativeCommonsLicenseWithoutImage, ILicenseWithImage
	{

		private CreativeCommonsLicense()
		{

		}

		public CreativeCommonsLicense(bool attributionRequired, bool commercialUseAllowed, DerivativeRules derivativeRule)
			: base(attributionRequired, commercialUseAllowed, derivativeRule, kDefaultVersion)
		{
		}

		public CreativeCommonsLicense(bool attributionRequired, bool commercialUseAllowed, DerivativeRules derivativeRule, string version)
			: base(attributionRequired, commercialUseAllowed, derivativeRule, version)
		{
		}

		public new static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("license") && properties["license"].Contains("creativecommons"))
				return CreativeCommonsLicense.FromMetadata(properties);

			if (properties.ContainsKey("rights (en)"))
				return CustomLicense.FromMetadata(properties);
			return new NullLicense();
		}

		public new static CreativeCommonsLicense FromLicenseUrl(string url)
		{
			if(url==null || url.Trim()=="")
			{
				throw new ArgumentOutOfRangeException();
			}
			var l = new CreativeCommonsLicense();
			l.Url = url;
			return l;
		}

		public Image GetImage()
		{
			if (AttributionRequired && CommercialUseAllowed && DerivativeRule == DerivativeRules.Derivatives)
				return LicenseLogos.by;

			if (AttributionRequired && CommercialUseAllowed && DerivativeRule == DerivativeRules.NoDerivatives)
				return LicenseLogos.by_nd;

			if (AttributionRequired && CommercialUseAllowed && DerivativeRule == DerivativeRules.DerivativesWithShareAndShareAlike)
				return LicenseLogos.by_sa;

			if (AttributionRequired && !CommercialUseAllowed && DerivativeRule == DerivativeRules.NoDerivatives)
				return LicenseLogos.by_nc_nd;

			if (AttributionRequired && !CommercialUseAllowed && DerivativeRule == DerivativeRules.Derivatives)
				return LicenseLogos.by_nc;

			if (AttributionRequired && !CommercialUseAllowed && DerivativeRule == DerivativeRules.DerivativesWithShareAndShareAlike)
				return LicenseLogos.by_nc_sa;

			return LicenseLogos.cc0;
		}
	}
}