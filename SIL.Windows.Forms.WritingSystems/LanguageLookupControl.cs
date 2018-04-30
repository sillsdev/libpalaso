using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using SIL.Extensions;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	public partial class LanguageLookupControl : UserControl
	{
		private readonly LanguageLookupModel _model;
		private string _lastSearchedForText;
		private Dictionary<string, string> _languageNameAliases = new Dictionary<string, string>();

		public event EventHandler ReadinessChanged;

		public void UpdateReadiness()
		{
			SetScriptsAndVariantsLabel();

			EventHandler handler = ReadinessChanged;
			if (handler != null)
				handler(this, null);
		}

		public event EventHandler DoubleClicked;

		public LanguageLookupControl()
		{
			InitializeComponent();
			_model = new LanguageLookupModel();
		}

		public void LoadLanguages()
		{
			_model.LoadLanguages();
		}

		public bool AreLanguagesLoaded
		{
			get { return _model.AreLanguagesLoaded; }
		}

		public bool IsDesiredLanguageNameFieldVisible
		{
			set { _desiredLanguageDisplayName.Visible = _desiredLanguageLabel.Visible = value; }
		}

		public bool IsShowRegionalDialectsCheckBoxVisible
		{
			set { _showRegionalDialectsCheckBox.Visible = value; }
		}

		public bool IsScriptAndVariantLinkVisible
		{
			set { _scriptsAndVariantsLink.Visible = value; }
		}

		public Func<LanguageInfo, bool> MatchingLanguageFilter
		{
			set { _model.MatchingLanguageFilter = value; }
		}

		public LanguageInfo SelectedLanguage
		{
			get { return _model.SelectedLanguage; }
			set
			{
				string oldLangTag = _model.LanguageTag;
				_model.SelectedLanguage = value;
				var newName = _model.DesiredLanguageName;
				string alias;
				if (value != null && _languageNameAliases.TryGetValue(value.LanguageTag, out alias))
				{
					_model.DesiredLanguageName = newName = alias;
					_scriptsAndVariantsLink.Enabled = _scriptsAndVariantsLink.Visible;
				}
				_desiredLanguageDisplayName.Text = newName;
				if (_model.LanguageTag != oldLangTag)
					UpdateReadiness();
			}
		}

		public string DesiredLanguageName
		{
			get { return _model.DesiredLanguageName; }
		}

		public bool HaveSufficientInformation
		{
			get { return _model.HaveSufficientInformation; }
		}

		public string SearchText
		{
			get { return _model.SearchText; }
			set { _searchText.Text = value; }
		}

		private void OnLoad(object sender, EventArgs e)
		{
			if (DesignMode)
				return;

			if (!_model.AreLanguagesLoaded)
				_model.LoadLanguages();
			if (_desiredLanguageDisplayName.Visible)
				AdjustDesiredLanguageNameFieldLocations();

			UpdateReadiness();
			_searchTimer.Start();
		}

		/// <summary>
		/// The label and textbox controls overlap on Linux/Mono.  Adjust them to not overlap.
		/// </summary>
		private void AdjustDesiredLanguageNameFieldLocations()
		{
			var labelLocation = _desiredLanguageLabel.Location;
			var labelWidth = _desiredLanguageLabel.Width;
			var nameLocation = _desiredLanguageDisplayName.Location;
			if (labelLocation.X + labelWidth + 5 < nameLocation.X)
				return;
			var newLabelLoc = new Point(nameLocation.X - (labelWidth + 6), labelLocation.Y);
			_desiredLanguageLabel.Location = newLabelLoc;
		}

		private new bool DesignMode
		{
			get
			{
				return (base.DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if(_listView.SelectedIndices != null && _listView.SelectedIndices.Count > 0)
			{
				ListViewItem item = _listView.Items[_listView.SelectedIndices[0]];
				var oldLangInfo = SelectedLanguage;
				var newLangInfo = (LanguageInfo) item.Tag;
				// If the user has already set some Script/Region/Variant info, we don't want
				// to undo that just because the listview is set to that main language in the search.
				if (_model.LanguageTagContainsScriptRegionVariantInfo &&
				    newLangInfo.LanguageTag == _model.LanguageTagWithoutScriptRegionVariant)
				{
					newLangInfo.DesiredName = oldLangInfo.DesiredName;
					newLangInfo.LanguageTag = oldLangInfo.LanguageTag;
				}
				SelectedLanguage = newLangInfo;
			}
		}

		/// <summary>
		/// ideally, people dispose of controls properly, so that this isn't needed
		/// But if you're a dialog using and you can't rely on your caller disposing
		/// of you, just call this when you're done with this controll
		/// </summary>
		public void StopTimer()
		{
			_searchTimer.Stop();
		}

		/// <summary>
		/// Requests that the specified language, if matched, should be displayed with the specified name.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="name"></param>
		public void SetLanguageAlias(string code, string name)
		{
			_languageNameAliases[code] = name;
		}

		/// <summary>
		/// Set up a filter so we don't offer codes 'zh' and 'cmn' at all, and use some more
		/// familiar names (in both English and Chinese) for the four main useful Chinese codes.
		/// </summary>
		public void UseSimplifiedChinese()
		{
			// per BL-4780 we don't offer these codes.
			// The first two are too generic to be useful.
			// The difference between zh-CN and zh-Hans, and between zh-TW and zh-Hant, is
			// too subtle for the sorts of contexts where this mode is used; zh-CN is the
			// region code for the main region where zh-Hans is used, and is the commonly used
			// code for data so encoded.
			MatchingLanguageFilter = info => info.LanguageTag != "zh" && info.LanguageTag != "cmn"
				&& info.LanguageTag!= "zh-Hans" && info.LanguageTag != "zh-Hant";
			// per BL-4780 we prefer these names for the common Chinese codes.
			// One reason is to that they sort alphabetically as Chinese.
			SetLanguageAlias("zh-CN", "Chinese, Simplified (简体中文)");
			SetLanguageAlias("zh-TW", "Chinese, Traditional (繁体中文)");
		}

		private void _searchTimer_Tick(object sender, EventArgs e)
		{
			if (_model.SearchText == _lastSearchedForText)
				return;

			_lastSearchedForText = _model.SearchText;
			_listView.SuspendLayout();

			_listView.Items.Clear();
			_listView.SelectedIndices.Clear();
			var toShow = new List<ListViewItem>();

			string multipleCountriesLabel = LocalizationManager.GetString("LanguageLookup.CountryCount", "{0} Countries", "Shown when there are multiple countries and it is just confusing to list them all. {0} is a count of countries.");

			var itemSelected = false;
			foreach (LanguageInfo lang in _model.MatchingLanguages)
			{
				var langName = lang.Names[0];
				string alias;
				if (_languageNameAliases.TryGetValue(lang.LanguageTag, out alias))
					langName = alias;
				var item = new ListViewItem(langName);
				item.SubItems.Add(lang.LanguageTag);

				// Users were having problems when they looked up things like "English" and were presented with "United Arab Emirates"
				// and such, as these colonial languages are spoken in so many countries. So this just displays the number of countries.
				// 6 or more was chosen because generally 5 languages fit in the space allowed
				string country = lang.Countries.Count > 5 ? string.Format(multipleCountriesLabel, lang.Countries.Count) : string.Join(", ", lang.Countries);
				item.SubItems.Add(country);
				item.Tag = lang;
				toShow.Add(item);

				// Keep the current selection if there is one.  This minimizes user confusion.
				if (SelectedLanguage != null && lang.LanguageTag == SelectedLanguage.LanguageTag)
				{
					item.Selected = true;
					itemSelected = true; // we want to select only one.
				}
			}
			// Select the first language if one wasn't already selected.
			if (!itemSelected && toShow.Count > 0)
			{
				toShow[0].Selected = true;
				itemSelected = true;
			}
			if (!itemSelected)
				_model.SelectedLanguage = null;

			_desiredLanguageDisplayName.Enabled = itemSelected;
			_listView.Items.AddRange(toShow.ToArray());
			//scroll down to the selected item
			if (_listView.SelectedItems.Count > 0)
				_listView.SelectedItems[0].EnsureVisible();

			_listView.ResumeLayout();
			UpdateReadiness();
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			if (DoubleClicked != null)
				DoubleClicked(this, null);
		}

		private void _cannotFindLanguageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new CannotFindMyLanguageDialog())
			{
				dlg.ShowDialog();

				_desiredLanguageDisplayName.Text = _searchText.Text.ToUpperFirstLetter();
				_searchText.Text = "?";
				if (_desiredLanguageDisplayName.Visible)
				{
					_desiredLanguageDisplayName.Select();
					_desiredLanguageDisplayName.Enabled = true;
				}
			}
		}

		private void _scriptsAndVariants_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// This is a Script link, so for now we'll limit the dialog to only the ScriptRegionVariant combo option.
			var wsSetupModel = new WritingSystemSetupModel(new WritingSystemDefinition(_model.SelectedLanguage.LanguageTag),
				WritingSystemSetupModel.SelectionsForSpecialCombo.ScriptRegionVariant, false);
			wsSetupModel.IdentifierScriptRegionVariantSelected();
			using (var dlg = new ScriptsAndVariantsDialog())
			{
				dlg.BindToModel(wsSetupModel);
				if (dlg.ShowDialog() != DialogResult.OK)
					return;
				_model.SelectedLanguage.LanguageTag = wsSetupModel.CurrentDefinition.LanguageTag;
				UpdateReadiness();
			}
		}

		private void SetScriptsAndVariantsLabel()
		{
			_scriptsAndVariantsLink.Enabled = _model.HaveSufficientInformation;
			if (!_model.HaveSufficientInformation)
			{
				_scriptsAndVariantsLabel.Visible = false;
			}
			else
			{
				if (_model.LanguageTagContainsScriptRegionVariantInfo)
				{
					_scriptsAndVariantsLabel.Text = "(" + _model.LanguageTag + ")";
					_scriptsAndVariantsLabel.Visible = true;
				}
				else
				{
					_scriptsAndVariantsLabel.Visible = false;
				}
			}
		}

		private void _desiredLanguageDisplayName_TextChanged(object sender, EventArgs e)
		{
			_model.DesiredLanguageName = _desiredLanguageDisplayName.Text;
			UpdateReadiness();
		}

		private void _searchText_TextChanged(object sender, EventArgs e)
		{
			_model.SearchText = _searchText.Text;
		}

		private void _showRegionalDialectsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			_model.IncludeRegionalDialects = _showRegionalDialectsCheckBox.Checked;
			_lastSearchedForText = null;
		}
	}
}