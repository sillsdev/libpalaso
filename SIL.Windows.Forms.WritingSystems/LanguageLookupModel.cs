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
		public EventHandler SelectedLanguageChanged;

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
			_languageLookup = new LanguageLookup();
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

				foreach (LanguageInfo li in _languageLookup.SuggestLanguages(_searchText).Where(li => (MatchingLanguageFilter == null || MatchingLanguageFilter(li)) && RegionalDialectsFilter(li)))
					yield return li;
			}
		}

		private bool RegionalDialectsFilter(LanguageInfo li)
		{
			if (IncludeRegionalDialects)
				return true;

			// always include Chinese languages with region codes
			if (li.LanguageTag.IsOneOf("zh-CN", "zh-TW"))
				return true;

			return string.IsNullOrEmpty(IetfLanguageTag.GetRegionPart(li.LanguageTag));
		}

		public LanguageInfo SelectedLanguage
		{
			get { return _selectedLanguage; }
			set
			{
				var oldValue = _selectedLanguage;
				_selectedLanguage = value;
				if (_selectedLanguage == null)
				{
					RaiseSelectedLanguageChanged(oldValue);
					return;
				}
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

				RaiseSelectedLanguageChanged(oldValue);
			}
		}

		private void RaiseSelectedLanguageChanged(LanguageInfo oldValue)
		{
			if (SelectedLanguageChanged != null && _selectedLanguage != oldValue)
			{
				SelectedLanguageChanged.Invoke(this, EventArgs.Empty);
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
	}
}