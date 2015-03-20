using System.Xml.Linq;

namespace SIL.LexiconUtils
{
	public class LexiconProjectSettingsDataMapper
	{
		private readonly ISettingsStore _settingsStore;

		public LexiconProjectSettingsDataMapper(ISettingsStore settingsStore)
		{
			_settingsStore = settingsStore;
		}

		public void Read(LexiconProjectSettings settings)
		{
			XElement settingsElem = _settingsStore.GetSettings();
			if (settingsElem == null)
				return;

			XElement wssElem = settingsElem.Element("WritingSystems");
			if (wssElem != null)
				settings.AllowAddWritingSystemsToSldr = (bool?) wssElem.Attribute("allowAddToSldr") ?? false;

			settings.AcceptChanges();
		}

		public void Write(LexiconProjectSettings settings)
		{
			if (!settings.IsChanged)
				return;

			XElement settingsElem = _settingsStore.GetSettings() ?? new XElement("LexiconProjectSettings");
			_settingsStore.SaveSettings(settingsElem);
			XElement wssElem = settingsElem.Element("WritingSystems");
			if (wssElem == null)
			{
				wssElem = new XElement("WritingSystems");
				settingsElem.Add(wssElem);
			}
			wssElem.SetAttributeValue("allowAddToSldr", settings.AllowAddWritingSystemsToSldr);
			_settingsStore.SaveSettings(settingsElem);
			settings.AcceptChanges();
		}
	}
}
