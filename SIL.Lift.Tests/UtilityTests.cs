using System.IO;
using NUnit.Framework;

namespace SIL.Lift.Tests
{
	[TestFixture]
	public class UtilityTests
	{
		[SetUp]
		public void Setup()
		{

		}

		[TearDown]
		public void TearDown()
		{

		}

//       even an empty only is getting cannonicalized [Test]
//        public void EmptyLiftUnchanged()
//        {
//            string input = Path.GetTempFileName();
//            Utilities.CreateEmptyLiftFile(input,"test",true);
//            string output = Utilities.ProcessLiftForLaterMerging(input);
//            Assert.AreEqual(File.ReadAllText(input), File.ReadAllText(output));
//        }

		[Test]
		public void ProcessLiftForLaterMerging_ExistingGuidsUnchanged()
		{
			using (TempFile f = TempFile.CreateWithXmlHeader("<entry guid='123abc'/>"))
			{
				string output = Utilities.ProcessLiftForLaterMerging(f.Path);
				XmlTestHelper.AssertXPathNotNull(output, "//entry[@guid='123abc']");
				File.Delete(output);
			}
		}

		[Test]
		public void ProcessLiftForLaterMerging_MissingGuidsAdded()
		{
			using (TempFile file =  TempFile.CreateWithXmlHeader("<entry id='one'/><entry id='two'/>"))
			{
				string output = Utilities.ProcessLiftForLaterMerging(file.Path);
				XmlTestHelper.AssertXPathNotNull(output, "//entry[@id='one' and @guid]");
				XmlTestHelper.AssertXPathNotNull(output, "//entry[@id='two' and @guid]");
				File.Delete(output);
			}
		}


		[Test]
		public void ProcessLiftForLaterMerging_MissingHumanReadableIdsAdded_NoGuid()
		{
			using (TempFile f =  TempFile.CreateWithXmlHeader("<entry><lexical-unit><form lang='v'><text>kindness</text></form></lexical-unit></entry>"))
			{
				string output = Utilities.ProcessLiftForLaterMerging(f.Path);
				XmlTestHelper.AssertXPathNotNull(output, "//entry[@id and @guid]");
				File.Delete(output);
			}
		}

		[Test]
		public void ProcessLiftForLaterMerging_MissingHumanReadableIdsAdded_AlreadyHadGuid()
		{
			using (TempFile f = TempFile.CreateWithXmlHeader("<entry guid='6b4269b9-f5d4-4e48-ad91-17109d9882e4'><lexical-unit ><form lang='v'><text>kindness</text></form></lexical-unit></entry>"))
			{
				string output = Utilities.ProcessLiftForLaterMerging(f.Path);
				XmlTestHelper.AssertXPathNotNull(output, "//entry[@id and @guid]");
				File.Delete(output);
			}
		}

		[Test]
		public void ProcessLiftForLaterMerging_NoIdAddedIf_NoLexemeFormToUse()
		{
			using (TempFile f = TempFile.CreateWithXmlHeader("<entry></entry>"))
			{
				string output = Utilities.ProcessLiftForLaterMerging(f.Path);
				XmlTestHelper.AssertXPathNotNull(output, "//entry[@guid and not(@id)]");
				File.Delete(output);
			}
		}

		[Test]
		public void ProcessLiftForLaterMerging_InnerContentsUntouched()
		{
			using (TempFile f = TempFile.CreateWithXmlHeader("<entry id='one'><sense id='foo'><example/></sense></entry>"))
			{
				string output = Utilities.ProcessLiftForLaterMerging(f.Path);
				XmlTestHelper.AssertXPathNotNull(output, "//entry/sense[@id='foo']/example");
				File.Delete(output);
			}
		}


		[Test, Ignore("Not Yet")]
		public void AreXmlElementsEqual_ElementClosing_Match()
		{
			string ours = @"<open></open>";

			string theirs = @"<open/>";
			Assert.IsTrue(Utilities.AreXmlElementsEqual(ours,theirs));
		}

		[Test, Ignore("Not Yet")]
		public void AreXmlElementsEqual_DifferentAttributeOrder_Match()
		{
			string ours = "<foo one='1' two='2'/>";

			string theirs =  "<foo two='2' one='1'/>";

			Assert.IsTrue(Utilities.AreXmlElementsEqual(ours, theirs));
		}
	}

}