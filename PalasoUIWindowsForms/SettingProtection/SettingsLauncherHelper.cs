using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.SettingProtection
{
	/// <summary>
	/// This class takes an Settings-launching control & adds the behaviors needed to conform to the standard SIL "Rice Farmer" settings protection behavior.
	/// If you use the standard SettingsLauncherButton, you don't have to worry about this class (it uses this).  But if you have a need custom control for look/feel,
	/// then add this compenent to the form and, *in code*, set the CustomSettingsControl.  When you control is clicked, have it call LaunchSettingsIfAppropriate()
	/// </summary>
	public partial class SettingsLauncherHelper : Component
	{
		private Control _customSettingsControl;

		public SettingsLauncherHelper(IContainer container)
		{
			InitializeComponent();

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				if(container!=null)
					container.Add(this);
				_checkForCtrlKeyTimer.Enabled = true;
			}
		}

		/// <summary>
		/// The control we are suppose to help.
		/// </summary>
		public Control CustomSettingsControl
		{
			set
			{
				if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
				{
					_customSettingsControl = value;
					UpdateDisplay();
				}
			}
		}

		/// <summary>
		/// The control should call this when the user clicks on it. It will challenge if necessary, and carry out the supplied code if everything is rosy.
		/// </summary>
		/// <param name="settingsLaunchingFunction"></param>
		/// <returns>DialogResult.Cancel if the challenge fails, otherwise whatever the settingsLaunchingFunction returns.</returns>
		public DialogResult LaunchSettingsIfAppropriate(Func<DialogResult> settingsLaunchingFunction)
		{
			if (SettingsProtectionSingleton.Configuration.RequirePassword)
			{
				using (var dlg = new SettingsPasswordDialog(SettingsProtectionSingleton.FactoryPassword, SettingsPasswordDialog.Mode.Challenge))
				{
					if (DialogResult.OK != dlg.ShowDialog())
						return DialogResult.Cancel;
				}
			}
			var result = settingsLaunchingFunction();
			UpdateDisplay();
			return result;
		}

		private void UpdateDisplay()
		{
			if (_customSettingsControl == null)//sometimes get a tick before this has been set
				return;

			var keys = (Keys.Control | Keys.Shift);
			_customSettingsControl.Visible = !SettingsProtectionSingleton.Configuration.NormallyHidden
				|| ((Control.ModifierKeys & keys) == keys);
		}

		private void _checkForCtrlKeyTimer_Tick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
