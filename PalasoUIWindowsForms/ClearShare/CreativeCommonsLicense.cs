using System;
using System.Collections.Generic;
using System.Drawing;

namespace Palaso.UI.WindowsForms.ClearShare
{
	public class CreativeCommonsLicense : LicenseInfo
	{
		public enum DerivativeRules
		{
			NoDerivatives, DerivativesWithShareAndShareAlike, Derivatives
		}
		public bool AttributionRequired { get; set; }
		public bool CommercialUseAllowed { get; set; }
		public DerivativeRules DerivativeRule { get; set; }
		public string kDefaultVersion = "3.0";

		/// <summary>
		/// at the moment, we only use the license url, but in future we could add other custom provisions, like "ok to crop" (if they're allowed by cc?)
		/// </summary>
		public static LicenseInfo FromMetadata(Dictionary<string, string> metadataProperties)
		{
			if(!metadataProperties.ContainsKey("license"))
				throw new ApplicationException("A license property is required in order to make a Creative Commons License from metadata.");

			return FromLicenseUrl(metadataProperties["license"]);
		}


		public static CreativeCommonsLicense FromLicenseUrl(string url)
		{
			var l = new CreativeCommonsLicense();
			l.Url = url;
			return l;
		}

		private CreativeCommonsLicense()
		{

		}

		public CreativeCommonsLicense(bool attributionRequired, bool commercialUseAllowed, DerivativeRules derivativeRule)
		{
			AttributionRequired = attributionRequired;
			CommercialUseAllowed = commercialUseAllowed;
			DerivativeRule = derivativeRule;
			Version = kDefaultVersion;
		}

		public override string Url
		{
			get
			{
				var url = "";
				if (AttributionRequired)
					url += "by-";
				if (!CommercialUseAllowed)
					url += "nc-";
				switch (DerivativeRule)
				{
					case DerivativeRules.NoDerivatives:
						url += "nd";
						break;
					case DerivativeRules.DerivativesWithShareAndShareAlike:
						url += "sa";
						break;
					case DerivativeRules.Derivatives:
						break;
					default:
						throw new ArgumentOutOfRangeException("derivativeRule");
				}
				url = url.TrimEnd(new char[] {'-'});
				if (url == "")
					url = "srr"; //some rights reserved

				url += "/";

				if(!string.IsNullOrEmpty(Version))
					url+=Version+"/";
				return "http://creativecommons.org/licenses/" + url;
			}
			set
			{
				CommercialUseAllowed = true;
				DerivativeRule = DerivativeRules.Derivatives;

				if (value.Contains("by"))
					AttributionRequired = true;
				if (value.Contains("nc"))
					CommercialUseAllowed = false;
				if (value.Contains("nd"))
					DerivativeRule = DerivativeRules.NoDerivatives;
				if (value.Contains("sa"))
					DerivativeRule = DerivativeRules.DerivativesWithShareAndShareAlike;

				var urlWithoutTrailingSlash = value.TrimEnd(new char[] {'/'});
				var parts = urlWithoutTrailingSlash.Split(new char[] { '/' });
				var v=  parts[parts.Length - 1];
				decimal result;
				if (decimal.TryParse(v, out result))
					Version = v;
			}

		}

		//we'll need to give out an image, description, url.
		//what you *store* in the image metadata is a different question.
		public override string GetDescription(string iso639_3LanguageCode)
		{
			return Url;
		}

		public override Image GetImage()
		{
			if(AttributionRequired && DerivativeRule == DerivativeRules.NoDerivatives)
				return LicenseLogos.by_nd;

			return LicenseLogos.srr;
		}

		public override bool EditingAllowed
		{
			get { return false; }
		}

		public string Version { get; set; }
	}
}