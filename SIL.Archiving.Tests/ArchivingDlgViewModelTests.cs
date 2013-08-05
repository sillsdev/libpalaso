using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
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

			_helper = new ArchivingDlgViewModel("Test Title", "tst", null, null, GetFileDescription, null, CustomFilenameNormalization);
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
				var filesToAdd = new Dictionary<string, IEnumerable<string>>();
				filesToAdd.Add(string.Empty, new [] {Path.Combine(tmpFolder.Path, "ddo.session")});
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
			Assert.AreEqual("\"dc.type.mode\":[\"Video\"]", _helper.GetMode(new[] { "blah.mpg" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_MultipleTypesInList_ReturnsCorrectMetsList()
		{
			var mode = _helper.GetMode(new[] { "blah.mp3", "blah.doc", "blah.mov" });
			Assert.AreEqual("\"dc.type.mode\":[\"Speech\",\"Text\",\"Video\"]", mode);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetMode_ListContainsMultiplesOfOneType_ReturnsOnlyOneTypeInList()
		{
			var mode = _helper.GetMode(new[] { "blah.mp3", "blah.wma", "blah.wav" });
			Assert.AreEqual("\"dc.type.mode\":[\"Speech\"]", mode);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlySessionMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, IEnumerable<string>>();
			fileLists[string.Empty] = new[] { "blah.session" };

			var expected = "\" \":\"blah.session\",\"description\":\"MyApp Session Metadata (XML)\",\"relationship\":\"source\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyPersonMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, IEnumerable<string>>();
			fileLists[string.Empty] = new[] { "blah.person" };

			var expected = "\" \":\"blah.person\",\"description\":\"MyApp Contributor Metadata (XML)\",\"relationship\":\"source\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsOnlyMetaFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, IEnumerable<string>>();
			fileLists[string.Empty] = new[] { "blah.meta" };

			var expected = "\" \":\"blah.meta\",\"description\":\"MyApp File Metadata (XML)\",\"relationship\":\"source\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericSessionFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, IEnumerable<string>>();
			fileLists[string.Empty] = new[] { "blah.wav" };

			var expected = "\" \":\"blah.wav\",\"description\":\"MyApp Session File\",\"relationship\":\"source\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListContainsGenericPersonFile_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, IEnumerable<string>>();
			fileLists["Carmen"] = new[] { "Carmen_blah.wav" };

			var expected = "\" \":\"__AppSpecific__Carmen_blah.wav\",\"description\":\"MyApp Contributor File\",\"relationship\":\"source\"";
			Assert.AreEqual(expected, _helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetSourceFilesForMetsData_ListMultipleFiles_ReturnsCorrectMetsData()
		{
			var fileLists = new Dictionary<string, IEnumerable<string>>();
			fileLists[string.Empty] = new[] { "blah.session", "really cool.wav" };
			fileLists["person id"] = new[] { "person id_blah.person", "person id_baa.mpg", "person id_baa.mpg.meta" };

			Assert.AreEqual("\" \":\"blah.session\",\"description\":\"MyApp Session Metadata (XML)\",\"relationship\":\"source\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(0));

			Assert.AreEqual("\" \":\"really+cool.wav\",\"description\":\"MyApp Session File\",\"relationship\":\"source\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(1));

			Assert.AreEqual("\" \":\"__AppSpecific__person+id_blah.person\",\"description\":\"MyApp Contributor Metadata (XML)\",\"relationship\":\"source\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(2));

			Assert.AreEqual("\" \":\"__AppSpecific__person+id_baa.mpg\",\"description\":\"MyApp Contributor File\",\"relationship\":\"source\"",
				_helper.GetSourceFilesForMetsData(fileLists).ElementAt(3));

			Assert.AreEqual("\" \":\"__AppSpecific__person+id_baa#mpg.meta\",\"description\":\"MyApp File Metadata (XML)\",\"relationship\":\"source\"",
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
