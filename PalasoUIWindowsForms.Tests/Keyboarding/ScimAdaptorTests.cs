using System;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;
using System.Text;
using System.Collections.Generic;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class ScimAdaptorTests
	{
		[Test]
		[Category("Scim")]
		public void EngineAvailable_ScimIsSetUpAndConfiguredCorrectly_ReturnsTrue()
		{
			Assert.IsTrue(ScimAdaptor.EngineAvailable);
		}

		[Test]
		[Category("Scim")]
		public void GetActiveKeyboard_ScimIsSetUpAndConfiguredToDefault_ReturnsEnglishKeyboard()
		{
			ResetKeyboardToDefault();
			Assert.AreEqual("English/Keyboard", ScimAdaptor.GetActiveKeyboard());
		}

		[Test]
		[Category("Scim")]
		public void KeyboardDescriptors_ScimIsSetUpAndConfiguredToDefault_3KeyboardsReturned()
		{
			Assert.AreEqual("English/European", ScimAdaptor.KeyboardDescriptors[0].Name);
			Assert.AreEqual("RAW CODE", ScimAdaptor.KeyboardDescriptors[1].Name);
			Assert.AreEqual("English/Keyboard", ScimAdaptor.KeyboardDescriptors[2].Name);
		}

		[Test]
		[Category("Scim")]
		public void HasKeyboardNamed_ScimHasKeyboard_ReturnsTrue()
		{
			Assert.IsTrue(ScimAdaptor.HasKeyboardNamed("English/Keyboard"));
		}

		[Test]
		[Category("Scim")]
		public void HasKeyboardNamed_ScimDoesNotHaveKeyboard_ReturnsFalse()
		{
			Assert.IsFalse(ScimAdaptor.HasKeyboardNamed("Nonexistant Keyboard"));
		}

		[Test]
		[Category("Scim")]
		public void Deactivate_ScimIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			ScimAdaptor.ActivateKeyboard("English/European");
			ScimAdaptor.Deactivate();
			Assert.AreEqual("English/Keyboard", ScimAdaptor.GetActiveKeyboard());
		}

		[Test]
		[Category("Scim")]
		public void ActivateKeyBoard_ScimHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			ResetKeyboardToDefault();
			ScimAdaptor.ActivateKeyboard("English/European");
			Assert.AreEqual("English/European", ScimAdaptor.GetActiveKeyboard());
			ResetKeyboardToDefault();
		}

		[Test]
		[Category("Scim")]
		[ExpectedException( typeof(ArgumentOutOfRangeException))]
		public void ActivateKeyBoard_ScimDoesNotHaveKeyboard_Throws()
		{
			ScimAdaptor.ActivateKeyboard("Nonexistant Keyboard");
		}

		private void ResetKeyboardToDefault()
		{
			ScimAdaptor.Deactivate();
		}

		[Test]
		[Category("Scim not Running")]
		public void Deactivate_ScimIsNotRunning_DoesNotThrow()
		{
			ScimAdaptor.Deactivate();
		}

		[Test]
		[Category("Scim not Running")]
		public void ActivateKeyBoard_ScimIsNotRunning_DoesNotThrow()
		{
			ScimAdaptor.ActivateKeyboard("English/Keyboard");
		}

		[Test]
		[Category("Scim not Running")]
		public void KeyboardDescriptors_ScimIsNotRunning_ReturnsEmptyList()
		{
			List<KeyboardController.KeyboardDescriptor> availableKeyboards = ScimAdaptor.KeyboardDescriptors;
			Assert.AreEqual(0, availableKeyboards.Count);
		}

		[Test]
		[Category("Scim not Running")]
		public void GetActiveKeyboard_ScimIsNotRunning_ReturnsEmptyString()
		{
			string activeKeyboard = ScimAdaptor.GetActiveKeyboard();
			Assert.IsEmpty(activeKeyboard);
		}

		[Test]
		[Category("Scim not Running")]
		public void EngineAvailable_ScimIsnotRunning_returnsFalse()
		{
			Assert.IsFalse(ScimAdaptor.EngineAvailable);
		}

		[Test]
		[Category("Scim not Running")]
		public void HasKeyboardNamed_ScimIsNotRunning_ReturnsFalse()
		{
			Assert.IsFalse(ScimAdaptor.HasKeyboardNamed("English/Keyboard"));
		}
	}
}
