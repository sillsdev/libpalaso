using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using SIL.WritingSystems;

namespace SIL.Lexicon
{
	public class ProjectLexiconSettingsWritingSystemDataMapper : ProjectLexiconSettingsWritingSystemDataMapper<WritingSystemDefinition>
	{
		public ProjectLexiconSettingsWritingSystemDataMapper(ISettingsStore settingsStore)
			: base(settingsStore)
		{
		}
	}

	public class ProjectLexiconSettingsWritingSystemDataMapper<T> : ICustomDataMapper<T> where T : WritingSystemDefinition
	{
		private readonly ISettingsStore _settingsStore;

		public ProjectLexiconSettingsWritingSystemDataMapper(ISettingsStore settingsStore)
		{
			_settingsStore = settingsStore;
		}

		public virtual void Read(T ws)
		{
			XElement projectSettingsElem = _settingsStore.GetSettings();
			if (projectSettingsElem == null)
				return;

			XElement wsElem = projectSettingsElem.Elements("WritingSystems").Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == ws.LanguageTag);
			if (wsElem == null)
				return;

			var abbreviation = (string) wsElem.Element("Abbreviation");
			if (!string.IsNullOrEmpty(abbreviation))
				ws.Abbreviation = abbreviation;

			var languageName = (string) wsElem.Element("LanguageName");
			if (!string.IsNullOrEmpty(languageName) && ws.Language != null)
				ws.Language = new LanguageSubtag(ws.Language, languageName);

			var scriptName = (string) wsElem.Element("ScriptName");
			if (!string.IsNullOrEmpty(scriptName) && ws.Script != null && ws.Script.IsPrivateUse)
				ws.Script = new ScriptSubtag(ws.Script, scriptName);

			var regionName = (string) wsElem.Element("RegionName");
			if (!string.IsNullOrEmpty(regionName) && ws.Region != null && ws.Region.IsPrivateUse)
				ws.Region = new RegionSubtag(ws.Region, regionName);

			var spellCheckingId = (string) wsElem.Element("SpellCheckingId");
			if (!string.IsNullOrEmpty(spellCheckingId))
				ws.SpellCheckingId = spellCheckingId;

			var legacyMapping = (string) wsElem.Element("LegacyMapping");
			if (!string.IsNullOrEmpty(legacyMapping))
				ws.LegacyMapping = legacyMapping;

			var keyboard = (string) wsElem.Element("Keyboard");
			if (!string.IsNullOrEmpty(keyboard))
				ws.Keyboard = keyboard;

			var systemCollationElem = wsElem.Element("SystemCollation");
			if (systemCollationElem != null)
			{
				var scd = new SystemCollationDefinition { LanguageTag = (string) systemCollationElem };
				ws.DefaultCollation = scd;
			}
			ws.AcceptChanges();
		}

		public virtual void Write(T ws)
		{
			XElement projectSettingsElem = _settingsStore.GetSettings() ?? new XElement("ProjectLexiconSettings");
			XElement wssElem = projectSettingsElem.Element("WritingSystems");
			if (wssElem == null)
			{
				wssElem = new XElement("WritingSystems");
				projectSettingsElem.Add(wssElem);
			}
			XElement wsElem = wssElem.Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == ws.LanguageTag);

			if (wsElem == null)
			{
				wsElem = new XElement("WritingSystem", new XAttribute("id", ws.LanguageTag));
				wssElem.Add(wsElem);
			}
			wsElem.RemoveNodes();

			if (!string.IsNullOrEmpty(ws.Abbreviation))
				wsElem.Add(new XElement("Abbreviation", ws.Abbreviation));
			if (ws.Language != null && !string.IsNullOrEmpty(ws.Language.Name))
				wsElem.Add(new XElement("LanguageName", ws.Language.Name));
			if (ws.Script != null && ws.Script.IsPrivateUse && !string.IsNullOrEmpty(ws.Script.Name))
				wsElem.Add(new XElement("ScriptName", ws.Script.Name));
			if (ws.Region != null && ws.Region.IsPrivateUse && !string.IsNullOrEmpty(ws.Region.Name))
				wsElem.Add(new XElement("RegionName", ws.Region.Name));
			if (!string.IsNullOrEmpty(ws.SpellCheckingId))
				wsElem.Add(new XElement("SpellCheckingId", ws.SpellCheckingId));
			if (!string.IsNullOrEmpty(ws.LegacyMapping))
				wsElem.Add(new XElement("LegacyMapping", ws.LegacyMapping));
			if (!string.IsNullOrEmpty(ws.Keyboard))
				wsElem.Add(new XElement("Keyboard", ws.Keyboard));
			var sysCollation = ws.DefaultCollation as SystemCollationDefinition;
			if (sysCollation != null)
			{
				wsElem.Add(new XElement("SystemCollation", sysCollation.LanguageTag));
			}
			_settingsStore.SaveSettings(projectSettingsElem);
		}

		public void Remove(string wsId)
		{
			XElement projectSettingsElem = _settingsStore.GetSettings();
			if (projectSettingsElem == null)
				return;

			Debug.Assert(projectSettingsElem != null);
			XElement wssElem = projectSettingsElem.Element("WritingSystems");
			if (wssElem == null)
				return;

			XElement wsElem = wssElem.Elements("WritingSystem").FirstOrDefault(e => (string) e.Attribute("id") == wsId);
			if (wsElem != null)
			{
				wsElem.Remove();
				if (!wssElem.HasElements)
					wssElem.Remove();

				_settingsStore.SaveSettings(projectSettingsElem);
			}
		}
	}
}
