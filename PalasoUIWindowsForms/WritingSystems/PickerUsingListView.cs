using System;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.WritingSystems;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class PickerUsingListView : UserControl
	{
		private string _identifierOfSelectedWritingSystem;
		private static bool _ignoreCheckEvents = false;

		/// <summary>
		/// THe container can use this, for example, as a signal to close the containing box.
		/// </summary>
		public event EventHandler DoubleClicked;

		public PickerUsingListView()
		{
			InitializeComponent();
		}

		public string IdentifierOfSelectedWritingSystem
		{
			get
			{
				return _identifierOfSelectedWritingSystem;
			}
			set
			{
				_identifierOfSelectedWritingSystem = value;
			}
		}

		private void PickerUsingListView_Load(object sender, EventArgs e)
		{
			LoadDefinitions();

		}

		private Font _normalItemFont;
		private void LoadDefinitions()
		{
			_ignoreCheckEvents = true;
			if (listView1.Items.Count > 0)
			{
				//this can be empty when there are no defs and this control is re-used.
				_normalItemFont = listView1.Items[0].Font;
			}
			listView1.Items.Clear();
			Palaso.WritingSystems.LdmlInFolderWritingSystemRepository repository =
				new LdmlInFolderWritingSystemRepository();
			foreach (WritingSystemDefinition definition in repository.WritingSystemDefinitions)
			{
				ListViewItem item = new ListViewItem(definition.DisplayLabel);
				item.Tag = definition;
				item.SubItems.Add(definition.RFC4646);
				item.Checked = false;
				listView1.Items.Add(item);
				if (definition.RFC4646 == IdentifierOfSelectedWritingSystem)
				{
					_ignoreCheckEvents = false;
					item.Checked = true;
					_ignoreCheckEvents = true;
				}
				item.ToolTipText = definition.VerboseDescription;
			}

			_ignoreCheckEvents = false;
		}

		private void _editListLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WSListDialog dialog = new WSListDialog();
			dialog.ShowDialog();
			LoadDefinitions();
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listView1.SelectedItems != null && listView1.SelectedItems.Count == 1)
			{
				ListViewItem item = listView1.SelectedItems[0];
				item.Selected = false;
				item.Checked = true;
			}
		}

		private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if(_ignoreCheckEvents )
				return;
			_ignoreCheckEvents = true;
			foreach (ListViewItem item in listView1.Items)
			{
				if (item != e.Item && item.Checked == true)
				{
					item.Checked = false;
					item.Font = _normalItemFont;
				}
			}
			if (_normalItemFont != null)
			{
				e.Item.Font = new Font(_normalItemFont, FontStyle.Bold);
				e.Item.Checked = true;//needed for some reason on first display
				WritingSystemDefinition def = (WritingSystemDefinition)e.Item.Tag;
				IdentifierOfSelectedWritingSystem = def.RFC4646;
			}

			_ignoreCheckEvents = false;
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			if (DoubleClicked != null)
			{
				DoubleClicked.Invoke(this,null);
			}
		}
	}
}
