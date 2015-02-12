using System.Configuration;
using System.Xml.Linq;

namespace SIL.LexiconUtils
{
	public class ApplicationSettingsStore : ISettingsStore
	{
		private readonly SettingsBase _appSettings;
		private readonly string _propertyName;

		public ApplicationSettingsStore(SettingsBase appSettings, string propertyName)
		{
			_appSettings = appSettings;
			_propertyName = propertyName;
		}

		public XElement GetSettings()
		{
			var settingsStr = (string) _appSettings[_propertyName];
			return string.IsNullOrEmpty(settingsStr) ? null : XElement.Parse(settingsStr);
		}

		public void SaveSettings(XElement settingsElem)
		{
			_appSettings[_propertyName] = settingsElem.ToString();
			_appSettings.Save();
		}
	}
}
