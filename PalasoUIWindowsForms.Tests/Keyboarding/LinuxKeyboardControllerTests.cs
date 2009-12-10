#if MONO
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class LinuxKeyboardControllerTests
	{
		private Form _window;

		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
		}

		private void RequiresWindowForFocus()
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
			List<KeyboardController.KeyboardDescriptor> keyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All);
			Assert.Greater(keyboards.Count, 1, "This test requires that the Windows IME has at least two languages installed.");
		}

		[Test, ExpectedException(typeof(ErrorReport.ProblemNotificationSentToUserException))]
		public void ActivateKeyboard_BogusName_RaisesMessageBox()
		{
			KeyboardController.ActivateKeyboard("foobar");
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

		/// <summary>
		/// The main thing here is that it doesn't crash doing a LoadLibrary()
		/// </summary>
		[Test]
		public void NoKeyman7_GetKeyboards_DoesNotCrash()
		{
		   KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7);
		}

		[Test]
		[Category("Scim")]
		public void EngineAvailable_ScimIsSetUpAndConfiguredCorrectly_ReturnsTrue()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Scim));
		}

		[Test]
		[Category("Scim")]
		public void GetActiveKeyboard_ScimIsSetUpAndConfiguredToDefault_ReturnsEnglishKeyboard()
		{
			RequiresWindowForFocus();
			ResetKeyboardToDefault();
			Assert.AreEqual("English/Keyboard", KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Scim")]
		public void KeyboardDescriptors_ScimIsSetUpAndConfiguredToDefault_3KeyboardsReturned()
		{
			List<KeyboardController.KeyboardDescriptor> availableKeyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Scim);
			Assert.AreEqual("English/European", availableKeyboards[0].Name);
			Assert.AreEqual("RAW CODE", availableKeyboards[1].Name);
			Assert.AreEqual("English/Keyboard", availableKeyboards[2].Name);
		}

		[Test]
		[Category("Scim")]
		public void Deactivate_ScimIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			RequiresWindowForFocus();
			KeyboardController.ActivateKeyboard("English/European");
			KeyboardController.DeactivateKeyboard();
			Assert.AreEqual("English/Keyboard", KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("Scim")]
		public void ActivateKeyBoard_ScimHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			RequiresWindowForFocus();
			ResetKeyboardToDefault();
			KeyboardController.ActivateKeyboard("English/European");
			Assert.AreEqual("English/European", KeyboardController.GetActiveKeyboard());
			ResetKeyboardToDefault();
		}

		[Test]
		[Category("Scim")]
		[ExpectedException( typeof(Palaso.Reporting.ErrorReport.ProblemNotificationSentToUserException))]
		public void ActivateKeyBoard_ScimDoesNotHaveKeyboard_Throws()
		{
			KeyboardController.ActivateKeyboard("Nonexistant Keyboard");
		}

		private void ResetKeyboardToDefault()
		{
			KeyboardController.DeactivateKeyboard();
		}

		[Test]
		[Category("No IM Running")]
		public void Deactivate_NoIMRunning_DoesNotThrow()
		{
			KeyboardController.DeactivateKeyboard();
		}

		[Test]
		[Category("No IM Running")]
		public void GetAvailableKeyboards_NoIMRunning_ReturnsEmptyList()
		{
			List<KeyboardController.KeyboardDescriptor> availableKeyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Scim);
			Assert.AreEqual(0, availableKeyboards.Count);
		}

		[Test]
		[Category("Scim not Running")]
		public void EngineAvailable_ScimIsnotRunning_returnsFalse()
		{
			Assert.IsFalse(KeyboardController.EngineAvailable(KeyboardController.Engines.Scim));
		}

		[Test]
		[Category("IBus not Running")]
		public void EngineAvailable_IBusIsnotRunning_returnsFalse()
		{
			Assert.IsFalse(KeyboardController.EngineAvailable(KeyboardController.Engines.IBus));
		}

		[Test]
		[Category("IBus")]
		public void EngineAvailable_IBusIsSetUpAndConfiguredCorrectly_ReturnsTrue()
		{
			// needed for focus
			RequiresWindowForFocus();

			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.IBus));
		}

		[Test]
		[Category("IBus")]
		[ExpectedException( typeof(Palaso.Reporting.ErrorReport.ProblemNotificationSentToUserException))]
		public void GetActiveKeyboard_IBusIsSetUpAndConfiguredToDefault_ReturnsEnglishKeyboard()
		{
			// needed for focus
			RequiresWindowForFocus();

			KeyboardController.DeactivateKeyboard();
			KeyboardController.GetActiveKeyboard();
		}

		[Test]
		[Category("IBus")]
		public void KeyboardDescriptors_IBusIsSetUpAndConfiguredToDefault_0KeyboardsReturned()
		{
			// needed for focus
			RequiresWindowForFocus();

			List<KeyboardController.KeyboardDescriptor> availableKeyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.IBus);

			// Assuming default ibus install doesn't have any active keyboards
			Assert.AreEqual(0, availableKeyboards.Count);
		}

		[Test]
		[Category("IBus")]
		public void Deactivate_IBusIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			// needed for focus
			RequiresWindowForFocus();

			KeyboardController.ActivateKeyboard("am:sera");
			KeyboardController.DeactivateKeyboard();
			Assert.AreEqual("am:sera", KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Category("IBus")]
		public void ActivateKeyBoard_IBusHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			// needed for focus
			RequiresWindowForFocus();

			KeyboardController.DeactivateKeyboard();
			KeyboardController.ActivateKeyboard("am:sera");
			Assert.AreEqual("am:sera", KeyboardController.GetActiveKeyboard());
			KeyboardController.DeactivateKeyboard();
		}

		[Test]
		[Category("IBus")]
		[ExpectedException( typeof(Palaso.Reporting.ErrorReport.ProblemNotificationSentToUserException))]
		public void ActivateKeyBoard_IBusDoesNotHaveKeyboard_Throws()
		{
			// needed for focus
			RequiresWindowForFocus();

			KeyboardController.ActivateKeyboard("Nonexistant Keyboard");
		}
	}
}
#endif