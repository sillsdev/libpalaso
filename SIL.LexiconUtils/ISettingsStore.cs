using System.Xml.Linq;

namespace SIL.LexiconUtils
{
	public interface ISettingsStore
	{
		XElement GetSettings();

		void SaveSettings(XElement settingsElem);
	}
}
