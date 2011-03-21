using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Migration;
using Palaso.Tests.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using Palaso.TestUtilities;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV1To2Migration;

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

			public Migrator GetMigrator
			{
				get
				{
					var migrator = new Migrator(1, PathToWritingSystemLdmlFile);
					migrator.AddVersionStrategy(new WritingSystemLdmlVersionGetter());
					migrator.AddMigrationStrategy(new Version0MigrationStrategy());
					return migrator;
				}
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
		public void Migrate_LanguageSubtagContainsFonipa_VariantContainsIpaVariantSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-fonipa", "","",""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='fonipa']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashEtic_xDashEticIsMovedToPrivateUseSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-etic", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-etic']");
			}
		}

		[Test]
		public void Migrate_VariantContainsMultipleValidVariants_ValidVariantsAreLeftUntouched()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "biske-bogus-bauddha"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='biske-bauddha-x-bogus']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashEmic_xDashEmicIsMovedToPrivateUseSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-emic", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-emic']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagInLdmlContainsxDashaudio_VariantSubtagInLdmlContainsxDashaudio()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio",String.Empty,String.Empty,String.Empty));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashaudioAndScriptSubtagIsNotEmpty_ScriptSubtagPartsAreMovedToPrivateUseAndScriptIsOverwrittenToBecomeZxxx()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio", "Latn", String.Empty, String.Empty));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio-Latn']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashaudioAndScriptSubtagcontainsZxxx_ZxxxIsNotAppendedToPrivateUseSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio", "Zxxx", String.Empty, String.Empty));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidScript_ScriptIsMovedToScript()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-Latn", String.Empty, String.Empty, String.Empty));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidRegion_RegionIsMovedToRegion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-US", String.Empty, String.Empty, String.Empty));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='US']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidVariant_VariantIsMovedToVariant()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-1901", String.Empty, String.Empty, String.Empty));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
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
		public void Migrate_LanguageSubtagContainsx_xIsNotDuplicated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-audio", String.Empty, String.Empty, "x-test"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio-test']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDataThatIsNotValidLanguageScriptRegionOrVariant_DataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-bogus-stuff", String.Empty, String.Empty, String.Empty));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsMultipleValidLanguageSubtagsAsWellAsDataThatIsNotValidLanguageScriptRegionOrVariant_LanguageIsSetToFirstValidLanguageSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("x-bogus-en-audio-de-bogus2-x-", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsMultipleValidLanguageSubtagsAsWellAsDataThatIsNotValidLanguageScriptRegionOrVariant_AllSubtagsButFirstValidLanguageSubtagAreMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("x-bogus-en-audio-tpi-bogus2-x-", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-audio-bogus2-tpi']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsInvalidData_InvalidDataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "bogus-stuff", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsValidScript_ScriptIsMigrated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Latn", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsMultipleValidScriptSubtagsAsWellAsDataThatIsNotValidScript_NonValidDataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-bogus-Latn-test-Zxxx-bogus2-x-", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-test-bogus2-Zxxx']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsMultipleValidScriptSubtagsAsWellAsDataThatIsNotValidScriptAsWellAsAudio_NonValidDataisMovedToPrivateUseAsScriptIsZxxx()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-bogus-Latn-audio-Afak-bogus2-x-", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-audio-bogus2-Afak-Latn']");
			}
		}

		[Test]
		public void Migrate_LanguageAndScriptSubtagsContainInvalidData_AllInvalidDataIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("bogus-stuff", "more-bogus", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-more-bogus']");
			}
		}

		[Test]
		public void Migrate_RegionSubtagContainsAnythingButValidRegion_InvalidContentIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("qaa", "", "bogus-stuff", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_RegionContainsTagThatIsValidRegion_IsMigrated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "DE", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='DE']");
			}
		}

		[Test]
		public void Migrate_VariantSubtagContainsAnythingButValidVariantOrPrivateUse_InvalidContentIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", "bogus-stuff"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDatathatIsNotValidLanguageScriptRegionOrVariantAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-bogus", "", "", "x-BoGuS-stuff"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_ScriptContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx-zxXX", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-zxXX']");
			}
		}

		[Test]
		public void Migrate_RegionContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "Us-US", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='Us']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-US']");
			}
		}

		[Test]
		public void Migrate_VariantContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "biske-BiSKe"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='biske-x-BiSKe']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagAndRegionSubtagContainValidDuplicates_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de", "", "DE", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='DE']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant[@type='x-']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDuplicateValidlanguage_DuplicateIsMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-En", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-En']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsOnlyInvalidData_DataIsMovedToPrivateUseAndLanguageSubtagIsSetToQaa()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("bogus-stuff", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsSubtagThatIsToLongForPrivateUse_SubtagIsTruncated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "x-bogusstuffistolong"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogusstu']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDuplicatePrivateUse_DuplicateIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("bogus", "", "", "x-bogus"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsAnythingButValidScriptAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "bogus-stuff", "", "x-bogus"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-bogus']");
			}
		}

		[Test]
		public void Migrate_RegionSubtagContainsAnythingButValidRegionAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "bogus-stuff", "x-bogus"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-bogus']");
			}
		}

		[Test]
		public void Migrate_VariantSubtagContainsAnythingButValidVariantOrprivateUseAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", "bogus-stuff-x-bogus"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-stuff-bogus']");
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsCustomLdmlData_DataIsCopied()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContentWithLdmlInfoWeDontCareAbout("","","",""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
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
		public void Migrate_OriginalFileContainsAllSortsOfDataThatShouldJustBeCopiedOver_DataIsMigrated()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(
					LdmlFileContentForTests.CreateVersion0LdmlContentWithAllSortsOfDatathatdoesNotNeedSpecialAttention("", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/special/palaso:defaultFontFamily");
			}
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_DateModified_IsLaterThanBeforeMigration()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(
					LdmlFileContentForTests.CreateVersion0LdmlContentWithAllSortsOfDatathatdoesNotNeedSpecialAttention("", "", "", ""));
				var wsV0 = new WritingSystemDefinitionV0();
				new LdmlAdaptorV0().Read(_environment.PathToWritingSystemLdmlFile, wsV0);
				DateTime dateBeforeMigration = wsV0.DateModified;
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				var wsV1= new WritingSystemDefinitionV1();
				new LdmlAdaptorV1().Read(_environment.PathToWritingSystemLdmlFile, wsV1);
				DateTime dateAfterMigration = wsV1.DateModified;
				Assert.IsTrue(dateAfterMigration > dateBeforeMigration);
			}
		}

		[Test]
		public void Migrate_OriginalFileIsNotVersionThatWeCanMigrate_Throws()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion99LdmlContent());
				var migrator = _environment.GetMigrator;
				Assert.Throws<InvalidOperationException>(() => migrator.Migrate());
			}
		}

		[Test]
		public void Migrate_LanguageNameIsSetTootherThanWhatIanaSubtagRegistrySays_LanguageNameIsMaintained()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContentwithLanguageSubtagAndName("en", "German"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/special/palaso:languageName[@value = 'German'");
			}
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_LanguageNameIsNotSet_LanguageNameIsSetToWhatIanaSubtagRegistrySays()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/special/palaso:languageName[@value = 'English'");
			}
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_LdmlIsVersion0_IsLatestVersion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version0LdmlFile);
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(1, versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile));
			}
		}

		[Test]
		public void Migrate_LdmlIsVersion0_NeedsMigratingIsTrue()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version1LdmlFile);
				var migrator = _environment.GetMigrator;
				Assert.IsFalse(migrator.NeedsMigration());
			}
		}

		[Test]
		public void Migrate_LdmlIsAlreadyLatestVersion_NeedsMigratingIsFalse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version1LdmlFile);
				var migrator = _environment.GetMigrator;
				Assert.IsFalse(migrator.NeedsMigration());
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsExcessiveXs_XsAreIgnoredAndNonIanaConformDataIsWrittenToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-x-audio", "","",""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidSubtagWithPrecedingX_LanguageSubtagIsSetToValidSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("x-en", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsExcessiveXs_XsAreIgnoredAndNonIanaConformDataIsWrittenToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-Latn-x-test", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-test']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsValidSubtagWithPrecedingX_ScriptSubtagIsSetToValidSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "x-latn", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='latn']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagInLdmlIsEmpty_LanguageSubtagIsSetToQaa()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-x-", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "Latn-x-", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
			}

		}

		[Test]
		public void Migrate_RegionSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "US-x-", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='US']");
			}

		}

		[Test]
		public void Migrate_VariantSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "", "", "x-bogus-x-"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogus']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de-e...n", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-en']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("", "Z._x!x%x-Latn", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-Zxxx']");
			}
		}

		[Test]
		public void Migrate_RegionSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "U.!$%^S-gb", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='gb']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-US']");
			}
		}

		[Test]
		public void Migrate_VariantSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "b*is^k_e-1901"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='1901-x-biske']");
			}
		}

		[Test]
		public void Migrate_PrivateUseSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "x-t@#$e_st-hi"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-test-hi']");
			}
		}

		[Test]
		public void Migrate_RfcTagContainsOnlyNonAlphaNumericCharactersAndEndsInDashXDash_WritingsystemIsSetToqaa()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("!@$#-x-", "(*^%$-x-@#%-x", "x-@^**__", "x-@#$-x-_-x-x-"));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsAmbiguousLanguageOrRegionAndNoPrecedingLanguageSubtagExists_LeaveAmbiguousTagInLanguageSubtag()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("DE", "","",""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='DE']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsAmbiguousLanguageOrRegionAndPrecedingLanguageSubtagExists_MoveAmbiguousTagToRegion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de-DE", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='DE']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsAmbiguousLanguageOrRegionAndidenticalPrecedingLanguageSubtagExists_MoveAmbiguousTagToRegion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("de-de", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='de']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagStartsWithDashAndContainsValidScript_LangugaeIsSetToQaaAndScriptIsMoved()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("-Zxxx", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsZxxx_VariantContainsxDashaudio()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en-Zxxx", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(_environment.PathToWritingSystemLdmlFile).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_FileIsVersion0_IsMigratedToLatestVersion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", ""));
				var migrator = _environment.GetMigrator;
				migrator.Migrate();
				Assert.AreEqual(1, migrator.GetFileVersion());
			}
		}

		[Test]
		public void NeedsMigration_FileIsVersion1_IsFalse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContentForTests.Version1LdmlFile);
				var migrator = _environment.GetMigrator;
				migrator.NeedsMigration();
				Assert.IsFalse(migrator.NeedsMigration());
			}
		}
	}
}
