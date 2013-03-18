using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.SettingProtection
{
	public class SettingsProtectionSingleton
	{
		private SettingsProtectionSettings config;
		private static SettingsProtectionSingleton _singleton;

		private SettingsProtectionSingleton()
		{
			config = SettingsProtectionSettings.Default;

			//bring in settings from any previous version
			if (config.NeedUpgrade)
			{
				//see http://stackoverflow.com/questions/3498561/net-applicationsettingsbase-should-i-call-upgrade-every-time-i-load
				config.Upgrade();
				config.NeedUpgrade = false;
				config.Save();
			}
		}

		public static Image GetImage(int width)
		{
			if (_singleton == null)
			{
				_singleton = new SettingsProtectionSingleton();
			}
			if(_singleton.config.RequirePassword)
			{
				if(width>16)
					return _singleton.config.NormallyHidden ? SettingsProtectionIcons.lockClosedHidden48x48 : SettingsProtectionIcons.lockClosed48x48;
				else
				{
					return _singleton.config.NormallyHidden ? SettingsProtectionIcons.lockClosedHidden16x16 : SettingsProtectionIcons.lockClosed16x16;
				}
			}
			else
			{
				if (width > 16)
					return _singleton.config.NormallyHidden ? SettingsProtectionIcons.lockOpenHidden48x48 : SettingsProtectionIcons.lockOpen48x48;
				else
				{
					return _singleton.config.NormallyHidden ? SettingsProtectionIcons.lockOpenHidden16x16 : SettingsProtectionIcons.lockOpen16x16;
				}
			}
		}

		public static SettingsProtectionSettings Settings
		{
			get
			{
				if(_singleton == null)
				{
					_singleton = new SettingsProtectionSingleton();
				}
				return _singleton.config;
			}
		}

		public static string FactoryPassword
		{
			get { return  (Application.ProductName.Substring(0, 1) + "7" + Application.ProductName.Substring(1, Application.ProductName.Length-1)).ToLower(); }

		}
	}
}
