using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.SettingProtection
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
			textBox1.PasswordChar = '●';
			_okButton.Enabled = false;

			if(mode == Mode.MakeSureTheyKnowPassword)
			{
				_explanation.Text = "Let's make sure that you know the factory password before we lock things down.";
			}
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			textBox1.PasswordChar = (char) (checkBox1.Checked ? 0 : '●');
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
			_okButton.Enabled = textBox1.Text == _password;
		}
	}
}
