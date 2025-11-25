using System;
using System.Collections.Generic;

namespace SIL.Core.ClearShare
{
	/// <remarks>
	/// CustomLicenseBare is a WindowsForms-free base version of the CustomLicense class. In order to be WindowsForms independent,
	/// it does not include information about any license images (for example, the "0 Public Domain" or "CC BY" images used by creative commons licenses).
	/// To include license images, use the CustomLicense class in SIL.WindowsForms.Clearshare.
	/// </remarks>
	public class CustomLicenseInfo : LicenseInfo
	{
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
		/// Currently, we don't know the language of custom license strings, so we use the ISO 639-2 code for undetermined, "und"
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
			//I hereby coin "Zook's First Law": Eventually any string entered by a user will wish it had been tagged with a language identifier
			//"Zook's Second Law" can be: Eventually any string entered by a user will wish it was a multi-string (multiple (language,value) pairs)
			idOfLanguageUsed = "und";
			return RightsStatement;
		}

		public override string Token =>
			//do not think of changing this, there is data out there that could get messed up
			"custom";

		public override string Url { get; set; }

		public static LicenseInfo CustomLicenseInfoFromMetadata(Dictionary<string, string> properties)
		{
			if (!properties.ContainsKey("rights (en)"))
				throw new ApplicationException("A license property is required in order to make a Custom License from metadata.");

			var license = new CustomLicenseInfo();
			license.RightsStatement = properties["rights (en)"];
			return license;
		}
	}
}