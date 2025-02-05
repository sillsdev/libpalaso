// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2025 SIL Global
// <copyright from='2011' to='2024' company='SIL Global'>
//		Copyright (c) 2025 SIL Global
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
//
// This class originated in FieldWorks (under the GNU Lesser General Public License), but we
// decided to make it available in SIL.Windows.Forms.Keyboarding to make it more readily
// available to other projects.
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;
using SIL.Keyboarding;
using X11.XKlavier;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	[SetUICulture("en-US")]
	public class XkbKeyboardAdapterTests
	{
		/// <summary>
		/// Fakes the installed keyboards
		/// </summary>
		private class XklEngineResponder: XklEngine
		{
			public static string[] SetGroupNames { set; private get; }

			public override string[] GroupNames => SetGroupNames;
		}

		#region Helper class/method to set the LANGUAGE environment variable

		/// <summary>Helper class/method to set the LANGUAGE environment variable. This is
		/// necessary to get localized texts</summary>
		/// <remarks>A different, probably cleaner approach, would be to derive a class from
		/// NUnit's ActionAttribute. However, this currently doesn't work when running the tests
		/// in MonoDevelop (at least up to version 4.3).</remarks>
		class LanguageHelper : IDisposable
		{
			private string OldLanguage { get; set; }
			public LanguageHelper(string language)
			{
				// To get XklConfigRegistry to return values in the expected language we have to
				// set the LANGUAGE environment variable.

				OldLanguage = Environment.GetEnvironmentVariable("LANGUAGE");
				Environment.SetEnvironmentVariable("LANGUAGE",
					$"{language.Replace('-', '_')}:{OldLanguage}");
			}

			#region Disposable stuff

			#if DEBUG
			/// <summary/>
			~LanguageHelper()
			{
				Dispose(false);
			}
			#endif
			/// <summary/>
			public bool IsDisposed { get; private set; }

			/// <summary/>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary/>
			protected virtual void Dispose(bool fDisposing)
			{
				Debug.WriteLineIf(!fDisposing, "****** Missing Dispose() call for " + GetType() + ". *******");
				if (fDisposing && !IsDisposed)
				{
					// dispose managed and unmanaged objects
					Environment.SetEnvironmentVariable("LANGUAGE", OldLanguage);
				}
				IsDisposed = true;
			}

			#endregion
			/// <summary>
			/// Checks the list of installed languages and ignores the test if the desired language
			/// is not installed.
			/// </summary>
			/// <param name="desiredLanguage">Desired language.</param>
			public static void CheckInstalledLanguages(string desiredLanguage)
			{
				var language = desiredLanguage.Replace('-', '_');
				// Some systems (notably Mint 17/Cinnamon aka Wasta-14) don't have a LANGUAGE environment
				// variable, so we need to check for a null value.
				var langFromEnv = Environment.GetEnvironmentVariable ("LANGUAGE");
				if (langFromEnv != null && langFromEnv.Contains(language))
					return;

				using (var process = new Process())
				{
					process.StartInfo.FileName = "locale";
					process.StartInfo.Arguments = "-a";
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.RedirectStandardOutput = true;
					process.Start();
					process.WaitForExit();

					for (var line = process.StandardOutput.ReadLine();
						line != null;
						line = process.StandardOutput.ReadLine())
					{
						if (line.StartsWith(language, StringComparison.InvariantCultureIgnoreCase))
							return;
					}

					Assert.Ignore("Can't run test because language pack for {0} is not installed.",
						desiredLanguage);
				}
			}
		}

		private static LanguageHelper SetLanguage(string language)
		{
			LanguageHelper.CheckInstalledLanguages(language);
			return new LanguageHelper(language);
		}
		#endregion

		[DllImportAttribute("libgtk-x11-2.0")]
		[return: MarshalAs(UnmanagedType.I4)]
		private static extern bool gtk_init_check(ref int argc, ref IntPtr argv) ;

		private string KeyboardUSA => KeyboardNames[0];
		private string KeyboardGermany => KeyboardNames[1];
		private string KeyboardFranceEliminateDeadKeys => KeyboardNames[2];
		private string KeyboardUK => KeyboardNames[3];
		private string KeyboardBelgium => KeyboardNames[4];
		private string KeyboardFinlandNorthernSaami => KeyboardNames[5];

		private string[] KeyboardNames;
		private static readonly string[] KeyboardNamesOfUbuntu1404 = { "USA", "Germany",
			"France - Eliminate dead keys", "United Kingdom", "Belgium",
			"Finland - Northern Saami" };
		private static readonly string[] KeyboardNamesOfUbuntu1604 = { "English (US)", "German",
			"French (eliminate dead keys)", "English (UK)", "Belgian",
			"Northern Saami (Finland)" };
		private static readonly string[] KeyboardNamesOfUbuntu1804 = { "English (US)", "German",
			"French (no dead keys)", "English (UK)", "Belgian",
			"Northern Saami (Finland)" };

		private static readonly string[][] AllKeyboardNames = {
			KeyboardNamesOfUbuntu1404,
			KeyboardNamesOfUbuntu1604,
			KeyboardNamesOfUbuntu1804
		};

		private string ExpectedKeyboardUSA => ExpectedKeyboardNames[0];
		private string ExpectedKeyboardGermany => ExpectedKeyboardNames[1];
		private string ExpectedKeyboardFranceEliminateDeadKeys => ExpectedKeyboardNames[2];

		private string ExpectedKeyboardUK => ExpectedKeyboardNames[3];

		//private string ExpectedKeyboardBelgium => ExpectedKeyboardNames[4];
		private string ExpectedKeyboardFinlandNorthernSaami => ExpectedKeyboardNames[5];

		private string[] ExpectedKeyboardNames;
		private static readonly string[] ExpectedKeyboardNamesOfUbuntu1404 = {
			"English (US) - English (United States)",
			"German - German (Germany)", "Eliminate dead keys - French (France)",
			"English (UK) - English (United Kingdom)", "", "Northern Saami - Northern Sami (Finland)" };
		private static readonly string[] ExpectedKeyboardNamesOfUbuntu1604 = {
			"English (US) - English (United States)",
			"German - German (Germany)", "French (eliminate dead keys) - French (France)",
			"English (UK) - English (United Kingdom)", "", "Northern Saami (Finland) - Northern Sami (Finland)" };
		private static readonly string[] ExpectedKeyboardNamesOfUbuntu1804 = {
			"English (US) - English (United States)",
			"German - German (Germany)", "French (no dead keys) - French (France)",
			"English (UK) - English (United Kingdom)", "", "Northern Saami (Finland) - Northern Sami (Finland)" };

		private static readonly string[][] AllExpectedKeyboardNames = {
			ExpectedKeyboardNamesOfUbuntu1404,
			ExpectedKeyboardNamesOfUbuntu1604,
			ExpectedKeyboardNamesOfUbuntu1804
		};

		private static int KeyboardNamesIndex
		{
			get
			{
				// Debian/Ubuntu version 2.2.1 of xkeyboard-config changed the way keyboard names
				// are stored in evdev.xml: previously the country name was used ("Belgium"), now
				// they use the adjective ("Belgian"). We detect this by greping evdev.xml and then
				// use the appropriate names
				using (var process = new Process())
				{
					process.StartInfo.FileName = "/bin/grep";
					process.StartInfo.Arguments = "-q Belgian /usr/share/X11/xkb/rules/evdev.xml";
					process.Start();
					process.WaitForExit();
					if (process.ExitCode != 0)
						return 0; // Ubuntu <= 14.04
				}

				// Ubuntu 18.04 changed the naming for layouts without dead keys: "no dead keys"
				// instead of "eliminate dead keys"
				using (var process = new Process())
				{
					process.StartInfo.FileName = "/bin/grep";
					process.StartInfo.Arguments = "-q 'French (no dead keys)' /usr/share/X11/xkb/rules/evdev.xml";
					process.Start();
					process.WaitForExit();
					if (process.ExitCode != 0)
						return 1; // Ubuntu 16.04
				}

				return 2; // Ubuntu >= 18.04
			}
		}

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			// We're using GTK functions, so we need to initialize when we run in
			// nunit-console. I'm doing it through p/invoke rather than gtk-sharp (Application.Init())
			// so that we don't need to reference gtk-sharp (which might cause
			// problems on Windows)
			var argc = 0;
			var argv = IntPtr.Zero;
			Assert.IsTrue(gtk_init_check(ref argc, ref argv));

			var index = KeyboardNamesIndex;
			KeyboardNames = AllKeyboardNames[index];
			ExpectedKeyboardNames = AllExpectedKeyboardNames[index];
		}

		[TearDown]
		public void TearDown()
		{
			KeyboardController.Shutdown();
		}

		/// <summary>
		/// Tests converting the keyboard layouts that XKB reports to LCIDs with the help of ICU
		/// and list of available layouts.
		/// </summary>
		[Test]
		public void InstalledKeyboards_USA()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardUSA };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			IKeyboardDefinition[] keyboards = Keyboard.Controller.AvailableKeyboards.ToArray();
			Assert.AreEqual(1, keyboards.Length);
			Assert.AreEqual("en-US_us", keyboards[0].Id);
			Assert.AreEqual(ExpectedKeyboardUSA, keyboards[0].Name);
		}

		[Test]
		public void InstalledKeyboards_Germany()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardGermany };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			IKeyboardDefinition[] keyboards = Keyboard.Controller.AvailableKeyboards.ToArray();
			Assert.AreEqual(1, keyboards.Length);
			Assert.AreEqual("de-DE_de", keyboards[0].Id);
			Assert.AreEqual(ExpectedKeyboardGermany, keyboards[0].Name);
		}

		[Test]
		public void InstalledKeyboards_FrenchWithVariant()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardFranceEliminateDeadKeys };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			var keyboards = Keyboard.Controller.AvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual(ExpectedKeyboardFranceEliminateDeadKeys, keyboards.First().Name);
		}

		[Test]
		public void InstalledKeyboards_GB()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardUK };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			IKeyboardDefinition[] keyboards = Keyboard.Controller.AvailableKeyboards.ToArray();
			Assert.AreEqual(1, keyboards.Length);
			Assert.AreEqual("en-GB_gb", keyboards[0].Id);
			Assert.AreEqual(ExpectedKeyboardUK, keyboards[0].Name);
		}

		private IKeyboardDefinition CreateKeyboard(string layoutName, string layout, string locale)
		{
			return new KeyboardDescription($"{layout}_{locale}", layoutName,
				layout, locale, true, null);
		}

		[Test]
		public void InstalledKeyboards_Belgium()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardBelgium };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			var keyboards = Keyboard.Controller.AvailableKeyboards.OrderBy(kbd => kbd.Id).ToArray();
			// It seems that Dutch (Belgium) got added recently, so some machines are missing
			// this.
			Assert.That(keyboards.Length, Is.EqualTo(3).Or.EqualTo(2));
			var expectedKeyboardIds = new List<string>()
				{ "de-BE_be", "fr-BE_be" };

			if (keyboards.Length > 2)
				expectedKeyboardIds.Add("nl-BE_be");

			Assert.That(keyboards.Select(kbd => kbd.Id), Is.EquivalentTo(expectedKeyboardIds));
		}

		[Test]
		public void InstalledKeyboards_Multiple()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardUSA, KeyboardGermany };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			var keyboards = Keyboard.Controller.AvailableKeyboards.ToArray();
			Assert.AreEqual(2, keyboards.Length);
			Assert.AreEqual("en-US_us", keyboards[0].Id);
			Assert.AreEqual(ExpectedKeyboardUSA, keyboards[0].Name);
			Assert.AreEqual("de-DE_de", keyboards[1].Id);
			Assert.AreEqual(ExpectedKeyboardGermany, keyboards[1].Name);
		}

		/// <summary>
		/// Tests the values returned by InstalledKeyboards if the UICulture is set to German
		/// </summary>
		[Test]
		[SetUICulture("de-DE")]
		public void InstalledKeyboards_Germany_GermanCulture()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardGermany };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			var keyboards = Keyboard.Controller.AvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual("de-DE_de", keyboards.First().Id);
			Assert.AreEqual("German - Deutsch (Deutschland)", keyboards.First().Name);
		}

		/// <summary>
		/// Tests the values returned by InstalledKeyboards if the UICulture is set to German
		/// and we're getting localized keyboard layouts
		/// </summary>
		[Test]
		[SetUICulture("de-DE")]
		public void InstalledKeyboards_Germany_AllGerman() // FWNX-1388
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardGermany };

			using (SetLanguage("de-DE"))
			{
				KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
				IKeyboardDefinition[] keyboards = Keyboard.Controller.AvailableKeyboards.ToArray();
				Assert.AreEqual(1, keyboards.Length);
				Assert.AreEqual("de-DE_de", keyboards[0].Id);
				Assert.AreEqual("Deutsch - Deutsch (Deutschland)", keyboards[0].Name);
			}
		}

		/// <summary>
		/// Tests InstalledKeyboards property. "Finland - Northern Saami" gives us two
		/// layouts (smi_FIN and sme_FIN), but ICU returns a LCID only for one of them.
		/// </summary>
		[Test]
		public void InstalledKeyboards_NorthernSaami()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardFinlandNorthernSaami };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			IKeyboardDefinition[] keyboards = Keyboard.Controller.AvailableKeyboards.ToArray();
			Assert.AreEqual(1, keyboards.Length);
			Assert.AreEqual(ExpectedKeyboardFinlandNorthernSaami, keyboards[0].Name);
		}

		[Test]
		public void ErrorKeyboards()
		{
			XklEngineResponder.SetGroupNames = new[] { "Fake" };

			KeyboardController.Initialize(new XkbKeyboardRetrievingAdaptor(new XklEngineResponder()));
			IEnumerable<IKeyboardDefinition> keyboards = Keyboard.Controller.AvailableKeyboards;
			Assert.AreEqual(0, keyboards.Count());
			//Assert.AreEqual(1, KeyboardController.ErrorKeyboards.Count);
			//Assert.AreEqual("Fake", KeyboardController.ErrorKeyboards.First().Details);
		}

		/// <summary/>
		[Test]
		public void ActivateKeyboard_FirstTime_NotCrash()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardUSA };

			var adaptor = new XkbKeyboardRetrievingAdaptor(new XklEngineResponder());
			KeyboardController.Initialize(adaptor);
			Assert.That(() => adaptor.SwitchingAdaptor.ActivateKeyboard(
				KeyboardController.Instance.Keyboards.First()), Throws.Nothing);
		}

		/// <summary>
		/// FWNX-895
		/// </summary>
		[Test]
		public void ActivateKeyboard_SecondTime_NotCrash()
		{
			XklEngineResponder.SetGroupNames = new[] { KeyboardUSA };

			var adaptor = new XkbKeyboardRetrievingAdaptor(new XklEngineResponder());
			KeyboardController.Initialize(adaptor);
			adaptor.SwitchingAdaptor.ActivateKeyboard(KeyboardController.Instance.Keyboards.First());
			KeyboardController.Shutdown();

			adaptor = new XkbKeyboardRetrievingAdaptor(new XklEngineResponder());
			KeyboardController.Initialize(adaptor);
			Assert.That(() => adaptor.SwitchingAdaptor.ActivateKeyboard(
				KeyboardController.Instance.Keyboards.First()), Throws.Nothing);
		}

		[Test]
		public void CreateKeyboardDefinition()
		{
			// Setup
			XklEngineResponder.SetGroupNames = new[] { KeyboardUSA };
			var adaptor = new XkbKeyboardRetrievingAdaptor(new XklEngineResponder());
			KeyboardController.Initialize(adaptor);

			// This mimics a KMFL ibus keyboard (which come through as path to the keyman file)
			const string kmflKeyboard = "/some/keyboard/without/dash";
			const string expectedKeyboardName = "[Missing] /some/keyboard/without/dash ()";

			// Exercise
			var keyboard = XkbKeyboardRetrievingAdaptor.CreateKeyboardDefinition(kmflKeyboard,
				adaptor.SwitchingAdaptor);

			// Verify
			Assert.That(keyboard, Is.Not.Null);
			Assert.That(keyboard.Name, Is.EqualTo(expectedKeyboardName));
		}
	}
}
