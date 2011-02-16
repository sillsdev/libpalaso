using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Tests.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
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

		private WritingSystemDefinition GetMigratedWs()
		{
			var migratedWs = new WritingSystemDefinition();
			var adaptor = new LdmlAdaptor();
			adaptor.Read(_environment.PathToWritingSystemLdmlFile, migratedWs);
			return migratedWs;
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsFonipa_IpaStatusIsSetToIpa()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsFonipa_FonipaIsMovedToVariantSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsFonipa_FonipaIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEtic_IpaStatusIsSetToPhonetic()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEtic_xDashEticIsMovedToPrivateUseSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEtic_xDashEticIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEmic_IpaStatusIsSetToPhonemic()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEmic_xDashEmicIsMovedToPrivateUseSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashEmic_xDashEmicIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashaudio_IsVoiceIsSetToTrue()
		{
			using (_environment = new TestEnvironment())
			{
				var migrator = new WritingSystemDefinitionLdmlMigrator(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				WritingSystemDefinition migratedWs = GetMigratedWs();
				Assert.IsTrue(migratedWs.IsVoice);
			}
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashaudio_xDashaudioIsMovedToPrivateUseSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsxDashaudio_xDashaudioIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidScript_ScriptIsMovedToScript()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidScript_ScriptIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidRegion_RegionIsMovedToRegion()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidRegion_RegionIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidVariant_VariantIsMovedToVariant()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidVariant_VariantIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsValidPrivateUse_xIsNotDuplicated()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagconatinsDatathatIsNotValidLanguageScriptRegionOrVariant_DataIsMovedToPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsAnythingButValidScript_InvalidContentIsMovedToPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_RegionSubtagContainsAnythingButValidRegion_InvalidContentIsMovedToPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_VariantSubtagContainsAnythingButValidVariantOrPrivateUse_InvalidContentIsMovedToPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsDatathatIsNotValidLanguageScriptRegionOrVariantAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_ScriptSubtagContainsAnythingButValidScriptAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_RegionSubtagContainsAnythingButValidRegionAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_VariantSubtagContainsAnythingButValidVariantOrprivateUseAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
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
	}
}
