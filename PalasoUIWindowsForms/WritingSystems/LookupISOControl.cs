using System;
using System.Collections.Generic;
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

		public Iso639LanguageCode ISOCodeAndName
		{
			get { return _model.ISOCodeAndName; }
		}

		public string ISOCode
		{
			get { return _model.ISOCode; }
			set { _searchText.Text  = value; }
		}

		private void OnLoad(object sender, EventArgs e)
		{
			UpdateReadiness();
			_searchTimer.Start();
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			var oldIso = _model.ISOCode;
			if (_listView.SelectedIndices != null && _listView.SelectedIndices.Count > 0)
			{
				ListViewItem item = _listView.Items[_listView.SelectedIndices[0]];
				_model.ISOCodeAndName = item.Tag as Iso639LanguageCode;

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
			var itemSelected = false;
			foreach(Iso639LanguageCode lang in _model.GetMatchingWritingSystems(typedText))
			{
					ListViewItem item = new ListViewItem(lang.Name);
					item.SubItems.Add(lang.Code);
					item.Tag = lang;
					toShow.Add(item);

					if(!itemSelected && typedText.Length>1 &&
						(lang.Code.ToLower() == typedText || lang.Name.ToLower().StartsWith(typedText.ToLower())))
					{
						item.Selected = true;
						itemSelected = true;//we only want to select the first one
					}
			}
			_listView.Items.AddRange(toShow.ToArray());
			_listView.ResumeLayout();
//            if (_listView.Items.Count > 0)
//            {
//                _listView.SelectedIndices.Add(0);
//            }

			if(_model.ISOCode != oldIso)
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
				foreach (ListViewItem item in _listView.Items)
				{
					var tag = item.Tag as Iso639LanguageCode;
					if (tag.ISO3Code == "qaa")
					{
						_listView.Select();
					}
				}
			}
		}
	}
}
