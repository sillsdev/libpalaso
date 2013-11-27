using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class GlobalWritingSystemRepositoryTests
	{
		private void OnMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}

		[Test]
		[Platform(Exclude = "Linux", Reason="Test tries to create directory under /var/lib where user doesn't have write permissions by default")]
		public void DefaultInitializer_HasCorrectPath()
		{
			var repo = GlobalWritingSystemRepository.Initialize(OnMigration);
			Assert.That(repo.PathToWritingSystems, Is.StringMatching(".*SIL.WritingSystemRepository.2"));
		}

		[Test]
		public void PathConstructor_HasCorrectPath()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				Assert.That(repo.PathToWritingSystems, Is.StringMatching(".*GlobalWritingSystemRepositoryTests.2"));
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

	}
}
