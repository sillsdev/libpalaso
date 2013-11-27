#if !MONO
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.Code;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Windows;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	[Category("SkipOnTeamCity")] // TeamCity builds don't seem to be able to see any installed keyboards.
	public class WindowsKeyboardControllerTests
	{
		private Form _window;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			KeyboardController.Initialize();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			KeyboardController.Shutdown();
		}

		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
		}

		private void RequiresWindow()
		{
			_window = new Form();
			var box = new TextBox();
			box.Dock = DockStyle.Fill;
			_window.Controls.Add(box);

			_window.Show();
			box.Select();
			Application.DoEvents();
		}

		[TearDown]
		public void Teardown()
		{
			if (_window != null)
			{
				_window.Close();
				_window.Dispose();
			}
		}

		[Test]
		[Category("Windows IME")]
		public void GetAllKeyboards_GivesSeveral()
		{
			var keyboards = Keyboard.Controller.AllAvailableKeyboards;
			Assert.Greater(keyboards.Count(), 1, "This test requires that the Windows IME has at least two languages installed.");
		}

		[Test]
		public void ActivateKeyboard_BogusName_RaisesMessageBox()
		{
			Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
				() => Keyboard.Controller.SetKeyboard("foobar")
			);
		}

		[Test]
		public void ActivateKeyboard_BogusName_SecondTimeNoLongerRaisesMessageBox()
		{
			// the keyboardName for this test and above need to be different
			const string keyboardName = "This should never be the same as the name of an installed keyboard";
			try
			{
				Keyboard.Controller.SetKeyboard(keyboardName);
				Assert.Fail("Should have thrown exception but didn't.");
			}
			catch (ErrorReport.ProblemNotificationSentToUserException)
			{

			}
			Assert.DoesNotThrow(() => Keyboard.Controller.SetKeyboard(keyboardName));
		}

		[Test]
		public void ActivateKeyboard_BogusNameWithLocale_DoesntThrow()
		{
			// REVIEW: Should this show an error?
			Assert.DoesNotThrow(
				() => Keyboard.Controller.SetKeyboard("foobar", "en-US")
			);
		}

		IKeyboardDefinition FirstInactiveKeyboard
		{
			get
			{
				var keyboards = Keyboard.Controller.AllAvailableKeyboards.Where(x => x.Type == KeyboardType.System);
				Assert.Greater(keyboards.Count(), 0, "This test requires that the Windows IME has at least one language installed.");
				var d = keyboards.FirstOrDefault(x => x != Keyboard.Controller.ActiveKeyboard);
				if (d == null)
					return keyboards.First(); // Some tests have some value even if it is an active keyboard.
				return d;
			}
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_ActivateKeyboardUsingId_ReportsItWasActivated()
		{
			var d = FirstInactiveKeyboard;
			Keyboard.Controller.SetKeyboard(d.Id);
			Assert.AreEqual(d, Keyboard.Controller.ActiveKeyboard);
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_ActivateKeyboardUsingLayoutAndLocale_ReportsItWasActivated()
		{
			var d = FirstInactiveKeyboard;
			Keyboard.Controller.SetKeyboard(d.Layout, d.Locale);
			Assert.AreEqual(d, Keyboard.Controller.ActiveKeyboard);
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_ActivateKeyboardUsingKeyboard_ReportsItWasActivated()
		{
			var d = FirstInactiveKeyboard;
			Keyboard.Controller.SetKeyboard(d);
			Assert.AreEqual(d, Keyboard.Controller.ActiveKeyboard);
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_DeActivateKeyboard_RevertsToDefault()
		{
			var keyboards = Keyboard.Controller.AllAvailableKeyboards.Where(x => x.Type == KeyboardType.System);
			Assert.Greater(keyboards.Count(), 1, "This test requires that the Windows IME has at least two languages installed.");
			var d = GetNonDefaultKeyboard(keyboards.ToList());
			d.Activate();
			Assert.AreEqual(d, Keyboard.Controller.ActiveKeyboard);
			Keyboard.Controller.ActivateDefaultKeyboard();
			Assert.AreNotEqual(d, Keyboard.Controller.ActiveKeyboard);
		}

		private static IKeyboardDefinition GetNonDefaultKeyboard(IList<IKeyboardDefinition> keyboards)
		{
			// The default language is not necessarily the first one, so we have to make sure
			// that we don't select the default one.
			var defaultKeyboard =
			Keyboard.Controller.GetKeyboard(InputLanguage.DefaultInputLanguage.LayoutName,
			InputLanguage.DefaultInputLanguage.Culture.Name);
			int index = keyboards.Count - 1;
			while (index >= 0)
			{
				if (!keyboards[index].Equals(defaultKeyboard))
					break;
				index--;
			}
			if (index < 0)
				Assert.Fail("Could not find a non-default keyboard !?");

			return keyboards[index];
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_GetKeyboards_GivesSeveralButOnlyWindowsOnes()
		{
			var keyboards = Keyboard.Controller.AllAvailableKeyboards.Where(x => x.Type == KeyboardType.System);
			Assert.Greater(keyboards.Count(), 1, "This test requires that the Windows IME has at least two languages installed.");

			Assert.That(keyboards.Select(keyboard => ((KeyboardDescription)keyboard).Engine), Is.All.TypeOf<WinKeyboardAdaptor>());
		}

		[Test]
		public void CheckWindowsAssumptions()
		{
			// For Windows we expect to have exactly one keyboard adaptor. If we implement
			// additional ones, e.g. for Keyman, we might need to change some methods, e.g.
			// ActivateDefaultKeyboard, DefaultForWritingSystem and CreateKeyboardDefinition.
			Assert.That(KeyboardController.Adaptors.Length, Is.EqualTo(1));
			Assert.That(KeyboardController.Adaptors.Select(adaptor => adaptor.Type),
				Has.All.EqualTo(KeyboardType.System));
		}

		[Test]
		public void ActivateDefaultKeyboard_ActivatesDefaultInputLanguage()
		{
			Keyboard.Controller.ActivateDefaultKeyboard();
			Assert.That(WinKeyboardAdaptor.GetInputLanguage(Keyboard.Controller.ActiveKeyboard),
				Is.EqualTo(InputLanguage.DefaultInputLanguage));
		}

		[Test]
		public void CreateKeyboardDefinition_NewKeyboard_ReturnsNewObject()
		{
			var keyboard = Keyboard.Controller.CreateKeyboardDefinition("foo", "en-US");
			Assert.That(keyboard, Is.Not.Null);
			Assert.That(keyboard, Is.TypeOf<WinKeyboardDescription>());
			Assert.That((keyboard as KeyboardDescription).InputLanguage, Is.Not.Null);
		}

		// TODO: Remove or implement
#if WANT_PORT
		[Test]
		[Category("Keyman6")]
		[Platform(Exclude = "Linux", Reason = "Keyman not supported on Linux")]
		public void Keyman6_GetKeyboards_GivesAtLeastOneAndOnlyKeyman6Ones()
		{
			RequiresKeyman6();
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6);
			Assert.Greater(keyboards.Count, 0);
			foreach (KeyboardController.KeyboardDescriptor keyboard in keyboards)
			{
				Assert.AreEqual(KeyboardController.Engines.Keyman6, keyboard.engine);
			}
		}

		[Test]
		[Category("Keyman6")]
		[Platform(Exclude = "Linux", Reason = "Doesn't need to run on Linux")]
		public void Keyman6_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresKeyman6();
			RequiresWindow();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0];
			Application.DoEvents(); //required
			Keyboard.Controller.SetKeyboard(d.ShortName);
			Application.DoEvents(); //required
			Assert.AreEqual(d.ShortName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Keyman6")]
		[Platform(Exclude = "Linux", Reason = "Doesn't need to run on Linux")]
		public void Keyman6_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresKeyman6();
			var d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0];
			Keyboard.Controller.SetKeyboard(d.ShortName);
			Application.DoEvents();//required
			KeyboardController.DeactivateKeyboard();
			Application.DoEvents();//required
			Assert.AreNotEqual(d.ShortName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Keyman7")]
		[Platform(Exclude = "Linux", Reason = "Doesn't need to run on Linux")]
		public void Keyman7_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresKeyman7();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0];
			Keyboard.Controller.SetKeyboard(d.ShortName);
			Application.DoEvents();//required
			Assert.AreEqual(d.ShortName, KeyboardController.GetActiveKeyboard());
		}


		[Test]
		[Category("Keyman7")]
		[Platform(Exclude = "Linux", Reason = "Doesn't need to run on Linux")]
		public void Keyman7_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresKeyman7();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0];
			Keyboard.Controller.SetKeyboard(d.ShortName);
			Application.DoEvents();//required
			KeyboardController.DeactivateKeyboard();
			Application.DoEvents();//required
			Assert.AreNotEqual(d.ShortName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Keyman7")]
		[Platform(Exclude = "Linux", Reason = "Doesn't need to run on Linux")]
		public void Keyman7_GetKeyboards_GivesAtLeastOneAndOnlyKeyman7Ones()
		{
			RequiresKeyman7();
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7);
			Assert.Greater(keyboards.Count, 0);
			foreach (KeyboardController.KeyboardDescriptor keyboard in keyboards)
			{
				Assert.AreEqual(KeyboardController.Engines.Keyman7, keyboard.engine);
			}
		}

		 /// <summary>
		/// The main thing here is that it doesn't crash doing a LoadLibrary()
		/// </summary>
		[Test]
		public void NoKeyman7_GetKeyboards_DoesNotCrash()
		{
		   KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7);
		}

		private static void RequiresKeyman6()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Keyman6),
						  "Keyman 6 Not available");

		}
		private static void RequiresKeyman7()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Keyman7),
						  "Keyman 7 Not available");
		}
#endif
	}
}
#endif