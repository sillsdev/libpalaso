using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.NUnitExtensions.Is;
using SIL.WritingSystems;

namespace SIL.Lexicon.Tests
{
	[TestFixture]
	[OfflineSldr]
	public class ProjectLexiconSettingsWritingSystemDataMapperTests
	{
		[Test]
		public void Read_ValidXml_SetsAllProperties()
		{
			const string projectSettingsXml =
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2"">
      <Abbreviation>kal</Abbreviation>
      <LanguageName>Kalaba</LanguageName>
      <ScriptName>Fake</ScriptName>
      <RegionName>Zolrog</RegionName>
      <SystemCollation>snarf</SystemCollation>
    </WritingSystem>
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>";

			var projectSettingsDataMapper = new ProjectLexiconSettingsWritingSystemDataMapper(new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)});

			var ws1 = new WritingSystemDefinition("qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2");
			projectSettingsDataMapper.Read(ws1);

			Assert.That(ws1.Abbreviation, Is.EqualTo("kal"));
			Assert.That(ws1.Language.Name, Is.EqualTo("Kalaba"));
			Assert.That(ws1.Script.Name, Is.EqualTo("Fake"));
			Assert.That(ws1.Region.Name, Is.EqualTo("Zolrog"));
			Assert.That(ws1.SpellCheckingId, Is.EqualTo(string.Empty));
			Assert.That(ws1.LegacyMapping, Is.EqualTo(string.Empty));
			Assert.That(ws1.Keyboard, Is.EqualTo(string.Empty));
			var scd = new SystemCollationDefinition {LanguageTag = "snarf"};
			Assert.That(ws1.DefaultCollation, Is.ValueEqualTo(scd));

			var ws2 = new WritingSystemDefinition("fr-FR");
			projectSettingsDataMapper.Read(ws2);

			Assert.That(ws2.Abbreviation, Is.EqualTo("fr"));
			Assert.That(ws2.Language.Name, Is.EqualTo("French"));
			Assert.That(ws2.Script.Name, Is.EqualTo("Latin"));
			Assert.That(ws2.Region.Name, Is.EqualTo("France"));
			Assert.That(ws2.Variants, Is.Empty);
			Assert.That(ws2.SpellCheckingId, Is.EqualTo("fr_FR"));
			Assert.That(ws2.LegacyMapping, Is.EqualTo("converter"));
			Assert.That(ws2.Keyboard, Is.EqualTo("Old Keyboard"));

			var ws3 = new WritingSystemDefinition("es");
			projectSettingsDataMapper.Read(ws3);

