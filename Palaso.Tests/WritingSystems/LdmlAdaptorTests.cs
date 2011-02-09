using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.IO;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.WritingSystems;

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
			_ws = new WritingSystemDefinition("en", "Latn", "US", string.Empty, "English", "eng", false);
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
			StringWriter sw = new StringWriter();
			WritingSystemDefinition ws = new WritingSystemDefinition("xxx");
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			settings.Indent = false;
			settings.NewLineChars = string.Empty;
			settings.OmitXmlDeclaration = true;
			XmlWriter writer = XmlWriter.Create(sw, settings);
			_adaptor.Write(writer, ws, XmlReader.Create(new StringReader("<ldml><!--Comment--><dates/><special>hey</special></ldml>")));
			writer.Close();
			string s = "<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"xxx\" /></identity><dates /><collations /><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" /><special>hey</special></ldml>";
			Assert.AreEqual(s, sw.ToString());
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
		public void Read_LdmlContainsWellDefinedFaultyIsoThatDescribesAudioWritingSystem_RFC5646FieldsAreCorrected()
		{
			string ldml = "<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"tpi-Zxxx-x-audio\" /></identity><dates /><collations /><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" /><special></special></ldml>";
			string pathToLdmlFile = Path.GetTempFileName();
			File.WriteAllText(pathToLdmlFile, ldml);

			WritingSystemDefinition ws = new WritingSystemDefinition();
			LdmlAdaptor adaptor = new LdmlAdaptor();
			adaptor.Read(pathToLdmlFile,ws);
			Assert.AreEqual("tpi", ws.ISO);
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.AreEqual(WellKnownSubTags.Audio.PrivateUseSubtag, ws.Variant);
		}

		[Test]
		public void Read_LdmlContainsWellDefinedFaultyIsoThatDescribesAudioWritingSystem_OldRfcTagFieldIsSetCorrectly()
		{
			string ldml = "<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"lwl-east\" /><script type=\"Script\" /><territory type=\"overtherainbow\" /><variant type=\"x-audio\" /></identity><dates /><collations /><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" /><special></special></ldml>";
			string pathToLdmlFile = Path.GetTempFileName();
			File.WriteAllText(pathToLdmlFile, ldml);

			WritingSystemDefinition ws = new WritingSystemDefinition();
			LdmlAdaptor adaptor = new LdmlAdaptor();
			adaptor.Read(pathToLdmlFile, ws);
			Assert.AreEqual("lwl-Zxxx-overtherainbow-x-audio", ws.RFC5646);
			Assert.AreEqual("lwl-east", ws.Rfc5646TagOnLoad.Language);
			Assert.AreEqual("Script", ws.Rfc5646TagOnLoad.Script);
			Assert.AreEqual("overtherainbow", ws.Rfc5646TagOnLoad.Region);
			Assert.AreEqual(WellKnownSubTags.Audio.PrivateUseSubtag, ws.Rfc5646TagOnLoad.Variant);
		}
	}
}
