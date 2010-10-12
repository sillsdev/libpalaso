using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOCodeDialog : Form
	{
		private WritingSystemDefinition.LanguageCode _selectedWritingSystem;
		private string _lastSearchedForText;
		private LookupIsoCodeModel _model;
		public LookupISOCodeDialog()
		{
			InitializeComponent();
			_model = new LookupIsoCodeModel();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		public WritingSystemDefinition.LanguageCode ISOCodeAndName
		{
			get
			{
				return _selectedWritingSystem;
			}
		}

		public string ISOCode
		{
			get
			{
				if (ISOCodeAndName == null)
					return string.Empty;
				return ISOCodeAndName.Code;
			}
			set
			{
				_searchText.Text = value;
			}
		}

		private void LookupISOCodeDialog_Load(object sender, EventArgs e)
		{
			_searchTimer.Enabled = true;
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			_okButton_Click(sender, e);
		}

		private void _aboutLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.sil.org/iso639-3/");
		}


		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
			{
				ListViewItem item = listView1.Items[listView1.SelectedIndices[0]];
				_selectedWritingSystem = item.Tag as WritingSystemDefinition.LanguageCode;
			}
		}

		private void _aboutLink639_1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.infoterm.info/standardization/iso_639_1_2002.php");
		}

		private void _searchTimer_Tick(object sender, EventArgs e)
		{
			var typedText = _searchText.Text.Trim();
			if (typedText == _lastSearchedForText)
			{
				return;
			}
			_lastSearchedForText = typedText;
			listView1.SuspendLayout();

			listView1.Items.Clear();
			listView1.SelectedIndices.Clear();
			var toShow = new List<ListViewItem>();

			foreach(WritingSystemDefinition.LanguageCode lang in _model.GetMatchingWritingSystems(typedText))
			{
					ListViewItem item = new ListViewItem(lang.Name);
					item.SubItems.Add(lang.Code);
					item.Tag = lang;
					toShow.Add(item);
			}
			listView1.Items.AddRange(toShow.ToArray());
			listView1.ResumeLayout();
			if (listView1.Items.Count > 0)
			{
				listView1.SelectedIndices.Add(0);
			}

			_okButton.Enabled = listView1.Items.Count > 0 && listView1.SelectedItems.Count == 1;
		}
	}
}