using System.Drawing;
using SIL.Core.ClearShare;
using System.Collections.Generic;
using L10NSharp;

namespace SIL.Windows.Forms.ClearShare
{
	/// <summary>
	/// Describes a single license, under which many works can be licensed for use
	/// </summary>
	public class LicenseWithLogo : License
	{
		public Image Logo { get; private set; }

		protected virtual string GetBestLicenseTranslation(string idSuffix, string englishText, string comment,
			IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			idSuffix = "MetadataDisplay.Licenses." + idSuffix;
			foreach (var targetLanguage in languagePriorityIds)
			{
				if (targetLanguage == "en")
				{
					//do the query to make sure the string is there to be translated someday
					LocalizationManager.GetDynamicString("Palaso", idSuffix, englishText, comment);
					idOfLanguageUsed = "en";
					return englishText;
				}
				//otherwise, see if we have a translation
				if (LocalizationManager.GetIsStringAvailableForLangId(idSuffix, targetLanguage))
				{
					idOfLanguageUsed = targetLanguage;
					return LocalizationManager.GetDynamicStringOrEnglish("Palaso", idSuffix, englishText, comment, targetLanguage);
				}
			}
			idOfLanguageUsed = string.Empty;
			return "[Missing translation for " + idSuffix + "]";
		}

		//Review (JH asks in Oct 2016): Why does this exist? The only uses in libpalaso are in tests and examples. Bloom does not use it.
		// Review Ariel June 2025: CreativeCommonsLicenseWithImage.FromToken is used in FieldWorks and in Bloom, but LicenseWithLogo.FromToken has no uses in Bloom or anywhere in sillsdev except in libpalaso tests & examples.
		public static LicenseInfo FromToken(string abbr)
		{
			switch (abbr)
			{
				case "ask": return new NullLicense();
				case "custom": return new CustomLicenseWithImage();
				default:
					return CreativeCommonsLicenseWithImage.FromToken(abbr);
			}
		}

		/// ------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			unchecked
			{
				int result = base.GetHashCode();
				result = (result * 397) ^ (Logo != null ? Logo.GetHashCode() : 0);
				return result;
			}
		}

		/// ------------------------------------------------------------------------------------
		public override bool Equals(object other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (GetType() != other.GetType())
				return false;

			return AreContentsEqual(other as License);
		}
	}
}
