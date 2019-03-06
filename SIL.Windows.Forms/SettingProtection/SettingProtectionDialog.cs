using System;
using System.Windows.Forms;
using L10NSharp;

namespace SIL.Windows.Forms.SettingProtection
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

			if (SettingsProtectionSingleton.ProductSupportUrl == null)
			{
				_passwordNotice.Text = string.Format(_passwordNotice.Text, SettingsProtectionSingleton.FactoryPassword,
					SettingsProtectionSingleton.CoreProductName);
			}
			else
			{
				// The wording here should be kept essentially in sync with "SettingsProtection.PasswordNotice" in the Designer file.
				_passwordNotice.Text = string.Format(LocalizationManager.GetString("SettingsProtection.PasswordNoticeWithSupportUrl",
					"Factory password for these settings is \"{0}\". If you forget it, you can always visit the {1} support page: {2}",
					"The localization for this should be kept in sync with \"SettingsProtection.PasswordNotice\". Param 0: Factory password; " +
					"Param 1: product name; Param 2: URL of support page"),
					SettingsProtectionSingleton.FactoryPassword, SettingsProtectionSingleton.CoreProductName, SettingsProtectionSingleton.ProductSupportUrl);
			}
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
