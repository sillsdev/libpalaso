using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SIL.IO;
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
								<sil:font name='Amdo Classic 1' engines='kaboom' />
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
		public void Save_NewAndUpdatedWritingSystem_LeavesNoTemporaryFile()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();
				ws.WindowsLcid = "test";
				repo.Save();
				Assert.That(Directory.GetFiles(repo.PathToWritingSystems, "*.tmp"), Is.Empty);
				Assert.That(Directory.GetFiles(repo.PathToWritingSystems, "*.bak"), Is.Empty);
			}
		}

		/// <summary>
		/// A writing system generated from a template begins as a copy of it. That copy has to reach
		/// the store the same way any other definition does, so that a save interrupted while it is
		/// being made cannot leave a partial definition behind.
		/// </summary>
		[Test]
		public void Save_WritingSystemFromTemplate_StoresItAndLeavesNoTemporaryFile()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			using (var templates = CreateTemporaryFolder("TemplateSource"))
			{
				string templatePath = Path.Combine(templates.Path, "fr.ldml");
				File.WriteAllText(templatePath, FrenchTemplateLdml);

				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("fr") { Template = templatePath };
				repo.Set(ws);
				repo.Save();

				Assert.That(File.Exists(repo.GetFilePathFromLanguageTag("fr")), Is.True);
				Assert.That(Directory.GetFiles(repo.PathToWritingSystems, "*.tmp"), Is.Empty);
				Assert.That(Directory.GetFiles(repo.PathToWritingSystems, "*.bak"), Is.Empty);
				Assert.That(File.Exists(templatePath), Is.True, "the template itself must be left alone");

				var reread = new GlobalWritingSystemRepository(e.Path);
				Assert.That(reread.AllWritingSystems.Select(w => w.Id), Is.EquivalentTo(new[] { "fr" }));
			}
		}

		/// <summary>
		/// A template comes from a per-user cache, so its own permissions are not the shared store's.
		/// The definition seeded from it has to be as editable by the group as any other.
		/// </summary>
		[Test]
		[Platform(Include = "Linux,MacOsX", Reason = "permission bits of this kind exist only on Unix")]
		public void Save_WritingSystemFromTemplate_StaysGroupWritable()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			using (var templates = CreateTemporaryFolder("TemplateSource"))
			{
				string templatePath = Path.Combine(templates.Path, "fr.ldml");
				File.WriteAllText(templatePath, FrenchTemplateLdml);
				// A template only its author can write, as the SLDR cache leaves it.
				Assert.That(UnixFilePermissions.TrySetMode(templatePath, 0x180), Is.True); // 0600

				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("fr") { Template = templatePath };
				repo.Set(ws);
				repo.Save();

				string filePath = repo.GetFilePathFromLanguageTag("fr");
				Assert.That(UnixFilePermissions.TryGetMode(filePath, out uint mode), Is.True);
				Assert.That(mode & 0x10, Is.EqualTo(0x10), // group write
					$"a definition seeded from a template should be group-writable but is {Convert.ToString(mode, 8)}");
			}
		}

		private const string FrenchTemplateLdml =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<ldml>
	<identity>
		<version number=""$Revision: 11161 $""/>
		<generation date=""$Date: 2015-01-30 22:33 +0000 $""/>
		<language type=""fr""/>
	</identity>
