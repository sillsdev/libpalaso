using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

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

		private CreativeCommonsLicense()
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
			:this(attributionRequired, commercialUseAllowed, derivativeRule, kDefaultVersion)
		{
		}

		public override string Url
		{
			get
			{
				return MakeUrlFromParts(Token, Version, _qualifier);
			}
			set
			{
				if(value!=Url)
				{
					HasChanges = true;
				}
				CommercialUseAllowed = true;
				DerivativeRule = DerivativeRules.Derivatives;
				AttributionRequired = false;

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

				string version;
				try
				{
					version = parts[5];
				}
				catch (IndexOutOfRangeException)
				{
					// We had a problem with some urls getting saved without a version.
					// We fixed the save problem, but now we need to handle that bad data.
					// See http://issues.bloomlibrary.org/youtrack/issue/BL-4108.
					version = kDefaultVersion;
				}

				//just a sanity check on the version
				decimal result;
				if (decimal.TryParse(version, NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"), out result))
					Version = version;

				if(parts.Length > 6)
					_qualifier = parts[6].ToLowerInvariant().Trim();
			}

		}

		private static string MakeUrlFromParts(string token, string version, string qualifier)
		{
			if(token == "cc0")
			{
				// this one is weird in a couple ways, including that it doesn't have /licenses/ in the path
				return "http://creativecommons.org/publicdomain/zero/" + version +"/";
			}

			var url = token + "/";
			if (token.StartsWith("cc-"))
				url = url.Substring("cc-".Length); // don't want this as part of URL.

			if (!string.IsNullOrEmpty(version))
				url += version + "/";

			if (!string.IsNullOrWhiteSpace(qualifier))
				url += qualifier + "/"; // e.g, igo as in https://creativecommons.org/licenses/by/3.0/igo/

			return "http://creativecommons.org/licenses/" + url;
		}


		/// <summary>
		/// A string form used for serialization
		/// </summary>
		/// <remarks>
		/// REVIEW: (asked by Hatton Oct 2016) Serialization by whom? Why not just use the url, which is the canonical form?
		/// Note that this does not include any qualifier (of which at the moment the one one is "igo", but who knows
		/// what the future holds.
		///</remarks>
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
					token = "cc0"; // public domain
				return token;
			}
		}

		/// <summary>
		/// A compact form of of this license that doesn't introduce any new text (though the license may itself have text)
		/// E.g. CC BY-NC
		/// </summary>
		public override string GetMinimalFormForCredits(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			idOfLanguageUsed = "*";

			var form = "CC ";
			if (AttributionRequired)
				form += "BY-";
			if (!CommercialUseAllowed)
				form += "NC-";
			switch (DerivativeRule)
			{
				case DerivativeRules.NoDerivatives:
					form += "ND";
					break;
				case DerivativeRules.DerivativesWithShareAndShareAlike:
					form += "SA";
					break;
				case DerivativeRules.Derivatives:
					break;
				default:
					throw new ArgumentOutOfRangeException("derivativeRule");
			}
			form = form.TrimEnd(new char[] { '-', ' ' });

			var additionalRights = (RightsStatement != null ? ". " + RightsStatement : "");
			return (form + " " + (IntergovernmentalOriganizationQualifier ? "IGO " : "") + Version + additionalRights).Trim();
			;
		}

		/// <summary>
		/// Get a simple, non-legal summary of the license, using the "best" language for which we can find a translation.
		/// </summary>
		/// <param name="languagePriorityIds"></param>
		/// <param name="idOfLanguageUsed">The id of the language we were able to use. Unreliable if we had to use a mix of languages.</param>
		/// <returns>The description of the license.</returns>
		public override string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			//Enhance labs.creativecommons.org has a lot of code, some of which might be useful, especially if we wanted a full, rather than concise, description.

			//Note that this isn't going to be able to convey to the caller the situation if some strings are translatable in some language, but others in some other language.
			//It will just end up being an amalgam in that case.

			//This IGO qualifier thing is new, and I'm not clear how I want to convey it on the page.
			//We could introduce text like "For more information, see", but the price of needing new
			// localizations at this point is somewhat daunting... for now, it seems enough that the "/igo/" is shown in the URL, if
			//it is in use in this license.

			string s= Url + System.Environment.NewLine;

			if(!AttributionRequired)
			{
				return GetComponentOfLicenseInBestLanguage("PublicDomain", "You can copy, modify, and distribute this work, even for commercial purposes, all without asking permission.", languagePriorityIds, out idOfLanguageUsed) + " ";
			}

			if (CommercialUseAllowed)
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

			return LicenseLogos.cc0;
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

		// For information on this qualifier, see https://wiki.creativecommons.org/wiki/Intergovernmental_Organizations
		private string _qualifier = null;
		public bool IntergovernmentalOriganizationQualifier
		{
			get { return _qualifier == "igo"; }
			set
			{
				var newValue = value ? "igo" : null;
				if (newValue != _qualifier)
				{
					HasChanges = true;
					if (!value)
					{
						_version = kDefaultVersion; // Undo the switch to 3.0 below
					}
				}
				_qualifier = newValue;
				if(value)
				{
					_version = "3.0";// as of November 2016, igo only had a 3.0 version, while normal cc licenses were up to 4.0
				}
			}
		}

	}
}