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
  <WritingSystems allowAddToSldr=""true"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</LexiconProjectSettings>";

			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(new TestSettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)});

			var settings = new LexiconProjectSettings();
			projectSettingsDataMapper.Read(settings);
			Assert.That(settings.AllowAddWritingSystemsToSldr, Is.True);
		}

		[Test]
		public void Read_EmptyXml_NothingSet()
		{
			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(new TestSettingsStore());

			var settings = new LexiconProjectSettings();
			projectSettingsDataMapper.Read(settings);

			Assert.That(settings.AllowAddWritingSystemsToSldr, Is.False);
		}

		[Test]
		public void Write_EmptyXml_XmlUpdated()
		{
			var settingsStore = new TestSettingsStore();
			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(settingsStore);

			var settings = new LexiconProjectSettings {AllowAddWritingSystemsToSldr = true};
			projectSettingsDataMapper.Write(settings);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconProjectSettings>
  <WritingSystems allowAddToSldr=""true"" />
</LexiconProjectSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void Write_ValidXml_XmlUpdated()
		{
			const string projectSettingsXml =
@"<LexiconProjectSettings>
  <WritingSystems allowAddToSldr=""false"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</LexiconProjectSettings>";

			var settingsStore = new TestSettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)};
			var projectSettingsDataMapper = new LexiconProjectSettingsDataMapper(settingsStore);
			var settings = new LexiconProjectSettings {AllowAddWritingSystemsToSldr = true};
			projectSettingsDataMapper.Write(settings);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconProjectSettings>
  <WritingSystems allowAddToSldr=""true"">
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
