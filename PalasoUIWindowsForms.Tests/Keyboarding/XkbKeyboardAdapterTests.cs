// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------
#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Linux;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;
using X11.XKlavier;

namespace PalasoUIWindowsForms.Tests.Keyboarding
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

			public override string[] GroupNames { get { return SetGroupNames; } }

		}

		[DllImportAttribute("libgtk-x11-2.0")]
		[return: MarshalAs(UnmanagedType.I4)]
		private static extern bool gtk_init_check(ref int argc, ref IntPtr argv) ;

		private string KeyboardUSA { get { return KeyboardNames[0]; } }
		private string KeyboardGermany { get { return KeyboardNames[1]; } }
		private string KeyboardFranceEliminateDeadKeys { get { return KeyboardNames[2]; } }
		private string KeyboardUK { get { return KeyboardNames[3]; } }
		private string KeyboardBelgium { get { return KeyboardNames[4]; } }
		private string KeyboardFinlandNorthernSaami { get { return KeyboardNames[5]; } }

		private string[] KeyboardNames;
		private string[] OldKeyboardNames = new string[] { "USA", "Germany",
			"France - Eliminate dead keys", "United Kingdom", "Belgium",
			"Finland - Northern Saami" };
		private string[] NewKeyboardNames = new string[] { "English (US)", "German",
			"French (eliminate dead keys)", "English (UK)", "Belgian",
			"Northern Saami (Finland)" };

		private string ExpectedKeyboardUSA { get { return ExpectedKeyboardNames[0]; } }
		private string ExpectedKeyboardGermany { get { return ExpectedKeyboardNames[1]; } }
		private string ExpectedKeyboardFranceEliminateDeadKeys { get { return ExpectedKeyboardNames[2]; } }
		private string ExpectedKeyboardUK { get { return ExpectedKeyboardNames[3]; } }
		//private string ExpectedKeyboardBelgium { get { return ExpectedKeyboardNames[4]; } }
		private string ExpectedKeyboardFinlandNorthernSaami { get { return ExpectedKeyboardNames[5]; } }

		private string[] ExpectedKeyboardNames;
		private string[] OldExpectedKeyboardNames = new string[] { "English (US) - English (United States)",
			"German - German (Germany)", "Eliminate dead keys - French (France)",
			"English (UK) - English (United Kingdom)", "", "Northern Saami - Northern Sami (Finland)" };
		private string[] NewExpectedKeyboardNames = new string[] { "English (US) - English (United States)",
			"German - German (Germany)", "French (eliminate dead keys) - French (France)",
			"English (UK) - English (United Kingdom)", "", "Northern Saami (Finland) - Northern Sami (Finland)" };

		private static bool IsNewEvdevNames
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
					process.StartInfo.Arguments = "Belgian /usr/share/X11/xkb/rules/evdev.xml";
					process.Start();
					process.WaitForExit();
					return process.ExitCode == 0;
				}
			}
		}

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// We're using GTK functions, so we need to intialize when we run in
			// nunit-console. I'm doing it through p/invoke rather than gtk-sharp (Application.Init())
			// so that we don't need to reference gtk-sharp (which might cause
			// problems on Windows)
			int argc = 0;
			IntPtr argv = IntPtr.Zero;
			Assert.IsTrue(gtk_init_check(ref argc, ref argv));
			if (IsNewEvdevNames)
			{
				KeyboardNames = NewKeyboardNames;
				ExpectedKeyboardNames = NewExpectedKeyboardNames;
			}
			else
			{
				KeyboardNames = OldKeyboardNames;
				ExpectedKeyboardNames = OldExpectedKeyboardNames;
			}
			KeyboardController.Initialize();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
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
			XklEngineResponder.SetGroupNames = new string[] { KeyboardUSA };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual("en-US_us", keyboards.First().Id);
			Assert.AreEqual(ExpectedKeyboardUSA, keyboards.First().Name);
		}

		[Test]
		public void InstalledKeyboards_Germany()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardGermany };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual("de-DE_de", keyboards.First().Id);
			Assert.AreEqual(ExpectedKeyboardGermany, keyboards.First().Name);
		}

		[Test]
		public void InstalledKeyboards_FrenchWithVariant()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardFranceEliminateDeadKeys };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual(ExpectedKeyboardFranceEliminateDeadKeys, keyboards.First().Name);
		}

		[Test]
		public void InstalledKeyboards_GB()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardUK };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual("en-GB_gb", keyboards.First().Id);
			Assert.AreEqual(ExpectedKeyboardUK, keyboards.First().Name);
		}

		private IKeyboardDefinition CreateKeyboard(string layoutName, string layout, string locale)
		{
			CultureInfo culture = null;
			try
			{
				culture = new CultureInfo(locale);
			}
			catch (ArgumentException)
			{
				// this can happen if locale is not supported.
			}
			return new KeyboardDescription(layoutName, layout, locale,
				new InputLanguageWrapper(culture, IntPtr.Zero, layoutName), null);
		}

		[Test]
		public void InstalledKeyboards_Belgium()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardBelgium };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards.OrderBy(kbd => kbd.Id).ToArray();
			// It seems that Dutch (Belgium) got added recently, so some machines are missing
			// this.
			Assert.That(keyboards.Length == 3 || keyboards.Length == 2);
			var expectedKeyboards = new List<IKeyboardDefinition>()
				{ CreateKeyboard("German", "be", "de-BE") };
			expectedKeyboards.Add(CreateKeyboard("French", "be", "fr-BE"));

			if (keyboards.Length > 2)
				expectedKeyboards.Add(CreateKeyboard("Dutch", "be", "nl-BE"));

			Assert.That(keyboards, Is.EquivalentTo(expectedKeyboards));
		}

		[Test]
		public void InstalledKeyboards_Multiple()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardUSA, KeyboardGermany };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards.ToArray();
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
			XklEngineResponder.SetGroupNames = new string[] { KeyboardGermany };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual("de-DE_de", keyboards.First().Id);
			Assert.AreEqual("German - Deutsch (Deutschland)", keyboards.First().Name);
		}

		/// <summary>
		/// Tests InstalledKeyboards property. "Finland - Northern Saami" gives us two
		/// layouts (smi_FIN and sme_FIN), but ICU returns a LCID only for one of them.
		/// </summary>
		[Test]
		public void InstalledKeyboards_NorthernSami()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardFinlandNorthernSaami };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.AreEqual(1, keyboards.Count());
			Assert.AreEqual(ExpectedKeyboardFinlandNorthernSaami, keyboards.First().Name);
		}

		[Test]
		public void ErrorKeyboards()
		{
			XklEngineResponder.SetGroupNames = new string[] { "Fake" };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { new XkbKeyboardAdaptor() });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.AreEqual(0, keyboards.Count());
			//Assert.AreEqual(1, KeyboardController.ErrorKeyboards.Count);
			//Assert.AreEqual("Fake", KeyboardController.Errorkeyboards.First().Details);
		}

		/// <summary/>
		[Test]
		public void ActivateKeyboard_FirstTime_NotCrash()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardUSA };

			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();
			var adaptor = new XkbKeyboardAdaptor();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { adaptor });

			var keyboards = Keyboard.Controller.AllAvailableKeyboards;

			adaptor.ActivateKeyboard(keyboards.First());
		}

		/// <summary>
		/// FWNX-895
		/// </summary>
		[Test]
		public void ActivateKeyboard_SecondTime_NotCrash()
		{
			XklEngineResponder.SetGroupNames = new string[] { KeyboardUSA };
			XkbKeyboardAdaptor.SetXklEngineType<XklEngineResponder>();

			var adaptor = new XkbKeyboardAdaptor();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { adaptor });
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			adaptor.ActivateKeyboard(keyboards.First());

			adaptor = new XkbKeyboardAdaptor();
			KeyboardController.Manager.SetKeyboardAdaptors(new [] { adaptor });
			keyboards = Keyboard.Controller.AllAvailableKeyboards;
			adaptor.ActivateKeyboard(keyboards.First());
		}
	}
}
#endif
