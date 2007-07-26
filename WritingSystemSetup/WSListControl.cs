using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Palaso
{
	public partial class WSListControl : UserControl
	{
		private WritingSystemRepository _repository;

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
			foreach (WSListItem item in _writingSystemList.Items)
			{
				item.SaveToWritingSystemDefinition();
				_repository.SaveDefinition(item.Definition);
			}
		}

		public void LoadFromRepository(WritingSystemRepository repository)
		{
			_repository = repository;
			_writingSystemList.Clear();
			foreach (WritingSystemDefinition ws in repository.WritingSystemDefinitions)
			{
				WSListItem item = new WSListItem(ws);
				_writingSystemList.AddControlToBottom(item);
			}
			_writingSystemList.LayoutRows();
		}

		private void OnAddNewClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WritingSystemDefinition ws = _repository.CreateNewDefinition();
			WSListItem item = new WSListItem(ws);
			_writingSystemList.AddControlToBottom(item);
			item.Selected = true;
			this.ScrollControlIntoView(item);
		}
	}
}
