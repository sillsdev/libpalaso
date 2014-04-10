﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Forms;
using Palaso.Extensions;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOControl : UserControl
	{
		private LookupIsoCodeModel _model;
		private string _lastSearchedForText;
		private string _unlistedLanguageName;
		private string _incomingDesiredName;
		private LanguageInfo _incomingLanguageInfo;
		public event EventHandler ReadinessChanged;

		/// <summary>Force the dialog to return 3 letter iso codes even if a 2 letter code is available</summary>
		public bool Force3LetterCodes
		{
			get { return _model.Force3LetterCodes; }
			set { _model.Force3LetterCodes = value; }
		}

		public void UpdateReadiness()
		{
			EventHandler handler = ReadinessChanged;
			if (handler != null) handler(this, null);
		}

		public event EventHandler DoubleClicked;

		public LookupISOControl()
		{
			InitializeComponent();
			ShowDesiredLanguageNameField = true;
			_model = new LookupIsoCodeModel();
			_unlistedLanguageName = L10NSharp.LocalizationManager.GetString("LanguageLookup.UnlistedLanguage", "Unlisted Language");
		}

		public bool ShowDesiredLanguageNameField
		{
			set { _desiredLanguageDisplayName.Visible = _desiredLanguageLabel.Visible= value; }
		}

		public LanguageInfo LanguageInfo
		{
			get { return _model.LanguageInfo; }
			set { _model.LanguageInfo = value; }
		}

		public bool HaveSufficientInformation
		{
			get
			{
				return LanguageInfo != null &&
				_desiredLanguageDisplayName.Text != _unlistedLanguageName &&
					_desiredLanguageDisplayName.Text.Trim().Length > 0;
			}
		}

		public string ISOCode
		{
			get { return _model.ISOCode; }
			set { _searchText.Text  = value; }
		}

		string _initialDesiredName;
		/// <summary>
		/// Get the desired name of the language found, or set the name of the language to search for.
		/// </summary>
		public string DesiredLanguageName
		{
			get { return _desiredLanguageDisplayName.Text.Trim(); }
			set { _initialDesiredName = value; }
		}

		private void OnLoad(object sender, EventArgs e)
		{
			if (DesignMode)
				return;
			if (_model.LanguageInfo != null)
			{
				_searchText.Text = _model.LanguageInfo.Code;
				if (!string.IsNullOrEmpty(_model.LanguageInfo.DesiredName))
				{
					_incomingLanguageInfo = _model.LanguageInfo;
					_desiredLanguageDisplayName.Text = _model.LanguageInfo.DesiredName;
				}
			}
			else if (!String.IsNullOrWhiteSpace(_initialDesiredName))
			{
				_searchText.Text = _initialDesiredName;
				_desiredLanguageDisplayName.Text = _initialDesiredName;
			}
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
			var labelLocation = _desiredLanguageLabel.Location;
			var labelWidth = _desiredLanguageLabel.Width;
			var nameLocation = _desiredLanguageDisplayName.Location;
			if (labelLocation.X + labelWidth + 5 >= nameLocation.X)
			{
				var newLabelLoc = new System.Drawing.Point(_listView.Location.X, labelLocation.Y);
				_desiredLanguageLabel.Location = newLabelLoc;
				if (newLabelLoc.X + labelWidth + 5 >= nameLocation.X)
				{
					var newNameLoc = new System.Drawing.Point(newLabelLoc.X + labelWidth + 6, nameLocation.Y);
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
			var labelLocation = _cannotFindLanguageLink.Location;
			var labelWidth = _cannotFindLanguageLink.Width;
			var shortage = labelLocation.X + labelWidth - this.Width;
			if (shortage > 0)
			{
				var newLoc = new System.Drawing.Point(labelLocation.X - shortage, labelLocation.Y);
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
			var oldIso = _model.ISOCode;
			if (_listView.SelectedIndices != null && _listView.SelectedIndices.Count > 0)
			{
				ListViewItem item = _listView.Items[_listView.SelectedIndices[0]];
				_model.LanguageInfo = item.Tag as LanguageInfo;

				if (_incomingLanguageInfo != null && _incomingLanguageInfo.Code == _model.LanguageInfo.Code && !string.IsNullOrEmpty(_incomingLanguageInfo.DesiredName))
				{
					_desiredLanguageDisplayName.Text = _incomingLanguageInfo.DesiredName;
				}
				else if (_model.ISOCode == "qaa" && !String.IsNullOrEmpty(_initialDesiredName))
				{
					_desiredLanguageDisplayName.Text = _initialDesiredName;
					_model.LanguageInfo.Names.Insert(0, _initialDesiredName);
					_model.LanguageInfo.DesiredName = _initialDesiredName;
				}
				else
				{
					_desiredLanguageDisplayName.Text = _model.LanguageInfo.Names[0];
					//now if they were typing another form, well then that form makes a better default "Desired Name" than the official primary name
					foreach (var name in _model.LanguageInfo.Names)
					{
						if (name.ToLowerInvariant().StartsWith(_searchText.Text.ToLowerInvariant()))
						{
							_desiredLanguageDisplayName.Text = name;
							break;
						}
					}
				}
			}
			if (_model.ISOCode != oldIso)
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
			var oldIso = _model.ISOCode;
			var typedText = _searchText.Text.Trim();
			if (typedText == _lastSearchedForText)
			{
				return;
			}
			_lastSearchedForText = typedText;
			_listView.SuspendLayout();

			_listView.Items.Clear();
			_listView.SelectedIndices.Clear();
			var toShow = new List<ListViewItem>();


			if (_searchText.Text == "?")
			{
				var description = L10NSharp.LocalizationManager.GetString("LanguageLookup.UnlistedLanguage", "Unlisted Language");
				List<string> names = new List<string>(new string[] {description});
				LanguageInfo unlistedLanguage = new LanguageInfo() {Code = "qaa", Country = "", Names = names};
				ListViewItem item = new ListViewItem(description);
				item.SubItems.Add("qaa");
				item.Tag = unlistedLanguage;
				item.Selected = true;
				_listView.Items.Add(item);

			}
			else
			{
				var itemSelected = false;
				foreach (LanguageInfo lang in _model.GetMatchingLanguages(typedText))
				{
					ListViewItem item = new ListViewItem(lang.Names[0]);
					item.SubItems.Add(lang.Code);
					item.SubItems.Add(lang.Country);
					item.SubItems.Add(string.Join(", ", lang.Names.Skip(1)));
					item.SubItems.Add(lang.Country);
					item.Tag = lang;
					toShow.Add(item);

//					if (!itemSelected && typedText.Length > 1 &&
//					    (lang.Code.ToLower() == typedText || lang.Names[0].ToLower().StartsWith(typedText.ToLower())))
					if (!itemSelected)
					{
						item.Selected = true;
						itemSelected = true; //we only want to select the first one
					}
				}
				if (!itemSelected)
				{
					_model.LanguageInfo = null;
					//_desiredLanguageDisplayName.Text = _searchText.Text;
				}
				_desiredLanguageDisplayName.Enabled = itemSelected;
				_listView.Items.AddRange(toShow.ToArray());

			}
			_listView.ResumeLayout();
			//            if (_listView.Items.Count > 0)
			//            {
			//                _listView.SelectedIndices.Add(0);
			//            }			if(_model.ISOCode != oldIso)
			UpdateReadiness();
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			if (DoubleClicked != null)
			{
				DoubleClicked(this, null);
			}
		}

		private void _cannotFindLanguageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new CannotFindMyLanguageDialog())
			{
				dlg.ShowDialog();

				_desiredLanguageDisplayName.Text = _searchText.Text.ToUpperFirstLetter();
				if (String.IsNullOrEmpty(_initialDesiredName))
					_initialDesiredName = _desiredLanguageDisplayName.Text;
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
			if(_model.LanguageInfo!=null)
				_model.LanguageInfo.DesiredName = _desiredLanguageDisplayName.Text;
			UpdateReadiness();
		}
	}
}
