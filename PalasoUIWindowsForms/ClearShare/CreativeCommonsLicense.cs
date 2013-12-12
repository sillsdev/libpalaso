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

		private bool _attributionRequired;
		public bool AttributionRequired
		{
			get { return _attributionRequired; }
			set
			{
				if (value != _attributionRequired)
				{
					HasChanges = true;
				}
				_attributionRequired = value;
			}
		}

		public override string ToString()
		{
			return Abbreviation;  //by-nc-sa
		}

		private bool _commercialUseAllowed;
		public bool CommercialUseAllowed
		{
			get { return _commercialUseAllowed; }
			set
			{
				if (value != _commercialUseAllowed)
				{
					HasChanges = true;
				}
				_commercialUseAllowed = value;
			}
		}

		private DerivativeRules _derivativeRule;
		public DerivativeRules DerivativeRule
		{
			get { return _derivativeRule; }
			set
			{
				if (value != _derivativeRule)
				{
					HasChanges = true;
				}
				_derivativeRule = value;
			}
		}

		public const string kDefaultVersion = "3.0";

		/// <summary>
		/// at the moment, we only use the license url, but in future we could add other custom provisions, like "ok to crop" (if they're allowed by cc?)
		/// </summary>
		public static LicenseInfo FromMetadata(Dictionary<string, string> metadataProperties)
		{
			if(!metadataProperties.ContainsKey("license"))
				throw new ApplicationException("A license property is required in order to make a Creative Commons License from metadata.");

			return FromLicenseUrl(metadataProperties["license"]);
		}

		/// <summary>
		/// Create a CCL from a string produced by the Abbreviation method of a CCL.
		/// It will have the current default Version.
		/// Other values are not guaranteed to work, though at present they will.
		/// Enhance: Possibly we should try to verify that the abbreviation is a valid CCL one?
		/// </summary>
		/// <param name="abbr"></param>
		/// <returns></returns>
		public static LicenseInfo FromAbbreviation(string abbr)
		{
			var result = new CreativeCommonsLicense();
			result.Url = AssembleUrl(abbr, kDefaultVersion);
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
				return AssembleUrl(Abbreviation, Version);
			}
			set
			{
				if(value!=Url)
				{
					HasChanges = true;
				}
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

		private static string AssembleUrl(string abbreviation, string version)
		{
			var url = abbreviation + "/";

			if (!string.IsNullOrEmpty(version))
				url += version + "/";
			return "http://creativecommons.org/licenses/" + url;
		}

		public override string Abbreviation
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
				return url;
			}
		}

		//we'll need to give out an image, description, url.
		//what you *store* in the image metadata is a different question.
		public override string GetDescription(string iso639_3LanguageCode)
		{
			//Enanced labs.creativecommons.org has a lot of code, some of which might be useful, especially if we wanted a full, rather than consise, description.

			string s="";

			if(CommercialUseAllowed)
				s+="You are free to make commercial use of this work. ";
			else
				s += "You may not use this work for commercial purposes. ";

			if(DerivativeRule == DerivativeRules.Derivatives)
				s += "You are free to adapt, remix, copy, distribute, and transmit this work. ";

			if (DerivativeRule == DerivativeRules.NoDerivatives)
				s += "You may not alter, transform, or build upon this work without permission. ";

			if (DerivativeRule == DerivativeRules.DerivativesWithShareAndShareAlike)
				s += "You may adapt or build upon this work, but you may distribute the resulting work only under the same or similar license to this one.";

			if (AttributionRequired)
				s += "You must attribute the work in the manner specified by the author. ";

			return s;
		}

		public override Image GetImage()
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

			return LicenseLogos.srr;
		}

		public override bool EditingAllowed
		{
			get { return false; }
		}

		private string _version;
		public string Version
		{
			get { return _version; }
			set
			{
				if (value != _version)
				{
					HasChanges = true;
				}
				_version = value;
			}
		}
	}
}