using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using SIL.Lift.Merging;
using SIL.Lift.Validation;
using SIL.PlatformUtilities;

namespace SIL.Lift.Tests.Merging
{
	[TestFixture]
	public class SynchronicMergerTests
	{
		private const string _baseLiftFileName = "base.lift";

		private string _directory;
		private SynchronicMerger _merger;

		[SetUp]
		public void Setup()
		{
			_merger = new SynchronicMerger();
			_directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_directory);
		}

		[TearDown]
		public void TearDOwn()
		{
//            DirectoryInfo di = new DirectoryInfo(_directory);
			Directory.Delete(_directory, true);
		}

		private static string GetNextUpdateFileName()
		{
			// Linux filesystem only has resolution of 1 second so we add this to filename so will sort correctly
			return _baseLiftFileName + DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss'-'FFFFFFF UTC ") + SynchronicMerger.ExtensionOfIncrementalFiles;
		}

		[Test]
		public void GetPendingUpdateFiles_SimpleFileNameInsteadOfPath_Throws()
		{
			WriteFile(_baseLiftFileName, "", _directory);
			Assert.Throws<ArgumentException>(() => SynchronicMerger.GetPendingUpdateFiles(_baseLiftFileName));
		}

		[Test]
		public void OneFile_NoUpdates_LeftUntouched()
		{
			string content = WriteFile(_baseLiftFileName, "<entry id=\"\u0E0C\" guid=\"0ae89610-fc01-4bfd-a0d6-1125b7281dd1\"></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(false, _directory);
			ExpectFileCount(1, _directory);
			Assert.AreEqual(2, doc.SelectNodes("//entry").Count);
			Assert.IsTrue(GetBaseFileInfo().Length >= content.Length);

		}

		private FileInfo GetBaseFileInfo()
		{
			DirectoryInfo di = new DirectoryInfo(_directory);
			return di.GetFiles(_baseLiftFileName, SearchOption.TopDirectoryOnly)
				.OrderBy(filename => filename).FirstOrDefault();
		}

		//private FileInfo[] GetFileInfos()
		//{
		//    DirectoryInfo di = new DirectoryInfo(_directory);
		//    return di.GetFiles("*"+SynchronicMerger.ExtensionOfIncrementalFiles, SearchOption.TopDirectoryOnly);
		//}

		[Test]
		public void NewEntries_Added()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='three3' guid='f9bd730d-1279-4dae-a9f0-f0ed92fcbf85'></entry><entry id='four' guid='0689afcb-20ae-4cea-affd-92e1f9219d87'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(4, doc.SelectNodes("//entry").Count);
		}

