using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOCodeDialog : Form
	{
		private string _initialISOCode;
		private WritingSystemDefinition.LanguageCode _selectedCode;
		private int _indexOfInitiallySelectedCode=-1;
		private readonly IList<WritingSystemDefinition.LanguageCode> _languageCodes;

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
//                if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
//                {
//                    ListViewItem item = listView1.Items[listView1.SelectedIndices[0]];
//                    return item.Tag as WritingSystemDefinition.LanguageCode;
//                }
//                return null;
				return _selectedCode;
			}
		}

		public string ISOCode
		{
			get
			{
				if(ISOCodeAndName ==null)
					return string.Empty;
				return ISOCodeAndName.Code;
			}
			set
			{
				_initialISOCode = value;
				_indexOfInitiallySelectedCode = 0;
				foreach (WritingSystemDefinition.LanguageCode code in _languageCodes)
				{
					if (code.Code == _initialISOCode)
					{
						break;
					}
					_indexOfInitiallySelectedCode++;
				}
				if (_indexOfInitiallySelectedCode >= _languageCodes.Count)
				{
					_indexOfInitiallySelectedCode = -1;
				}
			}
		}

		private void LookupISOCodeDialog_Load(object sender, EventArgs e)
		{
			this.listView1.VirtualListSize = _languageCodes.Count;
			listView1.SelectedIndices.Clear();
			if (_indexOfInitiallySelectedCode > 0)
			{
				listView1.SelectedIndices.Add(_indexOfInitiallySelectedCode);
				listView1.EnsureVisible(_indexOfInitiallySelectedCode);
			}
			else
			{  //select the first to make it easy to just start typing (NOTE: typing doesn't work with virtual mode anyhow)
				listView1.SelectedIndices.Add(0);
			}
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			_okButton_Click(sender, e);
		}

		private void _aboutLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.sil.org/iso639-3/");
		}

		private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			WritingSystemDefinition.LanguageCode code= _languageCodes[e.ItemIndex];
			e.Item = new ListViewItem(code.Name);
			e.Item.SubItems.Add(code.Code);
			e.Item.Tag = code;
			if (code.Code == _initialISOCode)
			{
				_selectedCode = code;
				e.Item.Selected = true;
				//none of this worked to get the selected item into view
				//listView1.TopItem = e.Item;
				//e.Item.Focused = true;
			}
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
			{
				ListViewItem item = listView1.Items[listView1.SelectedIndices[0]];
				_selectedCode = item.Tag as WritingSystemDefinition.LanguageCode;
			}
		}

		private void listView1_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
		{
			if (!e.IsTextSearch)
				return;
			int increment = 1;
			if (e.Direction == SearchDirectionHint.Up || e.Direction == SearchDirectionHint.Left)
			{
				increment = -1;
			}
			string searchText = e.Text.ToLowerInvariant();
			int startIndex = e.StartIndex;
			if (startIndex == -1 && increment == 1)
			{
				startIndex = 0;
			}
			else if (startIndex == -1 && increment == -1)
			{
				startIndex = _languageCodes.Count - 1;
			}
			bool first = true;
			for (int i=startIndex; i != startIndex || first; i += increment)
			{
				first = false;
				if (i < 0)
				{
					i = _languageCodes.Count - 1;
				}
				else if (i >= _languageCodes.Count)
				{
					i = 0;
				}
				if ((e.IsPrefixSearch && _languageCodes[i].Name.ToLowerInvariant().StartsWith(searchText))
					|| (!e.IsPrefixSearch && _languageCodes[i].Name.ToLowerInvariant().Contains(searchText)))
				{
					e.Index = i;
					return;
				}
			}
		}

		private void _aboutLink639_1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.infoterm.info/standardization/iso_639_1_2002.php");
		}
	}
}