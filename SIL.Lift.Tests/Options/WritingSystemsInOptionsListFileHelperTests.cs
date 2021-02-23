using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.Lift.Options;
using SIL.TestUtilities;
using SIL.WritingSystems;
using SIL.WritingSystems.Tests;

namespace SIL.Lift.Tests.Options
{
	[TestFixture]
	[OfflineSldr]
	public class WritingSystemsInOptionsListFileHelperTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _folder;
			private readonly IO.TempFile _optionListFile;

			public TestEnvironment(string rfctag)
				: this(rfctag, "x-dontcare")
			{
			}

			public TestEnvironment(string rfctag, string rfctag2)
			{
				_folder = new TemporaryFolder("WritingSystemsInoptionListFileHelper");
				var pathtoOptionsListFile1 = Path.Combine(_folder.Path, "test1.xml");
				_optionListFile = new IO.TempFile(String.Format(_optionListFileContent, rfctag, rfctag2));
				_optionListFile.MoveTo(pathtoOptionsListFile1);
			}

			#region LongFileContent
			private readonly string _optionListFileContent =
				@"<?xml version='1.0' encoding='utf-8'?>
<optionsList>
	<options>
		<option>
			<key>Verb</key>
			<name>
				<form lang='{0}'>verb</form>
				<form lang='{1}'>verbe</form>
			</name>
			<abbreviation>
				<form lang='{0}'>verb</form>
			</abbreviation>
			<description>
				<form lang='{1}'>verbe</form>
			</description>
		</option>
		<option>
			<key>Noun</key>
			<name>
				<form lang='{0}'>noun</form>
				<form lang='{1}'>nom</form>
			</name>
			<abbreviation>
				<form lang='{0}'>noun</form>
				<form lang='{1}'>nom</form>
			</abbreviation>
			<description>
				<form lang='{0}'>noun</form>
				<form lang='{1}'>nom</form>
			</description>
		</option>
	</options>
</optionsList>".Replace("'", "\"");

			private WritingSystemsInOptionsListFileHelper _helper;

			#endregion

			private void CreateWritingSystemRepository()
			{
				WritingSystemRepository = new TestLdmlInFolderWritingSystemRepository(WritingSystemsPath);
			}

			private string ProjectPath
			{
				get { return _folder.Path; }
			}

			public WritingSystemsInOptionsListFileHelper Helper {
				get
				{
					if (_helper == null)
					{
						if (WritingSystemRepository == null)
						{
							CreateWritingSystemRepository();
						}
						_helper = new WritingSystemsInOptionsListFileHelper(WritingSystemRepository, _optionListFile.Path);
					}
					return _helper;
				}
			}

			public void Dispose()
			{
				_optionListFile.Dispose();
				_folder.Dispose();
			}

			private IWritingSystemRepository WritingSystemRepository { get; set; }

			public string WritingSystemsPath
			{
				get { return Path.Combine(ProjectPath, "WritingSystems"); }
			}

			public string PathToOptionsListFile
			{
				get { return _optionListFile.Path; }
			}

