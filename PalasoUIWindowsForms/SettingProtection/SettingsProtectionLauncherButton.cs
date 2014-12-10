using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.SettingProtection
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
