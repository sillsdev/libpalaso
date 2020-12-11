using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems;
using SIL.WritingSystems.Tests;

namespace SIL.Lift.Tests
{
	[TestFixture]
	[OfflineSldr]
	public class WritingSystemsInLiftFileHelperTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _folder;
			private readonly SIL.IO.TempFile _liftFile1;

			public TestEnvironment(string liftFileContent)
			{
				_folder = new TemporaryFolder("WritingSystemsInLiftFileHelper");
				var pathtoLiftFile1 = Path.Combine(_folder.Path, "test1.lift");
				_liftFile1 = new SIL.IO.TempFile(liftFileContent);
				_liftFile1.MoveTo(pathtoLiftFile1);
				Helper = new WritingSystemsInLiftFileHelper(WritingSystems, _liftFile1.Path);
			}

			#region LongFileContent
			public static readonly string LiftFile1Content =
 @"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='WeSay 1.0.0.0'>
	<entry
		id='chùuchìi mǔu rɔ̂ɔp_dd15cbc4-9085-4d66-af3d-8428f078a7da'
		dateCreated='2008-11-03T06:17:24Z'
		dateModified='2009-10-12T04:05:40Z'
		guid='dd15cbc4-9085-4d66-af3d-8428f078a7da'>
		<lexical-unit>
			<form
				lang='{0}'>
				<text>chùuchìi mǔu krɔ̂ɔp</text>
			</form>
			<form
				lang='{1}'>
				<text>ฉู่ฉี่หมูรอบ</text>
			</form>
		</lexical-unit>
		<sense
			id='df801833-d55b-4492-b501-650da7bc7b73'>
			<definition>
				<form
					lang='{0}'>
					<text>A kind of curry fried with crispy pork</text>
				</form>
			<form
				lang='{1}'>
				<text>ฉู่ฉี่หมูรอบ</text>
			</form>
			</definition>
		</sense>
	</entry>
</lift>".Replace("'", "\"");

			private IWritingSystemRepository _writingSystems;

			#endregion

			private string ProjectPath
			{
				get { return _folder.Path; }
			}

			public WritingSystemsInLiftFileHelper Helper { get; private set; }

			public void Dispose()
			{
				_liftFile1.Dispose();
				_folder.Dispose();
			}

			private IWritingSystemRepository WritingSystems
			{
				get { return _writingSystems ?? (_writingSystems = new TestLdmlInFolderWritingSystemRepository(WritingSystemsPath)); }
			}

			public string WritingSystemsPath
			{
				get { return Path.Combine(ProjectPath, "WritingSystems"); }
			}

			public string PathToLiftFile
			{
				get { return _liftFile1.Path; }
			}

