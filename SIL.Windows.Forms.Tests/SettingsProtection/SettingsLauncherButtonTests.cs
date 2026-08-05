using System;
using System.Reflection;
using NUnit.Framework;
using SIL.Windows.Forms.SettingProtection;

namespace SIL.Windows.Forms.Tests.SettingsProtection
{
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class SettingsLauncherButtonTests
	{
		[Test]
		public void Dispose_DisposesTheSettingsProtectionHelperItCreated()
		{
			var helperField = typeof(SettingsLauncherButton).GetField("_helper",
				BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.That(helperField, Is.Not.Null,
				"SettingsLauncherButton no longer has a _helper field; update this test");

			SettingsProtectionHelper helper;
			using (var button = new SettingsLauncherButton())
			{
				helper = (SettingsProtectionHelper)helperField.GetValue(button);
				Assert.That(helper, Is.Not.Null, "Precondition: the button creates a helper");
			}

			Assert.That(() => helper.CanExtend(new object()),
				Throws.InstanceOf<ObjectDisposedException>());
		}
	}
}
