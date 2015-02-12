using System.Xml.Linq;

namespace SIL.LexiconUtils.Tests
{
	public class TestSettingsStore : ISettingsStore
	{
		public XElement SettingsElement { get; set; }

		public XElement GetSettings()
		{
			return SettingsElement;
		}

		public void SaveSettings(XElement settingsElem)
		{
			SettingsElement = settingsElem;
		}
	}
}
