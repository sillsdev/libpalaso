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

		/// <summary>
		/// Return a full language subtag given its official iso639 code. Note that this means you must use the 2-letter code
		/// where it exists; 'ara' and 'eng' will not match.
		/// </summary>
		/// <remarks>
		/// This method may be obsolete...originally for Bloom but now so trivial Bloom just uses StandardSubtags.RegisteredLanguages.
		/// Leaving it here in case some other client has started using it.
		/// </remarks>
		/// <param name="iso639Code"></param>
		/// <returns></returns>
		public LanguageSubtag GetExactLanguageMatch(string iso639Code)
		{
			LanguageSubtag language;
			if (StandardSubtags.RegisteredLanguages.TryGet(iso639Code.ToLowerInvariant(), out language))
				return language;
			return null;
		}

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

				foreach (LanguageInfo li in _languageLookup.SuggestLanguages(_searchText).Where(li => MatchingLanguageFilter == null || MatchingLanguageFilter(li)))
					yield return li;
			}
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
	}
}