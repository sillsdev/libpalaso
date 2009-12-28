using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Palaso.WritingSystems;
using System.Linq;

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
			System.Globalization.CultureAndRegionInfoBuilder b = new CultureAndRegionInfoBuilder(language.Culture.ThreeLetterISOLanguageName, CultureAndRegionModifiers.None);
			b.LoadDataFromCultureInfo(language.Culture);
			return b.TwoLetterISORegionName;
#endif
		}

		#region Implementation of IEnumerable

		public IEnumerator<WritingSystemDefinition> GetEnumerator()
		{
			var defs = GetLanguageAndKeyboardCombinations();
			var tst = defs.GroupBy(d => d.RFC4646);
			//now just return the unique ones (Works because no keyboard in the rfc4646)
			var unique= defs.GroupBy(d => d.RFC4646)
				.Select(g => g.First());
			return unique.GetEnumerator();
		}

		private IEnumerable<WritingSystemDefinition> GetLanguageAndKeyboardCombinations()
		{
			foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
			{
				string region = string.Empty;
				if (Environment.OSVersion.Platform != PlatformID.Unix)
				{
					region = GetRegion(language);
				}
				WritingSystemDefinition def =
					new WritingSystemDefinition(language.Culture.ThreeLetterISOLanguageName, "", region, "",
												language.Culture.EnglishName, language.Culture.ThreeLetterISOLanguageName,
												language.Culture.TextInfo.IsRightToLeft);
				def.NativeName = language.Culture.NativeName;
				def.Keyboard = language.LayoutName;
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
