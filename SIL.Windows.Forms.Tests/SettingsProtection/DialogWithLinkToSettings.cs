using System;
using System.ComponentModel;
using System.Windows.Forms;
#pragma warning disable CS0618 // Purposely testing more restrictive behavior of deprecated method.

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

			settingsLauncherButton1.LaunchSettingsCallback = () =>
			{
				using var dlg = new DialogWithSomeSettings(false);
					return dlg.ShowDialog();
			};

			var toolTip = new Forms.SuperToolTip.SuperToolTip(new Container());
			try
			{
				_settingsProtectionHelper.ManageComponent(toolTip);
				MessageBox.Show(
					"Passing a tooltip to SettingsProtectionHelper.ManageComponent should have thrown an exception.",
					"Unexpected success");
			}
			catch (ArgumentException)
			{
				Console.WriteLine("This exception was expected.");
			}
		}

		private void _customSettingsButton_Click(object sender, EventArgs e)
		{
			_settingsProtectionHelper.LaunchSettingsIfAppropriate(() =>
			{
				using var dlg = new DialogWithSomeSettings(true);
				{
					var result = dlg.ShowDialog();
					_settingsProtectionHelper.SetSettingsProtection(_toolStripButtonMaybe, dlg.ManageTheMaybeButton);
					return result;
				}
			});
		}
	}
}
