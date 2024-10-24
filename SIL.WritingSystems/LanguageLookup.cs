// Copyright (c) 2016-2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SIL.Code;
using SIL.Extensions;
using SIL.IO;
using SIL.Text;
using static SIL.WritingSystems.IetfLanguageTag;
using static SIL.WritingSystems.WellKnownSubtags;

namespace SIL.WritingSystems
{
	/// <summary>
	/// Lets you find a language using data from the Ethnologue, IANA subtag repository and the SLDR.
	/// It obtains the data from langtags.json.
	///
	/// We want to keep all fields in LanguageInfo corresponding to fields in langtags.json
	/// </summary>
	public class LanguageLookup
	{
		private readonly Dictionary<string, LanguageInfo> _codeToLanguageIndex = new Dictionary<string, LanguageInfo>();
		private readonly Dictionary<string, List<LanguageInfo>> _nameToLanguageIndex = new Dictionary<string, List<LanguageInfo>>();
		private readonly Dictionary<string, List<LanguageInfo>> _countryToLanguageIndex = new Dictionary<string, List<LanguageInfo>>();

		/// <summary>For unit testing the sort order</summary>
		internal LanguageLookup(List<AllTagEntry> languageTagEntries, bool ensureDefaultTags = false)
		{
			foreach (AllTagEntry entry in languageTagEntries)
			{
				AddLanguage(entry.tag, entry.iso639_3, entry.full, entry.name, entry.localname, entry.region, entry.names, entry.regions, entry.tags, entry.iana, entry.regionName);
			}
			if (ensureDefaultTags)
				EnsureDefaultTags();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageLookup"/> class.
		/// It gets its data from the LanguageDataIndex resource
		/// </summary>
		public LanguageLookup(bool ensureDefaultTags = false)
		{
			Sldr.InitializeLanguageTags(); // initialise SLDR language tags for implicit script codes

			string langTagsContent = LanguageRegistryResources.langTags;
			// The cached file is renamed if it's invalid during Sldr.InitializeLanguageTags().
			var cachedAllTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");
			// But if that file is somehow not accessible, don't crash here!
			try
			{
				if (File.Exists(cachedAllTagsPath))
					langTagsContent = RobustFile.ReadAllText(cachedAllTagsPath);
			}
			// The above call to Sldr.InitializeLanguageTags() should have already sent a Debug message,
			// if either of these 2 catches trap an exception.
			catch (UnauthorizedAccessException)
			{
			}
			catch (IOException)
			{
			}
			List<AllTagEntry> rootObject = JsonConvert.DeserializeObject<List<AllTagEntry>>(langTagsContent);

			foreach (AllTagEntry entry in rootObject)
			{
				// tags starting with x- have undefined structure so ignoring them as well as deprecated tags
				// tags starting with _ may provide some sort of undefined variant information, but we ignore them as well
				if (!entry.deprecated && !entry.tag.StartsWith("x-", StringComparison.Ordinal) && !entry.tag.StartsWith("_", StringComparison.Ordinal))
				{
					AddLanguage(entry.tag, entry.iso639_3, entry.full, entry.name, entry.localname, entry.region, entry.names, entry.regions, entry.tags, entry.iana, entry.regionName);
				}
			}
			AddLanguage(UnlistedLanguage, UnlistedLanguage, UnlistedLanguage, "Unlisted Language");
			if (ensureDefaultTags)
				EnsureDefaultTags();
		}

		/// <summary>
		/// Some languages in langtags.json have not been normalized to have a default tag without a script marker
		/// in one of its entries.  For some uses of the data, we really want to see only the default tags but we
		/// also don't want to not see any languages.  So scan through the data for cases where every tag associated
		/// with a language contains a script marker and choose one as the default to receive a minimal tag that is
		/// equal to the language code alone.  (The one found in the most countries is chosen by default.)
		/// </summary>
		private void EnsureDefaultTags()
		{
			HashSet<string> tagSet = new HashSet<string>();
			foreach (var langInfo in _codeToLanguageIndex.Values)
				tagSet.Add(langInfo.LanguageTag);
			var tagList = tagSet.ToList();
			tagList.Sort((a,b) => string.Compare(a, b, StringComparison.Ordinal));
			var prevLang = string.Empty;
			var countChanged = 0;
			for (var i = 0; i < tagList.Count; ++i)
			{
				var tag = tagList[i];
				string language;
				string script;
				string region;
				string variant;
				if (!TryGetParts(tag, out language, out script, out region, out variant))
				{
					prevLang = tag;	// shouldn't happen, but if it does...
					continue;
				}
				// Check for a language without a simple tag that has a tag with a script.
				// (not quite foolproof in theory since a tag with region or variant might sort
				// in front of a tag with a script, but good enough in practice)
				if (language == prevLang || string.IsNullOrEmpty(script))
				{
					prevLang = language;
					continue;
				}
				// Go through all the entries for this language so we can attempt to choose
				// the "best" for being the default;
				var langInfo = _codeToLanguageIndex[tag];
				while (i + 1 < tagList.Count)
				{
					var tagNext = tagList[i + 1];
					if (tagNext.StartsWith(language + "-"))
					{
						++i;
						var langInfoNext = _codeToLanguageIndex[tagNext];
						// choose the one that's more widespread unless the name information
						// indicates a possibly less widespread region of origin.
						if (langInfoNext.Names.Count >= langInfo.Names.Count &&
							langInfoNext.Countries.Count > langInfo.Countries.Count)
						{
							langInfo = langInfoNext;
						}
					}
					else
					{
						break;
					}
				}
				langInfo.LanguageTag = language;		// force tag to default form arbitrarily for now.
				++countChanged;
				prevLang = language;
			}
			Debug.WriteLine($"LanguageLookup.EnsureDefaultTags() changed {countChanged} language tags");
		}

		private bool AddLanguage(string code, string threelettercode, string full = null,
			string name = null, string localName = null, string region = null, List<string> names = null, List<string> regions = null, List<string> tags = null, List<string> ianaNames = null, string regionName = null)
		{
			string primarycountry;
			if (region == null)
			{
				primarycountry = "";
			}
			else if (StandardSubtags.IsValidIso3166RegionCode(region))
			{
				if (StandardSubtags.IsPrivateUseRegionCode(region))
				{
					if (region == "XK")
					{
						primarycountry = "Kosovo";
					}
					else
					{
						primarycountry = "Unknown private use";
					}
				}
				else
				{
					primarycountry = StandardSubtags.RegisteredRegions[region].Name; // convert to full region name
				}
			}
			else
			{
				primarycountry = "Invalid region";
			}
			LanguageInfo language = new LanguageInfo
			{
				LanguageTag = code,
				ThreeLetterTag = threelettercode,
				// DesiredName defaults to Names[0], which is set below.
				PrimaryCountry = primarycountry
			};
			language.Countries.Add(primarycountry);

			if (regions != null)
			{
				foreach (var country in regions)
				{
					if (!country.Contains('?') && country != "")
					{
						language.Countries.Add(StandardSubtags.RegisteredRegions[country].Name);
					}
				}
			}

			// For sorting, it is better to store name first instead of localName, which may be in a local script.
			// Names[0] is used in several ways in sorting languages in the list of possible matches: 1) bring
			// to the top of the list languages where Names[0] matches what the user typed, 2) order by the
			// "typing distance" of Names[0] from what the user typed, and 3) order by comparing the Names[0]
			// value of the two languages if neither matches the search string and their typing distances from
			// the search string are the same.
			// All other names in the collection are used in the case of exact match to sort the language closer to the top.
			if (name != null && !language.Names.Contains(name))
			{
				language.Names.Add(name.Trim());
			}
			if (ianaNames != null)
			{
				// add each name that is not already in language.Names
				language.Names.AddRange(ianaNames.Select(n => n.Trim()).Where(n => !language.Names.Contains(n)));
			}
			if (localName != null && localName != name)
			{
				language.Names.Add(localName.Trim());
			}
			if (names != null)
			{
				// add each name that is not already in language.Names
				language.Names.AddRange(names.Select(n => n.Trim()).Where(n => !language.Names.Contains(n)));
			}
			// If we end up needing to add the language code, that reflects a deficiency in the data.  But
			// having a bogus name value probably hurts less that not having any name at all.  The sort
			// process mentioned above using the language tag as well as the first two items in the Names list.
			Debug.Assert(language.Names.Count > 0);
			if (language.Names.Count == 0)
				language.Names.Add(code);

			// add language to _codeToLanguageIndex and _nameToLanguageIndex
			// if 2 letter code then add both 2 and 3 letter codes to _codeToLanguageIndex

			_codeToLanguageIndex[code] = language;
			if (full != null && !string.Equals(full, code))
			{
				_codeToLanguageIndex[full] = language; // add the full expanded tag
			}

			if (threelettercode != null && !string.Equals(code, threelettercode))
			{
				_codeToLanguageIndex[threelettercode] = language;
			}

			if (tags != null)
			{
				foreach (string langtag in tags)
				{
					_codeToLanguageIndex[langtag] = language;
				}
			}

			foreach (string langname in language.Names)
				GetOrCreateListFromName(langname).Add(language);
			// add to _countryToLanguageIndex
			foreach (var country in language.Countries)
			{
				if (!string.IsNullOrEmpty(country))
				{
					List<LanguageInfo> list;
					if (!_countryToLanguageIndex.TryGetValue(country, out list))
					{
						list = new List<LanguageInfo>();
						_countryToLanguageIndex[country] = list;
					}
						list.Add(language);
				}
			}

			return true;
		}


		/// <summary>
		///  For testing; used to detect if we need more special cases where LanguageDataIndex()
		///  populates LanguageInfo.PrimaryCountry.
		/// </summary>
		/// <returns></returns>
		internal List<LanguageInfo> LanguagesWithoutRegions()
		{
			var result = new List<LanguageInfo>();
			foreach (var lang in _codeToLanguageIndex.Values)
			{
				if (String.IsNullOrEmpty(lang.PrimaryCountry) && !result.Contains(lang))
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
				// there will be duplicate LanguageInfo entries for 2 and 3 letter codes and equivalent tags
				var all_languages = new HashSet<LanguageInfo>(_codeToLanguageIndex.Select(l => l.Value));
				foreach (LanguageInfo languageInfo in all_languages.OrderBy(l => l, new ResultComparer(searchString)))
					yield return languageInfo;
			}
			// if the search string exactly matches a hard-coded way to say "sign", show all the sign languages
			// there will be duplicate LanguageInfo entries for equivalent tags
			else if (new []{"sign", "sign language","signes", "langage des signes", "señas","lenguaje de señas"}.Contains(searchString.ToLowerInvariant()))
			{
				var parallelSearch = new HashSet<LanguageInfo>(_codeToLanguageIndex.AsParallel().Select(li => li.Value).Where(l =>
					l.Names.AsQueryable().Any(n => n.ToLowerInvariant().Contains("sign"))));
				foreach (LanguageInfo languageInfo in parallelSearch)
				{
					yield return languageInfo;
				}
			}
			else
			{
				IEnumerable<LanguageInfo> matchOnCode = from x in _codeToLanguageIndex where x.Key.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase) select x.Value;
				List<LanguageInfo>[] matchOnName = (from x in _nameToLanguageIndex where x.Key.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase) select x.Value).ToArray();
				// Apostrophes can cause trouble in lookup.  Unicode TR-29 inexplicably says to use
				// u2019 (RIGHT SINGLE QUOTATION MARK) for the English apostrophe when it also defines
				// u02BC (MODIFIER LETTER APOSTROPHE) as a Letter character.  Users are quite likely to
				// type the ASCII apostrophe (u0027) which is defined as Punctuation.  The current
				// data appears to use u2019 in several language names, which means that users might
				// end up thinking the language isn't in our database.
				// See https://silbloom.myjetbrains.com/youtrack/issue/BL-6339.
				if (!matchOnName.Any() && searchString.Contains('\''))
				{
					searchString = searchString.Replace('\'','\u2019');
					matchOnName = (from x in _nameToLanguageIndex where x.Key.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase) select x.Value).ToArray();
				}
				List<LanguageInfo>[] matchOnCountry = (from x in _countryToLanguageIndex where x.Key.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase) select x.Value).ToArray();

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
				foreach (List<LanguageInfo> l in matchOnCountry)
					combined.UnionWith(l);
				var ordered = combined.OrderBy(l => l, new ResultComparer(searchString));
				foreach (LanguageInfo languageInfo in ordered)
					yield return languageInfo;
			}
		}

		private class ResultComparer : IComparer<LanguageInfo>
		{
			private readonly string _searchString;
			private readonly string _lowerSearch;

			public ResultComparer(string searchString)
			{
				_searchString = searchString;
				_lowerSearch = searchString.ToLowerInvariant();
			}

			/// <summary>
			/// Sorting the languages for display is tricky: we want the most relevant languages at the
			/// top of the list, so we can't simply sort alphabetically by language name or by language tag,
			/// but need to take both items into account together with the current search string.  Ordering
			/// by relevance is clearly impossible since we'd have to read the user's mind and apply that
			/// knowledge to the data.  But the heuristics we use here may be better than nothing...
			/// </summary>
			public int Compare(LanguageInfo x, LanguageInfo y)
			{
				if (x.LanguageTag == y.LanguageTag)
					return 0;

				// Favor ones where some language name matches the search string to solve BL-1141
				// We restrict this to the top 2 names of each language, and to cases where the
				// corresponding names of the two languages are different.  (If both language names
				// match the search string, there's no good reason to favor one over the other!)
				if (!x.Names[0].Equals(y.Names[0], StringComparison.InvariantCultureIgnoreCase))
				{
					if (x.Names[0].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return -1;
					if (y.Names[0].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return 1;
				}
				else if (x.Names.Count == 1 || y.Names.Count == 1 || !x.Names[1].Equals(y.Names[1], StringComparison.InvariantCultureIgnoreCase))
				{
					// If we get here, x.Names[0] == y.Names[0].  If both equal the search string, then neither x.Names[1]
					// nor y.Names[1] should equal the search string since the code adding to Names checks for redundancy.
					// Also it's possible that neither x.Names[1] nor y.Names[1] exists at this point in the code, or that
					// only one of them exists, or that both of them exist (in which case they are not equal).
					if (x.Names.Count > 1 && x.Names[1].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return -1;
					if (y.Names.Count > 1 && y.Names[1].Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
						return 1;
				}

				// Favor a language whose tag matches the search string exactly.  (equal tags are handled above)
				if (x.LanguageTag.Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
					return -1;
				if (y.LanguageTag.Equals(_searchString, StringComparison.InvariantCultureIgnoreCase))
					return 1;

				// written this way to avoid having to catch predictable exceptions as the user is typing
				string xlanguage;
				string ylanguage;
				string script;
				string region;
				string variant;
				var xtagParses = TryGetParts(x.LanguageTag, out xlanguage, out script, out region, out variant);
				var ytagParses = TryGetParts(y.LanguageTag, out ylanguage, out script, out region, out variant);
				var bothTagLanguagesMatchSearch = xtagParses && ytagParses && xlanguage == ylanguage &&
					_searchString.Equals(xlanguage, StringComparison.InvariantCultureIgnoreCase);
				if (!bothTagLanguagesMatchSearch)
				{
					// One of the tag language pieces may match the search string even though not both match.  In that case,
					// sort the matching language earlier in the list.
					if (xtagParses && _searchString.Equals(xlanguage, StringComparison.InvariantCultureIgnoreCase))
						return -1;  // x.Tag's language part matches search string exactly, so sort it earlier in the list.
					if (ytagParses && _searchString.Equals(ylanguage, StringComparison.InvariantCultureIgnoreCase))
						return 1;   // y.Tag's language part matches search string exactly, so sort it earlier in the list.
				}

				// If only one language has a name that is an exact match prefer that language
				var xMatchingNameLoc = x.Names.IndexOf(l => _searchString.Equals(l, StringComparison.InvariantCultureIgnoreCase));
				var yMatchingNameLoc = y.Names.IndexOf(l => _searchString.Equals(l, StringComparison.InvariantCultureIgnoreCase));
				if (xMatchingNameLoc > 0 && yMatchingNameLoc < 0)
				{
					return -1;
				}
				if (yMatchingNameLoc > 0 && xMatchingNameLoc < 0)
				{
					return 1;
				}

				// Order the by country information for exact matches before dropping to editing distance
				if (x.PrimaryCountry.ToLowerInvariant() == _lowerSearch || y.PrimaryCountry.ToLowerInvariant() == _lowerSearch)
				{
					if (x.PrimaryCountry.ToLowerInvariant() == _lowerSearch &&
						y.PrimaryCountry.ToLowerInvariant() != _lowerSearch)
					{
						return -1;
					}
					if (y.PrimaryCountry.ToLowerInvariant() == _lowerSearch &&
						x.PrimaryCountry.ToLowerInvariant() != _lowerSearch)
					{
						return 1;
					}
					// Both match the country exactly sort by language name
					return string.Compare(x.Names[0], y.Names[0], StringComparison.InvariantCultureIgnoreCase);
				}

				// Editing distance to a language name is not useful when we've detected that the user appears to be
				// typing a language tag in that both language tags match what the user has typed.  (For example,
				// it gives a strange and unwanted order to the variants of zh.)  In such a case we just order the
				// matching codes by length (already done) and then alphabetically by code, skipping the sort by
				// editing distance to the language names.
				if (!bothTagLanguagesMatchSearch)
				{
					// Use the "editing distance" relative to the search string to sort by the primary name.
					// (But we don't really care once the editing distance gets very large.)
					// See https://silbloom.myjetbrains.com/youtrack/issue/BL-5847 for motivation.
					// Timing tests indicate that 1) calculating these distances doesn't slow down the sorting noticeably
					// and 2) caching these distances in a dictionary also doesn't speed up the sorting noticeably.
					var xDistance = ApproximateMatcher.EditDistance(_lowerSearch, x.Names[0].ToLowerInvariant(), 25, false);
					var yDistance = ApproximateMatcher.EditDistance(_lowerSearch, y.Names[0].ToLowerInvariant(), 25, false);
					var distanceDiff = xDistance - yDistance;
					if (distanceDiff != 0)
						return distanceDiff;

					// If the editing distances for the primary names are the same, sort by the primary name.
					int res = string.Compare(x.Names[0], y.Names[0], StringComparison.InvariantCultureIgnoreCase);
					if (res != 0)
						return res;
				}

				// Alphabetize by Language tag if 3 characters or less or if there is a hyphen after the first 2 or 3 chars
				if (_lowerSearch.Length <= 3 || _lowerSearch.IndexOf('-') == 3 || _lowerSearch.IndexOf('-') == 4)
				{
					return string.Compare(x.LanguageTag, y.LanguageTag, StringComparison.InvariantCultureIgnoreCase);
				}
				// Otherwise alphabetize by the language name
				// (tags often have a completely different alphabetical order from the name e.g. 'auc' -> "Waoroni")
				return string.Compare(x.Names[0], y.Names[0], StringComparison.InvariantCultureIgnoreCase);

			}
		}
	}
}