</ldml>";

		/// <summary>
		/// A definition is built beside the old one and swapped in, so a temporary file that an
		/// interrupted save left behind must never be mistaken for a writing system.
		/// </summary>
		[Test]
		public void AllWritingSystems_StrayTemporaryFile_IgnoresIt()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();

				File.Copy(repo.GetFilePathFromLanguageTag("en-US"),
					Path.Combine(repo.PathToWritingSystems, "fr.ldml.99999.tmp"));

				var otherRepo = new GlobalWritingSystemRepository(e.Path);
				Assert.That(otherRepo.Count, Is.EqualTo(1));
				Assert.That(otherRepo.AllWritingSystems.Select(w => w.Id), Is.EquivalentTo(new[] { "en-US" }));
			}
		}

		/// <summary>
		/// The swap is all or nothing. When the replacement cannot be put in place, the definition
		/// already in the store survives intact instead of being left truncated or missing.
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux,MacOsX",
			Reason = "a read-only file is not a reliable way to deny a write on Unix")]
		public void Save_CannotReplaceExistingFile_LeavesItIntact()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();

				string filePath = repo.GetFilePathFromLanguageTag("en-US");
				string originalContents = File.ReadAllText(filePath);
				File.SetAttributes(filePath, FileAttributes.ReadOnly);
				try
				{
					ws.WindowsLcid = "test";
					repo.Save();
				}
				finally
				{
					File.SetAttributes(filePath, FileAttributes.Normal);
				}

				Assert.That(File.ReadAllText(filePath), Is.EqualTo(originalContents));
				Assert.That(Directory.GetFiles(repo.PathToWritingSystems, "*.tmp"), Is.Empty);
			}
		}

		/// <summary>
		/// A shared store is only usable by a group if its definitions stay group-writable. Replacing a
		/// file can carry the permissions of either the replacement or the file it displaces, and the
		/// mask that makes new files group-writable applies only while they are being created, so the
		/// permissions that survive a save have to be checked rather than assumed.
		/// </summary>
		[Test]
		[Platform(Include = "Linux", Reason = "permission bits of this kind exist only on Unix")]
		public void Save_NewAndUpdatedWritingSystem_StaysGroupWritable()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();

				string filePath = repo.GetFilePathFromLanguageTag("en-US");
				Assert.That(IsGroupWritable(filePath), Is.True,
					$"a newly created definition should be group-writable but is {FileMode(filePath)}");

				ws.WindowsLcid = "test";
				repo.Save();

				Assert.That(IsGroupWritable(filePath), Is.True,
					$"a replaced definition should stay group-writable but is {FileMode(filePath)}");
			}
		}

		private static string FileMode(string path)
		{
			var startInfo = new ProcessStartInfo("stat", $"-c %a \"{path}\"")
			{
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			using (var process = Process.Start(startInfo))
			{
				string mode = process.StandardOutput.ReadToEnd().Trim();
				process.WaitForExit();
				return mode;
			}
		}

		private static bool IsGroupWritable(string path)
		{
			string mode = FileMode(path);
			// The group's permissions are the second digit from the right, and 2 is its write bit.
			int group = (int) char.GetNumericValue(mode[mode.Length - 2]);
			return (group & 2) == 2;
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
		public void Replace_StaleLocalRepoUpdateFileExists_ReplacesAndRemovesStaleFile()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();
				string wsFilePath = repo.GetFilePathFromLanguageTag("en-US");
				Assert.That(File.Exists(wsFilePath), Is.True);

				// Simulate debris left by a process that died mid-Replace on an earlier run.
				string staleFile = wsFilePath + ".localrepoupdate";
				File.WriteAllText(staleFile, "stale");

				var newWs = new WritingSystemDefinition("en-US") {WindowsLcid = "test"};
				repo.Replace("en-US", newWs);

				Assert.That(repo.Get("en-US").WindowsLcid, Is.EqualTo("test"),
					"Replace should have taken effect");
				Assert.That(File.Exists(staleFile), Is.False,
					"Stale .localrepoupdate file should not be left behind");
			}
		}

		[Test]
		public void Replace_StaleLocalRepoUpdateFileAndNoLdmlFile_ReplacesAndRemovesStaleFile()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws);
				repo.Save();
				string wsFilePath = repo.GetFilePathFromLanguageTag("en-US");
				Assert.That(File.Exists(wsFilePath), Is.True);

				// Simulate a process that died after stashing the file but before restoring it.
				string staleFile = wsFilePath + ".localrepoupdate";
				File.Copy(wsFilePath, staleFile);
				File.Delete(wsFilePath);

				var newWs = new WritingSystemDefinition("en-US") {WindowsLcid = "test"};
				repo.Replace("en-US", newWs);

				Assert.That(repo.Get("en-US").WindowsLcid, Is.EqualTo("test"),
					"Replace should have taken effect");
				Assert.That(File.Exists(staleFile), Is.False,
					"Stale .localrepoupdate file should not be left behind");
			}
		}

		[Test]
		public void Replace_NoLdmlFileAndNoLocalRepoUpdateFile_Replaces()
		{
			using (var e = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				var repo = new GlobalWritingSystemRepository(e.Path);
				var ws = new WritingSystemDefinition("en-US");
				repo.Set(ws); // in memory only; never saved, so no .ldml file exists yet
				string wsFilePath = repo.GetFilePathFromLanguageTag("en-US");
				Assert.That(File.Exists(wsFilePath), Is.False);

				var newWs = new WritingSystemDefinition("en-US") {WindowsLcid = "test"};
				repo.Replace("en-US", newWs);

				Assert.That(repo.Get("en-US").WindowsLcid, Is.EqualTo("test"),
					"Replace should have taken effect");
				Assert.That(File.Exists(wsFilePath + ".localrepoupdate"), Is.False,
					"No stash file should be created when there is nothing to stash");
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
