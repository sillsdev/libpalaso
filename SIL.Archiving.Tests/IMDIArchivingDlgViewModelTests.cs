using NUnit.Framework;
using Palaso.Reporting;
using Palaso.TestUtilities;
using SIL.Archiving.IMDI;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	class IMDIArchivingDlgViewModelTests
	{
		private IMDIArchivingDlgViewModel _helper;
		private TemporaryFolder _tmpFolder;
		private const string kAppName = "Tèst App Náme";
		private const string kCorpusName = "Tèst Corpus Náme";  // include some invalid characters for testing
		private const string kTitle = "Tèst Title";
		private const string kArchiveId = "TestID";

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			_tmpFolder = new TemporaryFolder("IMDIArchiveHelperTestFolder");
			_helper = new IMDIArchivingDlgViewModel(kAppName, kCorpusName, kTitle, kArchiveId, _tmpFolder.Path);
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_helper.CleanUp();
			_tmpFolder.Dispose();
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void NormalizeFilename_FileName_NormalizedFileName()
		{
			const string fileName = "My# \nFile %\t Name&^%.mp3";
			var normalized = _helper.NormalizeFilename("", fileName);
			Assert.AreEqual("My+File+Name_.mp3", normalized);
		}

		[Test]
		public void CorpusDirectoryName_ValidNameForNewDirectory()
		{
			var dirName = _helper.CorpusDirectoryName;
			Assert.AreEqual("T_st_Corpus_N_me", dirName);
		}


	}
}
