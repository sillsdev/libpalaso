using NUnit.Framework;
using Palaso.IO;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	
	[TestFixture]
	public class SilLdmlVersionTests
	{
		[Test]
		public void GetFileVersion_WithVersionAttribute_CorrectVersion()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
</ldml>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(3));
			}
		}

		[Test]
		public void GetFileVersion_NoVersion_ReturnsMinus1()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<ldml>
</ldml>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(-1));
			}
		}
	}
}