			public string GetLdmlFileforWs(string id)
			{
				return Path.Combine(WritingSystemsPath, String.Format("{0}.ldml", id));
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsNonConformantRfcTag_CreatesConformingWritingSystem()
		{
			using (var e = new TestEnvironment("en", "fr"))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				Assert.That(File.Exists(e.GetLdmlFileforWs("en")));
				Assert.That(File.Exists(e.GetLdmlFileforWs("fr")));
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsNonConformantRfcTag_WsIdChangeLogUpdated()
		{
			using (var e = new TestEnvironment("x-bogusws1", "audio"))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-x-bogusws1")));
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-Zxxx-x-audio")));
				string idChangeLogFilePath = Path.Combine(e.WritingSystemsPath, "idchangelog.xml");
				AssertThatXmlIn.File(idChangeLogFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes[Add/Id/text()='qaa-x-bogusws1' and Add/Id/text()='qaa-Zxxx-x-audio']");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsNonConformantRfcTag_UpdatesRfcTagInOptionsListFile()
		{
			using (var environment = new TestEnvironment("Zxxx-x-bogusws1", "audio"))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-x-Zxxx-bogusws1']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-Zxxx-x-audio']");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsNonConformantRfcTagWithDuplicates_UpdatesRfcTagInOptionsListFile()
		{
			using (var environment = new TestEnvironment("wee", "x-wee"))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-x-wee-dupl0']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-x-wee']");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsConformantRfcTagWithNoCorrespondingLdml_CreatesLdml()
		{
			using (var e = new TestEnvironment("de", "x-dontcare"))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(e.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='de']");
				Assert.That(File.Exists(e.GetLdmlFileforWs("de")), Is.True);
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsEntirelyPrivateUseRfcTagThatDoesNotExistInRepo_RfcTagIsMigrated()
		{
			using (var e = new TestEnvironment("x-blah"))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-x-blah")), Is.True);
				AssertThatXmlIn.File(e.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-x-blah']");
			}
		}

		[Test]
		//This test makes sure that nonexisting private use tags are migrated if necessary
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsAudioTagThatDoesNotExistInRepo_RfcTagIsMigrated()
		{
			using (var e = new TestEnvironment("x-audio"))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				Assert.That(File.Exists(e.GetLdmlFileforWs("x-audio")), Is.False);
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-Zxxx-x-audio")), Is.True);
				AssertThatXmlIn.File(e.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/name/form[@lang='x-audio']");
				AssertThatXmlIn.File(e.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-Zxxx-x-audio']");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInOptionsList_OptionsListFileContainsNonConformantRfcTagWithDuplicatesContainingduplicateMarker_UpdatesRfcTagInOptionsListFile()
		{
			using (var environment = new TestEnvironment("wee-dupl1", "x-wee-dupl1"))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-x-wee-dupl1']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasAtLeastOneMatchForXpath("/optionsList/options/option/name/form[@lang='qaa-x-wee-dupl1-dupl0']");
			}
		}

		[Test]
		public void Ctr_FileIsNotXml_MethodsBehave()
		{
			using (var environment = new TestEnvironment("wee-dupl1", "x-wee-dupl1"))
			{
				File.WriteAllText(environment.PathToOptionsListFile, "text");
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				environment.Helper.ReplaceWritingSystemId("text", "test");
				Assert.That(environment.Helper.WritingSystemsInUse.Count(), Is.EqualTo(0));
				Assert.That(File.ReadAllText(environment.PathToOptionsListFile), Is.EqualTo("text"));
			}
		}

		[Test]
		public void Ctr_FileIsXmlButNotOptionList_MethodsBehave()
		{
			using (var environment = new TestEnvironment("wee-dupl1", "x-wee-dupl1"))
			{
				File.WriteAllText(environment.PathToOptionsListFile, "<?xml version='1.0' encoding='utf-8'?>\r\n<form>yo</form>".Replace("'", "\""));
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				environment.Helper.ReplaceWritingSystemId("text", "test");
				Assert.That(environment.Helper.WritingSystemsInUse.Count(), Is.EqualTo(0));
				Assert.That(File.ReadAllText(environment.PathToOptionsListFile), Is.EqualTo("<?xml version='1.0' encoding='utf-8'?>\r\n<form>yo</form>".Replace("'", "\"")));
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FormForNewWritingSystemAlreadyExists_RemoveOldWritingSystemForm()
		{
			using (var environment = new TestEnvironment("th", "de"))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				environment.Helper.ReplaceWritingSystemId("th", "de");
				Assert.That(environment.Helper.WritingSystemsInUse.Count(), Is.EqualTo(1));
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("//form[@lang='th']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/name/form[@lang='de'][text()='verb']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/name/form[@lang='de'][text()='verbe']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/abbreviation/form[@lang='de'][text()='verb']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/description/form[@lang='de'][text()='verbe']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/name/form[@lang='de'][text()='nom']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/name/form[@lang='de'][text()='noun']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/abbreviation/form[@lang='de'][text()='nom']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/abbreviation/form[@lang='de'][text()='noun']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/description/form[@lang='de'][text()='nom']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/description/form[@lang='de'][text()='noun']");
			}
		}

		[Test]
		public void DeleteWritingSystemId_FormForNewWritingSystemAlreadyExists_RemoveOldWritingSystemForm()
		{
			using (var environment = new TestEnvironment("th", "de"))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				environment.Helper.DeleteWritingSystemId("th");
				Assert.That(environment.Helper.WritingSystemsInUse.Count(), Is.EqualTo(1));
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("//form[@lang='th']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/name/form[@lang='th'][text()='verb']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/name/form[@lang='de'][text()='verbe']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/abbreviation/form[@lang='th'][text()='verb']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/description/form[@lang='de'][text()='verbe']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/name/form[@lang='de'][text()='nom']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/name/form[@lang='th'][text()='noun']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/abbreviation/form[@lang='de'][text()='nom']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/abbreviation/form[@lang='th'][text()='noun']");
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasSpecifiedNumberOfMatchesForXpath("/optionsList/options/option/description/form[@lang='de'][text()='nom']", 1);
				AssertThatXmlIn.File(environment.PathToOptionsListFile).HasNoMatchForXpath("/optionsList/options/option/description/form[@lang='th'][text()='noun']");
			}
		}
	}
}