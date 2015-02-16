using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.IO;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	
	[TestFixture]
	public class SilLdmlVersionTests
	{
		[Test]
		public void GetFileVersion_WithVersionAttribute_LatestVersion()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
</ldml>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(WritingSystemDefinition.LatestWritingSystemDefinitionVersion));
			}
		}

		[Test]
		public void GetFileVersion_WithoutLdml_ReturnsBadVersion()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<something>
</something>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(SilLdmlVersion.BadVersion));
			}
		}

		[Test]
		public void GetFileVersion_WithInvalidVerisonAttribute_ReturnsBadVersion()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<ldml xmlns:sil='urn://www.invalid.uri'>
</ldml>
".Replace("'", "\"");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(SilLdmlVersion.BadVersion));
			}
		}

		[Test]
		public void GetFileVersion_NoVersion_ReturnsBadVersion()
		{
			string xml = LdmlContentForTests.VersionInvalid;

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(SilLdmlVersion.BadVersion));
			}
		}
	}
}
