using System;
using System.Windows.Forms;
using Palaso.UI.WritingSystems;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class PickerUsingListView : UserControl
	{
		private string _identifierOfSelectedWritingSystem;

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

		private void LoadDefinitions()
		{
			listView1.Items.Clear();
			Palaso.WritingSystems.LdmlInFolderWritingSystemRepository repository =
				new LdmlInFolderWritingSystemRepository();
			foreach (WritingSystemDefinition definition in repository.WritingSystemDefinitions)
			{
				ListViewItem item = new ListViewItem(definition.DisplayLabel);
				item.Tag = definition;
				item.SubItems.Add(definition.RFC4646);
				listView1.Items.Add(item);
				if (definition.RFC4646 == IdentifierOfSelectedWritingSystem)
				{
					item.Selected = true;
				}
			}
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
				WritingSystemDefinition def = (WritingSystemDefinition)listView1.SelectedItems[0].Tag;
				IdentifierOfSelectedWritingSystem = def.RFC4646;
			}
		}
	}
}
