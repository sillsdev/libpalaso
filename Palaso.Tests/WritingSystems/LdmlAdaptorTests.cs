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
			StringWriter sw = new StringWriter();
			WritingSystemDefinition ws = new WritingSystemDefinition("en");
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			settings.Indent = false;
			settings.NewLineChars = string.Empty;
			settings.OmitXmlDeclaration = true;
			XmlWriter writer = XmlWriter.Create(sw, settings);
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
	}
}
