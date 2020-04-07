using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using NUnit.Framework;
using SIL.IO;
using SIL.Reporting;
using SIL.TestUtilities;
using SIL.Windows.Forms.ClearShare;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	public class RampArchivingDlgViewModelTests
	{
		private RampArchivingDlgViewModel _helper;
		private Dictionary<string, Tuple<IEnumerable<string>, string>> _filesToAdd;
		private bool? _isRampInstalled;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;

			_helper = new RampArchivingDlgViewModel("Test App", "Test Title", "tst", null,
				SetFilesToArchive, GetFileDescription);
			_helper.AppSpecificFilenameNormalization = CustomFilenameNormalization;
			_filesToAdd = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_helper.CleanUp();

			try { File.Delete(_helper.PackagePath); }
// ReSharper disable once EmptyGeneralCatchClause
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CreateMetsFile_CreatesFile()
		{
			var metsPath = _helper.CreateMetsFile();
			Assert.IsNotNull(metsPath);
			Assert.IsTrue(File.Exists(metsPath));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void CreateRampPackageWithSessionArchiveAndMetsFile_CreatesRampPackage()
		{
			TemporaryFolder tmpFolder = new TemporaryFolder("ArchiveHelperTestFolder");
			try
			{
				string fileName = Path.Combine(tmpFolder.Path, "ddo.session");
				File.CreateText(fileName).Close();
				var fileList = new[] { Path.Combine(tmpFolder.Path, "ddo.session") };
				_filesToAdd.Add(string.Empty, new Tuple<IEnumerable<string>, string>(fileList, "Message to display."));
				_helper.Initialize();
				_helper.CreateMetsFile();
				Assert.IsTrue(_helper.CreateRampPackage());
				Assert.IsTrue(File.Exists(_helper.PackagePath));
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
			Assert.IsNull(_helper.GetMode(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_EmptyList_ReturnsNull()
		{
			Assert.IsNull(_helper.GetMode(new string[0]));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_SingleTypeInList_ReturnsCorrectMetsList()
		{
			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				RampArchivingDlgViewModel.kModeVideo + "\"]", _helper.GetMode(new[] { "blah.mpg" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_MultipleTypesInList_ReturnsCorrectMetsList()
		{
			var mode = _helper.GetMode(new[] { "blah.mp3", "blah.doc", "blah.mov" });
			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				RampArchivingDlgViewModel.kModeSpeech + "\",\"" +
				RampArchivingDlgViewModel.kModeText + "\",\"" +
				RampArchivingDlgViewModel.kModeVideo + "\"]", mode);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_ZipFileWithMultipleTypesInList_ReturnsCorrectMetsList()
		{
			ZipFile zipFile = new ZipFile();
			zipFile.AddEntry("blah.mp3", "whatever");
			zipFile.AddEntry("blah.doc", "whatever");
			zipFile.AddEntry("blah.niff", "whatever");
			var tempFile = TempFile.WithExtension("zip");
			try
			{
				zipFile.Save(tempFile.Path);
				var mode = _helper.GetMode(new[] { zipFile.Name });
				Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
					RampArchivingDlgViewModel.kModeSpeech + "\",\"" +
					RampArchivingDlgViewModel.kModeText + "\",\"" +
					RampArchivingDlgViewModel.kModeMusicalNotation + "\"]", mode);
			}
			finally
			{
				zipFile.Dispose();
				tempFile.Dispose();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_FwbackupFileWithMultipleTypesInList_ReturnsCorrectMetsList()
		{
			ZipFile zipFile = new ZipFile();
			zipFile.AddEntry("blah.fwdata", "whatever");
			zipFile.AddEntry("fonts/blah.ttf", "whatever");
			zipFile.AddEntry("images/blah.jpeg", "whatever");
			var tempFile = TempFile.WithExtension("fwbackup");
			try
			{
				zipFile.Save(tempFile.Path);
				var mode = _helper.GetMode(new[] { zipFile.Name });
				Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
					RampArchivingDlgViewModel.kModeText + "\",\"" +
					RampArchivingDlgViewModel.kModeDataset + "\",\"" +
					RampArchivingDlgViewModel.kModeSoftwareOrFont + "\",\"" +
					RampArchivingDlgViewModel.kModePhotograph + "\"]", mode);
			}
			finally
			{
				zipFile.Dispose();
				tempFile.Dispose();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_ListContainsMultiplesOfOneType_ReturnsOnlyOneTypeInList()
		{
			var mode = _helper.GetMode(new[] { "blah.mp3", "blah.wma", "blah.wav" });
			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				RampArchivingDlgViewModel.kModeSpeech + "\"]", mode);
		}

		#region GetSourceFilesForMetsData tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlySessionMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.session" }, "Message to display.");

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.session\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session Metadata (XML)\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyPersonMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.person" }, "Message to display.");

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.person\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor Metadata (XML)\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.meta" }, "Message to display.");

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.meta\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp File Metadata (XML)\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";

			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericSessionFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.wav" }, "Message to display.");

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.wav\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session File\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";

			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericPersonFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists["Carmen"] = new Tuple<IEnumerable<string>, string>(new[] { "Carmen_blah.wav" }, "Message to display.");

			var expected = "\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__Carmen_blah.wav\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor File\"" + RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListMultipleFiles_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.session", "really cool.wav" }, "Message to display.");
			fileLists["person id"] = new Tuple<IEnumerable<string>, string>(new[] { "person id_blah.person", "person id_baa.mpg", "person id_baa.mpg.meta" }, "Message to display.");

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"blah.session\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session Metadata (XML)\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"really-cool.wav\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session File\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(1));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person-id_blah.person\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor Metadata (XML)\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(2));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person-id_baa.mpg\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor File\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(3));

			Assert.AreEqual("\"" + RampArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person-id_baa.mpg.meta\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileDescription + "\":\"MyApp File Metadata (XML)\"" +
				RampArchivingDlgViewModel.kSeparator + "\"" +
				RampArchivingDlgViewModel.kFileRelationship + "\":\"" +
				RampArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(4));
		}
		#endregion

		#region SetAudience tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAudience_ChangeAudience_ThrowsInvalidOperationException()
		{
			_helper.SetAudience(AudienceType.Vernacular);
			Assert.Throws<InvalidOperationException>(
				() => _helper.SetAudience(AudienceType.Training)
			);
		}
		#endregion

		#region SetVernacularMaterialsAndContentType tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetVernacularMaterialsAndContentType_IncompatibleWithAudience_ThrowsInvalidOperationException()
		{
			_helper.SetAudience(AudienceType.Training);
			Assert.Throws<InvalidOperationException>(
				() => _helper.SetVernacularMaterialsAndContentType(VernacularMaterialsType.BibleBackground)
			);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetVernacularMaterialsAndContentType_CompatibleWithAudience_IncludedInMetsData()
		{
			_helper.SetAudience(AudienceType.Vernacular);
			_helper.SetVernacularMaterialsAndContentType(VernacularMaterialsType.LiteracyEducation_Riddles);
			var data = _helper.GetMetadata();
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
				() => _helper.SetVernacularMaterialsAndContentType(VernacularMaterialsType.BibleStory | VernacularMaterialsType.CommunityAndCulture_Calendar)
			);
		}
		#endregion

		#region SetAbstract tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAbstract_SetSingleAbstractWithoutLanguage_IncludedInMetsData()
		{
			_helper.SetAbstract("SayMore doesn't let the user specify the language explicitly.", string.Empty);
			var data = _helper.GetMetadata();
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
			_helper.SetAbstract("This is pretty abstract", "eng");
			Dictionary<string, string> foreignLanguageAbstracts = new Dictionary<string, string>();
			foreignLanguageAbstracts["fra"] = "C'est assez abstrait";
			foreignLanguageAbstracts["spa"] = "Esto es bastante abstracto";
			Assert.Throws<InvalidOperationException>(
				() => _helper.SetAbstract(foreignLanguageAbstracts)
				);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAbstract_Null_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _helper.SetAbstract(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAbstract_ThreeLanguages_IncludedInMetsData()
		{
			Dictionary<string, string> abstracts = new Dictionary<string, string>();
			abstracts["eng"] = "This is pretty abstract";
			abstracts["fra"] = "C'est assez abstrait";
			abstracts["spa"] = "Esto es bastante abstracto";
			_helper.SetAbstract(abstracts);
			var data = _helper.GetMetadata();
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
			_helper.SetAudioVideoExtent("6 and a half seconds");
			var data = _helper.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kRecordingExtent + "\":\"6 and a half seconds\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAudioVideoExtent_ValidTimeSpan_IncludedInMetsData()
		{
			TimeSpan duration = new TimeSpan(0, 2, 3, 4);
			_helper.SetAudioVideoExtent(duration);
			var data = _helper.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kRecordingExtent + "\":\"02:03:04\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetAudioVideoExtent_SetTwice_ThrowsInvalidOperationException()
		{
			_helper.SetAudioVideoExtent("twelve years or more");
			TimeSpan duration = new TimeSpan(0, 2, 3, 4);
			Assert.Throws<InvalidOperationException>(() => _helper.SetAudioVideoExtent(duration));
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

			_helper.SetContentLanguages("eng", "fra");
			var data = _helper.GetMetadata();
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

			_helper.SetContentLanguages("eng", "fra");
			Assert.Throws<InvalidOperationException>(() => _helper.SetContentLanguages("spa", "fra"));
		}
		#endregion

		#region SetContributors tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributors_Null_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _helper.SetContributors(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributors_Empty_NoChangeToMetsData()
		{
			var dataBefore = _helper.GetMetadata();
			var empty = new ContributionCollection();
			_helper.SetContributors(empty);
			var dataAfter = _helper.GetMetadata();
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
			_helper.SetContributors(contributors);
			var data = _helper.GetMetadata();
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
			_helper.SetContributors(contributors);
			Assert.Throws<InvalidOperationException>(() => _helper.SetContributors(contributors));
		}
		#endregion

		#region SetCreationDate tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetCreationDate_FreeFormString_IncludedInMetsData()
		{
			_helper.SetCreationDate("four years ago");
			var data = _helper.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kDateCreated + "\":\"four years ago\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetCreationDate_ValidTimeSpan_IncludedInMetsData()
		{
			DateTime creationDate = new DateTime(2012, 4, 13);
			_helper.SetCreationDate(creationDate);
			var data = _helper.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kDateCreated + "\":\"2012-04-13\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetCreationDate_SetTwice_ThrowsInvalidOperationException()
		{
			_helper.SetCreationDate("tomorrow");
			Assert.Throws<InvalidOperationException>(() => _helper.SetCreationDate(new DateTime(2012, 4, 13)));
		}
		#endregion

		#region SetDatasetExtent tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDatasetExtent_FreeFormString_IncludedInMetsData()
		{
			_helper.SetDatasetExtent("6 voice records and maybe an odd text file or two");
			var data = _helper.GetMetadata();
			Assert.AreEqual("{\"dc.title\":\"Test Title\",\"" +
				RampArchivingDlgViewModel.kDatasetExtent + "\":\"6 voice records and maybe an odd text file or two\"}",
				data);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDatasetExtent_SetTwice_ThrowsInvalidOperationException()
		{
			_helper.SetDatasetExtent("practically nothing");
			Assert.Throws<InvalidOperationException>(() => _helper.SetDatasetExtent("lots of data"));
		}
		#endregion

		#region SetDescription tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDescription_Null_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _helper.SetDescription(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDescription_Empty_NoChangeToMetsData()
		{
			var dataBefore = _helper.GetMetadata();
			_helper.SetDescription(new Dictionary<string, string>());
			var dataAfter = _helper.GetMetadata();
			Assert.AreEqual(dataBefore, dataAfter);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetDescription_TwoLanguages_IncludedInMetsData()
		{
			var descriptions = new Dictionary<string, string>();
			descriptions["eng"] = "General data";
			descriptions["spa"] = "Datos generales";
			_helper.SetDescription(descriptions);
			var data = _helper.GetMetadata();
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
			_helper.SetDescription(descriptions);
			Assert.Throws<InvalidOperationException>(() => _helper.SetDescription(descriptions));
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

			var langName = _helper.GetLanguageName("eng");
			Assert.AreEqual(langName, "English");
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("RampRequired")]
		public void GetLanguageName_Gibberish_ReturnsNull()
		{
			IgnoreTestIfRampIsNotInstalled();

			var langName = _helper.GetLanguageName("z23");
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

			// RAMP reqires the name "Chinese, Mandarin"
			Assert.AreEqual("Chinese, Mandarin", _helper.GetLanguageName(lang.Iso3Code));
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
		private void SetFilesToArchive(ArchivingDlgViewModel model)
		{
			foreach (var kvp in _filesToAdd)
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
			if (!_isRampInstalled.HasValue)
			{
				// we remember the value so that we check only once. This won't change within
				// a test run.
				_isRampInstalled = !string.IsNullOrEmpty(RampArchivingDlgViewModel.GetExeFileLocation());
			}

			if (!_isRampInstalled.Value)
				Assert.Ignore("This test requires RAMP");
		}
		#endregion
	}
}
