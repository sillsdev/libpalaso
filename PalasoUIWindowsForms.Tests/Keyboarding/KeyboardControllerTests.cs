using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class KeyboardControllerTests
	{
		private Form _window;

		[SetUp]
		public void Setup()
		{
			Palaso.Reporting.ErrorReport.IsOkToInteractWithUser = false;
		}

		private void RequiresWindow()
		{
			_window = new System.Windows.Forms.Form();
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
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All);
			Assert.Greater(keyboards.Count, 1, "This test requires that the Windows IME has at least two languages installed.");
		}

		[Test, ExpectedException(typeof(ErrorReport.NonFatalMessageSentToUserException))]
		public void ActivateKeyboard_BogusName_RaisesMessageBox()
		{
			KeyboardController.ActivateKeyboard("foobar");
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresWindowsIME();
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Windows);
			Assert.Greater(keyboards.Count, 0, "This test requires that the Windows IME has at least one language installed.");
			KeyboardController.KeyboardDescriptor d = keyboards[0];
			KeyboardController.ActivateKeyboard(d.Name);
			Assert.AreEqual(d.Name, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Windows IME")]
		public void WindowsIME_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresWindowsIME();
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Windows);
			Assert.Greater(keyboards.Count, 1, "This test requires that the Windows IME has at least two languages installed.");
			KeyboardController.KeyboardDescriptor d = keyboards[1];
			KeyboardController.ActivateKeyboard(d.Name);
			KeyboardController.DeactivateKeyboard();
			Assert.AreNotEqual(d.Name, KeyboardController.GetActiveKeyboard());
		}
		[Test]
		[Category("Windows IME")]
		public void WindowsIME_GetKeyboards_GivesSeveralButOnlyWindowsOnes()
		{
			RequiresWindowsIME();
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Windows);
			Assert.Greater(keyboards.Count, 1, "This test requires that the Windows IME has at least two languages installed.");
			foreach (KeyboardController.KeyboardDescriptor keyboard in keyboards)
			{
				Assert.AreEqual(KeyboardController.Engines.Windows, keyboard.engine);
			}
		}


		[Test]
		[Category("Keyman6")]
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
		public void Keyman6_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresKeyman6();
			RequiresWindow();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0];
			Application.DoEvents();//required
			KeyboardController.ActivateKeyboard(d.Name);
			Application.DoEvents();//required
			Assert.AreEqual(d.Name, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Keyman6")]
		public void Keyman6_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresKeyman6();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman6)[0];
			KeyboardController.ActivateKeyboard(d.Name);
			Application.DoEvents();//required
			KeyboardController.DeactivateKeyboard();
			Application.DoEvents();//required
			Assert.AreNotEqual(d.Name, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Keyman7")]
		public void Keyman7_ActivateKeyboard_ReportsItWasActivated()
		{
			RequiresKeyman7();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0];
			KeyboardController.ActivateKeyboard(d.Name);
			Application.DoEvents();//required
			Assert.AreEqual(d.Name, KeyboardController.GetActiveKeyboard());
		}


		[Test]
		[Category("Keyman7")]
		public void Keyman7_DeActivateKeyboard_RevertsToDefault()
		{
			RequiresKeyman7();
			KeyboardController.KeyboardDescriptor d = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7)[0];
			KeyboardController.ActivateKeyboard(d.Name);
			Application.DoEvents();//required
			KeyboardController.DeactivateKeyboard();
			Application.DoEvents();//required
			Assert.AreNotEqual(d.Name, KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Keyman7")]
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

		}
		private void RequiresKeyman7()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Keyman7),
						  "Keyman 7 Not available");
		}
	}
}