using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class WritingSystemDefinitionLdmlMigratorTests
	{
		string _pathToLdml = Path.GetTempFileName();

		[Test]
		public void MigrateIfNecassary_LanguageSubtagContainsXDashAudio_ResultingWsIsVoice()
		{
			var migrator = new WritingSystemDefinitionLdmlMigrator(1, _pathToLdml);
			migrator.MigrateIfNecassary();
			var adaptor = new LdmlAdaptor();
			WritingSystemDefinition migratedWs = GetMigratedWs(adaptor);
			Assert.IsTrue(migratedWs.IsVoice);
		}

		private WritingSystemDefinition GetMigratedWs(LdmlAdaptor adaptor)
		{
			var migratedWs = new WritingSystemDefinition();
			adaptor.Read(_pathToLdml, migratedWs);
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
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_LdmlIsVersion0_NeedsMigratingIsTrue()
		{
			throw new NotImplementedException();
		}
	}
}
