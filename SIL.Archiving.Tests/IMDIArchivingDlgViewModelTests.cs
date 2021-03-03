using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using SIL.Archiving.IMDI;
using SIL.Reporting;
using SIL.TestUtilities;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[OfflineSldr]
	[Category("Archiving")]
	internal class IMDIArchivingDlgViewModelTests
	{
		private class MessageData
		{
			public string MsgText;
			public ArchivingDlgViewModel.MessageType MsgType;

			public MessageData(string msg, ArchivingDlgViewModel.MessageType type)
			{
				MsgText = msg;
				MsgType = type;
			}
		}

		private IMDIArchivingDlgViewModel _model;
		private TemporaryFolder _tmpFolder;
		private List<MessageData> _messages;
		private const string kAppName = "Tèst App Náme";
		private const string kTitle = "Tèst Title";
		private const string kArchiveId = "Tèst Corpus Náme"; // include some invalid characters for testing

		#region Setup and Teardown

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			_tmpFolder = new TemporaryFolder("IMDIArchiveHelperTestFolder");
			_model = new IMDIArchivingDlgViewModel(kAppName, kTitle, kArchiveId, null, true, dummyAction => { }, _tmpFolder.Path);
			_messages = new List<MessageData>();
			_model.OnDisplayMessage += (msg, type) => { _messages.Add(new MessageData(msg, type)); };
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

		/// ------------------------------------------------------------------------------------
		[Test]
		public void PathIsAccessible_WritablePath_True()
		{
			var dir = _tmpFolder.Path;
			var writable = _model.IsPathWritable(dir);
			Assert.True(writable);
			Assert.IsEmpty(_messages);
		}

		[Test]
		public void PathIsAccessible_NonexistentPath_False()
		{
			const string dir = "/one/two";
			var writable = _model.IsPathWritable(dir);
			Assert.False(writable);
			Assert.AreEqual(1, _messages.Count);
			Assert.AreEqual("The path is not writable: /one/two", _messages[0].MsgText);
			Assert.AreEqual(ArchivingDlgViewModel.MessageType.Warning, _messages[0].MsgType);
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "This test won't fail as expected on Linux - the only invalid character is NULL, and it produces a different message")]
		public void IsPathWritable_WindowsInvalidPath_False()
		{
			const string dir = ":?";
			var writable = _model.IsPathWritable(dir);
			Assert.False(writable);
			Assert.AreEqual(1, _messages.Count);
			Assert.AreEqual("The path is not of a legal form.", _messages[0].MsgText);
			Assert.AreEqual(ArchivingDlgViewModel.MessageType.Warning, _messages[0].MsgType);
		}

		[Test]
		public void IsPathWritable_IllegalCharacterInPath_False()
		{
			const string dir = "/\0";
			var writable = _model.IsPathWritable(dir);
			Assert.False(writable);
			Assert.AreEqual(1, _messages.Count);
			Assert.AreEqual("Illegal characters in path.", _messages[0].MsgText);
			Assert.AreEqual(ArchivingDlgViewModel.MessageType.Warning, _messages[0].MsgType);
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

			// AT THIS TIME WE ARE NOT SHOWING THE LAUNCH OPTION
			//Assert.AreEqual("Arbil", _model.NameOfProgramToLaunch);
			Assert.IsNull(_model.NameOfProgramToLaunch);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void GetNameOfProgramToLaunch_ExeNameContainsFolderName_ReturnsFolderName()
		{
			// fails on TeamCity because Arbil is not installed

			_model.ProgramPreset = "Arbil";

			// AT THIS TIME WE ARE NOT SHOWING THE LAUNCH OPTION
			//Assert.AreEqual("Arbil", _model.NameOfProgramToLaunch);
			Assert.IsNull(_model.NameOfProgramToLaunch);
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

		// We now accept languages not in the Arbil list
		//[Test]
		//public void SetAbstract_BogusLanguage_ThrowsException()
		//{
		//    _model.Initialize();
		//    Dictionary<string, string> descriptions = new Dictionary<string, string>();
		//    descriptions["eng"] = "Story about a frog";
		//    descriptions["frn"] = "L'histoire d'une grenouille";
		//    Assert.Throws(typeof (ArgumentException), () => _model.SetAbstract(descriptions));
		//}

		#endregion
	}
}
