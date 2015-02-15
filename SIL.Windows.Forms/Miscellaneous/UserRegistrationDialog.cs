using System;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.Windows.Forms.Miscellaneous
{
	public partial class UserRegistrationDialog : Form
	{
		public UserRegistrationDialog()
		{
			InitializeComponent();
			UpdateDisplay();
			_thePitchLabel.Text = string.Format(_thePitchLabel.Text, UsageReporter.AppNameToUseInDialogs);
			_noticeLabel.Text = string.Format(_noticeLabel.Text, UsageReporter.AppNameToUseInDialogs);
			_welcomeLabel.Text = string.Format(_welcomeLabel.Text, UsageReporter.AppNameToUseInDialogs);
		}

		private void OnOkButton(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			this.Close();
		}

		public string EmailAddress
		{
			get
			{
				return _emailAddress.Text;
			}
		}

		public bool OkToCollectBasicStats
		{
			get
			{
				return _okToPingBasicUsage.Checked;
			}
		}

		private void _emailAddress_TextChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			if (_okToPingBasicUsage.Checked)
			{
				_okButton.Enabled = _emailAddress.Text.Trim().Length > 4;
				_emailAddress.Enabled = true;
			}
			else
			{
				_okButton.Enabled = true;
				_emailAddress.Enabled = false;
			}
		}

		private void UserRegistrationDialog_Load(object sender, EventArgs e)
		{

		}

		private void _okToPingBasicUsage_CheckedChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void label3_Click(object sender, EventArgs e)
		{
			_okToPingBasicUsage.Checked = ! _okToPingBasicUsage.Checked;
		}

	}
}