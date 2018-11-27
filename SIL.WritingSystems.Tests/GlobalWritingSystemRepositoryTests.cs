using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.Extensions.Is;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class GlobalWritingSystemRepositoryTests
	{
		[Test]
		[Platform(Exclude = "Linux", Reason="Test tries to create directory under /var/lib where user doesn't have write permissions by default")]
		public void DefaultInitializer_HasCorrectPath()
		{
			GlobalWritingSystemRepository repo = GlobalWritingSystemRepository.Initialize();
			string expectedPath = string.Format(".*SIL.WritingSystemRepository.{0}", 
				LdmlDataMapper.CurrentLdmlLibraryVersion);
			Assert.That(repo.PathToWritingSystems, Is.StringMatching(expectedPath));
		}

		[Test]
		public void Initialize_SkipsBadFile()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				string versionPath = Path.Combine(e.Path, LdmlDataMapper.CurrentLdmlLibraryVersion.ToString());
				Directory.CreateDirectory(versionPath);
				string badFile = Path.Combine(versionPath, "en.ldml");
				File.WriteAllBytes(badFile, new byte[100]); // 100 nulls
				var repo = GlobalWritingSystemRepository.InitializeWithBasePath(e.Path, null);
				// main part of test is that we don't get any exception.
				Assert.That(repo.Count, Is.EqualTo(0));
				// original .ldml file should have been renamed
				Assert.That(File.Exists(badFile), Is.False);
				Assert.That(File.Exists(badFile + ".bad"), Is.True);
			}
		}

		[Test]
		public void PathConstructor_HasCorrectPath()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				string expectedPath = string.Format(".*GlobalWritingSystemRepositoryTests.{0}",
					LdmlDataMapper.CurrentLdmlLibraryVersion);
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

		[Test]
		public void Save_NewWritingSystem_CreatesLdmlFile()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();
				Assert.That(File.Exists(repo.GetFilePathFromLanguageTag("en-US")), Is.True);
			}
		}

		[Test]
		public void Save_DeletedWritingSystem_RemovesLdmlFile()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();
				Assert.That(File.Exists(repo.GetFilePathFromLanguageTag("en-US")), Is.True);

				ws.MarkedForDeletion = true;
				repo.Save();
				Assert.That(File.Exists(repo.GetFilePathFromLanguageTag("en-US")), Is.False);
			}
		}

		[Test]
		public void Save_UpdatedWritingSystem_UpdatesLdmlFile()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();
				DateTime modified = File.GetLastWriteTime(repo.GetFilePathFromLanguageTag("en-US"));
				// ensure that last modified timestamp changes
				Thread.Sleep(1000);
				ws.WindowsLcid = "test";
				repo.Save();
				Assert.That(File.GetLastWriteTime(repo.GetFilePathFromLanguageTag("en-US")), Is.Not.EqualTo(modified));
			}
		}

		[Test]
		public void Get_LdmlAddedByAnotherRepo_ReturnsDefinition()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				Assert.That(() => repo2.Get("en-US"), Throws.TypeOf<ArgumentOutOfRangeException>());
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(File.Exists(repo1.GetFilePathFromLanguageTag("en-US")), Is.True);
				Assert.That(repo2.Get("en-US").LanguageTag, Is.EqualTo("en-US"));
			}
		}

		[Test]
		public void Get_LdmlRemovedByAnotherRepo_Throws()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(repo1.Get("en-US").LanguageTag, Is.EqualTo("en-US"));
				repo2.Remove("en-US");
				Assert.That(() => repo1.Get("en-US"), Throws.TypeOf<ArgumentOutOfRangeException>());
			}
		}

		[Test]
		public void Get_LdmlUpdatedByAnotherRepo_ReturnsUpdatedDefinition()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(ws.WindowsLcid, Is.Empty);
				// ensure that last modified timestamp changes
				Thread.Sleep(1000);
				ws = repo2.Get("en-US");
				ws.WindowsLcid = "test";
				repo2.Save();
				Assert.That(repo1.Get("en-US").WindowsLcid, Is.EqualTo("test"));
			}
		}

		[Test]
		public void Get_UpdatedLdmlRemovedByAnotherRepo_ReturnUpdatedDefinition()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(repo1.Get("en-US").LanguageTag, Is.EqualTo("en-US"));
				repo2.Remove("en-US");
				ws.WindowsLcid = "test";
				Assert.That(repo1.Get("en-US").WindowsLcid, Is.EqualTo("test"));
			}
		}

		[Test]
		public void Get_UpdatedLdmlUpdatedByAnotherRepo_ReturnLastUpdatedDefinition()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(repo1.Get("en-US").LanguageTag, Is.EqualTo("en-US"));
				WritingSystemDefinition ws2 = repo2.Get("en-US");
				ws2.WindowsLcid = "test2";
				repo2.Save();
				ws.WindowsLcid = "test1";
				Assert.That(repo1.Get("en-US").WindowsLcid, Is.EqualTo("test1"));
			}
		}

		[Test]
		public void AllWritingSystems_LdmlAddedByAnotherRepo_ReturnsDefinition()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				Assert.That(repo2.AllWritingSystems, Is.Empty);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(File.Exists(repo1.GetFilePathFromLanguageTag("en-US")), Is.True);
				Assert.That(repo2.AllWritingSystems.First().LanguageTag, Is.EqualTo("en-US"));
			}
		}

		[Test]
		public void AllWritingSystems_LdmlRemovedByAnotherRepo_ReturnsEmpty()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(repo1.AllWritingSystems, Is.Not.Empty);
				repo2.Remove("en-US");
				Assert.That(repo1.AllWritingSystems, Is.Empty);
			}
		}

		[Test]
		public void AllWritingSystems_LdmlUpdatedByAnotherRepo_ReturnsUpdatedDefinition()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(ws.WindowsLcid, Is.Empty);
				ws = repo2.Get("en-US");
				ws.WindowsLcid = "test";
				repo2.Save();
				Assert.That(repo2.AllWritingSystems.First().WindowsLcid, Is.EqualTo("test"));
			}
		}

		[Test]
		public void Count_LdmlAddedByAnotherRepo_ReturnsOne()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				Assert.That(repo2.Count, Is.EqualTo(0));
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(File.Exists(repo1.GetFilePathFromLanguageTag("en-US")), Is.True);
				Assert.That(repo2.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void Count_LdmlRemovedByAnotherRepo_ReturnsZero()
		{
			using (var e = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(repo1.Count, Is.EqualTo(1));
				repo2.Remove("en-US");
				Assert.That(repo1.Count, Is.EqualTo(0));
			}
		}

		[Test]
		public void AllWritingSystems_LdmlCheckingSetEmptyandGetWSId()
		{
			using (var tf = new TemporaryFolder("GlobalWritingSystemRepositoryTests"))
			{
				var repo1 = new GlobalWritingSystemRepository(tf.Path);
				var repo2 = new GlobalWritingSystemRepository(tf.Path);

				var ws = new WritingSystemDefinition("en-US");
				repo1.Set(ws);
				repo1.Save();
				Assert.That(ws.WindowsLcid, Is.Empty);
				// ensure that last modified timestamp changes
				Thread.Sleep(1000);
				ws = repo2.Get("en-US");
				ws.WindowsLcid = "test";
				// Make an arbitrary change to force a save of the ldml file.
				ws.Id = string.Empty;
				repo2.Save();
				Assert.That(repo1.Get("en-US").WindowsLcid, Is.EqualTo("test"));
				Assert.That(ws.IsChanged, Is.False);
				Assert.AreEqual(ws.Id, "en-US");
			}
		}
	}
}
