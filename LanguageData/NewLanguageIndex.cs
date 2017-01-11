using System;
using System.Collections.Generic;
using System.Linq;
using SIL.WritingSystems;
using SIL.Extensions;

namespace LanguageData
{
	public class NewLanguageIndex
	{
		private readonly Dictionary<string, LanguageInfo> _codeToLanguageIndex = new Dictionary<string, LanguageInfo>();
		private readonly Dictionary<string, LanguageInfo> _codeToEthnologueData = new Dictionary<string, LanguageInfo>();

		/// <summary>
		/// Initializes a new instance of the <see cref="NewLanguageIndex"/> class.
		/// </summary>
		public NewLanguageIndex(Dictionary<string, string> sourcefiles)
		{
			LdStandardTags subtags = new LdStandardTags(sourcefiles);

			// First read in Ethnologue data file into temporary dictionary
			var threeToTwoLetter = new Dictionary<string, string>();
			string twotothreecodes = sourcefiles["TwoToThreeCodes.txt"];
			foreach (string line in twotothreecodes.Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split('\t');
				threeToTwoLetter.Add(items[1].Trim(), items[0].Trim());
			}

			//LanguageIndex.txt Format: LangID	CountryID	NameType	Name
			//a language appears on one row for each of its alternative langauges
			string languageindex = sourcefiles["LanguageIndex.txt"];
			var entries = new List<string>(languageindex.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
			entries.Add("qaa\t?\tL\tUnlisted Language");

			foreach (string entry in entries.Skip(1)) //skip the header
			{
				string[] items = entry.Split('\t');
				if (items.Length != 4)
					continue;
				if (items[2].Contains('!')) //temporary suppression of entries while waiting for Ethnologue changes
					continue;

				string code = items[0].Trim();
				string twoLetterCode;
				string threelettercode = code;
				if (threeToTwoLetter.TryGetValue(code, out twoLetterCode))
					code = twoLetterCode;

				string regionCode = items[1].Trim();
				LanguageInfo language = GetOrCreateLanguageFromCode(code, threelettercode, regionCode == "?" ? "?" : LdStandardTags.RegisteredRegions[regionCode].Name);

				string name = items[3].Trim();


				if (items[2] == "L")
				{
					while (language.Names.Contains(name))
						language.Names.Remove(name);
					language.Names.Insert(0, name);
				}
				else
				{
					if (items[2].Contains("P"))
					{
						//Skip pejorative
					}
					else if (items[1] == ("ET"))
					{
						//Skip alternatives for Ethiopia, as per request
					}
					else if (items[0] == "gax" || items[0] == "om")
					{
						//For these two "Oromo" languages, skip all related languages as per request
					}
					else if (!language.Names.Contains(name))
						language.Names.Add(name); //intentionally not lower-casing
				}
			}

			// Then for each registered ietf language tag create a real entry and add the ethnologue data to it
			IOrderedEnumerable<LanguageSubtag> languages =  LdStandardTags.RegisteredLanguages.OrderBy(lang => lang.Iso3Code);
			foreach (LanguageSubtag language in languages)
			{
				if (language.IsMacroLanguage || language.IsDeprecated)
				{
					continue;
				}
				LanguageInfo langinfo = GetOrCreateLanguageFromCode(language.Code, language.Iso3Code, null);
				langinfo.DesiredName = language.Name.Replace("'", "’");
				foreach (string name in language.Names)
				{
					string langname = name.Replace("'", "’");
					if (!langinfo.Names.Contains(langname))
					{
						langinfo.Names.Add(langname);
					}
				}
				_codeToLanguageIndex.Add(language.Code, langinfo);
			}

			// Do we need these tags? xx-YY or xx-Ssss format
			IEnumerable<IGrouping<string, string>> languageGroups = Sldr.LanguageTags.Where(info => info.IsAvailable && IetfLanguageTag.IsValid(info.LanguageTag))
	.Select(info => IetfLanguageTag.Canonicalize(info.LanguageTag))
	.GroupBy(IetfLanguageTag.GetLanguagePart);

			foreach (IGrouping<string, string> languageGroup in languageGroups)
			{
				string[] langTags = languageGroup.ToArray();
				if (langTags.Length == 1)
				{
					string langTag = langTags[0];
					LanguageInfo language;
					if (langTag != languageGroup.Key && _codeToLanguageIndex.TryGetValue(languageGroup.Key, out language))
					{
						_codeToLanguageIndex.Remove(languageGroup.Key);
						language.LanguageTag = langTag;
						_codeToLanguageIndex[langTag] = language;
					}
				}
				else
				{
					foreach (string langTag in langTags)
					{
						LanguageSubtag languageSubtag;
						ScriptSubtag scriptSubtag;
						RegionSubtag regionSubtag;
						IEnumerable<VariantSubtag> variantSubtags;
						if (IetfLanguageTag.TryGetSubtags(langTag, out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags))
						{
							if (langTag == languageSubtag)
								continue;

							LanguageInfo language = GetOrCreateLanguageFromCode(langTag, langTag, regionSubtag == null ? "?" : regionSubtag.Name);
							bool displayScript = scriptSubtag != null && !IetfLanguageTag.IsScriptImplied(langTag);
							LanguageInfo otherLanguage;
							if (langTag != languageSubtag && !displayScript && _codeToLanguageIndex.TryGetValue(languageSubtag, out otherLanguage) && language.Countries.SetEquals(otherLanguage.Countries))
							{
								language.Names.AddRange(otherLanguage.Names);
							}
							else
							{
								string name = displayScript ? string.Format("{0} ({1})", languageSubtag.Name, scriptSubtag.Name) : languageSubtag.Name;
								if (!language.Names.Contains(name))
									language.Names.Add(name); //intentionally not lower-casing
							}
							_codeToLanguageIndex.Add(langTag, language);
						}
					}
				}
			}
			

				// localise some language names
				foreach (LanguageInfo languageInfo in _codeToLanguageIndex.Values)
			{
				if (languageInfo.Names.Count == 0)
					continue; // this language is suppressed

				//Why just this small set? Only out of convenience. Ideally we'd have a db of all languages as they write it in their literature.
				string localName = null;
				switch (languageInfo.Names[0])
				{
					case "French":
						localName = "français";
						break;
					case "Spanish":
						localName = "español";
						break;
					case "Chinese":
						localName = "中文";
						break;
					case "Hindi":
						localName = "हिन्दी";
						break;
					case "Bengali":
						localName = "বাংলা";
						break;
					case "Telugu":
						localName = "తెలుగు";
						break;
					case "Tamil":
						localName = "தமிழ்";
						break;
					case "Urdu":
						localName = "اُردُو";
						break;
					case "Arabic":
						localName = "العربية/عربي";
						break;
					case "Thai":
						localName = "ภาษาไทย";
						break;
					case "Indonesian":
						localName = "Bahasa Indonesia";
						break;
				}
				if (!string.IsNullOrEmpty(localName))
				{
					if (languageInfo.Names.Contains(localName))
					{
						languageInfo.Names.Remove(localName);
					}
					languageInfo.Names.Insert(0, localName);
					languageInfo.DesiredName = localName;
				}
			}
		}

		private LanguageInfo GetOrCreateLanguageFromCode(string code, string threelettercode, string countryName)
		{
			LanguageInfo language;
			if (!_codeToEthnologueData.TryGetValue(code, out language))
			{
				language = new LanguageInfo { LanguageTag = code, ThreeLetterTag = threelettercode };
				_codeToEthnologueData.Add(code, language);
			}
			if (!string.IsNullOrEmpty(countryName))
			{
				language.Countries.Add(countryName);
			}
			return language;
		}

		public void WriteIndex(string output_file)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output_file))
			{
				string entry;
				foreach (LanguageInfo languageInfo in _codeToLanguageIndex.Values)
				{
					entry = String.Format("{0}\t{1}\t{2}\t{3}\t{4}",
						languageInfo.LanguageTag,
						languageInfo.ThreeLetterTag,
						languageInfo.DesiredName,
						String.Join(";", languageInfo.Names),
						String.Join(";", languageInfo.Countries)
						);
					file.WriteLine(entry);
				}
			}
		}
	}
}
