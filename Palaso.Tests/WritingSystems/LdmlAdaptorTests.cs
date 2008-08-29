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

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromFile_NullFileName_Throws()
		{
			_adaptor.Read((string)null, _ws);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromFile_NullWritingSystem_Throws()
		{
			_adaptor.Read("foo.ldml", null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromXmlReader_NullXmlReader_Throws()
		{
			_adaptor.Read((XmlReader)null, _ws);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromXmlReader_NullWritingSystem_Throws()
		{
			_adaptor.Read(XmlReader.Create(new StringReader("<ldml/>")), null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void WriteToFile_NullFileName_Throws()
		{
			_adaptor.Write((string)null, _ws);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void WriteToFile_NullWritingSystem_Throws()
		{
			_adaptor.Write("foo.ldml", null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void WriteToXmlWriter_NullXmlReader_Throws()
		{
			_adaptor.Write((XmlWriter)null, _ws);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void WriteToXmlWriter_NullWritingSystem_Throws()
		{
			_adaptor.Write(XmlWriter.Create(new MemoryStream()), null);
		}
	}
}
