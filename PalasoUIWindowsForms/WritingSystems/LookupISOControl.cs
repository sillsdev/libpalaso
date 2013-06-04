using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOControl : UserControl
	{
		private LookupIsoCodeModel _model;
		private string _lastSearchedForText;
		public event EventHandler ReadinessChanged;

		public void UpdateReadiness()
		{
			EventHandler handler = ReadinessChanged;
			if (handler != null) handler(this, null);
		}

		public event EventHandler DoubleClicked;

		public LookupISOControl()
		{
			InitializeComponent();
			_model = new LookupIsoCodeModel();
		}

		public LanguageInfo LanguageInfo
		{
			get { return _model.LanguageInfo; }
		}

		public string ISOCode
		{
			get { return _model.ISOCode; }
			set { _searchText.Text  = value; }
		}

		private void OnLoad(object sender, EventArgs e)
		{
			if (DesignMode)
				return;
			UpdateReadiness();
			_searchTimer.Start();
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

					if (!itemSelected && typedText.Length > 1 &&
						(lang.Code.ToLower() == typedText || lang.Names[0].ToLower().StartsWith(typedText.ToLower())))
					{
						item.Selected = true;
						itemSelected = true; //we only want to select the first one
					}
				}
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

				//select the unlisted guy for them
//				foreach (ListViewItem item in _listView.Items)
//				{
//					var tag = item.Tag as Iso639LanguageCode;
//					if (tag.ISO3Code == "qaa")
//					{
//						_listView.Select();
//					}
//				}

				_searchText.Text = "?";
			}
		}
	}
}
