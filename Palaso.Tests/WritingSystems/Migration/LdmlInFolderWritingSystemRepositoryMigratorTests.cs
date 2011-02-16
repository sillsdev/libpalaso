using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class LdmlInFolderWritingSystemRepositoryMigratorTests
	{
		[Test]
		public void MigrateIfNecassary_WritingSystemIsV0_MigratedToLatest()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_WritingSystemRepositoryContainsWsThatWouldBeMigratedToDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			TemporaryFolder ldmlWs = new TemporaryFolder("WsCollectionForTesting");
			TempFile pathToWs = ldmlWs.GetNewTempFile(true);
			pathToWs.MoveTo(ldmlWs.Path + "en-Zxxx-x-audio.ldml");
			TempFile pathToWsThatWillLeadToDuplicate = ldmlWs.GetNewTempFile(true);
			pathToWsThatWillLeadToDuplicate.MoveTo(ldmlWs.Path + "en-x-audio.ldml");
			TempFile pathToSecondWsThatWillLeadToDuplicate = ldmlWs.GetNewTempFile(true);
			pathToWsThatWillLeadToDuplicate.MoveTo(ldmlWs.Path + "en-Zxxx-Region-x-audio.ldml");
			var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(ldmlWs.Path);
			migrator.MigrateIfNecassary();
			var store = new LdmlInFolderWritingSystemRepository();
			Assert.IsTrue(store.WritingSystemDefinitions.Any(ws => ws.RFC5646.Equals("en-Zxxx-x-audio", StringComparison.OrdinalIgnoreCase)));
			Assert.IsTrue(store.WritingSystemDefinitions.Any(ws => ws.RFC5646.Equals("en-Zxxx-x-audio-dupl1", StringComparison.OrdinalIgnoreCase)));
			Assert.IsTrue(store.WritingSystemDefinitions.Any(ws => ws.RFC5646.Equals("en-Zxxx-x-audio-dupl2", StringComparison.OrdinalIgnoreCase)));
			throw new NotImplementedException("Why is pathToSecondWsThatWillLeadToDuplicate grayed out?");
		}

		[Test]
		public void MigrateIfNecassary_WritingSystemRepositoryContainsWsThatWouldBeMigratedToCaseInsensitiveDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			TemporaryFolder ldmlWs = new TemporaryFolder("WsCollectionForTesting");
			TempFile pathToWs = ldmlWs.GetNewTempFile(true);
			pathToWs.MoveTo(ldmlWs.Path + "en-ZXXX-x-audio.ldml");
			TempFile pathToWsThatWillLeadToDuplicate = ldmlWs.GetNewTempFile(true);
			pathToWsThatWillLeadToDuplicate.MoveTo(ldmlWs.Path + "en-x-audio.ldml");
			TempFile pathToSecondWsThatWillLeadToDuplicate = ldmlWs.GetNewTempFile(true);
			pathToSecondWsThatWillLeadToDuplicate.MoveTo(ldmlWs.Path + "en-zXxX-Region-x-AuDio.ldml");
			var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(ldmlWs.Path);
			migrator.MigrateIfNecassary();
			var store = new LdmlInFolderWritingSystemRepository();
			Assert.IsTrue(store.WritingSystemDefinitions.Any(ws => ws.RFC5646.Equals("en-Zxxx-x-audio", StringComparison.OrdinalIgnoreCase)));
			Assert.IsTrue(store.WritingSystemDefinitions.Any(ws => ws.RFC5646.Equals("en-Zxxx-x-audio-dupl1", StringComparison.OrdinalIgnoreCase)));
			Assert.IsTrue(store.WritingSystemDefinitions.Any(ws => ws.RFC5646.Equals("en-Zxxx-x-audio-dupl2", StringComparison.OrdinalIgnoreCase)));
		}

		[Test]
		public void MigrateIfNecassary_WritingSystemIsV0_WritingSystemIsLoadableByLdmlInFolderWritingSystemRepository()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_WritingSystemRepositoryContainsWsThatWouldBeMigratedToDuplicateOfExistingWs_WritingSystemIsLoadableByLdmlInFolderWritingSystemRepository()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void MigrateIfNecassary_WritingSystemRepositoryContainsWritingSystemsThatWouldBeMigratedToDuplicateOfEachother_WritingSystemIsLoadableByLdmlInFolderWritingSystemRepository()
		{
			throw new NotImplementedException();
		}
	}
}
