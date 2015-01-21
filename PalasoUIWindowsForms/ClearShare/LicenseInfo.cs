using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using L10NSharp;

namespace Palaso.UI.WindowsForms.ClearShare
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

		public abstract string GetDescription(string iso6393LanguageCode);
		public abstract string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed);

		/// <summary>
		/// A string that is a good short indication of the license type, and can be used in FromToken.
		/// </summary>
		public abstract string Token { get; }

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
		/// custom or extra rights. Note that accoring to Creative Commons, "extra" rights are expressly dissallowed by section 7 a:
		///     http://creativecommons.org/licenses/by-nc/4.0/legalcode#s7a
		///     "The Licensor shall not be bound by any additional or different terms or conditions communicated by You unless expressly agreed."
		///
		/// However, consider the case of one application that uses this library, Bloom. A significant portion of the material it is trying to
		/// help people license is restricted to the country of origin. Niether CC nor anyone else is going to allow for that, so we're
		/// allowing people to express that restriction in this field, but the UI also makes it clear that these are
		/// not legally enforceable if they are choosing a CC license. While not legally enforcable, they are not worthless, as they define
		/// what is ethical. We expect that the vast majority of people are going to abide by them.
		/// </summary>
		public string RightsStatement {get; set; }
	}

	public class NullLicense : LicenseInfo
	{
		/// <summary>
		/// Get a simple, non-legal summary of the license, using the "best" language for which we can find a translation.
		/// </summary>
		/// <param name="iso6393LanguageCode">A single language to try and use for the description.</param>
		/// <returns>The description of the license if we have it in this language, else the English</returns>
		public override string GetDescription(string iso6393LanguageCode)
		{
			string idOfLanguageUsed;
			return GetDescription(new string[] {iso6393LanguageCode, "en"}, out idOfLanguageUsed);
		}

		/// <summary>
		/// Get a simple, non-legal summary of the license, using the "best" language for which we can find a translation.
		/// </summary>
		/// <param name="languagePriorityIds"></param>
		/// <param name="idOfLanguageUsed">The id of the language we were able to use. Unreliable if we had to use a mix of languages.</param>
		/// <returns>The description of the license.</returns>
		public override string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			var id = "MetadataDisplay.Licenses.NullLicense";
			var englishText = "For permission to reuse, contact the copyright holder.";
			var comment = "This is used when all we have is a copyright, no other license.";
			foreach(var targetLanguage in languagePriorityIds)
			{
				if(targetLanguage == "en")
				{
					//do the query to make sure the string is there to be translated someday
					var unused = LocalizationManager.GetDynamicString("Palaso", id, englishText, comment);
					idOfLanguageUsed = "en";
					return englishText;
				}
				if(LocalizationManager.GetIsStringAvailableForLangId(id, targetLanguage))
				{
					idOfLanguageUsed = targetLanguage;
					
					return LocalizationManager.GetDynamicStringOrEnglish("Palaso", id, englishText, comment, targetLanguage);
				}
			}
			idOfLanguageUsed = string.Empty;
			return "[Missing translation for " + id + "]";
		}

		public override string Token
		{
			//do not think of changing this, there is data out there that could get messed up
			get { return "ask"; }
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

		/// <summary>
		/// Currently, we don't even know the language of custom license strings, so this implementation just gives you the string it has
		/// </summary>
		/// <returns></returns>
		public override string GetDescription(string iso6393LanguageCode)
		{
			if (string.IsNullOrEmpty(RightsStatement))
				return "For permission to reuse, contact the copyright holder.";
			return "";
		}

		/// <summary>
		/// Currently, we don't even know the language of custom license strings, so this implementation just pretends that it gave you the language you asked for.
		/// </summary>
		/// <param name="languagePriorityIds"></param>
		/// <param name="idOfLanguageUsed"></param>
		/// <returns></returns>
		public override string GetDescription(IEnumerable<string> languagePriorityIds, out string idOfLanguageUsed)
		{
			idOfLanguageUsed = languagePriorityIds.First();
			return GetDescription(idOfLanguageUsed);
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
			get { return false; }//it may be ok, but we can't read the description.
		}

		public override string Url { get; set; }

		public static LicenseInfo FromMetadata(Dictionary<string, string> properties)
		{
			 if(!properties.ContainsKey("rights (en)"))
				throw new ApplicationException("A license property is required in order to make a  Custom License from metadata.");

			var license = new CustomLicense();
			license.RightsStatement = properties["rights (en)"];
			return license;
		}
	}


}