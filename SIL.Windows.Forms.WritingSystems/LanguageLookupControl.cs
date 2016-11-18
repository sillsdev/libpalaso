﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
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

		public event EventHandler ReadinessChanged;

		public void UpdateReadiness()
		{
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
				_desiredLanguageDisplayName.Text = _model.DesiredLanguageName;
				if(_model.LanguageTag != oldLangTag)
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
			AdjustCannotFindLanguageLocation();

			UpdateReadiness();
			_searchTimer.Start();
		}

		/// <summary>
		/// The label and textbox controls overlap on Linux/Mono.  Adjust them to not overlap.
		/// </summary>
		private void AdjustDesiredLanguageNameFieldLocations()
		{
			Point labelLocation = _desiredLanguageLabel.Location;
			int labelWidth = _desiredLanguageLabel.Width;
			Point nameLocation = _desiredLanguageDisplayName.Location;
			if (labelLocation.X + labelWidth + 5 >= nameLocation.X)
			{
				var newLabelLoc = new Point(_listView.Location.X, labelLocation.Y);
				_desiredLanguageLabel.Location = newLabelLoc;
				if(newLabelLoc.X + labelWidth + 5 >= nameLocation.X)
				{
					var newNameLoc = new Point(newLabelLoc.X + labelWidth + 6, nameLocation.Y);
					_desiredLanguageDisplayName.Location = newNameLoc;
				}
			}
		}

		/// <summary>
		/// The link label for "Cannot find language?" is truncated on Linux/Mono.
		/// Adjust the location to allow it to display properly.
		/// </summary>
		private void AdjustCannotFindLanguageLocation()
		{
			//_cannotFindLanguageLink
			Point labelLocation = _cannotFindLanguageLink.Location;
			int labelWidth = _cannotFindLanguageLink.Width;
			int shortage = labelLocation.X + labelWidth - Width;
			if (shortage > 0)
			{
				var newLoc = new Point(labelLocation.X - shortage, labelLocation.Y);
				_cannotFindLanguageLink.Location = newLoc;
			}
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
			string oldLangTag = _model.LanguageTag;
			if(_listView.SelectedIndices != null && _listView.SelectedIndices.Count > 0)
			{
				ListViewItem item = _listView.Items[_listView.SelectedIndices[0]];
				_model.SelectedLanguage = (LanguageInfo) item.Tag;
				_desiredLanguageDisplayName.Text = _model.DesiredLanguageName;
			}
			if (_model.LanguageTag != oldLangTag)
				UpdateReadiness();
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
				var item = new ListViewItem(lang.Names[0]);
				item.SubItems.Add(lang.LanguageTag);

				// Users were having problems when they looked up things like "English" and were presented with "United Arab Emirates"
				// and such, as these colonial languages are spoken in so many countries. So this just displays the number of countries.
				// 3 or more was chosen because generally 2 languages fit in the space allowed
				string country = lang.Countries.Count > 2 ? string.Format(multipleCountriesLabel, lang.Countries.Count) : string.Join(", ", lang.Countries);
				item.SubItems.Add(country);
				item.SubItems.Add(string.Join(", ", lang.Names.Skip(1)));
				item.Tag = lang;
				toShow.Add(item);

				if (!itemSelected)
				{
					item.Selected = true;
					itemSelected = true; //we only want to select the first one
				}
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