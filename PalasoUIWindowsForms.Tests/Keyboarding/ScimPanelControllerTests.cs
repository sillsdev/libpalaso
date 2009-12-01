using System;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;
using System.Collections.Generic;

#if MONO

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class ScimPanelControllerTests
	{
		[Test]
		[Category("Scim")]
		public void EngineAvailable_ScimIsSetUpAndConfiguredCorrectly_ReturnsTrue()
		{
			Assert.IsTrue(ScimPanelController.Singleton.EngineAvailable);
		}

		[Test]
		[Category("Scim")]
		public void GetActiveKeyboard_ScimIsSetUpAndConfiguredToDefault_ReturnsEnglishKeyboard()
		{
			ResetKeyboardToDefault();
			Assert.AreEqual("English/Keyboard", ScimPanelController.Singleton.GetActiveKeyboard());
		}

		[Test]
		[Category("Scim")]
		public void KeyboardDescriptors_ScimIsSetUpAndConfiguredToDefault_3KeyboardsReturned()
		{
			Assert.AreEqual("English/European", ScimPanelController.Singleton.KeyboardDescriptors[0].Name);
			Assert.AreEqual("RAW CODE", ScimPanelController.Singleton.KeyboardDescriptors[1].Name);
			Assert.AreEqual("English/Keyboard", ScimPanelController.Singleton.KeyboardDescriptors[2].Name);
		}

		[Test]
		[Category("Scim")]
		public void HasKeyboardNamed_ScimHasKeyboard_ReturnsTrue()
		{
			Assert.IsTrue(ScimPanelController.Singleton.HasKeyboardNamed("English/Keyboard"));
		}

		[Test]
		[Category("Scim")]
		public void HasKeyboardNamed_ScimDoesNotHaveKeyboard_ReturnsFalse()
		{
			Assert.IsFalse(ScimPanelController.Singleton.HasKeyboardNamed("Nonexistant Keyboard"));
		}

		[Test]
		[Category("Scim")]
		public void Deactivate_ScimIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			ScimPanelController.Singleton.ActivateKeyboard("English/European");
			ScimPanelController.Singleton.Deactivate();
			Assert.AreEqual("English/Keyboard", ScimPanelController.Singleton.GetActiveKeyboard());
		}

		[Test]
		[Category("Scim")]
		public void ActivateKeyBoard_ScimHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			ResetKeyboardToDefault();
			ScimPanelController.Singleton.ActivateKeyboard("English/European");
			Assert.AreEqual("English/European", ScimPanelController.Singleton.GetActiveKeyboard());
			ResetKeyboardToDefault();
		}

		[Test]
		[Category("Scim")]
		[ExpectedException( typeof(ArgumentOutOfRangeException))]
		public void ActivateKeyBoard_ScimDoesNotHaveKeyboard_Throws()
		{
			ScimPanelController.Singleton.ActivateKeyboard("Nonexistant Keyboard");
		}

		[Test]
		[Category("Scim")]
		public void GetCurrentInputContext_ScimIsRunning_ReturnsContext()
		{
			const int unrealisticClientId = -2;
			const int unrealisticContextClientId = -2;

			ScimPanelController.ContextInfo currentContext;
			currentContext.frontendClient = unrealisticClientId;
			currentContext.context = unrealisticContextClientId;
			currentContext = ScimPanelController.Singleton.GetCurrentInputContext();
			Assert.AreNotEqual(unrealisticClientId, currentContext.frontendClient);
			Assert.AreNotEqual(unrealisticContextClientId, currentContext.context);
		}

		private void ResetKeyboardToDefault()
		{
			ScimPanelController.Singleton.Deactivate();
		}

		[Test]
		[Category("Scim not Running")]
		public void Deactivate_ScimIsNotRunning_DoesNotThrow()
		{
			ScimPanelController.Singleton.Deactivate();
		}

		[Test]
		[Category("Scim not Running")]
		public void ActivateKeyBoard_ScimIsNotRunning_DoesNotThrow()
		{
			ScimPanelController.Singleton.ActivateKeyboard("English/Keyboard");
		}

		[Test]
		[Category("Scim not Running")]
		public void KeyboardDescriptors_ScimIsNotRunning_ReturnsEmptyList()
		{
			List<KeyboardController.KeyboardDescriptor> availableKeyboards = ScimPanelController.Singleton.KeyboardDescriptors;
			Assert.AreEqual(0, availableKeyboards.Count);
		}

		[Test]
		[Category("Scim not Running")]
		public void GetActiveKeyboard_ScimIsNotRunning_ReturnsEmptyString()
		{
			string activeKeyboard = ScimPanelController.Singleton.GetActiveKeyboard();
			Assert.IsEmpty(activeKeyboard);
		}

		[Test]
		[Category("Scim not Running")]
		public void EngineAvailable_ScimIsnotRunning_returnsFalse()
		{
			Assert.IsFalse(ScimPanelController.Singleton.EngineAvailable);
		}

		[Test]
		[Category("Scim not Running")]
		public void HasKeyboardNamed_ScimIsNotRunning_ReturnsFalse()
		{
			Assert.IsFalse(ScimPanelController.Singleton.HasKeyboardNamed("English/Keyboard"));
		}
	}
}

#endif