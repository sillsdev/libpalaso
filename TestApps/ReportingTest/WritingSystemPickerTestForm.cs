using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace TestApp
{
	public partial class WritingSystemPickerTestForm : Form
	{
		private WritingSystemSetupModel _wsModel;
		private IWritingSystemRepository _repository;
		public WritingSystemPickerTestForm()
		{
			InitializeComponent();

			_repository = new LdmlInFolderWritingSystemRepository();
			_wsModel = new WritingSystemSetupModel(_repository);
			_wsModel.SelectionChanged += new EventHandler(_wsModel_SelectionChanged);
			this.wsPickerUsingListView1.BindToModel(_wsModel);
			this.pickerUsingComboBox1.BindToModel(_wsModel);
		}

		void _wsModel_SelectionChanged(object sender, EventArgs e)
		{
			_currentWsLabel.Text = _wsModel.CurrentVerboseDescription;
		}

		private void OnEditWsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var d = new WritingSystemSetupDialog(_wsModel))
			{
				if (_wsModel.HasCurrentSelection)
				{
					d.ShowDialog(_wsModel.CurrentRFC4646);
				}
				else
				{
					d.ShowDialog();
				}
			}
		}
	}
}
