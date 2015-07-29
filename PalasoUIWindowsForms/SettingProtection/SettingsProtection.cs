using System.Configuration;
using System.Drawing;
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
			get
			{
				var productName = CoreProductName;
				return productName.Insert(1, "7").ToLower();
			}

		}

		/// <summary>
		/// Use the CoreProductName value from the AppSettings in the application config file, if present
		/// </summary>
		internal static string CoreProductName
		{
			get
			{
				var productName = ConfigurationManager.AppSettings["CoreProductName"];
				if (string.IsNullOrEmpty(productName))
					productName = Application.ProductName;

				return productName;
			}
		}
	}
}
