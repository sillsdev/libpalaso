using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class KeyboardControllerTests
	{
		[SetUp]
		public void Setup()
		{
			Palaso.Reporting.ErrorReport.IsOkToInteractWithUser = false;
		}

		[Test]
		public void GetAllKeyboards_GivesSeveral()
		{
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All);
			Assert.Greater(keyboards.Count, 2);
		}

		[Test, ExpectedException(typeof(ErrorReport.NonFatalMessageSentToUserException))]
		public void ActivateKeyboard_BogusName_RaisesMessageBox()
		{
			KeyboardController.ActivateKeyboard("foobar");
		}

		[Test]
		public void WindowsIME_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresWindowsIME();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Windows)[0];
			KeyboardController.ActivateKeyboard(d.name);
			Assert.AreEqual(d.name, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		public void WindowsIME_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresWindowsIME();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Windows)[1];
			KeyboardController.ActivateKeyboard(d.name);
			KeyboardController.DeactivateKeyboard();
			Assert.AreNotEqual(d.name, KeyboardController.GetActiveKeyboard());
		}
	   [Test]
		public void WindowsIME_GetKeyboards_GivesSeveralButOnlyWindowsOnes()
		{
			Assert.AreEqual(PlatformID.Win32NT, Environment.OSVersion.Platform);
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Windows);
			Assert.Greater(keyboards.Count, 2);
			foreach (KeyboardController.KeyboardDescriptor keyboard in keyboards)
			{
				Assert.AreEqual(KeyboardController.Engines.Windows, keyboard.engine);
			}
		}


		[Test, Ignore("must have keyman 6")]
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


		[Test, Ignore("must have keyman 6")]
		public void Keyman6_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresKeyman6();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0];
			KeyboardController.ActivateKeyboard(d.name);
			Assert.AreEqual(d.name, KeyboardController.GetActiveKeyboard());
		}

		[Test, Ignore("must have keyman 6")]
		public void Keyman6_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresKeyman6();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0];
			KeyboardController.ActivateKeyboard(d.name);
			KeyboardController.DeactivateKeyboard();
			Assert.AreNotEqual(d.name, KeyboardController.GetActiveKeyboard());
		}

		[Test, Ignore("must have keyman 7")]
		public void Keyman7_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresKeyman7();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0];
			KeyboardController.ActivateKeyboard(d.name);
			Assert.AreEqual(d.name, KeyboardController.GetActiveKeyboard());
		}


		[Test, Ignore("must have keyman 7")]
		public void Keyman7_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresKeyman7();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0];
			KeyboardController.ActivateKeyboard(d.name);
			KeyboardController.DeactivateKeyboard();
			Assert.AreNotEqual(d.name, KeyboardController.GetActiveKeyboard());
		}

		[Test, Ignore("must have keyman 7")]
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


		private void RequiresWindowsIME()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Windows),
						  "Windows IME Not available");
		}

		private void RequiresKeyman6()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Keyman6),
						  "Keyman 6 Not available");

			Assert.IsTrue(Environment.OSVersion.Version.Major < 5,
						  "Keyman 6 tests are unreliable on OS's later than Windows XP");

		}
		private void RequiresKeyman7()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Keyman7),
						  "Keyman 7 Not available");
		}
	}
}