using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Tests.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using Palaso.TestUtilities;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class WritingSystemDefinitionLdmlMigratorTests
	{
		private class TestEnvironment : IDisposable
		{
			readonly string _pathToLdml = Path.GetTempFileName();

			public TestEnvironment()
			{
				_pathToLdml = Path.GetTempFileName();
			}

			public string PathToWritingSystemLdmlFile
			{
				get { return _pathToLdml; }
			}

			public void WriteContentToWritingSystemLdmlFile(string contentToWrite)
			{
				File.WriteAllText(_pathToLdml, contentToWrite);
			}

			public void Dispose()
			{
				File.Delete(_pathToLdml);
			}
		}

		private TestEnvironment _environment;

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsFonipa_VariantContainsIpaVariantSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-fonipa", "","",""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='fonipa']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEtic_xDashEticIsMovedToPrivateUseSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-etic", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-etic']");
			}
		}

		[Test]
		public void MigrateIfNecassary_VariantContainsMultipleValidVariants_ValidVariantsAreLeftUntouched()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "biske-bogus-bauddha"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='biske-bauddha-x-bogus']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEmic_xDashEmicIsMovedToPrivateUseSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-emic", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-emic']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagInLdmlContainsxDashaudio_VariantSubtagInLdmlContainsxDashaudio()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio",String.Empty,String.Empty,String.Empty));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashaudioAndScriptSubtagIsNotEmpty_ScriptSubtagPartsAreMovedToPrivateUseAndScriptIsOverwrittenToBecomeZxxx()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio", "Latn", String.Empty, String.Empty));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio-Latn']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashaudioAndScriptSubtagcontainsZxxx_ZxxxIsNotAppendedToPrivateUseSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio", "Zxxx", String.Empty, String.Empty));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidScript_ScriptIsMovedToScript()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-Latn", String.Empty, String.Empty, String.Empty));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidRegion_RegionIsMovedToRegion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-US", String.Empty, String.Empty, String.Empty));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='US']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidVariant_VariantIsMovedToVariant()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-1901", String.Empty, String.Empty, String.Empty));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='1901']");
			}
		}

		[Test]
		public void DO_NOT_FORGET()
		{
			throw new NotImplementedException("Move GetIsoxxxxCodesInXXXSubtag methods out of Rfc5646V0 and into migratorv0 class");
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsx_xIsNotDuplicated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio", String.Empty, String.Empty, "x-test"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio-test']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsDataThatIsNotValidLanguageScriptRegionOrVariant_DataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-bogus-stuff", String.Empty, String.Empty, String.Empty));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsMultipleValidLanguageSubtagsAsWellAsDataThatIsNotValidLanguageScriptRegionOrVariant_LanguageIsSetToFirstValidLanguageSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("x-bogus-en-audio-de-bogus2-x-", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsMultipleValidLanguageSubtagsAsWellAsDataThatIsNotValidLanguageScriptRegionOrVariant_AllSubtagsButFirstValidLanguageSubtagAreMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("x-bogus-en-audio-tpi-bogus2-x-", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-audio-bogus2-tpi']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsInvalidData_InvalidDataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "bogus-stuff", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsValidScript_ScriptIsMigrated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Latn", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsMultipleValidScriptSubtagsAsWellAsDataThatIsNotValidScript_NonValidDataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-bogus-Latn-test-Zxxx-bogus2-x-", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-test-bogus2-Zxxx']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsMultipleValidScriptSubtagsAsWellAsDataThatIsNotValidScriptAsWellAsAudio_NonValidDataisMovedToPrivateUseAsScriptIsZxxx()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-bogus-Latn-audio-Afak-bogus2-x-", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-audio-bogus2-Afak-Latn']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageAndScriptSubtagsContainInvalidData_AllInvalidDataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("bogus-stuff", "more-bogus", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-more-bogus']");
			}
		}

		[Test]
		public void MigrateIfNecassary_RegionSubtagContainsAnythingButValidRegion_InvalidContentIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("qaa", "", "bogus-stuff", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void MigrateIfNecassary_RegionContainsTagThatIsValidRegion_IsMigrated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "DE", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='DE']");
			}
		}

		[Test]
		public void MigrateIfNecassary_VariantSubtagContainsAnythingButValidVariantOrPrivateUse_InvalidContentIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", "bogus-stuff"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsDatathatIsNotValidLanguageScriptRegionOrVariantAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-bogus", "", "", "x-BoGuS-stuff"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx-zxXX", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-zxXX']");
			}
		}

		[Test]
		public void MigrateIfNecassary_RegionContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "Us-US", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='Us']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-US']");
			}
		}

		[Test]
		public void MigrateIfNecassary_VariantContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "biske-BiSKe"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='biske-x-BiSKe']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagAndRegionSubtagContainValidDuplicates_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de", "", "DE", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='DE']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant[@type='x-']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsDuplicateValidlanguage_DuplicateIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-En", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-En']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsOnlyInvalidData_DataIsMovedToPrivateUseAndLanguageSubtagIsSetToQaa()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("bogus-stuff", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsSubtagThatIsToLongForPrivateUse_SubtagIsTruncated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "x-bogusstuffistolong"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogusstu']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsDuplicatePrivateUse_DuplicateIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("bogus", "", "", "x-bogus"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsAnythingButValidScriptAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "bogus-stuff", "", "x-bogus"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-bogus']");
			}
		}

		[Test]
		public void MigrateIfNecassary_RegionSubtagContainsAnythingButValidRegionAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "bogus-stuff", "x-bogus"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-bogus']");
			}
		}

		[Test]
		public void MigrateIfNecassary_VariantSubtagContainsAnythingButValidVariantOrprivateUseAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", "bogus-stuff-x-bogus"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-bogus']");
			}
		}

		[Test]
		public void MigrateIfNecassary_OriginalFileContainsCustomLdmlData_DataIsCopied()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContentWithLdmlInfoWeDontCareAbout("","","",""));
				var migrator =
					new WritingSystemDefinitionLdmlMigrator(
						WritingSystemDefinition.LatestWritingSystemDefinitionVersion,
						_environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/fallback/testing[text()='fallback']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/layout/testing[text()='layout']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/characters/testing[text()='characters']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/delimiters/testing[text()='delimiters']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/measurement/testing[text()='measurement']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/dates/testing[text()='dates']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/numbers/testing[text()='numbers']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/units/testing[text()='units']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/listPatterns/testing[text()='listPatterns']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/posix/testing[text()='posix']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/segmentations/testing[text()='segmentations']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/rbnf/testing[text()='rbnf']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/references/testing[text()='references']");
			}
		}

		[Test]
		public void MigrateIfNecassary_OriginalFileContainsDefaultFontFamilyInfo_DataIsMigrated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(
					LdmlFileContentForTests.CreateVersion0LdmlContentWithLdmlInfoWeDontCareAbout("", "", "", ""));
				var migrator =
					new WritingSystemDefinitionLdmlMigrator(
						WritingSystemDefinition.LatestWritingSystemDefinitionVersion,
						_environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath(
					"/ldml/identity/language[@type='qaa']");
			}
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_OriginalFileIsNotLdmlVWhat_Throw()
		{
			//Need to make sure we are reading/writing the right vrsions of Ldml.rename LdmlAdaptor to LdmlDataMapper
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LdmlIsVersion0_IsLatestVersion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version0LdmlFile);
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(1, versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile));
			}
		}

		[Test]
		public void MigrateIfNecassary_LdmlIsVersion0_NeedsMigratingIsTrue()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version1LdmlFile);
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				Assert.IsFalse(migrator.FileNeedsMigrating);
			}
		}

		[Test]
		public void MigrateIfNecassary_LdmlIsAlreadyLatestVersion_NeedsMigratingIsFalse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version1LdmlFile);
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				Assert.IsFalse(migrator.FileNeedsMigrating);
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsExcessiveXs_XsAreIgnoredAndNonIanaConformDataIsWrittenToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-x-audio", "","",""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidSubtagWithPrecedingX_LanguageSubtagIsSetToValidSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("x-en", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsExcessiveXs_XsAreIgnoredAndNonIanaConformDataIsWrittenToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-Latn-x-test", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-test']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsValidSubtagWithPrecedingX_ScriptSubtagIsSetToValidSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-latn", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='latn']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagInLdmlIsEmpty_LanguageSubtagIsSetToQaa()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "Latn-x-", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
			}

		}

		[Test]
		public void MigrateIfNecassary_RegionSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "US-x-", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='US']");
			}

		}

		[Test]
		public void MigrateIfNecassary_VariantSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", "x-bogus-x-"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus']");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de-e...n", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-en']");
			}
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "Z._.x!x%x-Latn", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-Zxxx']");
			}
		}

		[Test]
		public void MigrateIfNecassary_RegionSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "U.!$%^S-gb", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='gb']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-US']");
			}
		}

		[Test]
		public void MigrateIfNecassary_VariantSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "b!is$^k*_e-1901"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='1901-x-biske']");
			}
		}

		[Test]
		public void MigrateIfNecassary_AmpersandInRfcTagCausesXmlReaderToThrow()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "&"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void MigrateIfNecassary_QuoteInRfcTagCausesXmlReaderToThrow()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "\""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void MigrateIfNecassary_PrivateUseSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "x-t@#$e_st-hi"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-test-hi']");
			}
		}

		[Test]
		public void MigrateIfNecassary_RfcTagContainsOnlyNonAlphaNumericCharactersAndEndsInDashXDash_WritingsystemIsSetToqaa()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("!@$#-x-", "(*^%$-x-@#%-x", "x-@^**__", "x-@#$-x-_-x-x-"));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsAmbiguousLanguageOrRegionAndNoPrecedingLanguageSubtagExists_LeaveAmbiguousTagInLanguageSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("DE", "","",""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='DE']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsAmbiguousLanguageOrRegionAndPrecedingLanguageSubtagExists_MoveAmbiguousTagToRegion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de-DE", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='DE']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsAmbiguousLanguageOrRegionAndidenticalPrecedingLanguageSubtagExists_MoveAmbiguousTagToRegion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de-de", "", "", ""));
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}
	}
}
