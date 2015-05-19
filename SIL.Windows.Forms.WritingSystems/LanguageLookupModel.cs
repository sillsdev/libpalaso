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
		private LanguageLookup _languageLookup;
		private string _searchText;
		private LanguageInfo _selectedLanguage;
		private static readonly string UnlistedLanguageName = LocalizationManager.GetString("LanguageLookup.UnlistedLanguage", "Unlisted Language");

		/// <summary>
		/// Maybe this doesn't belong here... using it in Bloom which is more concerned with language than writing system
		/// </summary>
		/// <param name="iso639Code"></param>
		/// <returns></returns>
		public LanguageSubtag GetExactLanguageMatch(string iso639Code)
		{
			LanguageSubtag language;
			if (StandardSubtags.TryGetLanguageFromIso3Code(iso639Code, out language) && language.Code != language.Iso3Code)
				return language;
			return null;
		}

		public string SearchText
		{
			get { return _searchText; }
			set { _searchText = value.Trim(); }
		}

		public bool HaveSufficientInformation
		{
			get
			{
				return SelectedLanguage != null && _selectedLanguage.DesiredName != UnlistedLanguageName
					&& _selectedLanguage.DesiredName.Length > 0;
			}
		}

		public string DesiredLanguageName
		{
			get
			{
				if (_selectedLanguage == null || _selectedLanguage.DesiredName == null)
					return string.Empty;
				return _selectedLanguage.DesiredName;
			}
			set { _selectedLanguage.DesiredName = value.Trim(); }
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

				if (_languageLookup == null)
					_languageLookup = new LanguageLookup();

				foreach (LanguageInfo li in _languageLookup.SuggestLanguages(_searchText))
					yield return li;
			}
		}

		public LanguageInfo SelectedLanguage
		{
			get { return _selectedLanguage; }
			set
			{
				_selectedLanguage = value;
				if (_selectedLanguage == null)
					return;

				if (LanguageTag == "qaa")
				{
					if (_searchText != "?")
					{
						string failedSearchText = _searchText.ToUpperFirstLetter();
						_selectedLanguage.DesiredName = failedSearchText;
						_selectedLanguage.Names.Insert(0, failedSearchText);
					}
				}
				else
				{
					IList<string> names = _selectedLanguage.Names;
					//now if they were typing another form, well then that form makes a better default "Desired Name" than the official primary name
					_selectedLanguage.DesiredName = names.FirstOrDefault(n => n.StartsWith(_searchText, StringComparison.InvariantCultureIgnoreCase)) ?? names[0];
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