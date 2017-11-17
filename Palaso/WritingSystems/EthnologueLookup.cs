using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Text;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// Lets you find a language using data from the Ethnologue. Currently doesn't do any live-lookup, just reads files stored in the LanguageRegistryResources
	/// </summary>
	public class EthnologueLookup
	{
		Dictionary<string, string> CountryCodeToCountryName = new Dictionary<string, string>();
		Dictionary<string, LanguageInfo> CodeToLanguageIndex = new Dictionary<string, LanguageInfo>();
		Dictionary<string, List<LanguageInfo>> NameToLanguageIndex = new Dictionary<string, List<LanguageInfo>>();
		Dictionary<string, string> ThreeToTwoLetter = new Dictionary<string, string>();
		private List<string> _macroLanguages = new List<string>();

		/// <summary>Force the dialog to return 3 letter iso codes even if a 2 letter code is available</summary>
		public bool Force3LetterCodes { get; set; }

		public EthnologueLookup()
		{
			Force3LetterCodes = false;
			foreach (var line in LanguageRegistryResources.TwoToThreeCodes.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				var items = line.Split('\t');
				ThreeToTwoLetter.Add(items[1].Trim(), items[0].Trim());
			}

			foreach (var line in LanguageRegistryResources.CountryCodes.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				var items = line.Split('\t'); //id name area
				CountryCodeToCountryName.Add(items[0].Trim(), items[1].Trim());
			}
			CountryCodeToCountryName.Add("?","?");//for unlisted language

			//LanguageIndex.txt Format: LangID	CountryID	NameType	Name
			//a language appears on one row for each of its alternative langauges
			List<string> entries = new List<string>(LanguageRegistryResources.LanguageIndex.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
			entries.Add("qaa\t?\tL\tUnlisted Language");
			foreach (string entry in entries.Skip(1)) //skip the header
			{
				var items = entry.Split('\t');
				if (items.Length != 4)
					continue;
				if (items[2].Contains('!')) //allows for temporary suppression of entries while waiting for Ethnologue changes 
					continue;
				var code = items[0].Trim();
				string TwoLetterCode;
				if (ThreeToTwoLetter.TryGetValue(code, out TwoLetterCode))
					code = TwoLetterCode;
				if (MacroLanguages.Contains(code)) // exclude known macrolanguages
					continue;

				LanguageInfo language = GetOrCreateLanguageFromCode(code, items[1].Trim());

				var name = items[3].Trim();

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
					else if (items[1].Trim() == ("ET"))
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

			//Why just this small set? Only out of convenience. Ideally we'd have a db of all languages as they write it in their literature.
			this.CodeToLanguageIndex["fr"].LocalName =  "français";
			this.CodeToLanguageIndex["es"].LocalName =  "español";
			this.CodeToLanguageIndex["cmn"].LocalName =  "中文"; //chinese
			this.CodeToLanguageIndex["hi"].LocalName =  "हिन्दी"; //hindi
			this.CodeToLanguageIndex["bn"].LocalName =  "বাংলা"; //bengali
			this.CodeToLanguageIndex["te"].LocalName =  "తెలుగు"; //telugu
			this.CodeToLanguageIndex["ta"].LocalName =  "தமிழ்"; //tamil
			this.CodeToLanguageIndex["ur"].LocalName =  "اُردُو"; //urdu
			this.CodeToLanguageIndex["arb"].LocalName =  "العربية/عربي"; //arabic
			this.CodeToLanguageIndex["th"].LocalName = "ภาษาไทย"; //thai
			this.CodeToLanguageIndex["id"].LocalName = "Bahasa Indonesia"; //indonesian


			foreach (var languageInfo in CodeToLanguageIndex.Values)
			{
				foreach (var name in languageInfo.Names)
				{
					GetOrCreateListFromName(name).Add(languageInfo);
				}
				if (!string.IsNullOrEmpty(languageInfo.LocalName))
				{
					GetOrCreateListFromName(languageInfo.LocalName).Add(languageInfo);
				}
			}
		}

		private List<string> MacroLanguages
		{
			get
			{
				if (_macroLanguages.Count == 0)
				{
					_macroLanguages.Add("ak");
					_macroLanguages.Add("ar");
					_macroLanguages.Add("ay");
					_macroLanguages.Add("az");
					_macroLanguages.Add("cr");
					_macroLanguages.Add("et");
					_macroLanguages.Add("fa");
					_macroLanguages.Add("ff");
					_macroLanguages.Add("gn");
					_macroLanguages.Add("ik");
					_macroLanguages.Add("iu");
					_macroLanguages.Add("kg");
					_macroLanguages.Add("kr");
					_macroLanguages.Add("ku");
					_macroLanguages.Add("kv");
					_macroLanguages.Add("lv");
					_macroLanguages.Add("mg");
					_macroLanguages.Add("mn");
					_macroLanguages.Add("ms");
					_macroLanguages.Add("ne");
					_macroLanguages.Add("no");
					_macroLanguages.Add("oj");
					_macroLanguages.Add("om");
					_macroLanguages.Add("or");
					_macroLanguages.Add("ps");
					_macroLanguages.Add("qu");
					_macroLanguages.Add("sc");
					_macroLanguages.Add("sh");
					_macroLanguages.Add("sq");
					_macroLanguages.Add("sw");
					_macroLanguages.Add("uz");
					_macroLanguages.Add("yi");
					_macroLanguages.Add("za");
					_macroLanguages.Add("zh");
					_macroLanguages.Add("bal");
					_macroLanguages.Add("bik");
					_macroLanguages.Add("bnc");
					_macroLanguages.Add("bua");
					_macroLanguages.Add("chm");
					_macroLanguages.Add("del");
					_macroLanguages.Add("den");
					_macroLanguages.Add("din");
					_macroLanguages.Add("doi");
					_macroLanguages.Add("gba");
					_macroLanguages.Add("gon");
					_macroLanguages.Add("grb");
					_macroLanguages.Add("hai");
					_macroLanguages.Add("hmn");
					_macroLanguages.Add("jrb");
					_macroLanguages.Add("kln");
					_macroLanguages.Add("kok");
					_macroLanguages.Add("kpe");
					_macroLanguages.Add("lah");
					_macroLanguages.Add("luy");
					_macroLanguages.Add("man");
					_macroLanguages.Add("mwr");
					_macroLanguages.Add("raj");
					_macroLanguages.Add("rom");
					_macroLanguages.Add("syr");
					_macroLanguages.Add("tmh");
					_macroLanguages.Add("zap");
					_macroLanguages.Add("zza");
				}
				return _macroLanguages;
			}
		}

		private List<LanguageInfo> GetOrCreateListFromName(string name)
		{
			List<LanguageInfo> languages;
			if (!NameToLanguageIndex.TryGetValue(name, out languages))
			{
				languages = new List<LanguageInfo>();
				NameToLanguageIndex.Add(name, languages);
			}
			return languages;
		}

		private LanguageInfo GetOrCreateLanguageFromCode(string code, string country)
		{
			LanguageInfo language;
			var countryName = CountryCodeToCountryName[country];
			if (!CodeToLanguageIndex.TryGetValue(code, out language))
			{
				language = new LanguageInfo() { Code = code, Country = countryName };
				CodeToLanguageIndex.Add(code, language);
			}
			else
			{
				if (!language.Country.Contains(countryName))
				{
					language.Country += ", " + countryName;
					++language.CountryCount;
				}
			}
			return language;
		}

		/// <summary>
		/// Get an list of languages that match the given string in some way (code, name, country)
		/// </summary>
		public IEnumerable<LanguageInfo> SuggestLanguages(string searchString)
		{
			if (searchString != null)
				searchString = searchString.Trim().ToLowerInvariant();
			if (string.IsNullOrEmpty(searchString))
			{
				yield break;
			}
			else if (searchString == "*")
			{
				foreach (var l in from x in CodeToLanguageIndex select x.Value)
					yield return Set3LetterCode(l);
			}
			else
			{
				IEnumerable<LanguageInfo> matchOnCode = from x in CodeToLanguageIndex where x.Key.ToLowerInvariant().StartsWith(searchString) select x.Value;
				var matchOnName = from x in NameToLanguageIndex where x.Key.ToLowerInvariant().StartsWith(searchString) select x.Value;

				if (!matchOnName.Any())
				{
					// look  for approximate matches
					const int kMaxEditDistance = 3;
					var itemFormExtractor = new ApproximateMatcher.GetStringDelegate<KeyValuePair<string, List<LanguageInfo>>>(pair => pair.Key);
					var matches = ApproximateMatcher.FindClosestForms<KeyValuePair<string, List<LanguageInfo>>>(NameToLanguageIndex, itemFormExtractor,
																										  searchString,
																										  ApproximateMatcherOptions.None,
																										  kMaxEditDistance);
					matchOnName = from m in matches select m.Value;
				}

				List<LanguageInfo> combined = new List<LanguageInfo>(matchOnCode);
				foreach (var l in matchOnName)
				{
					combined.AddRange(l);
				}

				List<LanguageInfo> sorted = new List<LanguageInfo>(combined.Distinct());
				sorted.Sort(new ResultComparer(searchString));
				foreach (var languageInfo in sorted)
				{
					yield return Set3LetterCode(languageInfo);
				}
			}
		}

		// look up 3 letter codes
		private LanguageInfo Set3LetterCode(LanguageInfo langInfo)
		{
			if (!Force3LetterCodes) return langInfo;
			if (langInfo.Code.Length == 3) return langInfo;

			var found = ThreeToTwoLetter.Where(p => p.Value == langInfo.Code).Select(p => p.Key).FirstOrDefault();
			if (!string.IsNullOrEmpty(found))
				langInfo.Code = found;

			return langInfo;
		}

		private class ResultComparer : IComparer<LanguageInfo>
		{
			private readonly string _searchString;

			public ResultComparer(string searchString)
			{
				_searchString = searchString;
			}

			public int Compare(LanguageInfo x, LanguageInfo y)
			{
				if (x.Code == y.Code)
					return 0;
				// Favor ones where some language matches to solve BL-1141
				if (x.Names[0].ToLowerInvariant() == _searchString)
					return -1;
				if (y.Names[0].ToLowerInvariant() == _searchString)
					return 1;
				if (x.Code == _searchString)
					return -1;
				if (y.Code == _searchString)
					return 1;
				return 0;
			}
		}
	}

	public class LanguageInfo
	{
		public List<string> Names = new List<string>();
		public string Country;
		public string Code;

		/// <summary>
		/// Currently, we only have English names in our database. This holds the language name in the language, when we know it
		/// </summary>
		public string LocalName;

		private string _desiredName;
		public int CountryCount;

		/// <summary>
		/// People sometimes don't want use the Ethnologue-supplied name
		/// </summary>
		public string DesiredName
		{
			get
			{
				if (string.IsNullOrEmpty(_desiredName))
					return Names.FirstOrDefault();
				return _desiredName;
			}
			set { _desiredName = string.IsNullOrEmpty(value) ? value : value.Trim(); }
		}
	}
}
