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
				"<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations><collation></collation></collations><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"1\" /></special></ldml>";
			string ldmlwithNoCollationElement =
				"<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations/><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"1\" /></special></ldml>";

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


		[Test]
		public void Read_LdmlContainsOnlyPrivateUse_IsoAndprivateUseSetCorrectly()
		{
			string ldmlWithOnlyPrivateUse =
				"<ldml><identity><version number=\"\" /><language type=\"\" /><variant type=\"x-private-use\" /></identity><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"1\" /></special></ldml>";


			string pathToLdmlWithEmptyCollationElement = Path.GetTempFileName();
			File.WriteAllText(pathToLdmlWithEmptyCollationElement, ldmlWithOnlyPrivateUse);

			var adaptor = new LdmlAdaptor();
			var wsFromLdml = new WritingSystemDefinition();
			adaptor.Read(pathToLdmlWithEmptyCollationElement, wsFromLdml);
			var Ldml = new WritingSystemDefinition();
			adaptor.Read(pathToLdmlWithEmptyCollationElement, Ldml);
			Assert.That(wsFromLdml.ISO639, Is.EqualTo(String.Empty));
			Assert.That(wsFromLdml.Variant, Is.EqualTo("x-private-use"));
		}


		[Test]
		public void Write_LdmlIsNicelyFormatted()
		{
			string expectedFileContent =
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='0001-01-01T00:00:00' />
		<language
			type='en' />
		<script
			type='Zxxx' />
		<territory
			type='US' />
		<variant
			type='x-audio' />
	</identity>
	<collations />
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='en' />
		<palaso:languageName
			value='English' />
		<palaso:version
			value='1' />
	</special>
</ldml>".Replace("'", "\"");
#endregion
			using (var file = new TempFile())
			{
				var adaptor = new LdmlAdaptor();
				var ws = WritingSystemDefinition.Parse("en-Zxxx-x-audio");
				adaptor.Write(file.Path, ws, null);

				var ws2 = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws2);
				ws2.Region = "US";
				adaptor.Write(file.Path, ws2, new MemoryStream(File.ReadAllBytes(file.Path)));

				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(expectedFileContent));
			}
		}


	}
}
