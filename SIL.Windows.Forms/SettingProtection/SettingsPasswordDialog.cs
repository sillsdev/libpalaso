using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.SettingProtection
{
	public partial class SettingsPasswordDialog : Form
	{
		private readonly string _password;

		public enum Mode
		{
			Challenge,
			MakeSureTheyKnowPassword
		};

		public SettingsPasswordDialog(string password, Mode mode)
		{
			_password = password;
			InitializeComponent();
			_okButton.Enabled = false;

			if(mode == Mode.MakeSureTheyKnowPassword)
			{
				_explanation.Text = "Let's make sure that you know the factory password before we lock things down.";
			}
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			_okButton.Enabled = passwordBox.Text == _password;
		}
	}
}
