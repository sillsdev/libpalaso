using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;

namespace SIL.Lexicon.Tests
{
	[TestFixture]
	public class ProjectLexiconSettingsDataMapperTests
	{
		[Test]
		public void Read_ValidXml_SetsAllProperties()
		{
			const string projectSettingsXml =
@"<ProjectLexiconSettings projectSharing=""true"">
  <WritingSystems addToSldr=""true"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>";

			var projectSettingsDataMapper = new ProjectLexiconSettingsDataMapper(new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)});

			var settings = new ProjectLexiconSettings();
			projectSettingsDataMapper.Read(settings);
			Assert.That(settings.AddWritingSystemsToSldr, Is.True);
			Assert.That(settings.ProjectSharing, Is.True);
		}

		[Test]
		public void Read_EmptyXml_NothingSet()
		{
			var projectSettingsDataMapper = new ProjectLexiconSettingsDataMapper(new MemorySettingsStore());

			var settings = new ProjectLexiconSettings();
			projectSettingsDataMapper.Read(settings);

			Assert.That(settings.AddWritingSystemsToSldr, Is.False);
		}

		[Test]
		public void Write_EmptyXml_XmlUpdated()
		{
			var settingsStore = new MemorySettingsStore();
			var projectSettingsDataMapper = new ProjectLexiconSettingsDataMapper(settingsStore);

			var settings = new ProjectLexiconSettings {AddWritingSystemsToSldr = true, ProjectSharing = true};
			projectSettingsDataMapper.Write(settings);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<ProjectLexiconSettings projectSharing=""true"">
  <WritingSystems addToSldr=""true"" />
</ProjectLexiconSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void Write_ValidXml_XmlUpdated()
		{
			const string projectSettingsXml =
@"<ProjectLexiconSettings projectSharing=""false"">
  <WritingSystems addToSldr=""false"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(projectSettingsXml)};
			var projectSettingsDataMapper = new ProjectLexiconSettingsDataMapper(settingsStore);
			var settings = new ProjectLexiconSettings {AddWritingSystemsToSldr = true, ProjectSharing = true };
			projectSettingsDataMapper.Write(settings);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<ProjectLexiconSettings projectSharing=""true"">
  <WritingSystems addToSldr=""true"">
    <WritingSystem id=""fr-FR"">
      <SpellCheckingId>fr_FR</SpellCheckingId>
      <LegacyMapping>converter</LegacyMapping>
      <Keyboard>Old Keyboard</Keyboard>
    </WritingSystem>
  </WritingSystems>
</ProjectLexiconSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void ProjectSharing_Is_Unchecked()
		{
			const string projectSettingsXml =
				@"<ProjectLexiconSettings projectSharing=""true"">
  <WritingSystems addToSldr=""false"">
	<WritingSystem id=""fr-FR"">
		<SpellCheckingId>fr_FR</SpellCheckingId>
		<LegacyMapping>converter</LegacyMapping>
		<Keyboard>Old Keyboard</Keyboard>
	</WritingSystem>
</WritingSystems>
</ProjectLexiconSettings>";

			var settingsStore = new MemorySettingsStore { SettingsElement = XElement.Parse(projectSettingsXml) };
			var projectSettingsDataMapper = new ProjectLexiconSettingsDataMapper(settingsStore);
			var settings = new ProjectLexiconSettings { AddWritingSystemsToSldr = true, ProjectSharing = false };
			projectSettingsDataMapper.Write(settings);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
				@"<ProjectLexiconSettings projectSharing=""false"">
  <WritingSystems addToSldr=""true"">
	<WritingSystem id=""fr-FR"">
		<SpellCheckingId>fr_FR</SpellCheckingId>
		<LegacyMapping>converter</LegacyMapping>
		<Keyboard>Old Keyboard</Keyboard>
	</WritingSystem>
</WritingSystems>
</ProjectLexiconSettings>")).Using((IEqualityComparer<XNode>)new XNodeEqualityComparer()));
		}
	}
}
