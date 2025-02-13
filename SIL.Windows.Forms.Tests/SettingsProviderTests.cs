// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using SIL.IO;
using SIL.Reporting;
using SIL.Settings;
using SIL.TestUtilities;
using SIL.Windows.Forms.Registration;

namespace SIL.Windows.Forms.Tests
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
				using (var tempFolder = new TemporaryFolder("SettingsFolderWithNoConfig_SortsAfterOneWithConfig"))
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

			using (var tempFolder = new TemporaryFolder("SettingsFolderWithNewerConfig_SortsBeforeOneWithOlderConfig"))
			{
				var firstDirPath = Path.Combine(tempFolder.Path, "first");
				var secondDirPath = Path.Combine(tempFolder.Path, "second");
				var firstConfigFile = Path.Combine(firstDirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				var secondConfigFile = Path.Combine(secondDirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				Directory.CreateDirectory(firstDirPath);
				Directory.CreateDirectory(secondDirPath);
				File.WriteAllText(firstConfigFile, @"nonsense");
				Thread.Sleep(1000); // May help ensure write times are sufficiently different on TeamCity.
				File.WriteAllText(secondConfigFile, @"nonsense"); // second is newer

				var result = TestCrossPlatformSettingsProvider.VersionDirectoryComparison(firstDirPath, secondDirPath);
				Assert.That(result, Is.GreaterThan(0));

				Thread.Sleep(1000); // May help ensure write times are sufficiently different on TeamCity.
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
			RegistrationSettingsProvider.SetProductName("SettingsProviderTests");
			var settingsProvider = new TestCrossPlatformSettingsProvider();
			settingsProvider.Initialize(null, null); // Seems to be what .NET does, despite warnings

			string dirPath = settingsProvider.UserConfigLocation;
			Directory.CreateDirectory(dirPath);
			using (TemporaryFolder.TrackExisting(dirPath))
			{
				Assert.That(dirPath, Does.Contain("SettingsProviderTests"));
				string filePath = Path.Combine(dirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				using (new TempFile(filePath, true))
				{
					File.WriteAllText(filePath,
						@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
	<userSettings>
		<SIL.Windows.Forms.Registration.Registration>
			<setting name='Email' serializeAs='String'>
				<value>someone@somewhere.org</value>
			</setting>
		</SIL.Windows.Forms.Registration.Registration>
	</userSettings>
</configuration>");

					Registration.Registration regSettings = Registration.Registration.Default;
					Assert.That(regSettings.Email, Is.EqualTo("someone@somewhere.org"));
				}
			}
		}

		[Test]
		public void CanSaveBothRegularAndRegistrationSettings()
		{
			// We need to START with a file that has no Registration settings.
			// (Note that we do NOT set a special location for the registration settings in this test.
			// For this text, we are interested in what happens when the two lots of settings are saved in the SAME file.
			// This can happen, even when the registration location is being customized; for example, all channels of
			// Bloom save Registration settings under the product name "Bloom", but the main stable release build will also
			// save its regular settings there.
			var provider = new TestCrossPlatformSettingsProvider();
			var settingsFilePath = Path.Combine(provider.UserConfigLocation, TestCrossPlatformSettingsProvider.UserConfigFileName);
			if (File.Exists(settingsFilePath))
				File.Delete(settingsFilePath);

			Properties.Settings.Default.TestString = "hello world";
			Properties.Settings.Default.Save();
			Properties.Settings.Default.AnotherTest = "another";
			Properties.Settings.Default.Save();
			Registration.Registration.Default.FirstName = "John";
			Registration.Registration.Default.Save();

			// This line was a problem in one version of the code, where Settings saved a version of the XML without the Registration stuff.
			// The thing this test is mainly about is that this subsequent Save() does not discard the registration settings.
			Properties.Settings.Default.Save();

			// Somehow a Reload() at this point does NOT detect that the registration settings are missing (if they are).
			string fileContent = File.ReadAllText(settingsFilePath);
			Assert.That(fileContent, Does.Contain("Registration"));
			Assert.That(fileContent, Does.Contain("John"));
		}

		/// <summary>
		/// This test is ignored on Mono, because of known issues in the ApplicationSettingsBase class (Xamarin-15818
		/// and Xamarin-2315). This issue is fixed in Mono-SIL.
		/// </summary>
		[Test]
		[Category("KnownMonoIssue")]
		public void Upgrade_SectionsRenamed_SettingsMigrated()
		{
			RegistrationSettingsProvider.SetProductName("SettingsProviderTests");
			var settingsProvider = new TestCrossPlatformSettingsProvider();
			settingsProvider.Initialize(null, null); // Seems to be what .NET does, despite warnings

			string appSettingsRoot = Path.Combine(settingsProvider.UserConfigLocation, "..");
			string dirPath = Path.Combine(appSettingsRoot, "0.0.0.0");
			Directory.CreateDirectory(dirPath);
			using (TemporaryFolder.TrackExisting(dirPath))
			{
				string filePath = Path.Combine(dirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				using (new TempFile(filePath, true))
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

					Registration.Registration regSettings = Registration.Registration.Default;
					regSettings.Upgrade();
					Assert.That(regSettings.Email, Is.EqualTo("someone@somewhere.org"));
				}
			}
		}

		/// <summary>
		/// This test is ignored on Mono, because of known issues in the ApplicationSettingsBase class (Xamarin-15818
		/// and Xamarin-2315). This issue is fixed in Mono-SIL.
		///
		/// This test adds and changes values from the earlier tests. This is necessary to confirm
		/// we're reading the temp file for this test, as opposed to a temp file from a previous.
		/// </summary>
		[Test]
		[Category("KnownMonoIssue")]
		public void Upgrade_ExtraFields_SettingsMigrated()
		{
			RegistrationSettingsProvider.SetProductName("SettingsProviderTests");
			var settingsProvider = new TestCrossPlatformSettingsProvider();
			settingsProvider.Initialize(null, null); // Seems to be what .NET does, despite warnings

			string appSettingsRoot = Path.Combine(settingsProvider.UserConfigLocation, "..");
			string dirPath = Path.Combine(appSettingsRoot, "0.0.0.0");
			Directory.CreateDirectory(dirPath);
			using (TemporaryFolder.TrackExisting(dirPath))
			{
				string filePath = Path.Combine(dirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				using (new TempFile(filePath, true))
				{
					File.WriteAllText(filePath,
						@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
	<userSettings>
		<Palaso.UI.WindowsForms.Registration.Registration>
			<setting name='Email' serializeAs='String'>
				<value>someone2@somewhere.org</value>
			</setting>
			<setting name='LaunchCount' serializeAs='String'>
				<value>10</value>
			</setting>
		</Palaso.UI.WindowsForms.Registration.Registration>
	</userSettings>
</configuration>");

					Registration.Registration regSettings = Registration.Registration.Default;
					regSettings.Upgrade();
					Assert.That(regSettings.Email, Is.EqualTo("someone2@somewhere.org"));
					Assert.That(regSettings.LaunchCount, Is.EqualTo(10));
				}
			}
		}

		[Test, Ignore("Probably due to statics on CrossPlatformSettingsProvider, this test is corrupted by other tests. Works fine in isolation")]
		public void LoadSettings_FileCorrupt_ShowsErrorAndSelfHeals()
		{
			RegistrationSettingsProvider.SetProductName("SettingsProviderTests");
			var settingsProvider = new TestCrossPlatformSettingsProvider();
			settingsProvider.Initialize(null, null); // Seems to be what .NET does, despite warnings

			string dirPath = settingsProvider.UserConfigLocation;
			Directory.CreateDirectory(dirPath);
			using (TemporaryFolder.TrackExisting(dirPath))
			{
				var filePath = Path.Combine(dirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				File.Delete(filePath);
				using (new TempFile(filePath, true))
				{
					File.WriteAllText(filePath,"hello world");
					using (new ErrorReport.NonFatalErrorReportExpected())
					{
						var dummy = Registration.Registration.Default;
						//by this point, we expect that the file has been fixed and we can continue normally
						dummy.Email = "foo@foo.com";
					}
				}

				//next time, it should be healed
				using (new ErrorReport.NoNonFatalErrorReportExpected())
				{
					var settingsProvider2 = new TestCrossPlatformSettingsProvider();
					settingsProvider2.Initialize(null, null); // Seems to be what .NET does, despite warnings
					Assert.That(Registration.Registration.Default.Email, Is.EqualTo("foo@foo.com"));
				}
			}
		}

		[Test]
		public void CheckForErrorsInFile_FileCorrupt_ReturnsMessage()
		{
			RegistrationSettingsProvider.SetProductName("SettingsProviderTests");
			var settingsProvider = new TestCrossPlatformSettingsProvider();
			settingsProvider.Initialize(null, null); // Seems to be what .NET does, despite warnings

			string dirPath = settingsProvider.UserConfigLocation;
			Directory.CreateDirectory(dirPath);
			using (TemporaryFolder.TrackExisting(dirPath))
			{
				var filePath = Path.Combine(dirPath, TestCrossPlatformSettingsProvider.UserConfigFileName);
				using (new TempFile(filePath, true))
				{
					File.WriteAllText(filePath, "hello world");

					Assert.That(settingsProvider.CheckForErrorsInSettingsFile(), Is.Not.Null);

					//because we already did the check, we don't expect to see any error now
					using (new ErrorReport.NoNonFatalErrorReportExpected())
					{
						var dummy = Registration.Registration.Default;
					}
				}
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
