using NUnit.Framework;
using Palaso.Migration;

namespace Palaso.Tests.Migration
{
	[TestFixture]
	public class DefaultVersionTests
	{
		[Test]
		public void GetFileVersion_ReturnsDefault()
		{
			var versionStrategy = new DefaultVersion(4, 4);
			Assert.AreEqual(4, versionStrategy.GetFileVersion(""));
		}
	}
}
