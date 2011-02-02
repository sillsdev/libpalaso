using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class WritingSystemDefinitionLdmlMigratorTests
	{
		private class TestEnvironment:IDisposable
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
		public void MigrateIfNecassary_LanguageSubtagContainsXDashAudio_ResultingWsIsVoice()
		{
			using (_environment = new TestEnvironment())
			{
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				migrator.MigrateIfNecassary();
				var adaptor = new LdmlAdaptor();
				WritingSystemDefinition migratedWs = GetMigratedWs(adaptor);
				Assert.IsTrue(migratedWs.IsVoice);
			}
		}

		private WritingSystemDefinition GetMigratedWs(LdmlAdaptor adaptor)
		{
				var migratedWs = new WritingSystemDefinition();
				adaptor.Read(_environment.PathToWritingSystemLdmlFile, migratedWs);
				return migratedWs;
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsIpa_IpaStatusIsSetToIpa()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsIpa_IpaIsRemovedFromLanguageSubtag()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LdmlIsVersion0_IsLatestVersion()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContent.Version0LdmlFile);
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
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContent.Version1LdmlFile);
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				Assert.IsFalse(migrator.FileNeedsMigrating);
			}
		}

		[Test]
		public void MigrateIfNecassary_LdmlIsAlreadyCorrectVersion_NeedsMigratingIsFalse()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContent.Version1LdmlFile);
				var migrator = new WritingSystemDefinitionLdmlMigrator(1, _environment.PathToWritingSystemLdmlFile);
				Assert.IsFalse(migrator.FileNeedsMigrating);
			}
		}

		[Test]
		public void WritingSystemLdmlVersionGetterGetFileVersion_FileHasNoVersion_Returns0()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContent.Version0LdmlFile);
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(0, versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile));
			}
		}

		[Test]
		public void WritingSystemLdmlVersionGetterGetFileVersion_FileIsVersion1_Returns1()
		{
			using (_environment = new TestEnvironment())
			{
				_environment.WriteContentToWritingSystemLdmlFile(LdmlFileContent.Version1LdmlFile);
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(1, versionGetter.GetFileVersion(_environment.PathToWritingSystemLdmlFile));
			}
		}

		private class LdmlFileContent
		{
			static public string Version0LdmlFile
			{
				get { return CreateVersion0LdmlContent("en", String.Empty,String.Empty, String.Empty); }
			}

			static public string Version1LdmlFile
			{
				get { return CreateVersion1LdmlContent("en", String.Empty, String.Empty, String.Empty); }
			}

			static private string CreateVersion0LdmlContent(string language, string script, string region, string variant)
			{
				return
String.Format(@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='0001-01-01T00:00:00' />
		<language type='{0}' />
		<script type='{1}' />
		<region type='{2}' />
		<variant type='{3}' />
	</identity>
	<collations />
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:defaultFontFamily value='Arial' />
		<palaso:defaultFontSize value='12' />
	</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
			}

			static private string CreateVersion1LdmlContent(string language, string script, string region, string variant)
			{
				return
String.Format(@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='0001-01-01T00:00:00' />
		<language type='{0}' />
		<script type='{1}' />
		<region type='{2}' />
		<variant type='{3}' />
	</identity>
	<collations />
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:version>1</palaso:version>
		<palaso:defaultFontFamily value='Arial' />
		<palaso:defaultFontSize value='12' />
	</special>
</ldml>".Replace('\'', '"'), language, script, region, variant);
			}
		}
	}
}
