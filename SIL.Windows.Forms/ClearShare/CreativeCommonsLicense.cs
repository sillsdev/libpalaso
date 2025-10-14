using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	public class CreativeCommonsLicense : CreativeCommonsLicenseBase, ILicenseWithImage
	{
		public CreativeCommonsLicense()
		{

		}

		public CreativeCommonsLicense(bool attributionRequired, bool commercialUseAllowed, DerivativeRules derivativeRule, string version)
		{
			AttributionRequired = attributionRequired;
			CommercialUseAllowed = commercialUseAllowed;
			DerivativeRule = derivativeRule;
			Version = version;
		}
		public CreativeCommonsLicense(bool attributionRequired, bool commercialUseAllowed, DerivativeRules derivativeRule)
			: this(attributionRequired, commercialUseAllowed, derivativeRule, kDefaultVersion)
		{
		}

		/// <summary>
		/// at the moment, we only use the license url, but in future we could add other custom provisions, like "ok to crop" (if they're allowed by cc?)
		/// </summary>
		public static LicenseInfo FromMetadata(Dictionary<string, string> metadataProperties)
		{
			if(!metadataProperties.ContainsKey("license"))
				throw new ApplicationException("A license property is required in order to make a Creative Commons License from metadata.");

			var result = FromLicenseUrl(metadataProperties["license"]);
			string rights;
			if (metadataProperties.TryGetValue("rights (en)", out rights))
				result.RightsStatement = rights;
			return result;
		}

		/// <summary>
		/// Create a CCL from a string produced by the Token method of a CCL.
		/// It will have the current default Version.
		/// Other values are not guaranteed to work, though at present they will.
		/// Enhance: Possibly we should try to verify that the token is a valid CCL one?
		/// </summary>
		public new static LicenseInfo FromToken(string token)
		{
			var result = new CreativeCommonsLicense();
			// Note (JH): Since version was set to default, as I add the qualifier, I'm going to let it be default as well.
			result.Url = MakeUrlFromParts(token, kDefaultVersion, null);
			return result;
		}


		public static CreativeCommonsLicense FromLicenseUrl(string url)
		{
			if(url==null || url.Trim()=="")
			{
				throw new ArgumentOutOfRangeException();
			}
			var l = new CreativeCommonsLicense();
			l.Url = url;
			return l;
		}

		public static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if (properties.ContainsKey("license") && properties["license"].Contains("creativecommons"))
				return CreativeCommonsLicense.FromMetadata(properties);

			if (properties.ContainsKey("rights (en)"))
				return CustomLicense.FromMetadata(properties);
			return new NullLicense();
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