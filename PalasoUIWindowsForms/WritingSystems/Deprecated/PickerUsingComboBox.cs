using System;
using System.Windows.Forms;
using Palaso.UI.WritingSystems;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	[Obsolete]
	public partial class PickerUsingComboBox : ComboBox
	{
		private string _identifierOfSelectedWritingSystem;
		private bool _inAdding = false;

		//nb: we have to get it in the constructor because there doesn't seem
		 //to be an event we can use to load, like we normally do.... so
		//we need to know now.
		public PickerUsingComboBox()
		{
		 //   _identifierOfSelectedWritingSystem =  identifierOfSelectedWritingSystem;
			InitializeComponent();
			if (!DesignMode)
			{
				LoadDefinitions();
			}
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
				foreach (object o in Items)
				{
					DefinitionWrapper wrapper = o as DefinitionWrapper;
					if (wrapper != null && wrapper.Definition.RFC4646 == value)
					{
						SelectedItem = wrapper;
					}
				}
			}
		}


		private void LoadDefinitions()
		{
			_inAdding = true;
			Items.Clear();
			Palaso.WritingSystems.LdmlInFolderWritingSystemStore repository =
				new LdmlInFolderWritingSystemStore();
			foreach (WritingSystemDefinition definition in repository.WritingSystemDefinitions)
			{
				DefinitionWrapper item = new DefinitionWrapper(definition);
				Items.Add(item);
				if (definition.RFC4646 == IdentifierOfSelectedWritingSystem)
				{
					this.SelectedItem = item;
				}
			}
			Items.Add("---");
			Items.Add("More...");
			_inAdding = false;
		}

		class DefinitionWrapper
		{
			private readonly WritingSystemDefinition _definition;

			public DefinitionWrapper(WritingSystemDefinition definition)
			{
				_definition = definition;
			}

			public override string ToString()
			{
				return Definition.DisplayLabel + " ("+Definition.RFC4646+")";
			}

			public string ToolTipText
			{
				get
				{
					return Definition.VerboseDescription;
				}
			}

			public WritingSystemDefinition Definition
			{
				get
				{
					return _definition;
				}
			}
		}


		private void PickerUsingComboBox_MouseHover(object sender, EventArgs e)
		{
			if (SelectedIndex > -1)
			{
				DefinitionWrapper wrapper = Items[SelectedIndex] as  DefinitionWrapper;
				if (wrapper != null)
				{
					toolTip1.SetToolTip(this, wrapper.ToolTipText);
				}
			}
		}

		private void PickerUsingComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_inAdding)
			{
				return;
			}
			if (SelectedIndex == Items.Count - 1)
			{
				SelectedIndex = -1;
				Cursor.Current = Cursors.WaitCursor;
				WSListDialog dialog = new WSListDialog();
				dialog.ShowDialog();
				LoadDefinitions();
			}
			if (SelectedIndex >-1 && SelectedIndex < Items.Count - 2)
			{
				_identifierOfSelectedWritingSystem = ((DefinitionWrapper) SelectedItem).Definition.RFC4646;

			}
		}
	}
}
