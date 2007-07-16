using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Palaso.Tests
{
	[TestFixture]
	public class SFMReaderTest
	{
		[Test]
		public void ReadNextText_SingleTag()
		{
			Stream stream = new FileStream(@"..\..\data\singleTag.txt", FileMode.Open);
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);

			token = test.ReadNextText();
			Assert.AreEqual("text and more text onto the next line", token);
		}

		[Test]
		public void ReadNextText_MultilineAndMultipleTags()
		{
			Stream stream = new FileStream(@"..\..\data\multilineMultiTag.txt", FileMode.Open);
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);

			token = test.ReadNextText();
			Assert.AreEqual("Through fire and flames we ", token);

			token = test.ReadNextTag();
			Assert.AreEqual("bf", token);

			token = test.ReadNextText();
			Assert.AreEqual("carry on. ", token);

			token = test.ReadNextTag();
			Assert.AreEqual("sp*", token);

			token = test.ReadNextText();
			Assert.AreEqual("once we traveled through many", token);

			token = test.ReadNextTag();
			Assert.AreEqual("ex", token);

			token = test.ReadNextText();
			Assert.AreEqual("lands and over many seas", token);
		}

		[Test]
		public void ReadNextText_MultipleTags()
		{
			Stream stream = new FileStream(@"..\..\data\multipleTags.txt", FileMode.Open);
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);

			token = test.ReadNextText();
			Assert.AreEqual("two words ", token);

			token = test.ReadNextTag();
			Assert.AreEqual("tag", token);

			token = test.ReadNextText();
			Assert.AreEqual("more words", token);
		}
	}
}
