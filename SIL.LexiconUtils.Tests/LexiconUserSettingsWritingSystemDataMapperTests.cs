using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.Keyboarding;
using SIL.WritingSystems;

namespace SIL.LexiconUtils.Tests
{
	[TestFixture]
	public class LexiconUserSettingsWritingSystemDataMapperTests
	{
		[Test]
		public void Read_ValidXml_SetsAllProperties()
		{
			const string userSettingsXml =
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English-IPA</LocalKeyboard>
      <KnownKeyboards>
        <KnownKeyboard id=""en-US_English"" />
      </KnownKeyboards>
      <DefaultFontName>Times New Roman</DefaultFontName>
    </WritingSystem>
    <WritingSystem id=""fr-FR"">
      <DefaultFontSize>12</DefaultFontSize>
      <IsGraphiteEnabled>false</IsGraphiteEnabled>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>";

			var userSettingsDataMapper = new LexiconUserSettingsWritingSystemDataMapper(new MemorySettingsStore {SettingsElement = XElement.Parse(userSettingsXml)});

			var ws1 = new WritingSystemDefinition("en-US");
			userSettingsDataMapper.Read(ws1);

			Assert.That(ws1.LocalKeyboard.Id, Is.EqualTo("en-US_English-IPA"));
			Assert.That(ws1.KnownKeyboards[1].Id, Is.EqualTo("en-US_English"));
			Assert.That(ws1.DefaultFont.Name, Is.EqualTo("Times New Roman"));
			Assert.That(ws1.DefaultFontSize, Is.EqualTo(0));
			Assert.That(ws1.IsGraphiteEnabled, Is.True);

			var ws2 = new WritingSystemDefinition("fr-FR");
			userSettingsDataMapper.Read(ws2);

			Assert.That(ws2.LocalKeyboard, Is.EqualTo(Keyboard.Controller.DefaultKeyboard));
			Assert.That(ws2.DefaultFont, Is.Null);
			Assert.That(ws2.DefaultFontSize, Is.EqualTo(12));
			Assert.That(ws2.IsGraphiteEnabled, Is.False);

			var ws3 = new WritingSystemDefinition("es");
			userSettingsDataMapper.Read(ws3);

			Assert.That(ws3.LocalKeyboard, Is.EqualTo(Keyboard.Controller.DefaultKeyboard));
			Assert.That(ws3.DefaultFont, Is.Null);
			Assert.That(ws3.DefaultFontSize, Is.EqualTo(0));
			Assert.That(ws3.IsGraphiteEnabled, Is.True);
		}

		[Test]
		public void Read_EmptyXml_NothingSet()
		{
			var userSettingsDataMapper = new LexiconUserSettingsWritingSystemDataMapper(new MemorySettingsStore());

			var ws1 = new WritingSystemDefinition("en-US");
			userSettingsDataMapper.Read(ws1);

			Assert.That(ws1.LocalKeyboard, Is.EqualTo(Keyboard.Controller.DefaultKeyboard));
			Assert.That(ws1.DefaultFont, Is.Null);
			Assert.That(ws1.DefaultFontSize, Is.EqualTo(0));
			Assert.That(ws1.IsGraphiteEnabled, Is.True);
		}

		[Test]
		public void Write_EmptyXml_XmlUpdated()
		{
			var settingsStore = new MemorySettingsStore();
			var userSettingsDataMapper = new LexiconUserSettingsWritingSystemDataMapper(settingsStore);

			var ws1 = new WritingSystemDefinition("en-US");
			ws1.LocalKeyboard = Keyboard.Controller.CreateKeyboard("en-US_English-IPA", KeyboardFormat.Unknown, Enumerable.Empty<string>());
			ws1.DefaultFont = new FontDefinition("Times New Roman");
			userSettingsDataMapper.Write(ws1);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English-IPA</LocalKeyboard>
      <KnownKeyboards>
        <KnownKeyboard id=""en-US_English-IPA"" />
      </KnownKeyboards>
      <DefaultFontName>Times New Roman</DefaultFontName>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void Write_ValidXml_XmlUpdated()
		{
			const string userSettingsXml =
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English-IPA</LocalKeyboard>
      <KnownKeyboards>
        <KnownKeyboard id=""en-US_English-IPA"" />
      </KnownKeyboards>
      <DefaultFontName>Times New Roman</DefaultFontName>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(userSettingsXml)};
			var userSettingsDataMapper = new LexiconUserSettingsWritingSystemDataMapper(settingsStore);
			var ws1 = new WritingSystemDefinition("en-US");
			ws1.LocalKeyboard = Keyboard.Controller.CreateKeyboard("en-US_English", KeyboardFormat.Unknown, Enumerable.Empty<string>());
			ws1.DefaultFont = null;
			ws1.DefaultFontSize = 12;
			ws1.IsGraphiteEnabled = false;
			userSettingsDataMapper.Write(ws1);

			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English</LocalKeyboard>
      <KnownKeyboards>
        <KnownKeyboard id=""en-US_English"" />
      </KnownKeyboards>
      <DefaultFontSize>12</DefaultFontSize>
      <IsGraphiteEnabled>false</IsGraphiteEnabled>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void Remove_ExistingWritingSystem_UpdatesXml()
		{
			const string userSettingsXml =
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English-IPA</LocalKeyboard>
      <DefaultFontName>Times New Roman</DefaultFontName>
    </WritingSystem>
    <WritingSystem id=""fr-FR"">
      <DefaultFontSize>12</DefaultFontSize>
      <IsGraphiteEnabled>false</IsGraphiteEnabled>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(userSettingsXml)};
			var userSettingsDataMapper = new LexiconUserSettingsWritingSystemDataMapper(settingsStore);
			userSettingsDataMapper.Remove("fr-FR");
			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English-IPA</LocalKeyboard>
      <DefaultFontName>Times New Roman</DefaultFontName>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));

			userSettingsDataMapper.Remove("en-US");
			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse("<LexiconUserSettings />")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}

		[Test]
		public void Remove_NonexistentWritingSystem_DoesNotUpdateXml()
		{
			const string userSettingsXml =
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English-IPA</LocalKeyboard>
      <DefaultFontName>Times New Roman</DefaultFontName>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>";

			var settingsStore = new MemorySettingsStore {SettingsElement = XElement.Parse(userSettingsXml)};
			var userSettingsDataMapper = new LexiconUserSettingsWritingSystemDataMapper(settingsStore);
			userSettingsDataMapper.Remove("fr-FR");
			Assert.That(settingsStore.SettingsElement, Is.EqualTo(XElement.Parse(
@"<LexiconUserSettings>
  <WritingSystems>
    <WritingSystem id=""en-US"">
      <LocalKeyboard>en-US_English-IPA</LocalKeyboard>
      <DefaultFontName>Times New Roman</DefaultFontName>
    </WritingSystem>
  </WritingSystems>
</LexiconUserSettings>")).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
		}
	}
}
