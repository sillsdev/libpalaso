using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Settings;
using Palaso.TestUtilities;
using Palaso.UI.WindowsForms.Registration;

namespace PalasoUIWindowsForms.Tests
{
	[TestFixture]
	public class SettingsProviderTests
	{
		[Test]
		public void SettingsFolderWithNoConfig_SortsAfterOneWithConfig()
		{
			// We do two iterations like this to vary whether the (originally) first or last directory lacks
			// the config file. (We can't achieve this reliably by deleting the file because it sometimes
			// takes the system some time to notice that it is gone.)
			for (var which = 0; which < 1; which++)
			{
				using (var tempFolder = new TemporaryFolder())
				{
					var firstDirPath = Path.Combine(tempFolder.Path, "first");
					var secondDirPath = Path.Combine(tempFolder.Path, "second");
					var firstConfigFile = Path.Combine(firstDirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
					var secondConfigFile = Path.Combine(firstDirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
					Directory.CreateDirectory(firstDirPath);
					Directory.CreateDirectory(secondDirPath);
					if (which == 0)
						File.WriteAllText(firstConfigFile, @"nonsense");
					else
						File.WriteAllText(secondConfigFile, @"nonsense");

					var list = new List<string>();
					list.Add(secondDirPath);
					list.Add(firstDirPath);
					list.Sort(TestCrossPlatformSettingsProvider.VersionDirectoryComparison);
					if (which == 0)
						Assert.That(list[0], Is.EqualTo(firstDirPath), "first directory has config so should have come first");
					else
						Assert.That(list[0], Is.EqualTo(secondDirPath), "second directory has config so should have come first");
				}
			}
		}

		[Test]
		public void SettingsFolderWithNewerConfig_SortsBeforeOneWithOlderConfig()
		{

			using (var tempFolder = new TemporaryFolder())
			{
				var firstDirPath = Path.Combine(tempFolder.Path, "first");
				var secondDirPath = Path.Combine(tempFolder.Path, "second");
				var firstConfigFile = Path.Combine(firstDirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				var secondConfigFile = Path.Combine(secondDirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				Directory.CreateDirectory(firstDirPath);
				Directory.CreateDirectory(secondDirPath);
				File.WriteAllText(firstConfigFile, @"nonsense");
				Thread.Sleep(100); // May help ensure write times are sufficiently different on TeamCity.
				File.WriteAllText(secondConfigFile, @"nonsense"); // second is newer

				var result = TestCrossPlatformSettingsProvider.VersionDirectoryComparison(firstDirPath, secondDirPath);
				Assert.That(result, Is.GreaterThan(0));

				Thread.Sleep(100); // May help ensure write times are sufficiently different on TeamCity.
				File.WriteAllText(firstConfigFile, @"nonsense"); // now first is newer
				result = TestCrossPlatformSettingsProvider.VersionDirectoryComparison(firstDirPath, secondDirPath);
				Assert.That(result, Is.LessThan(0));

				// A final check to make sure it is really working the way we want
				var list = new List<string>();
				list.Add(secondDirPath);
				list.Add(firstDirPath);
				list.Sort(TestCrossPlatformSettingsProvider.VersionDirectoryComparison);
				Assert.That(list[0], Is.EqualTo(firstDirPath));
			}
		}

		/// <summary>
		/// This test was primarily created to test that we can override the default location for the RegistrationSettingsProvider.
		/// However it's also quite a useful test of the whole business of reading settings.
		/// </summary>
		[Test]
		public void CanOverrideDefaultLocation()
		{
			RegistrationSettingsProvider.SetProductName("FlowerUnitTest");
			var settingsProvider = new TestCrossPlatformSettingsProvider();
			settingsProvider.Initialize(null, null); // Seems to be what .NET does, despite warnings
			var dirPath = settingsProvider.UserConfigLocation;
			Assert.That(dirPath, Is.StringContaining("FlowerUnitTest"));
			Directory.CreateDirectory(dirPath);
			var filePath = Path.Combine(dirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
			using (var tempFile = new TempFile(filePath, true))
			{
				File.WriteAllText(filePath,
					@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
    <userSettings>
        <Palaso.UI.WindowsForms.Registration.Registration>
            <setting name='Email' serializeAs='String'>
                <value>someone@somewhere.org</value>
            </setting>
        </Palaso.UI.WindowsForms.Registration.Registration>
    </userSettings>
</configuration>");

				var regSettings = Registration.Default;
				var email = regSettings.Email;
				Assert.That(email, Is.EqualTo("someone@somewhere.org"));
			}
		}

		/// <summary>
		/// Exposes protected stuff for testing.
		/// </summary>
		class TestCrossPlatformSettingsProvider : RegistrationSettingsProvider
		{
			new public static int VersionDirectoryComparison(string first, string second)
			{
				return CrossPlatformSettingsProvider.VersionDirectoryComparison(first, second);
			}

			new public static string UserConfigFileName { get { return CrossPlatformSettingsProvider.UserConfigFileName; } }

			new public string UserConfigLocation { get { return base.UserConfigLocation; } }
		}
	}
}
