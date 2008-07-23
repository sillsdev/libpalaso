using System;
using System.Drawing;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	internal partial class WSListControl : UserControl
	{
		private LdmlInFolderWritingSystemStore _repository;
		private WritingSystemDefinition _writingSystemForDeletionUndo;

		public WSListControl()
		{
			InitializeComponent();
			if(DesignMode)
			{
				return;
			}
		}

		public void SaveChangesToWSFiles()
		{
//            foreach (WSListItem item in _writingSystemList.Items)
//            {
//                item.PushToWritingSystemDefinition();
//            }

			_repository.Save();
		}

		public void LoadFromRepository(LdmlInFolderWritingSystemStore repository)
		{
			_repository = repository;
			_writingSystemList.Clear();
			foreach (WritingSystemDefinition ws in repository.WritingSystemDefinitions)
			{
				AddWritingSystem(ws);
			}
			_writingSystemList.LayoutRows();
			UpdateDisplay();
		}

		private WSListItem AddWritingSystem(WritingSystemDefinition ws)
		{
			WSListItem item = new WSListItem(ws);
			AddControl(item);
			return item;
		}

		private void AddControl(WSListItem item)
		{
			item.DuplicateRequested += new EventHandler(OnDuplicateRequested);
			item.DeleteRequested += new EventHandler(OnDeleteRequested);
			_writingSystemList.AddControlToBottom(item);
		}

		void OnDeleteRequested(object sender, EventArgs e)
		{
			WSListItem item = (WSListItem) sender;
			item.Definition.MarkedForDeletion = true;
			_writingSystemForDeletionUndo = item.Definition;
			_writingSystemList.RemoveControl(item);
			UpdateDisplay();
		}

		void OnDuplicateRequested(object sender, EventArgs e)
		{
			WritingSystemDefinition ws= _repository.MakeDuplicate(((WSListItem) sender).Definition);
			WSListItem item = new WSListItem(ws);
			AddControl(item);
			item.Selected = true;
		}

		private void OnAddNewClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WritingSystemDefinition ws = _repository.CreateNew();
			WSListItem item = AddWritingSystem(ws);
			item.Selected = true;
			this.ScrollControlIntoView(item);
		}

		//doesn't get called
		protected override Point ScrollToControl(Control activeControl)
		{
			return new Point(0, 100);
		}

		private void OnUndoDeleteLabel(object sender, LinkLabelLinkClickedEventArgs e)
		{
			_writingSystemForDeletionUndo.MarkedForDeletion = false;
		   WSListItem item = AddWritingSystem(_writingSystemForDeletionUndo);
			_writingSystemForDeletionUndo = null;
			item.Selected = true;
			//this.Scroll(this, new ScrollEventArgs(ScrollEventType.)
			//this.ScrollToControl();

			//doesn't work: item.AutoScrollOffset = new Point(0,-300);
			this.ScrollControlIntoView(item);
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			_undoDeleteLabel.Visible = _writingSystemForDeletionUndo != null;
		}
	}
}