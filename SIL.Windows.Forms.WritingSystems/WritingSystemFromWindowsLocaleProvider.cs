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
	/// This class gets a writing system for each installed input language.
	/// </summary>
	public class WritingSystemFromWindowsLocaleProvider : IEnumerable<WritingSystemDefinition>
	{
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


		private static string GetRegion(InputLanguage language) {
#if MONO // CultureAndRegionInfoBuilder not supported by Mono
			return string.Empty;
#else
			//  http://jira.palaso.org/issues/browse/WS-34216 has KonKani, which is a "macro language", as causing a crash here, on computers
			// with that locale installed (India). It dies when it looks like we're trying to make a new versionf of it, becuase the
			// CultureAndRegionModifiers flag here is "none". Someone in ChiangMai please review: should this be changed to "Replacement"
			// to avoid this?  Do we need an
			try
			{
				//REVIEW: if we changed the "none" to "Replacement", that would presumably get us past the KanKani crash, but is that the right
				//thing to do, or a hack which would mean we lose region information on other languages?
				var b = new CultureAndRegionInfoBuilder(language.Culture.ThreeLetterISOLanguageName, CultureAndRegionModifiers.None);
				b.LoadDataFromCultureInfo(language.Culture);
				return b.TwoLetterISORegionName ?? String.Empty;
			}
			catch (Exception)
			{
				Debug.Fail("This is a bug (http://jira.palaso.org/issues/browse/WS-34216 ) we would like to look into on a developer machine");
				return string.Empty;
			}
#endif
		}

		#region Implementation of IEnumerable

		public IEnumerator<WritingSystemDefinition> GetEnumerator()
		{
			IEnumerable<WritingSystemDefinition> defs = GetLanguageAndKeyboardCombinations();
			//now just return the unique ones (Works because no keyboard in the rfc4646)
			IEnumerable<WritingSystemDefinition> unique = defs.GroupBy(d => d.IetfLanguageTag)
				.Select(g => g.First());
			return unique.GetEnumerator();
		}

		private IEnumerable<WritingSystemDefinition> GetLanguageAndKeyboardCombinations()
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

				var def = new WritingSystemDefinition(
					cleaner.Language,
					cleaner.Script,
					cleaner.Region,
					IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse(cleaner.Variant, cleaner.PrivateUse),
					culture.ThreeLetterISOLanguageName,
					culture.TextInfo.IsRightToLeft);
				def.Keyboard = language.LayoutName;
				def.DefaultCollation = new InheritedCollationDefinition("standard") { BaseIetfLanguageTag = culture.IetfLanguageTag };
				def.DefaultFontSize = 12;

				yield return def;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
