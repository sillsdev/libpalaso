using System;
using System.IO;
using Palaso.DictionaryServices.Lift;
using Palaso.Lift;
using Palaso.Lift.Migration;
using Palaso.Lift.Validation;
using Palaso.Progress;
using Palaso.Reporting;

using NUnit.Framework;

namespace Palaso.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class LiftPreparerTests
	{
		private string _liftFilePath;

		[SetUp]
		public void Setup()
		{
			_liftFilePath = Path.GetTempFileName();
			_liftFilePath = _liftFilePath.Replace(".tmp", ".lift");
			ErrorReport.IsOkToInteractWithUser = false;
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_liftFilePath);
		}

		private void CreateLiftFileForTesting(string liftVersion, string xmlEntries)
		{
			Utilities.CreateEmptyLiftFile(_liftFilePath, LiftWriter.ProducerString, true);
			//overwrite the blank lift file
			string liftContents =
				String.Format(
					"<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>",
					liftVersion,
					xmlEntries);
			File.WriteAllText(_liftFilePath, liftContents);
		}

		private void CreateLiftFileForTesting(string liftVersion)
		{
			CreateLiftFileForTesting(liftVersion, String.Empty);
		}

		[Test]
		public void MigrateIfNeeded_GivenLiftVersionPoint10_LiftFileHasCurrentLiftVersionNumber()
		{
			//nb: most migration testing is done in the LiftIO library where the actual
			//  migration happens.  So here we're ensuring that the migration mechanism was
			//  triggered, and that the process left us with a modified (but not renamed)
			//  lift file.
			//nb: 0.10 was the first version where we started provinding a migration path.
			//FLEx support for Lift started with 0.12
			CreateLiftFileForTesting("0.10");
			LiftPreparer preparer = new LiftPreparer(_liftFilePath);
			Assert.IsTrue(preparer.IsMigrationNeeded(), "IsMigrationNeeded Failed");
			preparer.MigrateLiftFile(new ProgressState());
			Assert.AreEqual(Validator.LiftVersion, Validator.GetLiftVersion(_liftFilePath));
		}

		// TODO Move these tests to LiftDataMapperTests
		[Test]
		public void MigrateIfNeeded_AlreadyCurrentLift_LiftUntouched()
		{
			CreateLiftFileForTesting(Validator.LiftVersion);
			DateTime startModTime = File.GetLastWriteTimeUtc(_liftFilePath);
			LiftPreparer preparer = new LiftPreparer(_liftFilePath);
			Assert.IsFalse(preparer.IsMigrationNeeded(), "IsMigrationNeeded Failed");
			DateTime finishModTime = File.GetLastWriteTimeUtc(_liftFilePath);
			Assert.AreEqual(startModTime, finishModTime);
		}

#if notUsedSinceNov2008
		[Test]
		public void PopulateDefinitions_EmptyLift()
		{
			XmlDocument dom = PopulateDefinitionsInDom("");
			Expect(dom, "lift", 1);
		}

		[Test]
		public void PopulateDefinitions_GetsDefinitionWithConcatenatedGlosses()
		{
			string entriesXml =
					@"<entry id='foo1'>
						<sense>
							<gloss lang='en'>
								<text>one</text>
							</gloss>
							<gloss lang='en'>
								<text>two</text>
							</gloss>
						</sense>
					</entry>";
			XmlDocument dom = PopulateDefinitionsInDom(entriesXml);
			Expect(dom, "lift/entry/sense/gloss", 2);
			Expect(dom, "lift/entry/sense/definition", 1);
			ExpectSingleInstanceWithInnerXml(dom,
											 "lift/entry/sense/definition/form[@lang='en']/text",
											 "one; two");
		}

		[Test]
		public void PopulateDefinitions_MergesInWritingSystemsWithExistingDefinition()
		{
			string entriesXml =
					@"<entry id='foo1'>
						<sense>
							<definition>
								<form lang='a'>
									<text>a definition</text>
								</form>
								<form lang='b'>
									<text>b definition</text>
								</form>
							</definition>
							<gloss lang='b'>
								<text>SHOULD NOT SEE IN DEF</text>
							</gloss>
							<gloss lang='c'>
								<text>c gloss</text>
							</gloss>
						</sense>
					</entry>";
			XmlDocument dom = PopulateDefinitionsInDom(entriesXml);
			Expect(dom, "lift/entry/sense/gloss", 2);
			Expect(dom, "lift/entry/sense/definition", 1);
			ExpectSingleInstanceWithInnerXml(dom,
											 "lift/entry/sense/definition/form[@lang='a']/text",
											 "a definition");
			ExpectSingleInstanceWithInnerXml(dom,
											 "lift/entry/sense/definition/form[@lang='b']/text",
											 "b definition");
			ExpectSingleInstanceWithInnerXml(dom,
											 "lift/entry/sense/definition/form[@lang='c']/text",
											 "c gloss");
		}

		private static void Expect(XmlNode dom, string xpath, int expectedCount)
		{
			Assert.AreEqual(expectedCount, dom.SelectNodes(xpath).Count);
		}

		private static void ExpectSingleInstanceWithInnerXml(XmlNode dom,
															 string xpath,
															 string expectedValue)
		{
			Assert.AreEqual(1, dom.SelectNodes(xpath).Count);
			Assert.AreEqual(expectedValue, dom.SelectNodes(xpath)[0].InnerXml);
		}

		private XmlDocument PopulateDefinitionsInDom(string entriesXml)
		{
			XmlDocument doc = new XmlDocument();
			CreateLiftFileForTesting(Validator.LiftVersion, entriesXml);
			LiftPreparer preparer = new LiftPreparer(_liftFilePath);
			preparer.PopulateDefinitions(new ProgressState());
			Assert.IsTrue(File.Exists(_liftFilePath));
			doc.Load(_liftFilePath);
			return doc;
		}
#endif
	}
}