		[Test]
		public void NewEntriesAdded_MultipleFilesSucessiveChanges()
		{
			//This test demonstrates that LiftUpdate files are applied in the order they are created when time stamps are used to order the names up the LIFTUpdate files.
			//Notice that the files names of the LIFT update files are purposely created so that the alphabetical ordering does not match the time stamp ordering.

			//Create a LIFT file with 3 entries which will have updates applied to it.
			WriteFile(_baseLiftFileName, s_LiftData1, _directory);
			//Create a .lift.update file with three entries.  One to replace the second entry in the original LIFT file.
			//The other two are new and should be appended to the original LIFT file.
			WriteFile("LiftChangeFileB" + SynchronicMerger.ExtensionOfIncrementalFiles, s_LiftUpdate1, _directory);

			if (!Platform.IsWindows)
				Thread.Sleep(1000);		// Wait long enough to ensure different timestamps.  This is a problem for Linux/Mono.

			//Create a .lift.update file with two entries.  One to replace one of the changes from the first LiftUpdate file and one new entry.
			WriteFile("LiftChangeFileA" + SynchronicMerger.ExtensionOfIncrementalFiles, s_LiftUpdate2, _directory);
			FileInfo[] files = SynchronicMerger.GetPendingUpdateFiles(Path.Combine(_directory, _baseLiftFileName));

			XmlDocument doc = MergeAndGetResult(true, _directory, files);
			Console.WriteLine("------------------------BEGIN DEBUG----------------------");
			Console.WriteLine(doc.OuterXml);
			Console.WriteLine("------------------------END DEBUG----------------------");
			Assert.AreEqual(6, doc.SelectNodes("//entry").Count, "Should have been 6 entries");
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one']").Count, "should have been one entry with id of one");
			Assert.AreEqual(0, doc.SelectNodes("//entry[@id='two']").Count, "should not have been any entries with id of two");
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='twoblatblat']").Count, "should have had one entry with id twoblatblat");
			Assert.AreEqual(0, doc.SelectNodes("//entry[@id='four']").Count, "should have been no entries with id four");
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='fourChangedFirstAddition']").Count, "should have been one entry with id 'forchangedfirstaddition'");
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='six']").Count, "should have been one entry with id of 6");
		}

		private static readonly string s_LiftData1 =
			"<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1'></entry>"
			+ "<entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'><lexical-unit><form lang='nan'><text>test</text></form></lexical-unit></entry>"
			+ "<entry id='three' guid='80677C8E-9641-486e-ADA1-9D20ED2F5B69'></entry>";

		private static readonly string s_LiftUpdate1 =
			"<entry id='four' guid='6216074D-AD4F-4dae-BE5F-8E5E748EF68A'></entry>"
			+ "<entry id='twoblatblat' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>"
			+ "<entry id='five' guid='6D2EC48D-C3B5-4812-B130-5551DC4F13B6'></entry>";

		private static readonly string s_LiftUpdate2 =
			"<entry id='fourChangedFirstAddition' guid='6216074D-AD4F-4dae-BE5F-8E5E748EF68A'></entry>"
			+ "<entry id='six' guid='107136D0-5108-4b6b-9846-8590F28937E8'></entry>";


		[Test]
		public void EntryDeleted_DeletionDateAdded()
		{
			//This test demonstrates that a deletion of an entry is applied to a LIFT file.
			//Now 'tomb stoning' is done.  The entry is not actually deleted, but a dateDeleted attribute is added

			//Create a LIFT file with 3 entries which will have updates applied to it.
			WriteFile(_baseLiftFileName, s_LiftData1, _directory);
			//Create a .lift.update file with and entry which is indicating that an entry was deleted (tombstone).
			WriteFile("LiftChangeFile" + SynchronicMerger.ExtensionOfIncrementalFiles, s_LiftUpdateDeleteEntry, _directory);
			FileInfo[] files = SynchronicMerger.GetPendingUpdateFiles(Path.Combine(_directory, _baseLiftFileName));
			XmlDocument doc = MergeAndGetResult(true, _directory, files);
			Assert.AreEqual(4, doc.SelectNodes("//entry").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one']").Count);
			XmlNodeList nodesDeleted = doc.SelectNodes("//entry[@id='two' and @guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22']");
			Assert.AreEqual(1, nodesDeleted.Count);   //ensure there is only one entry with this guid
			XmlNode nodeDeleted = nodesDeleted[0];
			//Make sure the contents of the node was changed to match the deleted entry from the .lift.update file
			Assert.AreEqual("2012-05-08T06:40:44Z", nodeDeleted.Attributes["dateDeleted"].Value);
			Assert.That(nodeDeleted.InnerXml, Is.Null.Or.Empty);
		}

		private static readonly string s_LiftUpdateDeleteEntry =
			"<entry id='two' dateCreated='2012-05-04T04:19:57Z' dateModified='2012-05-04T04:19:57Z' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22' dateDeleted='2012-05-08T06:40:44Z'></entry>"
			+ "<entry id='six' guid='107136D0-5108-4b6b-9846-8590F28937E8'></entry>";

		[Category("Fails on Linux because of New Lines as well as xml namespaces")]
		[Test]
		public void NewEntries_SingleLineXml_FormattedCorrectly()
		{
			WriteLongSingleLineBaseFile();
			WriteLongSingleLineFragment();
			Merge(_directory);
			Assert.AreEqual(FormattedXml2entries, ReadFileContentIntoString(Path.Combine(_directory, _baseLiftFileName)));
		}

		private void WriteLongSingleLineBaseFile()
		{
			WriteFile(_baseLiftFileName, @"<entry id=""03d4b59a-9673-4f16-964e-4ea9f0636ef7"" dateCreated=""2009-10-07T04:07:54Z"" dateModified=""2009-10-07T04:10:34Z"" guid=""03d4b59a-9673-4f16-964e-4ea9f0636ef7""><lexical-unit><form lang=""nan""><text>test</text></form></lexical-unit><sense id=""e6f93a8d-7883-4c1c-877e-e34db9f06cdc""><definition><form lang=""en""><text>difficult; slow</text></form></definition></sense></entry>", _directory);
		}

		private void WriteLongSingleLineFragment()
		{
			WriteFile(GetNextUpdateFileName(), @"<entry id=""Xtest3_92f0c58a-9ad6-4715-ad9b-d56597217ac3"" dateCreated=""2010-02-04T03:58:33Z"" dateModified=""2010-02-04T03:58:40Z"" guid=""92f0c58a-9ad6-4715-ad9b-d56597217ac3""><lexical-unit><form lang=""v""><text>Xtest3</text></form></lexical-unit><sense id=""4bc90f1e-1d4a-418f-a6e0-a4191b3e04f3""><definition><form lang=""en""><text>test3</text></form></definition></sense></entry>", _directory);
		}

		private string ReadFileContentIntoString(string FileName)
		{
			StreamReader sr = null;
			string FileContents = null;

			try
			{
				FileStream fs = new FileStream(FileName, FileMode.Open,
											   FileAccess.Read);
				sr = new StreamReader(fs);
				FileContents = sr.ReadToEnd();
			}
			finally
			{
				if (sr != null)
					sr.Close();
			}

			return FileContents;
		}

		private string FormattedXml3entries
		{
			get
			{
				return
					"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<lift\r\n\tproducer=\"WeSay.1Pt0Alpha\"\r\n\tversion=\"0.13\">\r\n\t<entry\r\n\t\tdateCreated=\"2009-10-07T04:07:54Z\"\r\n\t\tdateModified=\"2009-10-07T04:10:34Z\"\r\n\t\tguid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef7\"\r\n\t\tid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef7\">\r\n\t\t<lexical-unit>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"nan\">\r\n\t\t\t\t<text>test</text>\r\n\t\t\t</form>\r\n\t\t</lexical-unit>\r\n\t\t<sense\r\n\t\t\tid=\"e6f93a8d-7883-4c1c-877e-e34db9f06cdc\">\r\n\t\t\t<definition>\r\n\t\t\t\t<form\r\n\t\t\t\t\tlang=\"en\">\r\n\t\t\t\t\t<text>difficult; slow</text>\r\n\t\t\t\t</form>\r\n\t\t\t</definition>\r\n\t\t</sense>\r\n\t</entry>\r\n\t<entry\r\n\t\tdateCreated=\"2009-10-07T04:07:54Z\"\r\n\t\tdateModified=\"2009-10-07T04:10:34Z\"\r\n\t\tguid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef8\"\r\n\t\tid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef8\">\r\n\t\t<lexical-unit>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"nan\">\r\n\t\t\t\t<text>test</text>\r\n\t\t\t</form>\r\n\t\t</lexical-unit>\r\n\t\t<sense\r\n\t\t\tid=\"e6f93a8d-7883-4c1c-877e-e34db9f06cdd\">\r\n\t\t\t<definition>\r\n\t\t\t\t<form\r\n\t\t\t\t\tlang=\"en\">\r\n\t\t\t\t\t<text>difficult; slow</text>\r\n\t\t\t\t</form>\r\n\t\t\t</definition>\r\n\t\t</sense>\r\n\t</entry>\r\n\t<entry\r\n\t\tdateCreated=\"2010-02-04T03:58:33Z\"\r\n\t\tdateModified=\"2010-02-04T03:58:40Z\"\r\n\t\tguid=\"92f0c58a-9ad6-4715-ad9b-d56597217ac3\"\r\n\t\tid=\"Xtest3_92f0c58a-9ad6-4715-ad9b-d56597217ac3\">\r\n\t\t<lexical-unit>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"v\">\r\n\t\t\t\t<text>Xtest3</text>\r\n\t\t\t</form>\r\n\t\t</lexical-unit>\r\n\t\t<sense\r\n\t\t\tid=\"4bc90f1e-1d4a-418f-a6e0-a4191b3e04f3\">\r\n\t\t\t<definition>\r\n\t\t\t\t<form\r\n\t\t\t\t\tlang=\"en\">\r\n\t\t\t\t\t<text>test3</text>\r\n\t\t\t\t</form>\r\n\t\t\t</definition>\r\n\t\t</sense>\r\n\t</entry>\r\n</lift>";
			}
		}

		private string FormattedXml2entries
		{
			get
			{
				return
					"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<lift\r\n\tproducer=\"WeSay.1Pt0Alpha\"\r\n\tversion=\"0.13\">\r\n\t<entry\r\n\t\tdateCreated=\"2009-10-07T04:07:54Z\"\r\n\t\tdateModified=\"2009-10-07T04:10:34Z\"\r\n\t\tguid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef7\"\r\n\t\tid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef7\">\r\n\t\t<lexical-unit>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"nan\">\r\n\t\t\t\t<text>test</text>\r\n\t\t\t</form>\r\n\t\t</lexical-unit>\r\n\t\t<sense\r\n\t\t\tid=\"e6f93a8d-7883-4c1c-877e-e34db9f06cdc\">\r\n\t\t\t<definition>\r\n\t\t\t\t<form\r\n\t\t\t\t\tlang=\"en\">\r\n\t\t\t\t\t<text>difficult; slow</text>\r\n\t\t\t\t</form>\r\n\t\t\t</definition>\r\n\t\t</sense>\r\n\t</entry>\r\n\t<entry\r\n\t\tdateCreated=\"2010-02-04T03:58:33Z\"\r\n\t\tdateModified=\"2010-02-04T03:58:40Z\"\r\n\t\tguid=\"92f0c58a-9ad6-4715-ad9b-d56597217ac3\"\r\n\t\tid=\"Xtest3_92f0c58a-9ad6-4715-ad9b-d56597217ac3\">\r\n\t\t<lexical-unit>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"v\">\r\n\t\t\t\t<text>Xtest3</text>\r\n\t\t\t</form>\r\n\t\t</lexical-unit>\r\n\t\t<sense\r\n\t\t\tid=\"4bc90f1e-1d4a-418f-a6e0-a4191b3e04f3\">\r\n\t\t\t<definition>\r\n\t\t\t\t<form\r\n\t\t\t\t\tlang=\"en\">\r\n\t\t\t\t\t<text>test3</text>\r\n\t\t\t\t</form>\r\n\t\t\t</definition>\r\n\t\t</sense>\r\n\t</entry>\r\n</lift>";
			}
		}

		private string FormattedXml2EntriesWithHeader
		{
			get
			{
				return
					"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<lift\r\n\tproducer=\"WeSay.1Pt0Alpha\"\r\n\tversion=\"0.13\">\r\n\t<header>\r\n\t\t<description>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"Nan\">\r\n\t\t\t\t<text>This Text Might Wreck Formatting</text>\r\n\t\t\t</form>\r\n\t\t</description>\r\n\t</header>\r\n\t<entry\r\n\t\tid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef7\"\r\n\t\tdateCreated=\"2009-10-07T04:07:54Z\"\r\n\t\tdateModified=\"2009-10-07T04:10:34Z\"\r\n\t\tguid=\"03d4b59a-9673-4f16-964e-4ea9f0636ef7\">\r\n\t\t<lexical-unit>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"nan\">\r\n\t\t\t\t<text>test</text>\r\n\t\t\t</form>\r\n\t\t</lexical-unit>\r\n\t\t<sense\r\n\t\t\tid=\"e6f93a8d-7883-4c1c-877e-e34db9f06cdc\">\r\n\t\t\t<definition>\r\n\t\t\t\t<form\r\n\t\t\t\t\tlang=\"en\">\r\n\t\t\t\t\t<text>difficult; slow</text>\r\n\t\t\t\t</form>\r\n\t\t\t</definition>\r\n\t\t</sense>\r\n\t</entry>\r\n\t<entry\r\n\t\tid=\"Xtest3_92f0c58a-9ad6-4715-ad9b-d56597217ac3\"\r\n\t\tdateCreated=\"2010-02-04T03:58:33Z\"\r\n\t\tdateModified=\"2010-02-04T03:58:40Z\"\r\n\t\tguid=\"92f0c58a-9ad6-4715-ad9b-d56597217ac3\">\r\n\t\t<lexical-unit>\r\n\t\t\t<form\r\n\t\t\t\tlang=\"v\">\r\n\t\t\t\t<text>Xtest3</text>\r\n\t\t\t</form>\r\n\t\t</lexical-unit>\r\n\t\t<sense\r\n\t\t\tid=\"4bc90f1e-1d4a-418f-a6e0-a4191b3e04f3\">\r\n\t\t\t<definition>\r\n\t\t\t\t<form\r\n\t\t\t\t\tlang=\"en\">\r\n\t\t\t\t\t<text>test3</text>\r\n\t\t\t\t</form>\r\n\t\t\t</definition>\r\n\t\t</sense>\r\n\t</entry>\r\n</lift>";
			}
		}

		private void WriteLongSingleLineBaseFileWithWhiteSpaceAfterEntryOpenElement()
		{
			WriteFile(_baseLiftFileName,
@"<entry id=""03d4b59a-9673-4f16-964e-4ea9f0636ef7"" dateCreated=""2009-10-07T04:07:54Z"" dateModified=""2009-10-07T04:10:34Z"" guid=""03d4b59a-9673-4f16-964e-4ea9f0636ef7"">
<lexical-unit><form lang=""nan""><text>test</text></form></lexical-unit><sense id=""e6f93a8d-7883-4c1c-877e-e34db9f06cdc""><definition><form lang=""en""><text>difficult; slow</text></form></definition></sense></entry>", _directory);
		}

		private void WriteLongSingleLineBaseFileWithWhiteSpaceMarkerAsSiblingOfEntryElements()
		{
			WriteFile(_baseLiftFileName,
@"<entry id=""03d4b59a-9673-4f16-964e-4ea9f0636ef7"" dateCreated=""2009-10-07T04:07:54Z"" dateModified=""2009-10-07T04:10:34Z"" guid=""03d4b59a-9673-4f16-964e-4ea9f0636ef7""><lexical-unit><form lang=""nan""><text>test</text></form></lexical-unit><sense id=""e6f93a8d-7883-4c1c-877e-e34db9f06cdc""><definition><form lang=""en""><text>difficult; slow</text></form></definition></sense></entry>
<entry id=""03d4b59a-9673-4f16-964e-4ea9f0636ef8"" dateCreated=""2009-10-07T04:07:54Z"" dateModified=""2009-10-07T04:10:34Z"" guid=""03d4b59a-9673-4f16-964e-4ea9f0636ef8""><lexical-unit><form lang=""nan""><text>test</text></form></lexical-unit><sense id=""e6f93a8d-7883-4c1c-877e-e34db9f06cdd""><definition><form lang=""en""><text>difficult; slow</text></form></definition></sense></entry>", _directory);
		}

		[Category("Fails on Linux because of New Lines as well as xml namespaces")]
		[Test]
		public void FormattedFragmentsWithWhiteSpaceAfterOpeningEntryelement_Merge_FormattedCorrectly()
		{
			WriteLongSingleLineBaseFileWithWhiteSpaceAfterEntryOpenElement();
			WriteLongSingleLineFragment();
			Merge(_directory);
			Validator.CheckLiftWithPossibleThrow(Path.Combine(_directory, _baseLiftFileName));
			Assert.AreEqual(FormattedXml2entries, ReadFileContentIntoString(Path.Combine(_directory, _baseLiftFileName)));
		}

		[Category("Fails on Linux because of New Lines as well as xml namespaces")]
		[Test]
		public void FormattedFragmentsWithWhiteSpaceAsSiblingOfToEntryElements_Merge_FormattedCorrectly()
		{
			WriteLongSingleLineBaseFileWithWhiteSpaceMarkerAsSiblingOfEntryElements();
			WriteLongSingleLineFragment();
			Merge(_directory);
			Validator.CheckLiftWithPossibleThrow(Path.Combine(_directory, _baseLiftFileName));
			Assert.AreEqual(FormattedXml3entries, ReadFileContentIntoString(Path.Combine(_directory, _baseLiftFileName)));
		}

		[Test]
		public void EdittedEntry_Updated()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(2, doc.SelectNodes("//entry").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hello']").Count);
		}

		[Test]
		public void MergeAndGetResult_EditedEntryWhereBothOldAndNewHaveEscapedIllegalCharacter_ThrowsBadUpdateFileException()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1'>&#x1F;Foo</entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1'>&#x1F;Bar</entry>", _directory);
			Assert.Throws<BadUpdateFileException>(() => MergeAndGetResult(true, _directory));
		}

		[Test]
		public void EdittedEntry_GuidSameIdDifferent_Updated()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='one1' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(2, doc.SelectNodes("//entry").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one1']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one1' and @greeting='hello']").Count);
		}

		[Test, Ignore("WS-236")]
		public void EdittedEntry_IdSameGuidDifferent_Updated()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd2' greeting='hello'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(3, doc.SelectNodes("//entry").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hi']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@greeting='hello']").Count);
		}

		[Test]
		public void MergeUpdatesIntoFile_BaseHasEntryWithoutGuid_ThrowsBadUpdateFileException()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' greeting='hi'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd2' greeting='hello'></entry>", _directory);

			Assert.Throws<BadUpdateFileException>(() => Merge(_directory));
		}


		[Test]
		public void ExistingBackup_Ok()
		{
			File.CreateText(Path.Combine(_directory, _baseLiftFileName + ".bak")).Dispose();
			WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(_directory, GetNextUpdateFileName());
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hello']").Count);
		}

		[Test]
		// This hasn't been running on the win7 agents because there was no other drive. Once it started trying to run
		// on the win10 agents, where there are other drives, it started failing. So, we're punting for now.
		[Category("SkipOnTeamCity")]
		public void WorksWithTempDirectoryOnADifferentVolumne()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				//Ignored on non-Windows
				return;
			}

			//testing approach: it's harder to get the temp locaiton changed, so we
			// instead put the destination project over on the non-default volume
			DriveInfo[] drives = DriveInfo.GetDrives();

			// get a drive I might be able to use
			string driveName = string.Empty;
			foreach (DriveInfo drive in drives)
			{
				if (drive.IsReady &&
					drive.DriveType != DriveType.CDRom &&
					drive.Name != "C:\\")
				{
					driveName = drive.Name;
					break;
				}
			}
			if (driveName.Length == 0)
			{
				Console.WriteLine("Ignored when there is not an additional volume");
			}
			else
			{
				string directory;
				do
				{
					directory = Path.Combine(driveName, Path.GetRandomFileName());
				} while (Directory.Exists(directory));

				Directory.CreateDirectory(directory);
				File.CreateText(Path.Combine(directory, _baseLiftFileName + ".bak")).Dispose();
				WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(directory, GetNextUpdateFileName());
				XmlDocument doc = MergeAndGetResult(true, directory);
				Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hello']").Count);
				Directory.Delete(directory, true);
			}
		}


		[Test]
		public void ReadOnlyBaseFile_DoesNothing()
		{
			string baseFilePath = Path.Combine(this._directory, _baseLiftFileName);
			try
			{
				WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(_directory, GetNextUpdateFileName());
				File.SetAttributes(baseFilePath, FileAttributes.ReadOnly);

				Merge(_directory);
			}
			finally
			{
				File.SetAttributes(baseFilePath, FileAttributes.Normal);
			}

			XmlDocument doc = GetResult(_directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hi']").Count);
			ExpectFileCount(2, _directory);
		}

		[Test]
		public void MergeUpdatesIntoFile_ReadOnlyUpdate_DoesNothing()
		{
			string updateFilePath = Path.Combine(this._directory, GetNextUpdateFileName());
			try
			{
				WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(_directory, updateFilePath);
				File.SetAttributes(updateFilePath, FileAttributes.ReadOnly);

				Merge(_directory);
			}
			finally
			{
				File.SetAttributes(updateFilePath, FileAttributes.Normal);
			}

			XmlDocument doc = GetResult(_directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hi']").Count);
			ExpectFileCount(2, _directory);
		}

		[Test]
		public void MergeUpdatesIntoFile_LockedBaseFile_ThrowsIOException()
		{
			Exception ex = null;
			string baseFilePath = Path.Combine(this._directory, _baseLiftFileName);
			WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(_directory, GetNextUpdateFileName());
			using(File.Open(baseFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
				try
				{
					Merge(_directory);
				}
				catch (Exception err)
				{
					ex = err;
				}
			}

			XmlDocument doc = GetResult(_directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hi']").Count);
			ExpectFileCount(2, _directory);

			Assert.That(ex is IOException);
		}

		[Test]
		public void MergeUpdatesIntoFile_LockedUpdate_ThrowsIOException()
		{
			Exception ex = null;
			string updateFilePath = Path.Combine(_directory, GetNextUpdateFileName());

			WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(_directory, updateFilePath);
			using (File.Open(updateFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
				try
				{
					Merge(_directory);
				}
				catch(Exception err)
				{
					ex = err; // Rethrow it later, after the end code has been run.
				}
			}

			var doc = GetResult(_directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hi']").Count);
			ExpectFileCount(2, _directory);

			Assert.That(ex is IOException);
		}

		[Test]
		public void ReadOnlyBackupFile_StillMakesBackup()
		{
			string backupFilePath = Path.Combine(_directory, _baseLiftFileName + ".bak");
			File.CreateText(backupFilePath).Dispose();

			WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(_directory, GetNextUpdateFileName());
			File.SetAttributes(backupFilePath, FileAttributes.ReadOnly);
			Merge(_directory);
			XmlDocument doc = GetResult(_directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hello']").Count);
			ExpectFileCount(3, _directory); //lift, readonly bak and new bak2
			File.SetAttributes(backupFilePath, FileAttributes.Normal);
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Locked files can still be moved on Linux")]
		public void LockedBackupFile_StillMakesBackup()
		{
			string backupFilePath = Path.Combine(_directory, _baseLiftFileName + ".bak");
			File.CreateText(backupFilePath).Dispose();

			WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(_directory, GetNextUpdateFileName());
			using (File.Open(backupFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
				Merge(_directory);
			}
			XmlDocument doc = GetResult(_directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hello']").Count);
			ExpectFileCount(3, _directory); //lift, locked bak and new unlocked bak2
		}

		static private void WriteBaseAndUpdateFilesSoMergedWillHaveHelloInsteadOfHi(string directory, string updateFile)
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>", directory);
			WriteFile(updateFile, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'></entry>", directory);
		}

		/// <summary>
		/// This is a regression test... had <lift> and the lack of a closing
		/// </lift> meant that the new item was never added
		/// </summary>
		[Test]
		public void AddingToEmptyLift()
		{
			WriteEmptyLift();
			WriteFile(GetNextUpdateFileName(), "<entry id='one' greeting='hello' guid='da6391b4-c61b-4b19-bb40-9fefa5de4a61'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(1, doc.SelectNodes("//lift[@preserveMe='foo']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hello']").Count);
		}

		private void WriteEmptyLift()
		{
			using (StreamWriter writer = File.CreateText(Path.Combine(_directory, _baseLiftFileName)))
			{
				string content = "<?xml version=\"1.0\" encoding=\"utf-8\"?><lift preserveMe='foo'/>";
				writer.Write(content);
				writer.Close();
			}
		}

		[Test]
		public void MergeUpdatesIntoFile_AddingEntryToEmptyLiftWithIllegalUnicode_ThrowsLiftFormatException()
		{
			using (StreamWriter writer = File.CreateText(Path.Combine(_directory, _baseLiftFileName)))
			{
				string content = "<?xml version=\"1.0\" encoding=\"utf-8\"?><lift/>";
				writer.Write(content);
				writer.Close();
			}
			WriteFile(GetNextUpdateFileName(), @"
				<entry id='one' guid='15489592-8939-4f52-a43f-4d1b4c12517d'>
					<lexical-unit>
						  <form lang='bth'>
							<text>&#x1F;</text>
						 </form>
					</lexical-unit>
				</entry>", _directory);
			Assert.Throws<LiftFormatException>(() => MergeAndGetResult(true, _directory));
		}

		[Test]
		public void EditOneAddOne()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='three' guid='4c1aace3-eefb-4090-88a4-0486cd932387'></entry><entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='two']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='three']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one' and @greeting='hello']").Count);
			Assert.AreEqual(3, doc.SelectNodes("//entry").Count);
		}

		[Test]
		public void ThreeFiles()
		{
			WriteFile(_baseLiftFileName, "<entry id='one' guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hi'></entry><entry id='two' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22' greeting='hi'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='three' guid='0ae89610-fc01-4bfd-a0d6-1125b7281d33'></entry><entry id='one'  guid='0ae89610-fc01-4bfd-a0d6-1125b7281dd1' greeting='hello'></entry>", _directory);
			WriteFile(GetNextUpdateFileName(), "<entry id='two'  guid='0ae89610-fc01-4bfd-a0d6-1125b7281d22' greeting='hello'></entry><entry id='four' guid='8d214346-7937-40e6-a34d-4f9360674f62'></entry>", _directory);
			XmlDocument doc = MergeAndGetResult(true, _directory);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='one']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='two']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='three']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='four']").Count);
			Assert.AreEqual(1, doc.SelectNodes("//entry[@id='two' and @greeting='hello']").Count);
			Assert.AreEqual(4, doc.SelectNodes("//entry").Count);
		}

		private XmlDocument MergeAndGetResult(bool isBackupFileExpected, string directory)
		{
			Merge(directory);
			ExpectFileCount(isBackupFileExpected?2:1, directory);

			return GetResult(directory);
		}

		private XmlDocument MergeAndGetResult(bool isBackupFileExpected, string directory, FileInfo[] files)
		{
			Merge(directory, files);
			ExpectFileCount(isBackupFileExpected ? 2 : 1, directory);

			return GetResult(directory);
		}

		private static XmlDocument GetResult(string directory) {
			XmlDocument doc = new XmlDocument();
			string outputPath = Path.Combine(directory,_baseLiftFileName);
			doc.Load(outputPath);
			Console.WriteLine(File.ReadAllText(outputPath));
			return doc;
		}

		private void Merge(string directory) {
			_merger.MergeUpdatesIntoFile(Path.Combine(directory, _baseLiftFileName));
		}

		private void Merge(string directory, FileInfo[] files)
		{
			_merger.MergeUpdatesIntoFile(Path.Combine(directory, _baseLiftFileName), files);
		}

		private static void ExpectFileCount(int count, string directory)
		{
			string[] files = Directory.GetFiles(directory);

			StringBuilder fileList = new StringBuilder();
			foreach (string s in files)
			{
				fileList.Append(s);
				fileList.Append('\n');
			}
			Assert.AreEqual(count, files.Length, fileList.ToString());
		}

		private static string WriteFile(string fileName, string xmlForEntries, string directory)
		{
			StreamWriter writer = File.CreateText(Path.Combine(directory, fileName));
			string content = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
							 +"<lift version =\""
							 + Validator.LiftVersion
							 +"\" producer=\"WeSay.1Pt0Alpha\">"
							 +xmlForEntries
							 +"</lift>";
			writer.Write(content);
			writer.Close();
			writer.Dispose();

			//pause so they don't all have the same time
			Thread.Sleep(100);

			return content;
		}
	}
}