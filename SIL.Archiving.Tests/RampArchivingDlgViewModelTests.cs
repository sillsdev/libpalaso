using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SIL.Core.ClearShare;
using SIL.IO;
using SIL.Reporting;
using SIL.TestUtilities;
using static SIL.Archiving.ArchivingDlgViewModel.MessageType;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	public class RampArchivingDlgViewModelTests
	{
		private RampArchivingDlgViewModel m_model;
		private TestProgress m_progress;
		private readonly Dictionary<string, Tuple<IEnumerable<string>, string>> m_filesToAdd =
			new Dictionary<string, Tuple<IEnumerable<string>, string>>();
		private bool? m_isRampInstalled;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public async Task Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;

			m_model = new RampArchivingDlgViewModel("Test App", "Test Title", "tst",
				SetFilesToArchive, GetFileDescription);
			m_progress = new TestProgress("RAMP");
			m_model.AppSpecificFilenameNormalization = CustomFilenameNormalization;
			var cancel = new CancellationToken();
			await m_model.Initialize(m_progress, cancel);
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			m_model.CleanUp();
			m_filesToAdd.Clear();
			try { File.Delete(m_model.PackagePath); }
// ReSharper disable once EmptyGeneralCatchClause
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CreateMetsFile_CreatesFile()
		{
			var metsPath = m_model.CreateMetsFile();
			Assert.IsNotNull(metsPath);
			Assert.IsTrue(File.Exists(metsPath));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public async Task CreateRampPackageWithSessionArchiveAndMetsFile_CreatesRampPackage()
		{
			TemporaryFolder tmpFolder = new TemporaryFolder("ArchiveHelperTestFolder");
			try
			{
				string fileName = Path.Combine(tmpFolder.Path, "ddo.session");
				File.CreateText(fileName).Close();
				var fileList = new[] { Path.Combine(tmpFolder.Path, "ddo.session") };
				m_filesToAdd.Add(string.Empty, new Tuple<IEnumerable<string>, string>(fileList, "Message to display."));
				var cancel = new CancellationToken();
				await m_model.Initialize(new TestProgress(RampArchivingDlgViewModel.kRampProcessName), cancel);
				m_model.CreateMetsFile();
				Assert.IsTrue(m_model.CreateRampPackage(new CancellationToken()).Result);
				Assert.IsTrue(File.Exists(m_model.PackagePath));
			}
			finally
			{
				tmpFolder.Dispose();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_NullList_ReturnsNull()
		{
			Assert.IsNull(m_model.GetMode(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_EmptyList_ReturnsNull()
		{
			Assert.IsNull(m_model.GetMode(Array.Empty<string>()));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_SingleTypeInList_ReturnsCorrectMetsList()
		{
			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				RampArchivingDlgViewModel.kModeVideo + "\"]", m_model.GetMode(new[] { "blah.mpg" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_MultipleTypesInList_ReturnsCorrectMetsList()
		{
			var mode = m_model.GetMode(new[] { "blah.mp3", "blah.doc", "blah.mov" });
			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				RampArchivingDlgViewModel.kModeSpeech + "\",\"" +
				RampArchivingDlgViewModel.kModeText + "\",\"" +
				RampArchivingDlgViewModel.kModeVideo + "\"]", mode);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_ZipFileWithMultipleTypesInList_ReturnsCorrectMetsList()
		{
			var tempFile = TempFile.WithExtension("zip");
			try
			{
				RobustFile.Delete(tempFile.Path);
				using (var zipFile = ZipFile.Open(tempFile.Path, ZipArchiveMode.Create))
				{
					// For good measure, make sure we can handle filenames with Unicode surrogate pairs
					zipFile.CreateEntry("blah\uD800\uDC00\ud803\ude6d\udbff\udfff.mp3");
					zipFile.CreateEntry("blah.doc");
					zipFile.CreateEntry("blah.niff");
				}
				var mode = m_model.GetMode(new[] { tempFile.Path });
				Assert.That(mode, Is.EqualTo("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
					RampArchivingDlgViewModel.kModeSpeech + "\",\"" +
					RampArchivingDlgViewModel.kModeText + "\",\"" +
					RampArchivingDlgViewModel.kModeMusicalNotation + "\"]"));
			}
			finally
			{
				tempFile.Dispose();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_FwBackupFileWithMultipleTypesInList_ReturnsCorrectMetsList()
		{
			var tempFile = TempFile.WithExtension("fwbackup");
			try
			{
				RobustFile.Delete(tempFile.Path);
				using (var zipFile = ZipFile.Open(tempFile.Path, ZipArchiveMode.Create))
				{
					zipFile.CreateEntry("blah.fwdata");
					zipFile.CreateEntry("fonts/blah.ttf");
					zipFile.CreateEntry("images/blah.jpeg");
				}
				var mode = m_model.GetMode(new[] { tempFile.Path });
				Assert.That(mode, Is.EqualTo("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
					RampArchivingDlgViewModel.kModeText + "\",\"" +
					RampArchivingDlgViewModel.kModeDataset + "\",\"" +
					RampArchivingDlgViewModel.kModeSoftwareOrFont + "\",\"" +
					RampArchivingDlgViewModel.kModePhotograph + "\"]"));
			}
			finally
			{
				tempFile.Dispose();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_ListContainsMultiplesOfOneType_ReturnsOnlyOneTypeInList()
		{
			var mode = m_model.GetMode(new[] { "blah.mp3", "blah.wma", "blah.wav" });
			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				RampArchivingDlgViewModel.kModeSpeech + "\"]", mode);
		}

		#region GetSourceFilesForMetsData tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlySessionMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>
			 {
				 [string.Empty] = new Tuple<IEnumerable<string>, string>(
					 new[] { "blah.session" }, "Message to display.")
			 };

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.session\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session Metadata (XML)\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, m_model.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyPersonMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>
			 {
				 [string.Empty] = new Tuple<IEnumerable<string>, string>(
					 new[] { "blah.person" }, "Message to display.")
			 };

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.person\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor Metadata (XML)\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, m_model.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>
			 {
				 [string.Empty] = new Tuple<IEnumerable<string>, string>(
					 new[] { "blah.meta" }, "Message to display.")
			 };

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.meta\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp File Metadata (XML)\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";

			Assert.AreEqual(expected, m_model.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericSessionFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>
			 {
				 [string.Empty] = new Tuple<IEnumerable<string>, string>(
					 new[] { "blah.wav" }, "Message to display.")
			 };

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.wav\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session File\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";

			Assert.AreEqual(expected, m_model.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericPersonFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>
			 {
				 ["Carmen"] = new Tuple<IEnumerable<string>, string>(
					 new[] { "Carmen_blah.wav" }, "Message to display.")
			 };

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__Carmen_blah.wav\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor File\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, m_model.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListMultipleFiles_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>
			 {
				 [string.Empty] = new Tuple<IEnumerable<string>, string>(
					 new[] { "blah.session", "really cool.wav" }, "Message to display."),
				 ["person id"] = new Tuple<IEnumerable<string>, string>(
					 new[] { "person id_blah.person", "person id_baa.mpg", "person id_baa.mpg.meta" }, "Message to display.")
			 };

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.session\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session Metadata (XML)\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				m_model.GetSourceFilesForMetsData(fileLists).ElementAt(0));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"really-cool.wav\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session File\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				m_model.GetSourceFilesForMetsData(fileLists).ElementAt(1));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person-id_blah.person\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor Metadata (XML)\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				m_model.GetSourceFilesForMetsData(fileLists).ElementAt(2));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person-id_baa.mpg\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor File\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				m_model.GetSourceFilesForMetsData(fileLists).ElementAt(3));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person-id_baa.mpg.meta\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp File Metadata (XML)\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				m_model.GetSourceFilesForMetsData(fileLists).ElementAt(4));
		}
		#endregion

		#region SetAudience tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAudience_ChangeAudience_ThrowsInvalidOperationException()
		{
			m_model.SetAudience(AudienceType.Vernacular);
			Assert.Throws<InvalidOperationException>(
				() => m_model.SetAudience(AudienceType.Training)
			);
		}
		#endregion

		#region SetVernacularMaterialsAndContentType tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetVernacularMaterialsAndContentType_IncompatibleWithAudience_ThrowsInvalidOperationException()
		{
			m_model.SetAudience(AudienceType.Training);
			Assert.Throws<InvalidOperationException>(
				() => m_model.SetVernacularMaterialsAndContentType(VernacularMaterialsType.BibleBackground)
			);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetVernacularMaterialsAndContentType_CompatibleWithAudience_IncludedInMetsData()
		{
			m_model.SetAudience(AudienceType.Vernacular);
			m_model.SetVernacularMaterialsAndContentType(VernacularMaterialsType.LiteracyEducation_Riddles);
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kAudience + "\":\"" + RampArchivingDlgViewModel.kAudienceVernacular + "\",\"" +
				RampArchivingDlgViewModel.kVernacularMaterialsType + "\":\"" + RampArchivingDlgViewModel.kVernacularMaterialGeneral + "\",\"" +
				RampArchivingDlgViewModel.kVernacularContent + "\":\"Riddles\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetVernacularMaterialsAndContentType_MixOfScriptureAndOther_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(
				() => m_model.SetVernacularMaterialsAndContentType(VernacularMaterialsType.BibleStory | VernacularMaterialsType.CommunityAndCulture_Calendar)
			);
		}
		#endregion

		#region SetAbstract tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAbstract_SetSingleAbstractWithoutLanguage_IncludedInMetsData()
		{
			m_model.SetAbstract("SayMore doesn't let the user specify the language explicitly.", string.Empty);
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\"," +
				"\"description.abstract.has\":\"Y\",\"dc.description.abstract\":{" +
				"\"0\":{\" \":\"SayMore doesn't let the user specify the language explicitly.\"}}}",
				data);

			//"{\"dc.title\":\"what\",\"broad_type\":\"wider_audience\",\"dc.type.mode\":[\"Text\"],\"dc.description.stage\":\"rough_draft\"," +
			//    "\"version.type\":\"first\",\"dc.type.scholarlyWork\":\"Data set\",\"dc.subject.subjectLanguage\":{\"0\":{\"dialect\":\"\"}}," +
			//    "\"dc.language.iso\":{\"0\":{\"dialect\":\"\"}},\"dc.subject.silDomain\":[\"LING:Linguistics\"],\"sil.sensitivity.metadata\":\"Public\"," +
			//    "\"files\":{\"0\":{\" \":\"gmreadme.txt\",\"description\":\"junk\"}},\"status\":\"ready\"," +
			//    "\"description.abstract.has\":\"Y\",\"dc.description.abstract\":{\"0\":{\" \":\"\"SayMore doesn't let the use specify the language explicitly.\"}}}"
		}

		[Test]
		public void SetAbstract_SetTwice_ThrowsInvalidOperationException()
		{
			m_model.SetAbstract("This is pretty abstract", "eng");
			Dictionary<string, string> foreignLanguageAbstracts = new Dictionary<string, string>();
			foreignLanguageAbstracts["fra"] = "C'est assez abstrait";
			foreignLanguageAbstracts["spa"] = "Esto es bastante abstracto";
			Assert.Throws<InvalidOperationException>(
				() => m_model.SetAbstract(foreignLanguageAbstracts)
				);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAbstract_Null_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => m_model.SetAbstract(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAbstract_ThreeLanguages_IncludedInMetsData()
		{
			Dictionary<string, string> abstracts = new Dictionary<string, string>
				{
					["eng"] = "This is pretty abstract",
					["fra"] = "C'est assez abstrait",
					["spa"] = "Esto es bastante abstracto"
				};
			m_model.SetAbstract(abstracts);
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\"," +
				"\"description.abstract.has\":\"Y\",\"dc.description.abstract\":{" +
				"\"0\":{\" \":\"This is pretty abstract\",\"lang\":\"eng\"}," +
				"\"1\":{\" \":\"C'est assez abstrait\",\"lang\":\"fra\"}," +
				"\"2\":{\" \":\"Esto es bastante abstracto\",\"lang\":\"spa\"}}}",
				data);
		}
		#endregion

		#region SetAudioVideoExtent tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAudioVideoExtent_FreeFormString_IncludedInMetsData()
		{
			m_model.SetAudioVideoExtent("6 and a half seconds");
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kRecordingExtent + "\":\"6 and a half seconds\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAudioVideoExtent_ValidTimeSpan_IncludedInMetsData()
		{
			TimeSpan duration = new TimeSpan(0, 2, 3, 4);
			m_model.SetAudioVideoExtent(duration);
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kRecordingExtent + "\":\"02:03:04\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAudioVideoExtent_SetTwice_ThrowsInvalidOperationException()
		{
			m_model.SetAudioVideoExtent("twelve years or more");
			TimeSpan duration = new TimeSpan(0, 2, 3, 4);
			Assert.Throws<InvalidOperationException>(() => m_model.SetAudioVideoExtent(duration));
		}
		#endregion

		#region SetContentLanguages tests
		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("RampRequired")]
		public void SetContentLanguages_TwoLanguages_IncludedInMetsData()
		{
			IgnoreTestIfRampIsNotInstalled();

			Assert.Ignore("This test is no longer valid because RAMP 3.0 does not have a languages file");

			m_model.SetContentLanguages("eng", "fra");
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kContentLanguages + "\":{\"0\":{\" \":\"eng:English\"},\"1\":{\" \":\"fra:French\"}}}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("RampRequired")]
		public void SetContentLanguages_SetTwice_ThrowsInvalidOperationException()
		{
			IgnoreTestIfRampIsNotInstalled();

			m_model.SetContentLanguages("eng", "fra");
			Assert.Throws<InvalidOperationException>(() => m_model.SetContentLanguages("spa", "fra"));
		}
		#endregion

		#region SetContributors tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributors_Null_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => m_model.SetContributors(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributors_Empty_NoChangeToMetsData()
		{
			var dataBefore = m_model.GetMetadata();
			var empty = new ContributionCollection();
			m_model.SetContributors(empty);
			var dataAfter = m_model.GetMetadata();
			Assert.AreEqual(dataBefore, dataAfter);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributors_TwoContributors_IncludedInMetsData()
		{
			var contributors = new ContributionCollection();
			OlacSystem olacSystem = new OlacSystem();
			contributors.Add(new Contribution("Erkel", olacSystem.GetRoleByCodeOrThrow("author")));
			contributors.Add(new Contribution("Sungfu", olacSystem.GetRoleByCodeOrThrow("recorder")));
			m_model.SetContributors(contributors);
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kContributor + "\":{\"0\":{\" \":\"Erkel\",\"role\":\"author\"},\"1\":{\" \":\"Sungfu\",\"role\":\"recorder\"}}}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributors_SetTwice_ThrowsInvalidOperationException()
		{
			var contributors = new ContributionCollection();
			OlacSystem olacSystem = new OlacSystem();
			Role role = olacSystem.GetRoleByCodeOrThrow("author");
			var contrib = new Contribution("Erkel", role);
			contributors.Add(contrib);
			m_model.SetContributors(contributors);
			Assert.Throws<InvalidOperationException>(() => m_model.SetContributors(contributors));
		}
		#endregion

		#region SetCreationDate tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetCreationDate_FreeFormString_IncludedInMetsData()
		{
			m_model.SetCreationDate("four years ago");
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kDateCreated + "\":\"four years ago\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetCreationDate_ValidTimeSpan_IncludedInMetsData()
		{
			DateTime creationDate = new DateTime(2012, 4, 13);
			m_model.SetCreationDate(creationDate);
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kDateCreated + "\":\"2012-04-13\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetCreationDate_SetTwice_ThrowsInvalidOperationException()
		{
			m_model.SetCreationDate("tomorrow");
			Assert.Throws<InvalidOperationException>(() => m_model.SetCreationDate(new DateTime(2012, 4, 13)));
		}
		#endregion

		#region SetDatasetExtent tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDatasetExtent_FreeFormString_IncludedInMetsData()
		{
			m_model.SetDatasetExtent("6 voice records and maybe an odd text file or two");
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kDatasetExtent + "\":\"6 voice records and maybe an odd text file or two\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDatasetExtent_SetTwice_ThrowsInvalidOperationException()
		{
			m_model.SetDatasetExtent("practically nothing");
			Assert.Throws<InvalidOperationException>(() => m_model.SetDatasetExtent("lots of data"));
		}
		#endregion

		#region SetDescription tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDescription_Null_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => m_model.SetDescription(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDescription_Empty_NoChangeToMetsData()
		{
			var dataBefore = m_model.GetMetadata();
			m_model.SetDescription(new Dictionary<string, string>());
			var dataAfter = m_model.GetMetadata();
			Assert.AreEqual(dataBefore, dataAfter);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDescription_TwoLanguages_IncludedInMetsData()
		{
			var descriptions = new Dictionary<string, string>();
			descriptions["eng"] = "General data";
			descriptions["spa"] = "Datos generales";
			m_model.SetDescription(descriptions);
			var data = m_model.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" + RampArchivingDlgViewModel.kFlagHasGeneralDescription + "\":\"Y\",\"" +
				RampArchivingDlgViewModel.kGeneralDescription + "\":{\"0\":{\" \":\"General data\",\"lang\":\"eng\"},\"1\":{\" \":\"Datos generales\",\"lang\":\"spa\"}}}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDescription_SetTwice_ThrowsInvalidOperationException()
		{
			var descriptions = new Dictionary<string, string>();
			descriptions["eng"] = "General data";
			m_model.SetDescription(descriptions);
			Assert.Throws<InvalidOperationException>(() => m_model.SetDescription(descriptions));
		}
		#endregion

		[Test]
		public void GetEnglishName_GetFromCulture_ReturnsEnglishName()
		{
			var eng = new ArchivingLanguage("eng");
			var fra = new ArchivingLanguage("fra");
			var spa = new ArchivingLanguage("spa");
			Assert.AreEqual("English", eng.EnglishName);
			Assert.AreEqual("French", fra.EnglishName);
			Assert.AreEqual("Spanish", spa.EnglishName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("RampRequired")]
		public void GetLanguageName_English_ReturnsEnglish()
		{
			IgnoreTestIfRampIsNotInstalled();

			Assert.Ignore("This test is no longer valid because RAMP 3.0 does not have a languages file");

			var langName = m_model.GetLanguageName("eng");
			Assert.AreEqual(langName, "English");
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("RampRequired")]
		public void GetLanguageName_Gibberish_ReturnsNull()
		{
			IgnoreTestIfRampIsNotInstalled();

			var langName = m_model.GetLanguageName("z23");
			Assert.IsNull(langName);
		}

		[Test]
		[Category("SkipOnTeamCity")]
		[Category("RampRequired")]
		public void GetLanguageName_ArchivingLanguage_ReturnsCorrectName()
		{
			IgnoreTestIfRampIsNotInstalled();

			Assert.Ignore("This test is no longer valid because RAMP 3.0 does not have a languages file");

			// FieldWorks associates the name "Chinese" with the ISO3 Code "cmn"
			ArchivingLanguage lang = new ArchivingLanguage("cmn", "Chinese");

			// RAMP requires the name "Chinese, Mandarin"
			Assert.AreEqual("Chinese, Mandarin", m_model.GetLanguageName(lang.Iso3Code));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("RampRequired")]
		public void GetRAMPFileLocation_RAMPInstalled_ReturnsFileLocation()
		{
			IgnoreTestIfRampIsNotInstalled();

			var fileName = RampArchivingDlgViewModel.GetExeFileLocation();
			Assert.IsTrue(File.Exists(fileName), "RAMP executable file not found.");
		}

		#region Private helper methods
		/// ------------------------------------------------------------------------------------
		private void SetFilesToArchive(ArchivingDlgViewModel model, CancellationToken cancellationToken)
		{
			foreach (var kvp in m_filesToAdd)
				model.AddFileGroup(kvp.Key, kvp.Value.Item1, kvp.Value.Item2);
		}

		/// ------------------------------------------------------------------------------------
		private string GetFileDescription(string key, string file)
		{
			var description = (key == string.Empty ? "MyApp Session File" : "MyApp Contributor File");

			if (file.ToLower().EndsWith(".session"))
				description = "MyApp Session Metadata (XML)";
			else if (file.ToLower().EndsWith(".person"))
				description = "MyApp Contributor Metadata (XML)";
			else if (file.ToLower().EndsWith(".meta"))
				description = "MyApp File Metadata (XML)";

			return description;
		}

		/// ------------------------------------------------------------------------------------
		private void CustomFilenameNormalization(string key, string file, StringBuilder bldr)
		{
			if (key != string.Empty)
				bldr.Insert(0, "__AppSpecific__");
		}

		private void IgnoreTestIfRampIsNotInstalled()
		{
			// we remember the value so that we check only once. This won't change within
			// a test run.
			m_isRampInstalled ??= !string.IsNullOrEmpty(RampArchivingDlgViewModel.GetExeFileLocation());

			if (!m_isRampInstalled.Value)
				Assert.Ignore("This test requires RAMP");
		}
		#endregion
	}

	internal class TestRampArchivingDlgViewModel : RampArchivingDlgViewModel
	{
		public TestRampArchivingDlgViewModel(
			Action<ArchivingDlgViewModel, CancellationToken> setFilesToArchive) :
			base("Test App", "Test Title", "tst", setFilesToArchive,
				(k, f) => throw new NotImplementedException())
		{
		}

		protected override bool DoArchiveSpecificInitialization()
		{
			DisplayMessage("Base implementation overridden", MessageType.Volatile);
			return true;
		}
	}

	[TestFixture]
	[Category("Archiving")]
	public class RampArchivingDlgViewModelWithOverrideDisplayInitialSummarySetTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public async Task DisplayInitialSummary_OverrideDisplayInitialSummaryIsSet_DefaultBehaviorOmitted()
		{
			ErrorReport.IsOkToInteractWithUser = false;

			bool filesToArchiveCalled = false;

			var model = new TestRampArchivingDlgViewModel((a, b) => { filesToArchiveCalled = true; });

			var progress = new TestProgress("RAMP");
			var customSummaryShown = 0;

			model.OverrideDisplayInitialSummary = (d, c) =>
			{
				customSummaryShown++;
				progress.IncrementProgress();
			};
			model.GetOverriddenPreArchivingMessages = d => throw new AssertionException(
				$"{nameof(ArchivingDlgViewModel.GetOverriddenPreArchivingMessages)} should not have been invoked");
			model.OverrideGetFileGroupDisplayMessage = s => throw new AssertionException(
				$"{nameof(ArchivingDlgViewModel.OverrideGetFileGroupDisplayMessage)} should not have been invoked");

			try
			{
				await model.Initialize(progress, new CancellationToken()).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Initialization threw an exception: {ex}");
			}

			Assert.True(filesToArchiveCalled);
			Assert.That(customSummaryShown, Is.EqualTo(1));
			Assert.False(File.Exists(model.PackagePath));
			Assert.That(progress.Step, Is.EqualTo(1));
		}
	}

	[TestFixture]
	[Category("Archiving")]
	public class RampArchivingDlgViewModelWithFineGrainedOverridesForDisplayInitialSummarySetTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public async Task DisplayInitialSummary_OverridenPropertiesForDisplayInitialSummaryAreSet_MessagesReflectOverrides()
		{
			ErrorReport.IsOkToInteractWithUser = false;

			void SetFilesToArchive(ArchivingDlgViewModel model, CancellationToken cancellationToken)
			{
				model.AddFileGroup(String.Empty, new[] { "green.frog" }, "These messages should not be displayed");
				model.AddFileGroup("Toads", new[] { "red.toad", "blue.toad" }, "because in this test we do not create a package.");

			}

			var model = new TestRampArchivingDlgViewModel(SetFilesToArchive);

			var messagesDisplayed = new List<Tuple<string, ArchivingDlgViewModel.MessageType>>();

			void ReportMessage(string msg, ArchivingDlgViewModel.MessageType type)
			{
				messagesDisplayed.Add(new Tuple<string, ArchivingDlgViewModel.MessageType>(msg, type));
			}

			model.OnReportMessage += ReportMessage;

			IEnumerable<Tuple<string, ArchivingDlgViewModel.MessageType>> GetMessages(IDictionary<string, Tuple<IEnumerable<string>, string>> arg)
			{
				yield return new Tuple<string, ArchivingDlgViewModel.MessageType>(
					"First pre-archiving message", Warning);
				yield return new Tuple<string, ArchivingDlgViewModel.MessageType>(
					"Second pre-archiving message", Indented);
			}

			model.GetOverriddenPreArchivingMessages = GetMessages;
			model.InitialFileGroupDisplayMessageType = Success;
			model.OverrideGetFileGroupDisplayMessage = s => (s == String.Empty) ? "Frogs" : $"Label: {s}";

			var progress = new TestProgress("RAMP");
			try
			{
				await model.Initialize(progress, new CancellationToken()).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Initialization threw an exception: {ex}");
			}

			Assert.That(messagesDisplayed, Is.EqualTo(new[]
			{
				("Base implementation overridden", ArchivingDlgViewModel.MessageType.Volatile).ToTuple(),
				("First pre-archiving message", Warning).ToTuple(),
				("Second pre-archiving message", Indented).ToTuple(),
				("Frogs", Success).ToTuple(),
				("green.frog", Bullet).ToTuple(),
				("Label: Toads", Success).ToTuple(),
				("red.toad", Bullet).ToTuple(),
				("blue.toad", Bullet).ToTuple()
			}));

			Assert.False(File.Exists(model.PackagePath));
			Assert.That(progress.Step, Is.EqualTo(1));
		}
	}
}
