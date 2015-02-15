using System;
using System.Collections.Generic;
using System.Drawing;

namespace SIL.Windows.Forms.ClearShare
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
			return Token;  //by-nc-sa
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

		public const string kDefaultVersion = "4.0";

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
		/// <param name="token"></param>
		/// <returns></returns>
		public static LicenseInfo FromToken(string token)
		{
			var result = new CreativeCommonsLicense();
			result.Url = MakeUrlFromTokenAndVersion(token, kDefaultVersion);
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
				return MakeUrlFromTokenAndVersion(Token, Version);
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

		private static string MakeUrlFromTokenAndVersion(string token, string version)
		{
			var url = token + "/";
			if (token.StartsWith("cc-"))
				url = url.Substring("cc-".Length); // don't want this as part of URL.

			if (!string.IsNullOrEmpty(version))
				url += version + "/";
			return "http://creativecommons.org/licenses/" + url;
		}


		/// <summary>
		/// A string form used for serialization
		/// </summary>
		public override string Token
		{
			get
			{
				var token = "cc-";
				if (AttributionRequired)
					token += "by-";
				if (!CommercialUseAllowed)
					token += "nc-";
				switch (DerivativeRule)
				{
					case DerivativeRules.NoDerivatives:
						token += "nd";
						break;
					case DerivativeRules.DerivativesWithShareAndShareAlike:
						token += "sa";
						break;
					case DerivativeRules.Derivatives:
						break;
					default:
						throw new ArgumentOutOfRangeException("derivativeRule");
				}
				token = token.TrimEnd(new char[] {'-'});
				if (token == "cc")
					token = "srr"; //some rights reserved
				return token;
			}
		}

		/// <summary>
		/// Get a simple, non-legal summary of the license, using the "best" language for which we can find a translation.
		/// </summary>
		/// <param name="languagePriorityIds"></param>
		/// <param name="idOfLanguageUsed">The id of the language we were able to use. Unreliable if we had to use a mix of languages.</param>
		/// <returns>The description of the license.</returns>
		public override string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			//Enanced labs.creativecommons.org has a lot of code, some of which might be useful, especially if we wanted a full, rather than consise, description.
			
			//Note that this isn't going to be able to convey to the caller the situation if some strings are translatable in some language, but others in some other language.
			//It will just end up being an amalgam in that case.
			string s="";

			if(CommercialUseAllowed)
				s += GetComponentOfLicenseInBestLanguage("CommercialUseAllowed", "You are free to make commercial use of this work.", languagePriorityIds, out idOfLanguageUsed) + " ";
			else
				s += GetComponentOfLicenseInBestLanguage("NonCommercial", "You may not use this work for commercial purposes.", languagePriorityIds, out idOfLanguageUsed) + " ";

			if(DerivativeRule == DerivativeRules.Derivatives)
				s += GetComponentOfLicenseInBestLanguage("Derivatives", "You may adapt and add to this work.", languagePriorityIds, out idOfLanguageUsed) + " ";

			if (DerivativeRule == DerivativeRules.NoDerivatives)
				s += GetComponentOfLicenseInBestLanguage("NoDerivatives", "You may not make changes or build upon this work without permission.", languagePriorityIds, out idOfLanguageUsed) + " ";

			if (DerivativeRule == DerivativeRules.DerivativesWithShareAndShareAlike)
				s += GetComponentOfLicenseInBestLanguage("DerivativesWithShareAndShareAlike", "You may adapt and add to this work, but you may distribute the resulting work only under the same or similar license to this one.", languagePriorityIds, out idOfLanguageUsed) + " ";

			if (AttributionRequired)
				s += GetComponentOfLicenseInBestLanguage("AttributionRequired", "You must keep the copyright and credits for authors, illustrators, etc.", languagePriorityIds, out idOfLanguageUsed) + " ";

			return s.Trim();
		}

		private string GetComponentOfLicenseInBestLanguage(string idSuffix, string englishText, IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			const string comment = "See http://creativecommons.org/ to find out how this is normally translated in your language. What we're aiming for here is an easy to understand summary.";

			//Note: GetBestLicenseTranslation will prepend "MetadataDisplay.Licenses.", we should not include it here
			return GetBestLicenseTranslation("CreativeCommons." + idSuffix, englishText, comment, languagePriorityIds, out idOfLanguageUsed);
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