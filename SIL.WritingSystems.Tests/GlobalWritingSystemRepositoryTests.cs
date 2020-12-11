using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class GlobalWritingSystemRepositoryTests
	{
		private static TemporaryFolder CreateTemporaryFolder(string testName)
		{
			return new TemporaryFolder($"{testName}_{Path.GetRandomFileName()}");
		}

		[Test]
		[Platform(Exclude = "Linux", Reason="Test tries to create directory under /var/lib where user doesn't have write permissions by default")]
		public void DefaultInitializer_HasCorrectPath()
		{
			GlobalWritingSystemRepository repo = GlobalWritingSystemRepository.Initialize();
			string expectedPath = string.Format(".*SIL.WritingSystemRepository.{0}",
				LdmlDataMapper.CurrentLdmlLibraryVersion);
			Assert.That(repo.PathToWritingSystems, Does.Match(expectedPath));
		}

		[Test]
		public void Initialize_SkipsBadFile()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string versionPath = Path.Combine(e.Path, LdmlDataMapper.CurrentLdmlLibraryVersion.ToString());
				Directory.CreateDirectory(versionPath);
				string badFile = Path.Combine(versionPath, "en.ldml");
				File.WriteAllBytes(badFile, new byte[100]); // 100 nulls
				var repo = GlobalWritingSystemRepository.InitializeWithBasePath(e.Path, null);
				// main part of test is that we don't get any exception.
				Assert.That(repo.Count, Is.EqualTo(1));
				// original .ldml file should have been renamed
				Assert.That(File.Exists(badFile), Is.False);
				Assert.That(File.Exists(badFile + ".bad"), Is.True);
			}
		}

		[Test]
		public void Initialize_SkipsBadFile_DataProblemInLDMLIdentity()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var versionPath = Path.Combine(e.Path, LdmlDataMapper.CurrentLdmlLibraryVersion.ToString());
				Directory.CreateDirectory(versionPath);
				var badFile = Path.Combine(versionPath, "en.ldml");
				const string ldmlData = @"<?xml version='1.0' encoding='utf-8'?>
					<ldml>
						<identity>
							<version number=''/>
							<generation date='2020-02-28T18:43:36Z'/>
							<language type=''/>
							<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
								<sil:identity windowsLCID='1033'/>
							</special>
						</identity>
					</ldml>";
				File.WriteAllText(badFile, ldmlData);
				var repo = GlobalWritingSystemRepository.InitializeWithBasePath(e.Path, null);
				// main part of test is that we don't get any exception.
				Assert.That(repo.Count, Is.EqualTo(1));
				// original .ldml file should have been renamed
				Assert.That(File.Exists(badFile), Is.False);
				Assert.That(File.Exists(badFile + ".bad"), Is.True);
				Assert.That(File.Exists(Path.Combine(versionPath, "badldml.log")), Is.True);
			}
		}

		[Test]
		public void Initialize_SkipsBadFile_DataProblemInLDMLFont()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var versionPath = Path.Combine(e.Path, LdmlDataMapper.CurrentLdmlLibraryVersion.ToString());
				Directory.CreateDirectory(versionPath);
				var badFile = Path.Combine(versionPath, "en.ldml");
				const string ldmlData = @"<?xml version='1.0' encoding='utf-8'?>
					<ldml>
						<identity>
							<version number=''/>
							<generation date='2020-02-28T18:43:36Z'/>
							<language type='en'/>
							<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
								<sil:identity windowsLCID='1033'/>
							</special>
						</identity>
						<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
							<sil:external-resources>
								<sil:font name='Amdo Classic 1' types='kaboom' />
							</sil:external-resources>
						</special>	 
					</ldml>";
				File.WriteAllText(badFile, ldmlData);
				var repo = GlobalWritingSystemRepository.InitializeWithBasePath(e.Path, null);
				// main part of test is that we don't get any exception.
				Assert.That(repo.Count, Is.EqualTo(0));
				// original .ldml file should have been renamed
				Assert.That(File.Exists(badFile), Is.False);
				Assert.That(File.Exists(badFile + ".bad"), Is.True);
				Assert.That(File.Exists(Path.Combine(versionPath, "badldml.log")), Is.True);
			}
		}

		[Test]
		public void PathConstructor_HasCorrectPath()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				Assert.That(repo.PathToWritingSystems,
					Does.Match($".*PathConstructor_HasCorrectPath.*{LdmlDataMapper.CurrentLdmlLibraryVersion}"));
			}
		}

		[Test]
		public void Constructor_CreatesFolders()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				Assert.That(Directory.Exists(repo.PathToWritingSystems), Is.True);
			}
		}

		[Test]
		public void Constructor_WithExistingFolders_NoThrow()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				new GlobalWritingSystemRepository(e.Path);
				var repo2 = new GlobalWritingSystemRepository(e.Path);
				Assert.That(Directory.Exists(repo2.PathToWritingSystems), Is.True);
			}
		}

		[Test]
		public void Set_NewWritingSystem_SetsId()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
		public void Save_ChangingIcuSort_DoesNotDuplicateInLdmlFile()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				ws.Collations.Add(new IcuRulesCollationDefinition("standard") { IcuRules = "&b < a" });
				repo.Set(ws);
				repo.Save();
				// ensure that last modified timestamp changes
				Thread.Sleep(1000);
				ws.WindowsLcid = "test";
				repo.Save();
				AssertThatXmlIn.File(repo.GetFilePathFromLanguageTag("en-US")).HasSpecifiedNumberOfMatchesForXpath("/ldml/collations/collation", 1);
			}
		}

		[Test]
		public void Get_LdmlAddedByAnotherRepo_ReturnsDefinition()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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

		// LF-297
		[TestCase("en-US")]
		[TestCase("en-us")]
		[Platform(Include = "Linux", Reason = "Requires a case-sensitive file system")]
		public void Get_CaseDifferingWritingSystems_DoesNotThrow(string id)
		{
			using (var temporaryFolder = CreateTemporaryFolder("Get_CaseDifferingWritingSystems_DoesNotThrow"))
			{
				// Setup
				var repo = new GlobalWritingSystemRepository(temporaryFolder.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();
				// Now we simulate that the user did a S/R which added a WS that differs by case
				File.Copy(Path.Combine(temporaryFolder.Path, "3", ws.Id + ".ldml"),
					Path.Combine(temporaryFolder.Path, "3", ws.Id.ToLower() + ".ldml"));

				// SUT/Verify
				Assert.That(() => repo.Get(id), Throws.Nothing);
			}
		}

		[TestCase("en-US")]
		[TestCase("en-us")]
		public void Remove_CaseDifferingWritingSystems_DoesNotThrow(string id)
		{
			using (var temporaryFolder = CreateTemporaryFolder("Remove_CaseDifferingWritingSystems_DoesNotThrow"))
			{
				// Setup
				var repo = new GlobalWritingSystemRepository(temporaryFolder.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();

				// SUT
				Assert.That(() => repo.Remove(id), Throws.Nothing);

				// Verify
				Assert.That(repo.Contains("en-US"), Is.False);
				Assert.That(repo.Contains("en-us"), Is.False);
				Assert.That(File.Exists(Path.Combine(temporaryFolder.Path, "3", "en-US.ldml")),
					Is.False);
				Assert.That(File.Exists(Path.Combine(temporaryFolder.Path, "3", "en-us.ldml")),
					Is.False);
			}
		}

		[Test]
		public void AllWritingSystems_LdmlAddedByAnotherRepo_ReturnsDefinition()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
		public void AllWritingSystems_LdmlCheckingSetEmptyCanNotSave()
		{
			using (var tf = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
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
				// a ws with an empty Id is assumed to be new, we can't save it if the LanguageTag is already found
				// in the repo.
				ws.Id = string.Empty;
				Assert.That(repo2.CanSet(ws), Is.False, "A ws with an empty ID will not save if the LanguageTag matches an existing ws");
				repo2.Save();
				Assert.That(repo1.Get("en-US").WindowsLcid, Is.Not.EqualTo("test"), "Changes should not have been saved.");
			}
		}
	}
}
