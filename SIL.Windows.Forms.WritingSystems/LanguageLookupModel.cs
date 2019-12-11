using System;
using System.Collections.Generic;
using System.Linq;
using L10NSharp;
using SIL.Extensions;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	public class LanguageLookupModel
	{
		private static readonly string UnlistedLanguageName = LocalizationManager.GetString("LanguageLookup.UnlistedLanguage", "Unlisted Language");

		private LanguageLookup _languageLookup;
		private string _searchText;
		private LanguageInfo _selectedLanguage;
		private string _desiredLanguageName;

		public Func<LanguageInfo, bool> MatchingLanguageFilter { get; set; }

		public string SearchText
		{
			get { return _searchText; }
			set { _searchText = value.Trim(); }
		}

		public bool HaveSufficientInformation
		{
			get
			{
				return _desiredLanguageName != null && SelectedLanguage != null &&
					_desiredLanguageName != UnlistedLanguageName && _desiredLanguageName.Length > 0;
			}
		}

		public string DesiredLanguageName
		{
			get
			{
				return _desiredLanguageName ?? string.Empty;
			}
			set
			{
				_desiredLanguageName = value == null ? null : value.Trim();
				if (SelectedLanguage != null)
					SelectedLanguage.DesiredName = _desiredLanguageName;
			}
		}

		public void LoadLanguages()
		{
			_languageLookup = new LanguageLookup(!_includeScriptMarkers);
		}

		public bool AreLanguagesLoaded
		{
			get { return _languageLookup != null; }
		}

		public IEnumerable<LanguageInfo> MatchingLanguages
		{
			get
			{
				if (_searchText == "?")
				{
					yield return new LanguageInfo {LanguageTag = "qaa", Names = {UnlistedLanguageName}};
					yield break;
				}

				foreach (LanguageInfo li in _languageLookup.SuggestLanguages(_searchText).Where(
					li =>
						(MatchingLanguageFilter == null || MatchingLanguageFilter(li)) &&
						RegionalDialectsFilter(li) && ScriptMarkerFilter(li)))
				{
					yield return li;
				}
			}
		}

		/// <summary>
		/// If so desired, filter out any language whose tags contain a Script value.  Except that there are 90+
		/// languages in the data whose tags all contain a Script value.  Since we don't want to lose access to
		/// those languages, we detect when that happens and pass the first occurrence with the tag adjusted to
		/// the bare language code.
		/// </summary>
		private bool ScriptMarkerFilter(LanguageInfo li)
		{
			if (IncludeScriptMarkers)
				return true;

			// written this way to avoid having to catch predictable exceptions as the user is typing
			string language;
			string script;
			string region;
			string variant;
			if (IetfLanguageTag.TryGetParts(li.LanguageTag, out language, out script, out region, out variant))
				return string.IsNullOrEmpty(script);	// OK only if no script.
			return true;	// Not a tag?  Don't filter it out.
		}

		/// <summary>
		/// Filter out tags that contain a region marker unless the caller has already specified that region
		/// markers are allowed in language tags.  Note that li.LanguageTag can be just a search string the
		/// user has typed, which might be a (partial) language tag or might be (part of) a language name.
		/// If the tag doesn't actually parse as a language tag, we assume the user is typing something other
		/// than a language tag and consider it not to be something we'd filter out as specifying a region.
		/// </summary>
		private bool RegionalDialectsFilter(LanguageInfo li)
		{
			if (IncludeRegionalDialects)
				return true;

			// always include Chinese languages with region codes
			if (li.LanguageTag.IsOneOf("zh-CN", "zh-TW"))
				return true;

			// written this way to avoid having to catch predictable exceptions as the user is typing
			string language;
			string script;
			string region;
			string variant;
			if (IetfLanguageTag.TryGetParts(li.LanguageTag, out language, out script, out region, out variant))
				return string.IsNullOrEmpty(region);	// OK only if no region.
			return true;	// Not a tag?  Don't filter it out.
		}

		public LanguageInfo SelectedLanguage
		{
			get { return _selectedLanguage; }
			set
			{
				var oldValue = _selectedLanguage;
				_selectedLanguage = value;
				if (_selectedLanguage == null)
					return;

				if (LanguageTag == "qaa")
				{
					if (_searchText != "?")
					{
						string failedSearchText = _searchText.ToUpperFirstLetter();
						_desiredLanguageName = failedSearchText;
						_selectedLanguage.Names.Insert(0, failedSearchText);
					}
				}
				else
				{
					// We set the selected language in two main ways: the client may set it, possibly using a newly created
					// and incomplete LanguageInfo, partly as a way of passing in the desiredName for the current selection;
					// It also gets set as we run the initial (or any subsequent) search based on choosing one of the search results.
					// The search result typically doesn't have a DesiredName set so we can easily overwrite the desired
					// name sent in by the client.
					if (oldValue != null && oldValue.LanguageTag == _selectedLanguage.LanguageTag)
					{
						// We're probably just setting the same language as selected earlier, but this time to a languageInfo
						// selected from our list. We want to update that object, so that (a) if the user switches back to it,
						// we can reinstate their desired name; and (b) so that if the client retrieves this object, rather
						// than the one they originally set, after the dialog closes, they will get the original desired name back.
						// (Unless the user subsequently changes it, of course.)
						_selectedLanguage.DesiredName = _desiredLanguageName;
					}
					else
					{
						// Either setting it for the first time (from client), or something made us really change language.
						// Either way we need to update _desiredLanguage name; it can't be useful for a different language.
						_desiredLanguageName = _selectedLanguage.DesiredName;
					}
				}
			}
		}

		public string LanguageTag
		{
			get
			{
				if (_selectedLanguage == null)
					return string.Empty;
				return _selectedLanguage.LanguageTag;
			}
		}

		public bool IncludeRegionalDialects { get; set; }

		private bool _includeScriptMarkers = true;	// preserve old default behavior
		public bool IncludeScriptMarkers
		{
			get { return _includeScriptMarkers;}
			set
			{
				_includeScriptMarkers = value;
				_languageLookup = new LanguageLookup(!_includeScriptMarkers);
			}
		}

		/// <summary>
		/// This would be simpler if there weren't some 2 letter tags out there...
		/// </summary>
		public bool LanguageTagContainsScriptRegionVariantInfo
		{
			get
			{
				if (SelectedLanguage == null)
					return false;

				var threeLetter = SelectedLanguage.ThreeLetterTag;
				return LanguageTag.Length >= 3 && (LanguageTag != threeLetter || threeLetter.Contains("-"));
			}
		}

		public string LanguageTagWithoutScriptRegionVariant
		{
			get
			{
				return LanguageTagContainsScriptRegionVariantInfo ? LanguageTag.Split('-')[0] : LanguageTag;
			}
		}
	}
}