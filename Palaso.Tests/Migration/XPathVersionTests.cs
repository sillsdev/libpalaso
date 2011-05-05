using Palaso.IO;
using Palaso.Migration;
using NUnit.Framework;

namespace Palaso.Tests.Migration
{
	[TestFixture]
	public class XPathVersionTests
	{
		[Test]
		public void GetFileVersion_WithVersionAttribute_CorrectVersion()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration version='3'>
  <blah />
</configuration>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var xPathVersion = new XPathVersion(10, "/configuration/@version");
				int result = xPathVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(3));
			}
		}

		[Test]
		public void GetFileVersion_WithNameSpace_CorrectVersion()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
	<palaso:version	value='3' />
</special>
</ldml>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var xPathVersion = new XPathVersion(10, "/ldml/special/palaso:version/@value");
				xPathVersion.NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
				int result = xPathVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(3));
			}
		}

		[Test]
		public void GetFileVersion_NoVersion_ReturnsMinus1()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration noversion='x'>
  <blah />
</configuration>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var xPathVersion = new XPathVersion(10, "/configuration/@version");
				int result = xPathVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(-1));
			}
		}

	}
}
