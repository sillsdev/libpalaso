using System;
using System.Windows.Forms;
using static System.String;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ChooseProject : Form
	{
		public string SelectedProject { get; set; }

		public ChooseProject()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateDisplay();
		}

		private void _projectsList_SelectedProjectChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			_okButton.Enabled = !IsNullOrEmpty(_projectsList.SelectedProject);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			if (_projectsList.SelectedProject == null)
				throw new NullReferenceException("Should not be able to click OK if no project is selected.");

			SelectedProject = _projectsList.SelectedProject;

			DialogResult = DialogResult.OK;
			Close();
		}

		private void _projectsList_DoubleClick(object sender, EventArgs e)
		{
			_okButton_Click(this, null);
		}
	}
}
