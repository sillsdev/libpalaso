using System;
using System.Collections.Generic;
using System.Drawing;
using L10NSharp;

namespace SIL.Windows.Forms.ClearShare
{
	/// <summary>
	/// !!!!!!!!!!!!!! To keep from losing edits (having the owning metaccess not know that changes should be saved) these need to be immutable
	/// </summary>
	public abstract class LicenseInfo
	{
		public static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if(properties.ContainsKey("license") && properties["license"].Contains("creativecommons"))
			{
				return CreativeCommonsLicense.FromMetadata(properties);
			}
			else if ( properties.ContainsKey("rights (en)"))
			{
				return CustomLicense.FromMetadata(properties);
			}
			return new NullLicense();
		}

		/// <summary>
		/// A compact form of of this license that doesn't introduce any new text (though the license may itself have text)
		/// E.g. "CC BY-NC"
		/// </summary>
		public abstract string GetMinimalFormForCredits(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed);

		public abstract string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed);

		/// <summary>
		/// A string that is a good short indication of the license type, and can be used in FromToken.
		/// </summary>
		public abstract string Token { get; }

		//Review (JH asks in Oct 2016): Why does this exist? The only uses in libpalaso are in tests and examples. Bloom does not use it.
		// Why is From Url not sufficient?
		public static LicenseInfo FromToken(string abbr)
		{
			switch (abbr)
			{
				case "ask": return new NullLicense();
				case "custom": return new CustomLicense();
				default:
					return CreativeCommonsLicense.FromToken(abbr);
			}
		}

		virtual public Image GetImage()
		{
			return null;
		}

		/// <summary>
		/// It doesn't make sense to let the user edit the description of a well-known license, even if the meta data is unlocked.
		/// </summary>
		public virtual bool EditingAllowed
		{
			get { return false; }//we don't konw
		}

		public abstract string Url { get; set; }

		public bool HasChanges { get; set; }

		/// <summary>
		/// custom or extra rights. Note that according to Creative Commons, "extra" rights are expressly disallowed by section 7 a:
		///     http://creativecommons.org/licenses/by-nc/4.0/legalcode#s7a
		///     "The Licensor shall not be bound by any additional or different terms or conditions communicated by You unless expressly agreed."
		///
		/// However, consider the case of one application that uses this library, Bloom. A significant portion of the material it is trying to
		/// help people license is restricted to the country of origin. Neither CC nor anyone else is going to allow for that, so we're
		/// allowing people to express that restriction in this field, but the UI also makes it clear that these are
		/// not legally enforceable if they are choosing a CC license. While not legally enforceable, they are not worthless, as they define
		/// what is ethical. We expect that the vast majority of people are going to abide by them.
		/// </summary>
		public string RightsStatement {get; set; }

		protected virtual string GetBestLicenseTranslation(string idSuffix, string englishText, string comment,
			IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			idSuffix = "MetadataDisplay.Licenses."+idSuffix;
			foreach (var targetLanguage in languagePriorityIds)
			{
				if (targetLanguage == "en")
				{
					//do the query to make sure the string is there to be translated someday
					var unused = LocalizationManager.GetDynamicString("Palaso", idSuffix, englishText, comment);
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

	}

	public class NullLicense : LicenseInfo
	{
		/// <summary>
		/// Get a simple, non-legal summary of the license, using the "best" language for which we can find a translation.
		/// </summary>
		/// <param name="languagePriorityIds"></param>
		/// <param name="idOfLanguageUsed">The idSuffix of the language we were able to use. Unreliable if we had to use a mix of languages.</param>
		/// <returns>The description of the license.</returns>
		public override string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			const string englishText = "For permission to reuse, contact the copyright holder.";
			const string comment = "This is used when all we have is a copyright, no other license.";
			return GetBestLicenseTranslation("NullLicense", englishText, comment, languagePriorityIds, out idOfLanguageUsed);
		}

		public override string Token
		{
			//do not think of changing this, there is data out there that could get messed up
			get { return "ask"; }
		}

		public override string GetMinimalFormForCredits(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			idOfLanguageUsed = "*";
			return ""; // that is, the license is unknown. We might be tempted to return "used by permission", but... was it?
		}

		public override string ToString()
		{
			return "";
		}

		public override string Url
		{
			get { return ""; }
			set { }
		}
	}


	public class CustomLicense : LicenseInfo
	{
//        public void SetDescription(string iso639_3LanguageCode, string description)
//        {
//			RightsStatement = description;
//        }

		public override string ToString()
		{
			return "Custom License";
		}

		public override string GetMinimalFormForCredits(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			return GetDescription(languagePriorityIds, out idOfLanguageUsed);
		}

		///<summary></summary>
		/// <remarks>
		/// Currently, we don't know the language of custom license strings, so we the ISO 639-2 code for undetermined, "und"
		/// </remarks>
		/// <param name="languagePriorityIds"></param>
		/// <param name="idOfLanguageUsed"></param>
		/// <returns></returns>
		public override string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			//if we're empty, we're equivalent to a NullLicense
			if (string.IsNullOrEmpty(RightsStatement))
			{
				return new NullLicense().GetDescription(languagePriorityIds, out idOfLanguageUsed);
			}

			//We don't actually have a way of knowing what language this is, so we use "und", from http://www.loc.gov/standards/iso639-2/faq.html#25
			//I hearby coin "Zook's First Law": Eventually any string entered by a user will wish it had been tagged with a language identifier
			//"Zook's Second Law" can be: Eventually any string entered by a user will wish it was a multi-string (multiple (language,value) pairs)
			idOfLanguageUsed = "und";
			return RightsStatement;
		}

		public override string Token
		{
			//do not think of changing this, there is data out there that could get messed up
			get { return "custom"; }
		}

		public override Image GetImage()
		{
			return null;
		}

		public override bool EditingAllowed
		{
			get { return false; } //it may be ok, but we can't read the description.
		}

		public override string Url { get; set; }

		public static LicenseInfo FromMetadata(Dictionary<string, string> properties)
		{
			if (!properties.ContainsKey("rights (en)"))
				throw new ApplicationException("A license property is required in order to make a  Custom License from metadata.");

			var license = new CustomLicense();
			license.RightsStatement = properties["rights (en)"];
			return license;
		}
	}
}