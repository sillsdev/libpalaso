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

		//        public static Palaso.WritingSystems.WritingSystemDefinition Get(string rfc4646)
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


		private static string GetRegion(InputLanguage language)
		{
#if MONO // CultureAndRegionInfoBuilder not supported by Mono
			return string.Empty;
#else
			//  http://jira.palaso.org/issues/browse/WS-34216 has KonKani, which is a "macro language", as causing a crash here, on computers
			// with that locale installed (India). It dies when it looks like we're trying to make a new version of it, because the
			// CultureAndRegionModifiers flag here is "none". Someone in ChiangMai please review: should this be changed to "Replacement"
			// to avoid this?  Do we need an
			try
			{
				if (language.Culture.ThreeLetterISOLanguageName == "und") // SIL IPA keyboard is language "und" which causes an exception
				{
					return string.Empty;
				}
				try
				{
					// If the region is in the input language name then just use that
					var r = new RegionInfo(language.Culture.Name);
					return r.TwoLetterISORegionName ?? String.Empty;
				}
				catch (Exception)
				{
					//REVIEW: if we changed the "none" to "Replacement", that would presumably get us past the KanKani crash, but is that the right
					//thing to do, or a hack which would mean we lose region information on other languages?
					var b = new CultureAndRegionInfoBuilder(language.Culture.Name, CultureAndRegionModifiers.None);
					b.LoadDataFromCultureInfo(language.Culture);
					return b.TwoLetterISORegionName ?? String.Empty;
				}
			}
			catch (Exception)
			{
				Debug.Fail("This is a bug (http://jira.palaso.org/issues/browse/WS-34216 ) we would like to look into on a developer machine");
				return string.Empty;
			}
#endif
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
					region = GetRegion(language);
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
