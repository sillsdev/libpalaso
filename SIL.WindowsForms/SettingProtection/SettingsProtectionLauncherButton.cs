using System;
using System.Windows.Forms;

namespace SIL.WindowsForms.SettingProtection
{
	public partial class SettingsProtectionLauncherButton : UserControl
	{
		public SettingsProtectionLauncherButton()
		{
			InitializeComponent();
			UpdateDisplay();
		}

		private void betterLinkLabel1_Click(object sender, EventArgs e)
		{
			using(var dlg = new SettingProtectionDialog())
			{
				dlg.ShowDialog();
			}
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			_image.Image = SettingsProtectionSingleton.GetImage(32);
		}


	}
}
