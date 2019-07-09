using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SIL.WritingSystems;
using SIL.WritingSystems.Migration;

namespace SIL.Windows.Forms.WritingSystems
{
	/// <summary>
	/// This class gets a combination of writing system label and keyboard layout for each installed input language.
	/// </summary>
	public class WritingSystemFromWindowsLocaleProvider : IEnumerable<Tuple<string, string>>
	{
		private readonly IWritingSystemFactory _writingSystemFactory;

		public WritingSystemFromWindowsLocaleProvider(IWritingSystemFactory writingSystemFactory)
		{
			_writingSystemFactory = writingSystemFactory;
		}

		//        public static SIL.WritingSystems.WritingSystemDefinition Get(string rfc4646)
		//        {
		//            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
		//            {
		//                Debug.WriteLine(string.Format("{0} {1}", language.Culture.Name, language.Culture.LCID));
		//            }
		//
		//            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
		//            {
		//                if (language.Culture.LCID.ToString().ToLower() == rfc4646.ToLower())
		//                {
		//                    WritingSystemDefinition def =
		//                        new WritingSystemDefinition(language.Culture.ThreeLetterISOLanguageName, "", "", "",
		//                                                    language.Culture.EnglishName, "");
		//                    return def;
		//                }
		//            }
		//            return null;
		//        }

		private static LanguageLookup _languageLookup;
		private static LanguageLookup LanguageLookup => _languageLookup ?? (_languageLookup = new LanguageLookup());

		protected static string GetRegion(string cultureName)
		{
			try
			{
				if (cultureName == "und") // SIL IPA keyboard is language "und" (undetermined) which causes an exception
				{
					return string.Empty;
				}
				try
				{
					// If the region is in the input language name then just use that
					var r = new RegionInfo(cultureName);
					return r.TwoLetterISORegionName;
				}
				catch (Exception)
				{
					// Otherwise return primary country for that language
					// ENHANCE: This would be more efficient if LanguageLookup would store the country code
					// instead of the country name
					var language = LanguageLookup.GetLanguageFromCode(cultureName);
					var regionName = language?.PrimaryCountry;
					return StandardSubtags.RegisteredRegions.FirstOrDefault(subtag =>
						subtag.Name == regionName)?.Code ?? string.Empty;
				}
			}
			catch (Exception)
			{
				Debug.Fail($"This is a bug (http://jira.palaso.org/issues/browse/WS-34216) we would like to look into on a developer machine (culture {cultureName})");
				return string.Empty;
			}
		}

		#region Implementation of IEnumerable

		public IEnumerator<Tuple<string, string>> GetEnumerator()
		{
			IEnumerable<Tuple<string, string>> combos = GetLanguageAndKeyboardCombinations();
			//now just return the unique ones (Works because no keyboard in the language tag)
			IEnumerable<Tuple<string, string>> unique = combos.GroupBy(c => c.Item1)
				.Select(g => g.First());
			return unique.GetEnumerator();
		}

		private IEnumerable<Tuple<string, string>> GetLanguageAndKeyboardCombinations()
		{
			foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
			{
				CultureInfo culture;
				try
				{
					//http://www.wesay.org/issues/browse/WS-34598
					//Oddly enough, this can throw. It seems like it might have to do with a badly applied .Net patch
					//http://www.ironspeed.com/Designer/3.2.4/WebHelp/Part_VI/Culture_ID__XXX__is_not_a_supported_culture.htm and others
					culture = language.Culture;
				}
				catch (CultureNotFoundException)
				{
					continue;
				}
				if (culture.EnglishName.StartsWith("Invariant"))
				{
					continue;
				}
				string region = string.Empty;
				if (Environment.OSVersion.Platform != PlatformID.Unix)
				{
					region = GetRegion(language.Culture.Name);
				}

				var cleaner = new IetfLanguageTagCleaner(culture.TwoLetterISOLanguageName, "", region, "", "");
				cleaner.Clean();

				yield return Tuple.Create(cleaner.GetCompleteTag(), language.LayoutName);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
