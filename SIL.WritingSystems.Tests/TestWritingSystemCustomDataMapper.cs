using System.Collections.Generic;

namespace SIL.WritingSystems.Tests
{
	public class TestWritingSystemCustomDataMapper : ICustomDataMapper<WritingSystemDefinition>
	{
		private readonly Dictionary<string, Dictionary<string, string>> _writingSystems = new Dictionary<string, Dictionary<string, string>>();

		public void Read(WritingSystemDefinition ws)
		{
			Dictionary<string, string> properties;
			if (_writingSystems.TryGetValue(ws.LanguageTag, out properties))
			{
				string abbreviation;
				if (properties.TryGetValue("Abbreviation", out abbreviation))
					ws.Abbreviation = abbreviation;

				string languageName;
				if (properties.TryGetValue("LanguageName", out languageName) && ws.Language != null && ws.Language.IsPrivateUse)
					ws.Language = new LanguageSubtag(ws.Language, languageName);

				string scriptName;
				if (properties.TryGetValue("ScriptName", out scriptName) && ws.Script != null && ws.Script.IsPrivateUse)
					ws.Script = new ScriptSubtag(ws.Script, scriptName);

				string regionName;
				if (properties.TryGetValue("RegionName", out regionName) && ws.Region != null && ws.Region.IsPrivateUse)
					ws.Region = new RegionSubtag(ws.Region, regionName);

				string spellCheckingId;
				if (properties.TryGetValue("SpellCheckingId", out spellCheckingId))
					ws.SpellCheckingId = spellCheckingId;

				string legacyMapping;
				if (properties.TryGetValue("LegacyMapping", out legacyMapping))
					ws.LegacyMapping = legacyMapping;

				string keyboard;
				if (properties.TryGetValue("Keyboard", out keyboard))
					ws.Keyboard = keyboard;

				string systemCollation;
				if (properties.TryGetValue("SystemCollation", out systemCollation))
					ws.DefaultCollation = new SystemCollationDefinition {LanguageTag = systemCollation};
				ws.AcceptChanges();
			}
		}

		public void Write(WritingSystemDefinition ws)
		{
			var properties = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(ws.Abbreviation))
				properties["Abbreviation"] = ws.Abbreviation;
			if (ws.Language != null && ws.Language.IsPrivateUse && !string.IsNullOrEmpty(ws.Language.Name))
				properties["LanguageName"] = ws.Language.Name;
			if (ws.Script != null && ws.Script.IsPrivateUse && !string.IsNullOrEmpty(ws.Script.Name))
				properties["ScriptName"] = ws.Script.Name;
			if (ws.Region != null && ws.Region.IsPrivateUse && !string.IsNullOrEmpty(ws.Region.Name))
				properties["RegionName"] = ws.Region.Name;
			if (!string.IsNullOrEmpty(ws.SpellCheckingId))
				properties["SpellCheckingId"] = ws.SpellCheckingId;
			if (!string.IsNullOrEmpty(ws.LegacyMapping))
				properties["LegacyMapping"] = ws.LegacyMapping;
			if (!string.IsNullOrEmpty(ws.Keyboard))
				properties["Keyboard"] = ws.Keyboard;
			var sysCollation = ws.DefaultCollation as SystemCollationDefinition;
			if (sysCollation != null)
				properties["SystemCollation"] = sysCollation.LanguageTag;
			_writingSystems[ws.LanguageTag] = properties;
		}

		public void Remove(string wsId)
		{
			_writingSystems.Remove(wsId);
		}
	}
}