			Assert.That(ws3.Abbreviation, Is.EqualTo("es"));
			Assert.That(ws3.Language.Name, Is.EqualTo("Spanish"));
			Assert.That(ws3.Script.Name, Is.EqualTo("Latin"));
			Assert.That(ws3.Region, Is.Null);
			Assert.That(ws3.Variants, Is.Empty);
			Assert.That(ws3.SpellCheckingId, Is.EqualTo(string.Empty));
			Assert.That(ws3.LegacyMapping, Is.EqualTo(string.Empty));
			Assert.That(ws3.Keyboard, Is.EqualTo(string.Empty));
		}

		[Test]
		public void Read_EmptyXml_NothingSet()
		{
			var projectSettingsDataMapper = new ProjectLexiconSettingsWritingSystemDataMapper(new MemorySettingsStore());

			var ws1 = new WritingSystemDefinition("en-US");
			projectSettingsDataMapper.Read(ws1);

			Assert.That(ws1.Abbreviation, Is.EqualTo("en"));
			Assert.That(ws1.Language.Name, Is.EqualTo("English"));
			Assert.That(ws1.Script.Name, Is.EqualTo("Latin"));
			Assert.That(ws1.Region.Name, Is.EqualTo("United States"));
			Assert.That(ws1.Variants, Is.Empty);
			Assert.That(ws1.SpellCheckingId, Is.EqualTo(string.Empty));
			Assert.That(ws1.LegacyMapping, Is.EqualTo(string.Empty));
			Assert.That(ws1.Keyboard, Is.EqualTo(string.Empty));
		}

		[Test]
		public void Write_EmptyXml_XmlUpdated()
		{
			var settingsStore = new MemorySettingsStore();
			var projectSettingsDataMapper = new ProjectLexiconSettingsWritingSystemDataMapper(settingsStore);

			var ws1 = new WritingSystemDefinition("qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2");
			ws1.Language = new LanguageSubtag(ws1.Language, "Kalaba");
			ws1.Script = new ScriptSubtag(ws1.Script, "Fake");
			ws1.Region = new RegionSubtag(ws1.Region, "Zolrog");
			ws1.Variants[0] = new VariantSubtag(ws1.Variants[0], "Custom 1");
			ws1.Variants[1] = new VariantSubtag(ws1.Variants[1], "Custom 2");
			projectSettingsDataMapper.Write(ws1);

			Assert.That(settingsStore.SettingsElement, Is.XmlEqualTo(
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2"">
      <Abbreviation>kal</Abbreviation>
      <LanguageName>Kalaba</LanguageName>
      <ScriptName>Fake</ScriptName>
      <RegionName>Zolrog</RegionName>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>"));
		}

		[Test]
		public void Write_ValidXml_XmlUpdated()
		{
			const string projectSettingsXml =
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2-var3"">
      <Abbreviation>kal</Abbreviation>
      <LanguageName>Kalaba</LanguageName>
      <ScriptName>Fake</ScriptName>
      <RegionName>Zolrog</RegionName>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)};
			var projectSettingsDataMapper = new ProjectLexiconSettingsWritingSystemDataMapper(settingsStore);
			var ws1 = new WritingSystemDefinition("qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2-var3");
			ws1.Abbreviation = "ka";
			ws1.SpellCheckingId = "en_US";
			ws1.LegacyMapping = "converter";
			ws1.Keyboard = "Old Keyboard";
			var scd = new SystemCollationDefinition {LanguageTag = "snarf"};
			ws1.DefaultCollation = scd;
			projectSettingsDataMapper.Write(ws1);

			Assert.That(settingsStore.SettingsElement, Is.XmlEqualTo(
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2-var3"">
      <Abbreviation>ka</Abbreviation>
      <SpellCheckingId>en_US</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
      <SystemCollation>snarf</SystemCollation>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>"));
		}

		[Test]
		public void Remove_ExistingWritingSystem_UpdatesXml()
		{
			const string projectSettingsXml =
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2"">
      <Abbreviation>kal</Abbreviation>
      <LanguageName>Kalaba</LanguageName>
      <ScriptName>Fake</ScriptName>
      <RegionName>Zolrog</RegionName>
      <VariantNames>
        <VariantName>Custom 1</VariantName>
        <VariantName>Custom 2</VariantName>
      </VariantNames>
    </WritingSystem>
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)};
			var projectSettingsDataMapper = new ProjectLexiconSettingsWritingSystemDataMapper(settingsStore);
			projectSettingsDataMapper.Remove("fr-FR");
			Assert.That(settingsStore.SettingsElement, Is.XmlEqualTo(
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2"">
      <Abbreviation>kal</Abbreviation>
      <LanguageName>Kalaba</LanguageName>
      <ScriptName>Fake</ScriptName>
      <RegionName>Zolrog</RegionName>
      <VariantNames>
        <VariantName>Custom 1</VariantName>
        <VariantName>Custom 2</VariantName>
      </VariantNames>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>"));

			projectSettingsDataMapper.Remove("qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2");
			Assert.That(settingsStore.SettingsElement, Is.XmlEqualTo("<ProjectLexiconSettings />"));
		}

		[Test]
		public void Remove_FinalWritingSystem_PreservesSettings()
		{
			const string projectSettingsXml =
@"<ProjectLexiconSettings>
  <WritingSystems addToSldr=""true"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)};
			var projectSettingsDataMapper = new ProjectLexiconSettingsWritingSystemDataMapper(settingsStore);
			projectSettingsDataMapper.Remove("fr-FR");
			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<ProjectLexiconSettings>
  <WritingSystems addToSldr=""true""/>
</ProjectLexiconSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void Remove_NonexistentWritingSystem_DoesNotUpdateFile()
		{
			const string projectSettingsXml =
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2"">
      <Abbreviation>kal</Abbreviation>
      <LanguageName>Kalaba</LanguageName>
      <ScriptName>Fake</ScriptName>
      <RegionName>Zolrog</RegionName>
      <VariantNames>
        <VariantName>Custom 1</VariantName>
        <VariantName>Custom 2</VariantName>
      </VariantNames>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)};
			var projectSettingsDataMapper = new ProjectLexiconSettingsWritingSystemDataMapper(settingsStore);
			projectSettingsDataMapper.Remove("fr-FR");
			Assert.That(settingsStore.SettingsElement, Is.XmlEqualTo(
@"<ProjectLexiconSettings>
  <WritingSystems>
    <WritingSystem id=""qaa-Qaaa-QM-x-kal-Fake-ZG-var1-var2"">
      <Abbreviation>kal</Abbreviation>
      <LanguageName>Kalaba</LanguageName>
      <ScriptName>Fake</ScriptName>
      <RegionName>Zolrog</RegionName>
      <VariantNames>
        <VariantName>Custom 1</VariantName>
        <VariantName>Custom 2</VariantName>
      </VariantNames>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>"));
		}
	}
}
