using System;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace TestApp
{
	public partial class WritingSystemPickerTestForm : Form
	{
		private readonly WritingSystemSetupModel _wsModel;
		private readonly IWritingSystemRepository _repository;
		public WritingSystemPickerTestForm()
		{
			InitializeComponent();

			_repository = GlobalWritingSystemRepository.Initialize(null);
			_wsModel = new WritingSystemSetupModel(_repository);
			_wsModel.SelectionChanged += _wsModel_SelectionChanged;
			wsPickerUsingListView1.BindToModel(_wsModel);
			pickerUsingComboBox1.BindToModel(_wsModel);
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
