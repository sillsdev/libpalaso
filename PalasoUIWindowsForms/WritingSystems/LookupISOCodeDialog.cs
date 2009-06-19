using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOCodeDialog : Form
	{
		private WritingSystemDefinition.LanguageCode _selectedWritingSystem;
		private readonly IList<WritingSystemDefinition.LanguageCode> _languageCodes;
		private string _lastSearchedForText;

		public LookupISOCodeDialog()
		{
			_languageCodes = WritingSystemDefinition.LanguageCodes;
			InitializeComponent();
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
			var s = _searchText.Text.Trim().ToLowerInvariant();
			if (s == _lastSearchedForText)
			{
				return;
			}
			listView1.SuspendLayout();

			listView1.Items.Clear();
			listView1.SelectedIndices.Clear();
			var toShow = new List<ListViewItem>();

			/* This works, but the results are satisfactory yet (they could be with some enancement to the matcher
			   We would need it to favor exact prefix matches... currently an exact match could be several items down the list.

			var d = new ApproximateMatcher.GetStringDelegate<WritingSystemDefinition.LanguageCode>(c => c.Name);
			var languages = ApproximateMatcher.FindClosestForms(_languageCodes, d, s, ApproximateMatcherOptions.IncludePrefixedAndNextClosestForms);
			foreach (var language in languages)
			{
				ListViewItem item = new ListViewItem(language.Name);
				item.SubItems.Add(language.Code);
				item.Tag = language;
				toShow.Add(item);
			}
			*/

			foreach (WritingSystemDefinition.LanguageCode ws in _languageCodes)
			{
				if (string.IsNullOrEmpty(s) // in which case, show all of them
					|| (ws.Code.ToLowerInvariant().StartsWith(_searchText.Text)
					|| ws.Name.ToLowerInvariant().StartsWith(_searchText.Text)))
				{
					ListViewItem item = new ListViewItem(ws.Name);
					item.SubItems.Add(ws.Code);
					item.Tag = ws;
					toShow.Add(item);
				}

			}
			listView1.Items.AddRange(toShow.ToArray());
			listView1.ResumeLayout();
			if (listView1.Items.Count > 0)
			{
				listView1.SelectedIndices.Add(0);
			}
			_lastSearchedForText = s;
		}


	}
}