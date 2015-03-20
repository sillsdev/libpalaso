using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;

namespace SIL.LexiconUtils.Tests
{
	[TestFixture]
	public class LexiconProjectSettingsDataMapperTests
	{
		[Test]
		public void Read_ValidXml_SetsAllProperties()
		{
			const string projectSettingsXml =
@"<LexiconProjectSettings>
  <WritingSystems addToSldr=""true"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</LexiconProjectSettings>";

			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)});

			var settings = new LexiconProjectSettings();
			projectSettingsDataMapper.Read(settings);
			Assert.That(settings.AddWritingSystemsToSldr, Is.True);
		}

		[Test]
		public void Read_EmptyXml_NothingSet()
		{
			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(new MemorySettingsStore());

			var settings = new LexiconProjectSettings();
			projectSettingsDataMapper.Read(settings);

			Assert.That(settings.AddWritingSystemsToSldr, Is.False);
		}

		[Test]
		public void Write_EmptyXml_XmlUpdated()
		{
			var settingsStore = new MemorySettingsStore();
			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(settingsStore);

			var settings = new LexiconProjectSettings {AddWritingSystemsToSldr = true};
			projectSettingsDataMapper.Write(settings);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconProjectSettings>
  <WritingSystems addToSldr=""true"" />
</LexiconProjectSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void Write_ValidXml_XmlUpdated()
		{
			const string projectSettingsXml =
@"<LexiconProjectSettings>
  <WritingSystems addToSldr=""false"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</LexiconProjectSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)};
			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(settingsStore);
			var settings = new LexiconProjectSettings {AddWritingSystemsToSldr = true};
			projectSettingsDataMapper.Write(settings);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconProjectSettings>
  <WritingSystems addToSldr=""true"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</LexiconProjectSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}
	}
}
