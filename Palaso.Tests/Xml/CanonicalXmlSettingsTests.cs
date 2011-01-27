using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.Xml;

namespace Palaso.Tests.Xml
{
	[TestFixture]
	public class CanonicalXmlSettingsTests
	{
		[Test]
		public void WriteReadWithBuilder_WithCanonicalXmlWriterSettings_MatchesExpected()
		{
			string xmlInput = @"<a attrib1='value1' attrib2='value2'><b>Content</b></a>".Replace('\'', '"');
			const string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<a\r\n\tattrib1=\"value1\"\r\n\tattrib2=\"value2\">\r\n\t<b>Content</b>\r\n</a>";
			// Note that a StringBuilder forces the stream to be 16 bit, so the output is force to utf-16.
			var builder = new StringBuilder();
			using (var reader = XmlReader.Create(new StringReader(xmlInput)))
			{
				using (var writer = XmlWriter.Create(builder, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteNode(reader, false);
				}
			}
			Assert.AreEqual(expected, builder.ToString());
		}

		[Test]
		public void WriteReadWithFile_WithCanonicalXmlWriterSettings_MatchesExpected()
		{
			string xmlInput = @"<a attrib1='value1' attrib2='value2'><b>Content</b></a>".Replace('\'', '"');
			const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<a\r\n\tattrib1=\"value1\"\r\n\tattrib2=\"value2\">\r\n\t<b>Content</b>\r\n</a>";
			using (var tempFile = TestUtilities.TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (var reader = XmlReader.Create(new StringReader(xmlInput)))
				{
					using (var writer = XmlWriter.Create(tempFile.Path, CanonicalXmlSettings.CreateXmlWriterSettings()))
					{
						writer.WriteNode(reader, false);
					}
				}
				string result = File.ReadAllText(tempFile.Path);
				Console.WriteLine(result);
				Assert.AreEqual(expected, result);
			}
		}
	}
}
