using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Reporting;
using Palaso.TestUtilities;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	public class SessionArchivingTests
	{
		private ArchivingDlgViewModel _helper;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;

			_helper = new ArchivingDlgViewModel("Test App", "Test Title", "tst", null, GetFileDescription);
			_helper.AppSpecificFilenameNormalization = CustomFilenameNormalization;
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_helper.CleanUp();

			try { File.Delete(_helper.RampPackagePath); }
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
				var filesToAdd = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
				var fileList = new[] {Path.Combine(tmpFolder.Path, "ddo.session")};
				filesToAdd.Add(string.Empty, new Tuple<IEnumerable<string>, string>(fileList, "Message to display."));
				int dummy;
				_helper.Initialize(() => filesToAdd, out dummy, null);
				_helper.CreateMetsFile();
				Assert.IsTrue(_helper.CreateRampPackage());
				Assert.IsTrue(File.Exists(_helper.RampPackagePath));
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
			Assert.AreEqual("\"" + ArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				ArchivingDlgViewModel.kModeVideo + "\"]", _helper.GetMode(new[] { "blah.mpg" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_MultipleTypesInList_ReturnsCorrectMetsList()
		{
			var mode = _helper.GetMode(new[] { "blah.mp3", "blah.doc", "blah.mov" });
			Assert.AreEqual("\"" + ArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				ArchivingDlgViewModel.kModeSpeech + "\",\"" +
				ArchivingDlgViewModel.kModeText + "\",\"" +
				ArchivingDlgViewModel.kModeVideo + "\"]", mode);
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
				Assert.AreEqual("\"" + ArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
					ArchivingDlgViewModel.kModeSpeech + "\",\"" +
					ArchivingDlgViewModel.kModeText + "\",\"" +
					ArchivingDlgViewModel.kModeMusicalNotation + "\"]", mode);
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
				Assert.AreEqual("\"" + ArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
					ArchivingDlgViewModel.kModeDataset + "\",\"" +
					ArchivingDlgViewModel.kModeSoftwareOrFont + "\",\"" +
					ArchivingDlgViewModel.kModePhotograph + "\"]", mode);
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
			Assert.AreEqual("\"" + ArchivingDlgViewModel.kFileTypeModeList + "\":[\"" +
				ArchivingDlgViewModel.kModeSpeech + "\"]", mode);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlySessionMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.session" }, "Message to display.");

			var expected = "\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"blah.session\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session Metadata (XML)\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyPersonMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.person" }, "Message to display.");

			var expected = "\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"blah.person\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor Metadata (XML)\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.meta" }, "Message to display.");

			var expected = "\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"blah.meta\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp File Metadata (XML)\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"";

			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericSessionFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.wav" }, "Message to display.");

			var expected = "\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"blah.wav\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session File\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"";

			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericPersonFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists["Carmen"] = new Tuple<IEnumerable<string>, string>(new[] { "Carmen_blah.wav" }, "Message to display.");

			var expected = "\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__Carmen_blah.wav\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor File\"" + ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListMultipleFiles_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, Tuple<IEnumerable<string>, string>>();
			fileLists[string.Empty] = new Tuple<IEnumerable<string>, string>(new[] { "blah.session", "really cool.wav" }, "Message to display.");
			fileLists["person id"] = new Tuple<IEnumerable<string>, string>(new[] { "person id_blah.person", "person id_baa.mpg", "person id_baa.mpg.meta" }, "Message to display.");

			Assert.AreEqual("\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"blah.session\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session Metadata (XML)\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));

			Assert.AreEqual("\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"really+cool.wav\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Session File\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(1));

			Assert.AreEqual("\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person+id_blah.person\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor Metadata (XML)\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(2));

			Assert.AreEqual("\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person+id_baa.mpg\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp Contributor File\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(3));

			Assert.AreEqual("\"" + ArchivingDlgViewModel.kDefaultKey + "\":\"__AppSpecific__person+id_baa#mpg.meta\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileDescription + "\":\"MyApp File Metadata (XML)\"" +
				ArchivingDlgViewModel.kSeparator + "\"" +
				ArchivingDlgViewModel.kFileRelationship + "\":\"" +
				ArchivingDlgViewModel.kRelationshipSource + "\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(4));
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
	}
}
