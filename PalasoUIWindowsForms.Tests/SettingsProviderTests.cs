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
			// We do two iterations like this because we can't count on deleting the file to really make it gone at once.
			for (var which = 0; which < 1; which++)
			{
				using (var tempFolder = new TemporaryFolder())
				{
					var firstDirPath = Path.Combine(tempFolder.Path, "first");
					var secondDirPath = Path.Combine(tempFolder.Path, "second");
					var firstConfigFile = Path.Combine(firstDirPath, CrossPlatformSettingsProvider.UserConfigFileName);
					var secondConfigFile = Path.Combine(firstDirPath, CrossPlatformSettingsProvider.UserConfigFileName);
					Directory.CreateDirectory(firstDirPath);
					Directory.CreateDirectory(secondDirPath);
					if (which == 0)
						File.WriteAllText(firstConfigFile, @"nonsense");
					else
						File.WriteAllText(secondConfigFile, @"nonsense");

					var list = new List<string>();
					list.Add(secondDirPath);
					list.Add(firstDirPath);
					list.Sort(CrossPlatformSettingsProvider.VersionDirectoryComparison);
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
				var firstConfigFile = Path.Combine(firstDirPath, CrossPlatformSettingsProvider.UserConfigFileName);
				var secondConfigFile = Path.Combine(secondDirPath, CrossPlatformSettingsProvider.UserConfigFileName);
				Directory.CreateDirectory(firstDirPath);
				Directory.CreateDirectory(secondDirPath);
				File.WriteAllText(firstConfigFile, @"nonsense");
				File.WriteAllText(secondConfigFile, @"nonsense"); // second is newer

				var result = CrossPlatformSettingsProvider.VersionDirectoryComparison(firstDirPath, secondDirPath);
				Assert.That(result, Is.GreaterThan(0));

				File.WriteAllText(firstConfigFile, @"nonsense"); // now first is newer
				result = CrossPlatformSettingsProvider.VersionDirectoryComparison(firstDirPath, secondDirPath);
				Assert.That(result, Is.LessThan(0));

				// A final check to make sure it is really working the way we want
				var list = new List<string>();
				list.Add(secondDirPath);
				list.Add(firstDirPath);
				list.Sort(CrossPlatformSettingsProvider.VersionDirectoryComparison);
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
			RegistrationSettingsProvider.SetProductName("BloomUnitTest");
			var settingsProvider = new RegistrationSettingsProvider();
			settingsProvider.Initialize(null, null); // Seems to be what .NET does, despite warnings
			var dirPath = settingsProvider.UserConfigLocation;
			Assert.That(dirPath, Is.StringContaining("BloomUnitTest"));
			Directory.CreateDirectory(dirPath);
			var filePath = Path.Combine(dirPath, CrossPlatformSettingsProvider.UserConfigFileName);
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
	}
}
