using NUnit.Framework;

namespace SIL.Archiving.Tests
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