			public string GetLdmlFileforWs(string id)
			{
				return Path.Combine(WritingSystemsPath, String.Format("{0}.ldml", id));
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInLift_LiftFileContainsNonConformantRfcTag_CreatesConformingWritingSystem()
		{
			using (var e = new TestEnvironment(String.Format(TestEnvironment.LiftFile1Content, "bogusws1", "audio")))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-x-bogusws1")));
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-Zxxx-x-audio")));
				AssertThatXmlIn.File(e.GetLdmlFileforWs("qaa-x-bogusws1")).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("qaa-x-bogusws1")).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-bogusws1']");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("qaa-Zxxx-x-audio")).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("qaa-Zxxx-x-audio")).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("qaa-Zxxx-x-audio")).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");

			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInLift_LiftFileContainsNonConformantRfcTag_WSIdChangeLogUpdated()
		{
			using (var e = new TestEnvironment(String.Format(TestEnvironment.LiftFile1Content, "bogusws1", "audio")))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-x-bogusws1")));
				Assert.That(File.Exists(e.GetLdmlFileforWs("qaa-Zxxx-x-audio")));
				string idChangeLogFilePath = Path.Combine(e.WritingSystemsPath, "idchangelog.xml");
				AssertThatXmlIn.File(idChangeLogFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes[Add/Id/text()='qaa-x-bogusws1' and Add/Id/text()='qaa-Zxxx-x-audio']");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInLift_LiftFileContainsNonConformantRfcTag_UpdatesRfcTagInLiftFile()
		{
			using (var environment = new TestEnvironment(String.Format(TestEnvironment.LiftFile1Content, "bogusws1", "audio")))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(environment.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='qaa-x-bogusws1']");
				AssertThatXmlIn.File(environment.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='qaa-Zxxx-x-audio']");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInLift_LiftFileContainsNonConformantRfcTagWithDuplicates_UpdatesRfcTagInLiftFile()
		{
			using (var environment = new TestEnvironment(String.Format(TestEnvironment.LiftFile1Content, "wee", "qaa-x-wee")))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(environment.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='qaa-x-wee-dupl0']");
				AssertThatXmlIn.File(environment.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='qaa-x-wee']");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInLift_LiftFileContainsConformantRfcTagWithNoCorrespondingLdml_CreatesLdml()
		{
			using (var e = new TestEnvironment(String.Format(TestEnvironment.LiftFile1Content, "de", "x-dontcare")))
			{
				e.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']");
				Assert.That(File.Exists(e.GetLdmlFileforWs("de")), Is.True);
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='de']");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasNoMatchForXpath("/ldml/identity/territory");
				AssertThatXmlIn.File(e.GetLdmlFileforWs("de")).HasNoMatchForXpath("/ldml/identity/variant");
			}
		}

		[Test]
		public void CreateNonExistentWritingSystemsFoundInLift_LiftFileContainsNonConformantRfcTagWithDuplicatesContainingduplicateMarker_UpdatesRfcTagInLiftFile()
		{
			using (var environment = new TestEnvironment(String.Format(TestEnvironment.LiftFile1Content, "wee-dupl1", "qaa-x-wee-dupl1")))
			{
				environment.Helper.CreateNonExistentWritingSystemsFoundInFile();
				AssertThatXmlIn.File(environment.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='qaa-x-wee-dupl1']");
				AssertThatXmlIn.File(environment.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='qaa-x-wee-dupl1-dupl0']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_OneOfOneFormContainsWritingSystem_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> {{"th", "thai"}};
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using(var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th","de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='thai']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_OneOfTwoFormsContainsOldWritingSystem_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, {"en", "en word"} };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='thai Word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoFormsContainsOldWritingSystemOtherContainsNewWritingSystemFirstHasContent_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']", 2);
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='thai Word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoFormsContainsOldWritingSystemOtherContainsNewWritingSystemSecondHasContent_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "" }, { "de", "de word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']", 2);
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='de word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_SecondOfTwoFormsContainsOldWritingSystemOtherContainsNewWritingSystemSecondHasContent_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "de", "" }, { "th", "thai Word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']", 2);
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='thai Word']");
				// can't test easily for empty: AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoFormsContainsOldWritingSystemOtherContainsNewWritingSystemBothHaveContent_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "de word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']", 2);
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='thai Word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='de word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoFormsContainsOldWritingSystemOtherContainsNewWritingSystemBothHaveIdenticalContent_WritingSystemIsReplacedSecondFormIsDeleted()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "de word" }, { "de", "de word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']", 1);
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='de word']", 1);
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoGlossesContainsOldWritingSystemOtherContainsNewWritingSystemBothHaveIdenticalContent_WritingSystemIsReplacedSecondFormIsDeleted()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "de word" }, { "de", "de word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']/text", 1);
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='de word']", 1);
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/sense/gloss[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_OneOfTwoGlossesContainsOldWritingSystem_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "en", "en word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/gloss[@lang='de']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='thai Word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/sense/gloss[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoGlossesContainsOldWritingSystemOtherContainsNewWritingSystemFirstHasContent_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']", 2);
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='thai Word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/sense/gloss[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_SecondOfTwoGlossesContainsOldWritingSystemOtherContainsNewWritingSystemSecondHasContent_WritingSystemIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "de", "" }, { "th", "thai Word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']", 2);
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='thai Word']");
				// can't test easily for empty: AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']/text[text()='']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/sense/gloss[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoGlossesContainsOldWritingSystemOtherContainsNewWritingSystemBothHaveContent_WritingSystemIsReplacedSecondFormIsDeleted()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "de word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']", 2);
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='thai Word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='de word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/sense/gloss[@lang='th']");
			}
		}

		[Test]
		public void ReplaceWritingSystemId_ContainsGlossesWithUninvolvedWritingSystems_GlossesAreLeftAlone()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "de word" }, { "en", "en word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/gloss[@lang='en']/text[text()='en word']");
				AssertThatXmlIn.File(e.PathToLiftFile).HasAtLeastOneMatchForXpath("/lift/entry/sense/definition/form[@lang='en']/text[text()='eng-mean']");
			}

		}

		[Test]
		public void DeleteWritingSystemId_LangIsFoundInFormThatIsOneOfMultipleForms_FormIsDeleted()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "de word" }, { "en", "en word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']", 1);
				e.Helper.DeleteWritingSystemId("de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit/form[@lang='de']");
			}
		}

		[Test]
		public void DeleteWritingSystemId_LangIsFoundInFormThatIsOnlyOne_ParentandFormAreDeleted()
		{
			var ws2Content = new Dictionary<string, string> { { "de", "de word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntryWithLexicalUnitContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/lexical-unit/form[@lang='de']", 1);
				e.Helper.DeleteWritingSystemId("de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/lexical-unit");
			}
		}

		[Test]
		public void DeleteWritingSystemId_LangIsFoundInGloss_GlossDeleted()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "de word" }, { "en", "en word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='de word']", 1);
				e.Helper.DeleteWritingSystemId("de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/sense/gloss[@lang='de']");
			}
		}

		[Test]
		public void DeleteWritingSystemId_TwoEntries_OutPutHasTwoEntriesAsWell()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai Word" }, { "de", "de word" }, { "en", "en word" } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content) + LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry", 2);
				e.Helper.DeleteWritingSystemId("de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry", 2);
			}
		}

		//WS-34563
		[Test]
		public void ReplaceWritingSystemId_FirstOfTwoGlossesContainsOldWritingSystemOtherContainsNewWritingSystemBothHaveIdenticalContentWithSingleAndDoubleQuotes_WritingSystemIsReplacedSecondFormIsDeleted()
		{
			var problematicString = "He said: \"Life's good.\"";
			var ws2Content = new Dictionary<string, string> { { "th", problematicString }, { "de", problematicString } };
			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content));
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("th", "de");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']/text", 1);
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/entry/sense/gloss[@lang='th']");
				// We are not using:
				//AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/entry/sense/gloss[@lang='de']/text[text()='de word']", 1);
				//because the whole point is that the syntax is problematic for xpath
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(e.PathToLiftFile);
				var node = xmlDoc.SelectSingleNode("/lift/entry/sense/gloss[@lang='de']/text/text()");
				Assert.That(node.Value, Is.EqualTo(problematicString));
			}
		}

		//WS-34568
		[Test]
		public void ReplaceWritingSystemId_headerContainsBadRfcTag_RfcTagIsReplaced()
		{
			var ws2Content = new Dictionary<string, string> { { "th", "thai" }, { "de", "deutsch" } };
			var entryXml = LiftContentForTests.GetSingleEntrywithGlossContainingWritingsystemsAndContent(ws2Content);
			var entryWithHeader = LiftContentForTests.AddHeaderWithSingleCustomField("bogus", entryXml);

			var liftFileContent =
				LiftContentForTests.WrapEntriesInLiftElements("0.13", entryWithHeader);
			using (var e = new TestEnvironment(liftFileContent))
			{
				e.Helper.ReplaceWritingSystemId("bogus", "qaa-x-bogus");
				AssertThatXmlIn.File(e.PathToLiftFile).HasSpecifiedNumberOfMatchesForXpath("/lift/header/fields/field/form[@lang='qaa-x-bogus']", 1);
				AssertThatXmlIn.File(e.PathToLiftFile).HasNoMatchForXpath("/lift/header/fields/field/form[@lang='bogus']");
			}
		}
	}
}
