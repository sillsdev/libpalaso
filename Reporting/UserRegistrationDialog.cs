using System;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public partial class UserRegistrationDialog : Form
	{
		public UserRegistrationDialog()
		{
			InitializeComponent();
			UpdateThings();
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

		private void _emailAddress_TextChanged(object sender, EventArgs e)
		{
			UpdateThings();
		}

		private void UpdateThings()
		{
			_okButton.Enabled = _emailAddress.Text.Trim().Length > 4;
		}

		private void UserRegistrationDialog_Load(object sender, EventArgs e)
		{

		}

	}
}