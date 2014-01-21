using System;
using System.Collections.Generic;
using System.Xml;
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

		#region Setup and Teardown
		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			_tmpFolder = new TemporaryFolder("IMDIArchiveHelperTestFolder");
			_model = new IMDIArchivingDlgViewModel(kAppName, kTitle, kArchiveId, null, true,
				SetFilesToArchive, _tmpFolder.Path);
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_model.CleanUp();
			_tmpFolder.Dispose();
		}
		#endregion

		#region Miscellaneous Tests
		/// ------------------------------------------------------------------------------------
		[Test]
		public void NormalizeFilename_FileName_NormalizedFileName()
		{
			const string fileName = "My# \nFile %\t Name&^%.mp3";
			var normalized = _model.NormalizeFilename("", fileName);
			Assert.AreEqual("My+File+Name_.mp3", normalized);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CorpusDirectoryName_ValidNameForNewDirectory()
		{
			var dirName = _model.CorpusDirectoryName;

			Assert.AreEqual(21, dirName.Length);
			Assert.AreEqual("T_st_Title_", dirName.Substring(0, 11));
		}
		#endregion

		#region GetNameOfProgramToLaunch tests
		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void GetNameOfProgramToLaunch_ShortExeName_ReturnsExeNameWithoutExtension()
		{
			// fails on TeamCity because Arbil is not installed

			_model.ProgramPreset = "Arbil";
			Assert.AreEqual("Arbil", _model.NameOfProgramToLaunch);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void GetNameOfProgramToLaunch_ExeNameContainsFolderName_ReturnsFolderName()
		{
			// fails on TeamCity because Arbil is not installed

			_model.ProgramPreset = "Arbil";
			Assert.AreEqual("Arbil", _model.NameOfProgramToLaunch);
		}
		#endregion

		#region SetAbstract Tests
		[Test]
		public void SetAbstract_UnspecifiedLanguage_AddsDescriptionToCorpusImdiFile()
		{
			_model.Initialize();
			_model.SetAbstract("Story about a frog", string.Empty);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(_model.GetMetadata());
			Assert.AreEqual(2, doc.ChildNodes.Count);
			var root = doc.ChildNodes[1];
			Assert.AreEqual("METATRANSCRIPT", root.Name);
			Assert.AreEqual(1, root.ChildNodes.Count);
			var corpus = root.ChildNodes[0];
			Assert.AreEqual("Corpus", corpus.Name);
			int cDescNodes = 0;
			foreach (XmlNode node in corpus.ChildNodes)
			{
				if (node.Name == "Description")
				{
					Assert.AreEqual("Story about a frog", node.InnerText);
					Assert.NotNull(node.Attributes);
					foreach (XmlAttribute attrib in node.Attributes)
					{
						if (attrib.Name == "LanguageId")
							Assert.AreEqual(string.Empty, attrib.Value);
					}
					cDescNodes++;
				}
			}
			Assert.AreEqual(1, cDescNodes);
		}


		[Test]
		public void SetAbstract_SingleLanguage_AddsDescriptionToCorpusImdiFile()
		{
			_model.Initialize();
			_model.SetAbstract("Story about a frog", "eng");
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(_model.GetMetadata());
			Assert.AreEqual(2, doc.ChildNodes.Count);
			var root = doc.ChildNodes[1];
			Assert.AreEqual("METATRANSCRIPT", root.Name);
			Assert.AreEqual(1, root.ChildNodes.Count);
			var corpus = root.ChildNodes[0];
			Assert.AreEqual("Corpus", corpus.Name);
			int cDescNodes = 0;
			foreach (XmlNode node in corpus.ChildNodes)
			{
				if (node.Name == "Description")
				{
					Assert.AreEqual("Story about a frog", node.InnerText);
					Assert.NotNull(node.Attributes);
					Assert.AreEqual("ISO639-3:eng", node.Attributes.GetNamedItem("LanguageId").Value);
					cDescNodes++;
				}
			}
			Assert.AreEqual(1, cDescNodes);
		}

		[Test]
		public void SetAbstract_MultipleLanguages_AddsDescriptionToCorpusImdiFile()
		{
			_model.Initialize();
			Dictionary<string, string> descriptions = new Dictionary<string, string>();
			descriptions["eng"] = "Story about a frog";
			descriptions["deu"] = "Geschichte über einen Frosch";
			descriptions["fra"] = "L'histoire d'une grenouille";
			_model.SetAbstract(descriptions);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(_model.GetMetadata());
			Assert.AreEqual(2, doc.ChildNodes.Count);
			var root = doc.ChildNodes[1];
			Assert.AreEqual("METATRANSCRIPT", root.Name);
			Assert.AreEqual(1, root.ChildNodes.Count);
			var corpus = root.ChildNodes[0];
			Assert.AreEqual("Corpus", corpus.Name);
			foreach (XmlNode node in corpus.ChildNodes)
			{
				if (node.Name == "Description")
				{
					Assert.NotNull(node.Attributes);
					var lang = node.Attributes.GetNamedItem("LanguageId").Value;
					Assert.IsTrue(lang.StartsWith("ISO639-3:"));
					lang = lang.Substring("ISO639-3:".Length);
					Assert.AreEqual(descriptions[lang], node.InnerText);
					descriptions.Remove(lang);
				}
			}
			Assert.AreEqual(0, descriptions.Count);
		}

		[Test]
		public void SetAbstract_BogusLanguage_ThrowsException()
		{
			_model.Initialize();
			Dictionary<string, string> descriptions = new Dictionary<string, string>();
			descriptions["eng"] = "Story about a frog";
			descriptions["frn"] = "L'histoire d'une grenouille";
			Assert.Throws(typeof(ArgumentException), () => _model.SetAbstract(descriptions));
		}
		#endregion

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
