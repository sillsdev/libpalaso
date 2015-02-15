#if __MonoCS__
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.Reporting;
using SIL.WritingSystems.WindowsForms.Keyboarding;
using SIL.WritingSystems.WindowsForms.Keyboarding.Linux;
using SIL.WritingSystems;

namespace SIL.WritingSystems.WindowsForms.Tests.Keyboarding
{
	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class LinuxKeyboardControllerTests
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

		[TearDown]
		public void Teardown()
		{
			if (_window != null)
			{
				_window.Close();
				Application.DoEvents();
				_window.Dispose();
				_window = null;
			}
		}

		private void RequiresWindowForFocus()
		{
			_window = new Form();
			var box = new TextBox();
			box.Dock = DockStyle.Fill;
			_window.Controls.Add(box);

			_window.Show();
			box.Select();
			Application.DoEvents();
		}

		[Test]
		public void GetAllKeyboards_GivesSeveral()
		{
			IKeyboardDefinition[] keyboards = Keyboard.Controller.AllAvailableKeyboards.ToArray();
			Assert.Greater(keyboards.Length, 1, "This test requires that the Windows IME has at least two languages installed.");
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
			Keyboard.Controller.SetKeyboard(keyboardName);
		}

#if WANT_PORT
		/// <summary>
		/// The main thing here is that it doesn't crash doing a LoadLibrary()
		/// </summary>
		[Test]
		public void NoKeyman7_GetKeyboards_DoesNotCrash()
		{
		   KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Keyman7);
		}

		[Test]
		[Ignore("SCIM deprecated")]
		public void EngineAvailable_ScimIsSetUpAndConfiguredCorrectly_ReturnsTrue()
		{
			Assert.IsTrue(KeyboardController.EngineAvailable(KeyboardController.Engines.Scim));
		}

		[Test]
		[Ignore("SCIM deprecated")]
		public void GetActiveKeyboard_ScimIsSetUpAndConfiguredToDefault_ReturnsEnglishKeyboard()
		{
			RequiresWindowForFocus();
			ResetKeyboardToDefault();
			Assert.AreEqual("English/Keyboard", KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Ignore("SCIM deprecated")]
		public void KeyboardDescriptors_ScimIsSetUpAndConfiguredToDefault_3KeyboardsReturned()
		{
			List<KeyboardController.KeyboardDescriptor> availableKeyboards = KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.Scim);
			Assert.AreEqual("English/European", availableKeyboards[0].ShortName);
			Assert.AreEqual("RAW CODE", availableKeyboards[1].ShortName);
			Assert.AreEqual("English/Keyboard", availableKeyboards[2].ShortName);
		}

		[Test]
		[Ignore("SCIM deprecated")]
		public void Deactivate_ScimIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			RequiresWindowForFocus();
			Keyboard.Controller.SetKeyboard("English/European");
			KeyboardController.DeactivateKeyboard();
			Assert.AreEqual("English/Keyboard", KeyboardController.GetActiveKeyboard());
		}

		[Test]
		[Ignore("SCIM deprecated")]
		public void ActivateKeyBoard_ScimHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			RequiresWindowForFocus();
			ResetKeyboardToDefault();
			Keyboard.Controller.SetKeyboard("English/European");
			Assert.AreEqual("English/European", KeyboardController.GetActiveKeyboard());
			ResetKeyboardToDefault();
		}

		[Test]
		[Ignore("SCIM deprecated")]
		public void ActivateKeyBoard_ScimDoesNotHaveKeyboard_Throws()
		{
			Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
				() => Keyboard.Controller.SetKeyboard("Nonexistent Keyboard")
			);
		}

		[Test]
		[Ignore("SCIM deprecated")]
		[Category("No IM Running")]
		public void GetAvailableKeyboards_NoIMRunning_ReturnsEmptyList()
		{
			var availableKeyboards = Keyboard.Controller.AllAvailableKeyboards.Where(kbd => kbd is KeyboardDescription && ((KeyboardDescription)kbd).Engine == "SCIM");
			Assert.AreEqual(0, availableKeyboards.Count());
		}

#endif

		private static void ResetKeyboardToDefault()
		{
			Keyboard.Controller.ActivateDefaultKeyboard();
		}

		[Test]
		[Category("No IM Running")]
		public void Deactivate_NoIMRunning_DoesNotThrow()
		{
			Keyboard.Controller.ActivateDefaultKeyboard();
		}

#if WANT_PORT
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
#endif

		[Test]
		[Category("IBus")]
		public void Deactivate_IBusIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			if (Keyboard.Controller.AllAvailableKeyboards.Count(kbd => kbd.Layout == "m17n:am:sera") <= 0)
				Assert.Ignore("Can't run this test without ibus keyboard 'm17n:am:sera' being installed.");

			// needed for focus
			RequiresWindowForFocus();

			Keyboard.Controller.SetKeyboard("m17n:am:sera");
			Keyboard.Controller.ActivateDefaultKeyboard();
			Assert.AreEqual("m17n:am:sera", Keyboard.Controller.ActiveKeyboard);
		}

		[Test]
		[Category("IBus")]
		public void ActivateKeyBoard_IBusHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			if (Keyboard.Controller.AllAvailableKeyboards.Count(kbd => kbd.Layout == "m17n:am:sera") <= 0)
				Assert.Ignore("Can't run this test without ibus keyboard 'm17n:am:sera' being installed.");

			// needed for focus
			RequiresWindowForFocus();

			Keyboard.Controller.ActivateDefaultKeyboard();
			Keyboard.Controller.SetKeyboard("m17n:am:sera");
			Assert.AreEqual("m17n:am:sera", Keyboard.Controller.ActiveKeyboard);
			Keyboard.Controller.ActivateDefaultKeyboard();
		}

		[Test]
		[Category("IBus")]
		public void ActivateKeyBoard_IBusDoesNotHaveKeyboard_Throws()
		{
			// needed for focus
			RequiresWindowForFocus();
			Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
				() => Keyboard.Controller.SetKeyboard("Nonexistent Keyboard")
			);
		}

		[Test]
		public void CreateKeyboardDefinition_NewKeyboard_ReturnsNewObject()
		{
			// REVIEW: adjust this test
			IKeyboardDefinition keyboard = Keyboard.Controller.CreateKeyboardDefinition("en-US_foo", KeyboardFormat.Unknown, null);
			Assert.That(keyboard, Is.Not.Null);
			Assert.That(keyboard, Is.TypeOf<XkbKeyboardDescription>());
		}

	}
}
#endif
