using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;
using SIL.Archiving.IMDI;
using SIL.Reporting;
using SIL.TestUtilities;
using static SIL.Archiving.ArchivingDlgViewModel.MessageType;
using CancellationToken = System.Threading.CancellationToken;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[OfflineSldr]
	[Category("Archiving")]
	internal class IMDIArchivingDlgViewModelTests
	{
		private class MessageData
		{
			public readonly string MsgText;
			public ArchivingDlgViewModel.MessageType MsgType;

			public MessageData(string msg, ArchivingDlgViewModel.MessageType type)
			{
				MsgText = msg;
				MsgType = type;
			}
		}

		private IMDIArchivingDlgViewModel _model;
		private TestProgress m_progress;
		private TemporaryFolder m_tmpFolder;
		private List<MessageData> m_messages;
		private const string kAppName = "Tèst App Náme";
		private const string kTitle = "Tèst Title";
		private const string kArchiveId = "Tèst Corpus Náme"; // include some invalid characters for testing

		#region Setup and Teardown

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public async Task Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			m_tmpFolder = new TemporaryFolder("IMDIArchiveHelperTestFolder");
			_model = new IMDIArchivingDlgViewModel(kAppName, kTitle, kArchiveId, true, (no, op) => { }, m_tmpFolder.Path);
			m_progress = new TestProgress("IMDI");
			var cancel = new CancellationToken();
			await _model.Initialize(m_progress, cancel);
			Assert.That(m_progress.Step, Is.EqualTo(1));
			m_messages = new List<MessageData>();
			_model.OnReportMessage += (msg, type) => { m_messages.Add(new MessageData(msg, type)); };
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_model.CleanUp();
			m_tmpFolder.Dispose();
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
			var dir = m_tmpFolder.Path;
			var writable = _model.IsPathWritable(dir);
			Assert.True(writable);
			Assert.IsEmpty(m_messages);
		}

		[Test]
		public void PathIsAccessible_NonexistentPath_False()
		{
			const string dir = "/one/two";
			var writable = _model.IsPathWritable(dir);
			Assert.False(writable);
			Assert.AreEqual(1, m_messages.Count);
			Assert.AreEqual("Test implementation message for PathNotWritable", m_messages[0].MsgText);
			Assert.AreEqual(Warning, m_messages[0].MsgType);
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "This test won't fail as expected on Linux - the only invalid character is NULL, and it produces a different message")]
		public void IsPathWritable_WindowsInvalidPath_False()
		{
			const string dir = ":?";
			var writable = _model.IsPathWritable(dir);
			Assert.False(writable);
			Assert.AreEqual(1, m_messages.Count);
			Assert.IsTrue(m_messages[0].MsgText.Contains("path"), "Error should mention the path in its explanation.");
			Assert.AreEqual(Warning, m_messages[0].MsgType);
		}

		[Test]
		public void IsPathWritable_IllegalCharacterInPath_False()
		{
			const string dir = "/\0";
			var writable = _model.IsPathWritable(dir);
			Assert.False(writable);
			Assert.AreEqual(1, m_messages.Count);
			Assert.IsTrue(m_messages[0].MsgText.Contains("path"), "Error should mention the path in its explanation.");
			Assert.AreEqual(Warning, m_messages[0].MsgType);
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

	[TestFixture]
	[Category("Archiving")]
	public class IMDIArchivingDlgViewModelWithOverrideDisplayInitialSummarySetTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public async Task DisplayInitialSummary_OverrideDisplayInitialSummaryIsSet_DefaultBehaviorOmitted()
		{
			ErrorReport.IsOkToInteractWithUser = false;

			bool filesToArchiveCalled = false;

			var model = new IMDIArchivingDlgViewModel("Test App", "Test Title", "tst", true,
				(a, b) => { filesToArchiveCalled = true; }, "whatever");

			var progress = new TestProgress("IMDI");
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

			model.InitializationFailed += (sender, e) => Assert.Fail("Initialization failed");

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
			Assert.That(progress.Step, Is.EqualTo(1));
		}
	}

	[TestFixture]
	[Category("Archiving")]
	public class IMDIArchivingDlgViewModelWithFineGrainedOverridesForDisplayInitialSummarySetTests
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

			var model = new IMDIArchivingDlgViewModel("Test App", "Test Title", "tst", true,
				SetFilesToArchive, "unused");

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
			model.InitializationFailed += (sender, e) => Assert.Fail("Initialization failed");

			var progress = new TestProgress("IMDI");
			try
			{
				await model.Initialize(progress, new CancellationToken()).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Initialization threw an exception: {ex}");
			}

			Assert.That(messagesDisplayed.Count, Is.EqualTo(7));
			Assert.That(messagesDisplayed[0].Item1, Is.EqualTo("First pre-archiving message"));
			Assert.That(messagesDisplayed[0].Item2, Is.EqualTo(Warning));
			Assert.That(messagesDisplayed[1].Item1, Is.EqualTo("Second pre-archiving message"));
			Assert.That(messagesDisplayed[1].Item2, Is.EqualTo(Indented));
			Assert.That(messagesDisplayed[2].Item1, Is.EqualTo("Frogs"));
			Assert.That(messagesDisplayed[2].Item2, Is.EqualTo(Success));
			Assert.That(messagesDisplayed[3].Item1, Is.EqualTo("green.frog"));
			Assert.That(messagesDisplayed[3].Item2, Is.EqualTo(Bullet));
			Assert.That(messagesDisplayed[4].Item1, Is.EqualTo("Label: Toads"));
			Assert.That(messagesDisplayed[4].Item2, Is.EqualTo(Success));
			Assert.That(messagesDisplayed[5].Item1, Is.EqualTo("red.toad"));
			Assert.That(messagesDisplayed[5].Item2, Is.EqualTo(Bullet));
			Assert.That(messagesDisplayed[6].Item1, Is.EqualTo("blue.toad"));
			Assert.That(messagesDisplayed[6].Item2, Is.EqualTo(Bullet));
			Assert.That(progress.Step, Is.EqualTo(1));
		}
	}
}
