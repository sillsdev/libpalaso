using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class GlobalWritingSystemRepositoryTests
	{
		private void OnMigration(int toVersion, IEnumerable<LdmlMigrationInfo> migrationInfo)
		{
		}

		[Test]
		[Platform(Exclude = "Linux", Reason="Test tries to create directory under /var/lib where user doesn't have write permissions by default")]
		public void DefaultInitializer_HasCorrectPath()
		{
			GlobalWritingSystemRepository repo = GlobalWritingSystemRepository.Initialize(OnMigration);
			string expectedPath = string.Format(".*SIL.WritingSystemRepository.{0}", 
				LdmlDataMapper.CurrentLdmlVersion);
			Assert.That(repo.PathToWritingSystems, Is.StringMatching(expectedPath));
		}

		[Test]
		public void PathConstructor_HasCorrectPath()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				string expectedPath = string.Format(".*GlobalWritingSystemRepositoryTests.{0}",
					LdmlDataMapper.CurrentLdmlVersion);
				Assert.That(repo.PathToWritingSystems, Is.StringMatching(expectedPath));
			}
		}

		[Test]
		public void Constructor_CreatesFolders()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				Assert.That(Directory.Exists(repo.PathToWritingSystems), Is.True);
			}
		}

		[Test]
		public void Constructor_WithExistingFolders_NoThrow()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				Assert.That(Directory.Exists(repo2.PathToWritingSystems), Is.True);
			}
		}

		[Test]
		public void Set_NewWritingSystem_SetsId()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				Assert.That(ws.Id, Is.Null);
				repo.Set(ws);
				Assert.That(ws.Id, Is.EqualTo("en-US"));
			}
		}

	}
}
