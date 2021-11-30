using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Tests.SettingsProtection
{
	public partial class DialogWithLinkToSettings : Form
	{
		public DialogWithLinkToSettings()
		{
			InitializeComponent();
			//settingsLauncherButton2.LaunchSettingsCallback = () => new DialogWithSomeSettings().ShowDialog();

			//Let the helper manage our visibility & password challenge
			//_settingsProtectionHelper.CustomSettingsControl = _customSettingsButton;


			_settingsProtectionHelper.ManageComponent(_customSettingsButton);

			_settingsProtectionHelper.ManageComponent(_toolStripButtonToHide);
		}

		private void _customSettingsButton_Click(object sender, EventArgs e)
		{
			_settingsProtectionHelper.LaunchSettingsIfAppropriate(() =>
				{
					using (var dlg = new DialogWithSomeSettings())
					{
						return dlg.ShowDialog();
					}
				});
		}
	}
}
