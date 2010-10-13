#if !MONO
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.Keyboarding;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class WindowsKeyboardControllerTests
	{
		private Form _window;

		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
		}

		private void RequiresWindow()
		{
			_window = new Form();
			TextBox box = new TextBox();
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
			List<KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(Engines.All);
			Assert.Greater(keyboards.Count, 1, "This test requires that the Windows IME has at least two languages installed.");
		}

		[Test]
		public void ActivateKeyboard_BogusName_RaisesMessageBox()
		{
			Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
				() => KeyboardController.ActivateKeyboard("foobar")
			);
		}

		[Test]
		public void ActivateKeyboard_BogusName_SecondTimeNoLongerRaisesMessageBox()
		{
			// the keyboardName for this test and above need to be different
			string keyboardName = "This should never be the same as the name of an installed keyboard";
			try
			{
				KeyboardController.ActivateKeyboard(keyboardName);
				Assert.Fail("Should have thrown exception but didn't.");
			}
			catch (ErrorReport.ProblemNotificationSentToUserException)
			{

			}
			KeyboardController.ActivateKeyboard(keyboardName);
		}

		[Test]
		[NUnit.Framework.Category("Windows IME")]
		public void WindowsIME_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresWindowsIME();
			List<KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(Engines.Windows);
			Assert.Greater(keyboards.Count, 0, "This test requires that the Windows IME has at least one language installed.");
			KeyboardDescriptor d = keyboards[0];
			KeyboardController.ActivateKeyboard(d.KeyboardName);
			Assert.AreEqual(d.KeyboardName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[NUnit.Framework.Category("Windows IME")]
		public void WindowsIME_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresWindowsIME();
			List<KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(Engines.Windows);
			Assert.Greater(keyboards.Count, 1, "This test requires that the Windows IME has at least two languages installed.");
			KeyboardDescriptor d = keyboards[1];
			KeyboardController.ActivateKeyboard(d.KeyboardName);
			KeyboardController.DeactivateKeyboard();
			Assert.AreNotEqual(d.KeyboardName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[NUnit.Framework.Category("Windows IME")]
		public void WindowsIME_GetKeyboards_GivesSeveralButOnlyWindowsOnes()
		{
			RequiresWindowsIME();
			List<KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(Engines.Windows);
			Assert.Greater(keyboards.Count, 1, "This test requires that the Windows IME has at least two languages installed.");
			foreach (KeyboardDescriptor keyboard in keyboards)
			{
				Assert.AreEqual(Engines.Windows, keyboard.KeyboardingEngine);
			}
		}

		[Test]
		[NUnit.Framework.Category("Windows IME")]
		public void WindowsIME_ActivateKeyboard_KeyboardDescriptorHasNoId_SetsKeyboardByName()
		{
			RequiresWindowsIME();
			KeyboardDescriptor availableKeyboard = KeyboardController.GetAvailableKeyboards(Engines.Windows)[0];
			KeyboardDescriptor keyboardWithNoId = new KeyboardDescriptor(availableKeyboard.KeyboardName, Engines.Windows, "");
			KeyboardController.ActivateKeyboard(keyboardWithNoId);
			Assert.AreEqual(availableKeyboard.KeyboardName, KeyboardController.GetActiveKeyboardDescriptor().KeyboardName);
		}

		[Test]
		[NUnit.Framework.Category("Windows IME")]
		public void WindowsIME_ActivateKeyboard_KeyboardDescriptorHasMalformedId_SetsKeyboardByName()
		{
			RequiresWindowsIME();
			KeyboardDescriptor availableKeyboard = KeyboardController.GetAvailableKeyboards(Engines.Windows)[0];
			KeyboardDescriptor keyboardWithGarbledId = new KeyboardDescriptor(availableKeyboard.KeyboardName, Engines.Windows, "Garb123led");
			KeyboardController.ActivateKeyboard(keyboardWithGarbledId);
			Assert.AreEqual(availableKeyboard.KeyboardName, KeyboardController.GetActiveKeyboardDescriptor().KeyboardName);
		}

		[Test]
		[NUnit.Framework.Category("Windows IME")]
		public void WindowsIME_ActivateKeyboard_KeyboardIsFromUnknownEngine_SetsByName()
		{
			RequiresWindowsIME();
			KeyboardDescriptor availableKeyboard = KeyboardController.GetAvailableKeyboards(Engines.Windows)[0];
			KeyboardDescriptor keyboardFromUnknownEngine = new KeyboardDescriptor(availableKeyboard.KeyboardName, Engines.Unknown, "");
			KeyboardController.ActivateKeyboard(keyboardFromUnknownEngine);
			Assert.AreEqual(availableKeyboard.KeyboardName, KeyboardController.GetActiveKeyboardDescriptor().KeyboardName);
		}


		[Test]
		[NUnit.Framework.Category("Keyman6")]
		public void Keyman6_GetKeyboards_GivesAtLeastOneAndOnlyKeyman6Ones()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return; // doesn't need to run on Unix
			}
			RequiresKeyman6();
			List<KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(Engines.Keyman6);
			Assert.Greater(keyboards.Count, 0);
			foreach (KeyboardDescriptor keyboard in keyboards)
			{
				Assert.AreEqual(Engines.Keyman6, keyboard.KeyboardingEngine);
			}
		}

		[Test]
		[NUnit.Framework.Category("Keyman6")]
		public void Keyman6_ActivateKeyboard_ReportsItWasActivated()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return; // doesn't need to run on Unix
			}
			RequiresKeyman6();
			RequiresWindow();
			KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(Engines.Keyman6)[0];
			Application.DoEvents();//required
			KeyboardController.ActivateKeyboard(d.KeyboardName);
			Application.DoEvents();//required
			Assert.AreEqual(d.KeyboardName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[NUnit.Framework.Category("Keyman6")]
		public void Keyman6_DeActivateKeyboard_RevertsToDefault()
		{
			if(Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return; // doesn't need to run on Unix
			}

			RequiresKeyman6();
			KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(Engines.Keyman6)[0];
			KeyboardController.ActivateKeyboard(d.KeyboardName);
			Application.DoEvents();//required
			KeyboardController.DeactivateKeyboard();
			Application.DoEvents();//required
			Assert.AreNotEqual(d.KeyboardName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[NUnit.Framework.Category("Keyman7")]
		public void Keyman7_ActivateKeyboard_ReportsItWasActivated()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return; // doesn't need to run on Unix
			}
			RequiresKeyman7();
			KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(Engines.Keyman7)[0];
			KeyboardController.ActivateKeyboard(d.KeyboardName);
			Application.DoEvents();//required
			Assert.AreEqual(d.KeyboardName, KeyboardController.GetActiveKeyboard());
		}


		[Test]
		[NUnit.Framework.Category("Keyman7")]
		public void Keyman7_DeActivateKeyboard_RevertsToDefault()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return; // doesn't need to run on Unix
			}
			RequiresKeyman7();
			KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(Engines.Keyman7)[0];
			KeyboardController.ActivateKeyboard(d.KeyboardName);
			Application.DoEvents();//required
			KeyboardController.DeactivateKeyboard();
			Application.DoEvents();//required
			Assert.AreNotEqual(d.KeyboardName, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[NUnit.Framework.Category("Keyman7")]
		public void Keyman7_GetKeyboards_GivesAtLeastOneAndOnlyKeyman7Ones()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return; // doesn't need to run on Unix
			}
			RequiresKeyman7();
			List<KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(Engines.Keyman7);
			Assert.Greater(keyboards.Count, 0);
			foreach (KeyboardDescriptor keyboard in keyboards)
			{
				Assert.AreEqual(Engines.Keyman7, keyboard.KeyboardingEngine);
			}
		}

		 /// <summary>
		/// The main thing here is that it doesn't crash doing a LoadLibrary()
		/// </summary>
		[Test]
		public void NoKeyman7_GetKeyboards_DoesNotCrash()
		{
		   KeyboardController.GetAvailableKeyboards(Engines.Keyman7);
		}


		private static void RequiresWindowsIME()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(Engines.Windows),
						  "Windows IME Not available");
		}

		private static void RequiresKeyman6()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(Engines.Keyman6),
						  "Keyman 6 Not available");

		}
		private static void RequiresKeyman7()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(Engines.Keyman7),
						  "Keyman 7 Not available");
		}

	}
}
#endif