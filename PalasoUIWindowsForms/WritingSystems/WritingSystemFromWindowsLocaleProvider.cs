using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using Palaso.WritingSystems;
using System.Linq;
using Palaso.WritingSystems.Migration;

namespace Palaso.UI.WindowsForms.WritingSystems
{
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
				System.Globalization.CultureAndRegionInfoBuilder b = new CultureAndRegionInfoBuilder(language.Culture.ThreeLetterISOLanguageName, CultureAndRegionModifiers.None);
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
			var defs = GetLanguageAndKeyboardCombinations();
			//now just return the unique ones (Works because no keyboard in the rfc4646)
			var unique= defs.GroupBy(d => d.Bcp47Tag)
				.Select(g => g.First());
			return unique.GetEnumerator();
		}

		private IEnumerable<WritingSystemDefinition> GetLanguageAndKeyboardCombinations()
		{
			foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
			{
				if (language.Culture.EnglishName.StartsWith("Invariant"))
				{
					continue;
				}
				string region = string.Empty;
				if (Environment.OSVersion.Platform != PlatformID.Unix)
				{
					region = GetRegion(language);
				}


				var name = TrimOffCountryNameOfMajorLanguage(language);

				var cleaner = new Rfc5646TagCleaner(language.Culture.TwoLetterISOLanguageName, "", region, "", "");
				cleaner.Clean();

				var def = new WritingSystemDefinition(
					cleaner.Language,
					cleaner.Script,
					cleaner.Region,
					WritingSystemDefinition.ConcatenateVariantAndPrivateUse(cleaner.Variant, cleaner.PrivateUse),
					language.Culture.ThreeLetterISOLanguageName,
					language.Culture.TextInfo.IsRightToLeft);
				def.NativeName = language.Culture.NativeName;
				def.Keyboard = language.LayoutName;
				def.SortUsing = WritingSystemDefinition.SortRulesType.OtherLanguage;
				def.SortRules = language.Culture.IetfLanguageTag;
				def.DefaultFontSize = 12;
				def.LanguageName = language.Culture.DisplayName;

				yield return def;
			}
		}

		/// <summary>
		/// It's confusing for people to be presented with "English (UnitedStates)" or Icelandic (Iceland)
		/// </summary>
		private string TrimOffCountryNameOfMajorLanguage(InputLanguage language)
		{
			var name = language.Culture.EnglishName;
			if (name.StartsWith("English"))
				name = "English";
			return name;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
