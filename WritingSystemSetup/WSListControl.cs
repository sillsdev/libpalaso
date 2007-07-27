using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso
{
	public partial class WSListControl : UserControl
	{
		private LdmlInFolderWritingSystemRepository _repository;

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

			_repository.SaveDefinitions();
		}

		public void LoadFromRepository(LdmlInFolderWritingSystemRepository repository)
		{
			_repository = repository;
			_writingSystemList.Clear();
			foreach (WritingSystemDefinition ws in repository.WritingSystemDefinitions)
			{
				AddControl(new WSListItem(ws));
			}
			_writingSystemList.LayoutRows();
		}

		private void AddControl(WSListItem item)
		{
			item.DuplicateRequested += new EventHandler(OnDuplicateRequested);
			_writingSystemList.AddControlToBottom(item);
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
			WritingSystemDefinition ws = _repository.AddNewDefinition();
			WSListItem item = new WSListItem(ws);
			AddControl(item);
			item.Selected = true;
			this.ScrollControlIntoView(item);
		}
	}
}
