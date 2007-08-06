using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public class WritingSystemFromWindowsLocaleProvider : IWritingSystemProvider
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

		public IEnumerable<WritingSystemDefinition> ActiveOSLanguages
		{
			get
			{
//            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.AllCultures ))
//            {
//                Debug.WriteLine(string.Format("{0} {1}", info.Name, info.EnglishName));
//            }

				foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
				{
					if (true)
					{
						System.Globalization.CultureAndRegionInfoBuilder b = new CultureAndRegionInfoBuilder(language.Culture.ThreeLetterISOLanguageName, CultureAndRegionModifiers.None);
						b.LoadDataFromCultureInfo(language.Culture);
						string region = b.TwoLetterISORegionName;

						WritingSystemDefinition def =
							new WritingSystemDefinition(language.Culture.ThreeLetterISOLanguageName, region, "", "",
														language.Culture.EnglishName, language.Culture.ThreeLetterISOLanguageName, false);
						def.NativeName = language.Culture.NativeName;
						def.Keyboard = language.LayoutName;
						yield return def;
					}
				}
			}
		}
	}
}
