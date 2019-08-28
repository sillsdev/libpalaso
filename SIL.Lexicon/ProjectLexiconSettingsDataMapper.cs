using System.Xml.Linq;

namespace SIL.Lexicon
{
	public class ProjectLexiconSettingsDataMapper
	{
		private readonly ISettingsStore _settingsStore;

		public ProjectLexiconSettingsDataMapper(ISettingsStore settingsStore)
		{
			_settingsStore = settingsStore;
		}

		public void Read(ProjectLexiconSettings settings)
		{
			XElement settingsElem = _settingsStore.GetSettings();
			if (settingsElem == null)
				return;

			if(settingsElem.HasAttributes)
				settings.ProjectSharing = (bool?)settingsElem.Attribute("projectSharing") ?? false;

			XElement wssElem = settingsElem.Element("WritingSystems");
			if (wssElem != null)
			{
				settings.AddWritingSystemsToSldr = (bool?) wssElem.Attribute("addToSldr") ?? false;
			}

			settings.AcceptChanges();
		}

		public void Write(ProjectLexiconSettings settings)
		{
			if (!settings.IsChanged)
				return;

			XElement settingsElem = _settingsStore.GetSettings() ?? new XElement("ProjectLexiconSettings");
			settingsElem.SetAttributeValue("projectSharing", settings.ProjectSharing);
			_settingsStore.SaveSettings(settingsElem);
			XElement wssElem = settingsElem.Element("WritingSystems");
			if (wssElem == null)
			{
				wssElem = new XElement("WritingSystems");
				settingsElem.Add(wssElem);
			}
			wssElem.SetAttributeValue("addToSldr", settings.AddWritingSystemsToSldr);
			_settingsStore.SaveSettings(settingsElem);
			settings.AcceptChanges();
		}
	}
}
