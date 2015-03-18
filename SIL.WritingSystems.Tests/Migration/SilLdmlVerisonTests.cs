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
		public void GetFileVersion_StandardLdml_LatestVersion()
		{
			string xml = LdmlContentForTests.Version3("en", "Latn", "", "");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(LdmlDataMapper.CurrentLdmlVersion));
			}
		}

		[Test]
		public void GetFileVersion_SilIdentity_LatestVersion()
		{
			string xml = LdmlContentForTests.Version3Identity("en", "Latn", "", "", "123456", "abcd", "US", "LatestAndGreatest");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(LdmlDataMapper.CurrentLdmlVersion));
			}
		}

		[Test]
		public void GetFileVersion_V0_ReturnsBadVersion()
		{
			string xml = LdmlContentForTests.Version0("en", "", "", "");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(SilLdmlVersion.BadVersion));
			}
		}

		[Test]
		public void GetFileVersion_V1_ReturnsBadVersion()
		{
			string xml = LdmlContentForTests.Version1("en", "", "", "");

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(SilLdmlVersion.BadVersion));
			}
		}

		[Test]
		public void GetFileVersion_WithoutLdml_ReturnsBadVersion()
		{
			string xml = LdmlContentForTests.NoLdml;

			using (var file = new TempFile(xml))
			{
				var silLdmlVersion = new SilLdmlVersion();
				int result = silLdmlVersion.GetFileVersion(file.Path);
				Assert.That(result, Is.EqualTo(SilLdmlVersion.BadVersion));
			}
		}
	}
}
