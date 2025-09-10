using System;
using System.Drawing;
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

		public static LicenseInfo FromToken(string token)
		{
			var result = new CreativeCommonsLicense();
			// Note (JH): Since version was set to default, as I add the qualifier, I'm going to let it be default as well.
			result.Url = MakeUrlFromParts(token, kDefaultVersion, null);
			return result;
		}

		// New implementation in order to return a CreativeCommonsLicense
		// instead of CreativeCommonsLicenseWithoutImage
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