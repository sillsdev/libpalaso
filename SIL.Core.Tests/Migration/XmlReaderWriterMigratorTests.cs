using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.IO;
using SIL.Migration;
using SIL.TestUtilities;

namespace SIL.Tests.Migration
{
	[TestFixture]
	public class XmlReaderWriterMigratorTests
	{

		class MigratorForTest : XmlReaderWriterMigrationStrategy
		{
			public MigratorForTest(int fromVersion, int toVersion) : base(fromVersion, toVersion)
			{
			}

			protected override void CopyNode(XmlReader reader, XmlWriter writer)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "configuration")
				{
					writer.WriteStartElement("configuration");
					writer.WriteAttributeString("version", "2");
					reader.Read();
				}
				else
				{
					writer.WriteNode(reader, true);
				}
			}
		}

		[Test]
		public void Constructor_FromLessThanTo_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => { _ = new MigratorForTest(5, 4); }
			);
		}

		[Test]
		public void Migrate_TestData_TransformsOk()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration version='1'>
  <blah />
</configuration>
".Replace("'", "\"");

			using (var sourceFile = new TempFile(xml))
			{
				using (var destinationFile = new TempFile())
				{
					var migrator = new MigratorForTest(1, 2);
					migrator.Migrate(sourceFile.Path, destinationFile.Path);
					string result = File.ReadAllText(destinationFile.Path);
					AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("configuration[@version='2']");
					AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("/configuration/blah");
				}
			}
		}

	}
}
