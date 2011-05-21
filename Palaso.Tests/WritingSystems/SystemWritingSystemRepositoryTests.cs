using System.IO;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class SystemWritingSystemRepositoryTests
	{
		[Test]
		public void DefaultConstructor_HasCorrectPath()
		{
			var repo = new SystemWritingSystemRepository();
			Assert.That(repo.PathToWritingSystems, Is.StringMatching(".*SIL.WritingSystemRepository.1"));
		}

		[Test]
		public void PathConstructor_HasCorrectPath()
		{
			using (var e = new TemporaryFolder("SystemWritingSystemRepositoryTests"))
			{
				var repo = new SystemWritingSystemRepository(e.Path);
				Assert.That(repo.PathToWritingSystems, Is.StringMatching(".*WritingSystemRepository.1"));
			}
		}

		[Test]
		public void Constructor_CreatesFolders()
		{
			using (var e = new TemporaryFolder("SystemWritingSystemRepositoryTests"))
			{
				var repo = new SystemWritingSystemRepository(e.Path);
				Assert.That(Directory.Exists(repo.PathToWritingSystems), Is.True);
			}
		}

		[Test]
		public void Constructor_WithExistingFolders_NoThrow()
		{
			using (var e = new TemporaryFolder("SystemWritingSystemRepositoryTests"))
			{
				new SystemWritingSystemRepository(e.Path);
				var repo2 = new SystemWritingSystemRepository(e.Path);
				Assert.That(Directory.Exists(repo2.PathToWritingSystems), Is.True);
			}
		}

	}
}
