// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Extensions;
using SIL.WritingSystems;

namespace LanguageData
{
	public class NewLanguageIndex
	{
		private readonly Dictionary<string, LanguageInfo> _codeToLanguageIndex = new Dictionary<string, LanguageInfo>();
		private readonly Dictionary<string, LanguageInfo> _codeToEthnologueData = new Dictionary<string, LanguageInfo>();

		// Completely exclude these language codes
		private List<string> ExcludedCodes = new List<string> {"gax", "gaz", "hae"};
		// Exclude alternative names from these regions
		private List<string> ExcludedRegions = new List<string> { "Ethiopia" };

		/// <summary>
		/// Initializes a new instance of the <see cref="NewLanguageIndex"/> class.
		/// </summary>
		public NewLanguageIndex(IDictionary<string, string> sourcefiles)
		{
			string twotothreecodes = sourcefiles["TwoToThreeCodes.txt"];
			string subtagregistry = sourcefiles["ianaSubtagRegistry.txt"];

			StandardSubtags.InitialiseIanaSubtags(twotothreecodes, subtagregistry);

			// First read in Ethnologue data file into temporary dictionary
			var threeToTwoLetter = StandardSubtags.TwoAndThreeMap(twotothreecodes, true);

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
				if (items[2].StartsWith("!")) //temporary suppression of entries while waiting for Ethnologue changes
					continue;
				// excluded by !
				// all gax (ET,KE,SO) including L
				// all gaz (ET) including L
				// all hae (ET) including L

				string code = items[0].Trim();
				string twoLetterCode;
				string threelettercode = code;
				if (threeToTwoLetter.TryGetValue(code, out twoLetterCode))
					code = twoLetterCode;

				//temporary suppression of entries while waiting for Ethnologue changes (those excluded by !)
				if (ExcludedCodes.Contains(code))
				{
					continue;
				}

				string regionCode = items[1].Trim();
				LanguageInfo language = GetOrCreateLanguageFromCode(code, threelettercode, regionCode == "?" ? "?" : StandardSubtags.RegisteredRegions[regionCode].Name);

				string name = items[3].Trim();


				if (items[2].Trim() == "L")
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
					else if (ExcludedRegions.Contains(StandardSubtags.RegisteredRegions[regionCode].Name))
					{
						//Skip alternatives for Ethiopia, as per request
					}
					else if (code == "gax" || code == "om")
					{
						//For these two "Oromo" languages, skip all related languages as per request
					}
					else if (!language.Names.Contains(name))
						language.Names.Add(name); //intentionally not lower-casing
				}
			}

			// Then for each registered ietf language tag create a real entry and add the ethnologue data to it
			IOrderedEnumerable<LanguageSubtag> languages =  StandardSubtags.RegisteredLanguages.OrderBy(lang => lang.Iso3Code);
			foreach (LanguageSubtag language in languages)
			{
				bool singlename = false;
				if (language.IsDeprecated || ExcludedCodes.Contains(language.Code))
				{
					continue;
				}
				LanguageInfo langinfo = GetOrCreateLanguageFromCode(language.Code, language.Iso3Code, null);
				langinfo.DesiredName = language.Name.Replace("'", "’");
				langinfo.IsMacroLanguage = language.IsMacroLanguage;

				foreach (string country in langinfo.Countries)
				{
					if (ExcludedRegions.Contains(country))
					{
						singlename = true;
					}
				}

				foreach (string name in language.Names)
				{
					string langname = name.Replace("'", "’");
					if (!langinfo.Names.Contains(langname))
					{
						if (singlename && langinfo.Names.Count == 1)
						{
							// leave single ethnologue names
							break;
						}
						else
						{
							langinfo.Names.Add(langname);
						}
					}
					if (singlename)
					{
						break;
					}
				}
				_codeToLanguageIndex.Add(language.Code, langinfo);
			}

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
							LanguageInfo keylanguage;
							if (_codeToLanguageIndex.TryGetValue(languageGroup.Key, out keylanguage))
							{
								language.IsMacroLanguage = keylanguage.IsMacroLanguage;
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
				// If you add another field here don't forget to change LanguageLookup to deal with it
				// if the teamcity project has been set up with any project having dependencies on a LanguageDataIndex.txt artifact
				// then the circular dependency needs to be broken to get the new version in
				foreach (LanguageInfo languageInfo in _codeToLanguageIndex.Values)
				{
					entry = String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
						languageInfo.LanguageTag,
						languageInfo.ThreeLetterTag,
						languageInfo.DesiredName,
						languageInfo.IsMacroLanguage ? "M" : ".",
						String.Join(";", languageInfo.Names),
						String.Join(";", languageInfo.Countries)
						);
					file.WriteLine(entry);
				}
			}
		}
	}
}
