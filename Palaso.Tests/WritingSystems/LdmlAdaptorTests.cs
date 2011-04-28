using System;
using System.Xml;
using System.IO;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;
using Palaso.WritingSystems;
using Palaso.Xml;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class LdmlAdaptorTests
	{
		private LdmlAdaptor _adaptor;
		private WritingSystemDefinition _ws;

		[SetUp]
		public void SetUp()
		{
			_adaptor = new LdmlAdaptor();
			_ws = new WritingSystemDefinition("en", "Latn", "US", string.Empty, "eng", false);
		}

		[Test]
		public void ReadFromFile_NullFileName_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read((string)null, _ws)
			);
		}

		[Test]
		public void ReadFromFile_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read("foo.ldml", null)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullXmlReader_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read((XmlReader)null, _ws)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read(XmlReader.Create(new StringReader("<ldml/>")), null)
			);
		}

		[Test]
		public void WriteToFile_NullFileName_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write((string)null, _ws, null)
			);
		}

		[Test]
		public void WriteToFile_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write("foo.ldml", null, null)
			);
		}

		[Test]
		public void WriteToXmlWriter_NullXmlReader_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write((XmlWriter)null, _ws, null)
			);
		}

		[Test]
		public void WriteToXmlWriter_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write(XmlWriter.Create(new MemoryStream()), null, null)
			);
		}

		[Test]
		public void ExistingUnusedLdml_Write_PreservesData()
		{
			var sw = new StringWriter();
			var ws = new WritingSystemDefinition("en");
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			_adaptor.Write(writer, ws, XmlReader.Create(new StringReader("<ldml><!--Comment--><dates/><special>hey</special></ldml>")));
			writer.Close();
			AssertThatXmlIn.String(sw.ToString()).HasAtLeastOneMatchForXpath("/ldml/special[text()=\"hey\"]");
		}

		[Test]
		public void RoundtripSimpleCustomSortRules_WS33715()
		{
			LdmlAdaptor ldmlAdaptor = new LdmlAdaptor();

			string sortRules = "(A̍ a̍)";
			WritingSystemDefinition wsWithSimpleCustomSortRules = new WritingSystemDefinition();
			wsWithSimpleCustomSortRules.SortUsing = WritingSystemDefinition.SortRulesType.CustomSimple;
			wsWithSimpleCustomSortRules.SortRules = sortRules;

			WritingSystemDefinition wsFromLdml = new WritingSystemDefinition();
			using (TempFile tempFile = new TempFile())
			{
				ldmlAdaptor.Write(tempFile.Path, wsWithSimpleCustomSortRules, null);
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}

			Assert.AreEqual(sortRules, wsFromLdml.SortRules);
		}


		[Test]
		//WS-33992
		public void Read_LdmlContainsEmptyCollationElement_SortUsingIsSetToSameAsIfNoCollationElementExisted()
		{
			string ldmlWithEmptyCollationElement =
				"<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations><collation></collation></collations><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" /><special></special></ldml>";
			string ldmlwithNoCollationElement =
				"<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations/><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" /><special></special></ldml>";

			string pathToLdmlWithEmptyCollationElement = Path.GetTempFileName();
			File.WriteAllText(pathToLdmlWithEmptyCollationElement, ldmlWithEmptyCollationElement);
			string pathToLdmlWithNoCollationElement = Path.GetTempFileName();
			File.WriteAllText(pathToLdmlWithNoCollationElement, ldmlwithNoCollationElement);


			var adaptor = new LdmlAdaptor();
			var wsFromEmptyCollationElement = new WritingSystemDefinition();
			adaptor.Read(pathToLdmlWithEmptyCollationElement, wsFromEmptyCollationElement);
			var wsFromNoCollationElement = new WritingSystemDefinition();
			adaptor.Read(pathToLdmlWithNoCollationElement, wsFromNoCollationElement);

			Assert.AreEqual(wsFromNoCollationElement.SortUsing, wsFromEmptyCollationElement.SortUsing);
		}
	}
}
