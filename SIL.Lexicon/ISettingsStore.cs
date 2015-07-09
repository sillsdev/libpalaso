using System.Xml.Linq;

namespace SIL.Lexicon
{
	public interface ISettingsStore
	{
		XElement GetSettings();

		void SaveSettings(XElement settingsElem);
	}
}
