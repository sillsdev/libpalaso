using NUnit.Framework;
using SIL.Migration;

namespace SIL.Tests.Migration
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
