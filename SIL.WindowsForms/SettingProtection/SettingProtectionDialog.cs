using System;
using System.Windows.Forms;

namespace SIL.WindowsForms.SettingProtection
{
	public partial class SettingProtectionDialog : Form
	{
		private bool _didHavePasswordSet;

		public SettingProtectionDialog()
		{
			InitializeComponent();
			_normallyHiddenCheckbox.Checked = SettingsProtectionSingleton.Settings.NormallyHidden;
			_requirePasswordCheckBox.Checked = SettingsProtectionSingleton.Settings.RequirePassword;

			_didHavePasswordSet = SettingsProtectionSingleton.Settings.RequirePassword;

			_passwordNotice.Text = string.Format(_passwordNotice.Text, SettingsProtectionSingleton.FactoryPassword,
												 Application.ProductName);
		}

		private void OnNormallHidden_CheckedChanged(object sender, EventArgs e)
		{
			SettingsProtectionSingleton.Settings.NormallyHidden = _normallyHiddenCheckbox.Checked;
			_image.Image = SettingsProtectionSingleton.GetImage(48);
		}

		private void OnRequirePasswordCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SettingsProtectionSingleton.Settings.RequirePassword = _requirePasswordCheckBox.Checked;
			_image.Image = SettingsProtectionSingleton.GetImage(48);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			if(!_didHavePasswordSet && SettingsProtectionSingleton.Settings.RequirePassword)
			{
				using(var dlg = new SettingsPasswordDialog(SettingsProtectionSingleton.FactoryPassword, SettingsPasswordDialog.Mode.MakeSureTheyKnowPassword))
				{
					if (DialogResult.Cancel == dlg.ShowDialog())
						return; //they couldn't come up with the password
				}
			}

			SettingsProtectionSingleton.Settings.Save();
			Close();
		}
	}
}
