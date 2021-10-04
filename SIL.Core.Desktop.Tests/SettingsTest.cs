using NUnit.Framework;

namespace SIL.Tests
{
	[TestFixture]
	public class SettingsTest
	{
		[Test]
		public void SettingsUseCrossPlatformSettingsProvider()
		{
			TestUtilities.TestUtilities.ValidateProperties(Properties.Settings.Default);
		}
	}
}