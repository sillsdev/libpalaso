using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using NUnit.Framework;
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

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromFile_NullFileName_Throws()
		{
			_adaptor.Read((string)null, _ws);
		}

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromFile_NullWritingSystem_Throws()
		{
			_adaptor.Read("foo.ldml", null);
		}

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromXmlReader_NullXmlReader_Throws()
		{
			_adaptor.Read((XmlReader)null, _ws);
		}

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromXmlReader_NullWritingSystem_Throws()
		{
			_adaptor.Read(XmlReader.Create(new StringReader("<ldml/>")), null);
		}

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void WriteToFile_NullFileName_Throws()
		{
			_adaptor.Write((string)null, _ws, null);
		}

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void WriteToFile_NullWritingSystem_Throws()
		{
			_adaptor.Write("foo.ldml", null, null);
		}

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void WriteToXmlWriter_NullXmlReader_Throws()
		{
			_adaptor.Write((XmlWriter)null, _ws, null);
		}

		[Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentNullException))]
		public void WriteToXmlWriter_NullWritingSystem_Throws()
		{
			_adaptor.Write(XmlWriter.Create(new MemoryStream()), null, null);
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
	}
}
