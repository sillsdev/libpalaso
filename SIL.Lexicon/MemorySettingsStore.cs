using System.Xml.Linq;

namespace SIL.Lexicon
{
	public class MemorySettingsStore : ISettingsStore
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
