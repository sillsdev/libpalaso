using System;
using System.Collections.Generic;
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
		private IMDIArchivingDlgViewModel _model;
		private TemporaryFolder _tmpFolder;
		private const string kAppName = "Tèst App Náme";
		private const string kTitle = "Tèst Title";
		private const string kArchiveId = "Tèst Corpus Náme";  // include some invalid characters for testing
		private Dictionary<string, Tuple<IEnumerable<string>, string>> _filesToAdd;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			_tmpFolder = new TemporaryFolder("IMDIArchiveHelperTestFolder");
			_model = new IMDIArchivingDlgViewModel(kAppName, kTitle, kArchiveId,
				SetFilesToArchive, _tmpFolder.Path);
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_model.CleanUp();
			_tmpFolder.Dispose();
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void NormalizeFilename_FileName_NormalizedFileName()
		{
			const string fileName = "My# \nFile %\t Name&^%.mp3";
			var normalized = _model.NormalizeFilename("", fileName);
			Assert.AreEqual("My+File+Name_.mp3", normalized);
		}

		[Test]
		public void CorpusDirectoryName_ValidNameForNewDirectory()
		{
			var dirName = _model.CorpusDirectoryName;
			Assert.AreEqual("T_st_Corpus_N_me", dirName);
		}

		[Test]
		public void SetAbstract_AddsDescriptionToCorpusImdiFile()
		{
			_model.Initialize();
			_model.SetAbstract("Story about a frog", "eng");
			Assert.AreEqual("some cool XML data", _model.GetMetadata());
		}

		#region Helper methods
		/// ------------------------------------------------------------------------------------
		private void SetFilesToArchive(ArchivingDlgViewModel model)
		{
			if (_filesToAdd == null)
				return;
			foreach (var kvp in _filesToAdd)
				model.AddFileGroup(kvp.Key, kvp.Value.Item1, kvp.Value.Item2);
		}
		#endregion
	}
}
