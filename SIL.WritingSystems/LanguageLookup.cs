﻿// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Code;
using SIL.Text;

namespace SIL.WritingSystems
{
	/// <summary>
	/// Lets you find a language using data from the Ethnologue, IANA subtag repository and the SLDR.
	/// </summary>
	public class LanguageLookup
	{
		private readonly Dictionary<string, LanguageInfo> _codeToLanguageIndex = new Dictionary<string, LanguageInfo>();
		private readonly Dictionary<string, List<LanguageInfo>> _nameToLanguageIndex = new Dictionary<string, List<LanguageInfo>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="NewLanguageLookup"/> class.
		/// </summary>
		public LanguageLookup()
		{
			// Load from file into the data structures instead of creating it from scratch
			var entries = LanguageRegistryResources.LanguageDataIndex.Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string entry in entries)
			{
				//Code ThreeLetterCode DesiredName Names Countries
				string[] items = entry.Split('\t');
				if (items.Length != 6) // This needs to be changed if the number of fields changes
					continue;
				string code = items[0];
				string threelettercode = items[1];
				string desiredname = items[2];
				bool macrolanguage = String.Equals("M", items[3]);
				string[] names = items[4].Split(';');
				string[] countries = items[5].Split(';');
				LanguageInfo language = new LanguageInfo { LanguageTag = code, ThreeLetterTag = threelettercode, DesiredName = desiredname, IsMacroLanguage = macrolanguage };
				foreach (string country in countries)
				{
					language.Countries.Add(country);
				}
				foreach (string langname in names)
				{
					language.Names.Add(langname.Trim());
				}

				// add language to _codeToLanguageIndex and _nameToLanguageIndex
				// if 2 letter code then add both 2 and 3 letter codes to _codeToLanguageIndex

				_codeToLanguageIndex[code] = language;
				if (!String.Equals(code, threelettercode))
				{
					_codeToLanguageIndex[threelettercode] = language;
				}
				foreach (string langname in language.Names)
					GetOrCreateListFromName(langname).Add(language);
			}

			var langData = LanguageRegistryResources.LanguageCodes.Replace("\r\n", "\n")
				.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var languageCode in langData)
			{
				var data = languageCode.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
				if (data.Length < 2)
					continue;
				var langCode = data[0];
				var countryCode = data[1];
				var lang = this.GetLanguageFromCode(langCode);
				if (lang == null)
					continue;
				RegionSubtag region;
				if (!StandardSubtags.RegisteredRegions.TryGet(countryCode, out region))
					continue;
				lang.PrimaryRegion = region;
			}
		}

		/// <summary>
		///  For testing; used to detect if we need more special cases in LanguageInfo.PrimaryCountry.
		/// </summary>
		/// <returns></returns>
		internal List<LanguageInfo> LanguagesWithoutRegions()
		{
			var result = new List<LanguageInfo>();
			foreach (var lang in _codeToLanguageIndex.Values)
			{
				if (lang.PrimaryRegion == null)
					result.Add(lang);
			}
			return result;
		}

		private List<LanguageInfo> GetOrCreateListFromName(string name)
		{
			List<LanguageInfo> languages;
			if (!_nameToLanguageIndex.TryGetValue(name, out languages))
			{
				languages = new List<LanguageInfo>();
				_nameToLanguageIndex.Add(name, languages);
			}
			return languages;
		}

		/// <summary>
		/// Just lookup the code in the index.
		/// </summary>
		/// <returns>null if not found</returns>
		public LanguageInfo GetLanguageFromCode(string isoCode)
		{
			Guard.AgainstNullOrEmptyString(isoCode, "Parameter to GetLanguageFromCode must not be null or empty.");
			LanguageInfo languageInfo = null;
			_codeToLanguageIndex.TryGetValue(isoCode, out languageInfo);
			return languageInfo;
		}

		/// <summary>
		/// Get an list of languages that match the given string in some way (code, name, country)
		/// </summary>
		public IEnumerable<LanguageInfo> SuggestLanguages(string searchString)
		{
			if (searchString != null)
				searchString = searchString.Trim();
			if (string.IsNullOrEmpty(searchString))
				yield break;

			if (searchString == "*")
			{
				// there will be duplicate LanguageInfo entries for 2 and 3 letter codes
				var all_languages = new HashSet<LanguageInfo>(_codeToLanguageIndex.Select(l => l.Value));
				foreach (LanguageInfo languageInfo in all_languages.OrderBy(l => l, new ResultComparer(searchString)))
					yield return languageInfo;
			}
			else
			{
				IEnumerable<LanguageInfo> matchOnCode = from x in _codeToLanguageIndex where x.Key.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase) select x.Value;
				List<LanguageInfo>[] matchOnName = (from x in _nameToLanguageIndex where x.Key.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase) select x.Value).ToArray();

				if (!matchOnName.Any())
				{
					// look  for approximate matches
					const int kMaxEditDistance = 3;
					var itemFormExtractor = new ApproximateMatcher.GetStringDelegate<KeyValuePair<string, List<LanguageInfo>>>(pair => pair.Key);
					IList<KeyValuePair<string, List<LanguageInfo>>> matches = ApproximateMatcher.FindClosestForms(_nameToLanguageIndex, itemFormExtractor,
						searchString,
						ApproximateMatcherOptions.None,
						kMaxEditDistance);
					matchOnName = (from m in matches select m.Value).ToArray();
				}

				var combined = new HashSet<LanguageInfo>(matchOnCode);
				foreach (List<LanguageInfo> l in matchOnName)
					combined.UnionWith(l);

				foreach (LanguageInfo languageInfo in combined.OrderBy(l => l, new ResultComparer(searchString)))
					yield return languageInfo;
			}
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
				if (x.LanguageTag == y.LanguageTag)
					return 0;
				if (!x.Names[0].Equals(y.Names[0], StringComparison.InvariantCultureIgnoreCase))
				{
					// Favor ones where some language matches to solve BL-1141
					if (x.Names[0].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return -1;
					if (y.Names[0].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return 1;
					if (x.Names.Count > 1 && x.Names[1].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return -1;
					if (y.Names.Count > 1 && y.Names[1].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return 1;
				}

				if (x.LanguageTag.Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
					return -1;
				if (y.LanguageTag.Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
					return 1;

				if (IetfLanguageTag.GetLanguagePart(x.LanguageTag).Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
					return -1;
				if (IetfLanguageTag.GetLanguagePart(y.LanguageTag).Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
					return 1;

				int res = string.Compare(x.Names[0], y.Names[0], StringComparison.InvariantCultureIgnoreCase);
				if (res != 0)
					return res;

				return string.Compare(x.LanguageTag, y.LanguageTag, StringComparison.InvariantCultureIgnoreCase);
			}
		}
	}
}
