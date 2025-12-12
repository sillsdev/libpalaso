using System;
using System.Collections.Generic;
using System.Drawing;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	/// <summary>
	/// Represents a Creative Commons license with WindowsForms-specific features,
	/// extending <see cref="SIL.Core.ClearShare.CreativeCommonsLicenseInfo"/> by adding support for
	/// associated license images (e.g., CC BY, CC BY-SA, CC0).
	/// </summary>
	/// <remarks>
	/// This class provides constructors for creating licenses from explicit
	/// Creative Commons terms (attribution, commercial use, derivative rules, and
	/// version), as well as factory methods for building a license from a token,
	/// metadata properties, or a full license URL.
	/// <para>
	/// Unlike the platform-neutral <see cref="SIL.Core.ClearShare.CreativeCommonsLicenseInfo"/>,
	/// this class includes UI-related functionality such as returning the
	/// appropriate license logo via <see cref="GetImage"/>.
	/// </para>
	/// </remarks>
	public class CreativeCommonsLicense : CreativeCommonsLicenseInfo, ILicenseWithImage
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

		/// <remarks>
		/// at the moment, we only use the license url, but in future we could add other custom provisions, like "ok to crop" (if they're allowed by cc?)
		/// </remarks>
		public static LicenseInfo FromMetadata(Dictionary<string, string> metadataProperties)
		{
			if (!metadataProperties.ContainsKey("license"))
				throw new ApplicationException("A license property is required in order to make a Creative Commons License from metadata.");

			var result = FromLicenseUrl(metadataProperties["license"]);
			string rights;
			if (metadataProperties.TryGetValue("rights (en)", out rights))
				result.RightsStatement = rights;
			return result;
		}

		// New implementation in order to return a CreativeCommonsLicense
		// instead of CreativeCommonsLicenseInfo
		public static CreativeCommonsLicense FromLicenseUrl(string url)
		{
			if (url==null || url.Trim()=="")
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