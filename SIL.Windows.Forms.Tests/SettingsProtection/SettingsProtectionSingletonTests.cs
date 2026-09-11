using NUnit.Framework;
using SIL.Windows.Forms.SettingProtection;

namespace SIL.Windows.Forms.Tests.SettingsProtection
{
	[TestFixture]
	public class SettingsProtectionSingletonTests
	{
		[Test]
		public void FactoryPassword_NoCoreProductNameAppSetting_FallsBackToApplicationProductNameBasedPassword()
		{
			// The test project's App.config has no "CoreProductName" appSetting, so this
			// exercises the ConfigurationManager.AppSettings fallback path.
			var expected = System.Windows.Forms.Application.ProductName.Insert(1, "7").ToLower();

			Assert.That(SettingsProtectionSingleton.FactoryPassword, Is.EqualTo(expected));
		}
	}
}
