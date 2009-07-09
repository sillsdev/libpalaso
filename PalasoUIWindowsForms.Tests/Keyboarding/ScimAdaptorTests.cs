using System;
using NUnit.Framework;
using Palaso.UI.WindowsForms.Keyboarding;
using System.Text;


namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	public class ScimAdaptorTests
	{
		[Test]
		public void EngineAvailable_ScimIsSetUpAndConfiguredCorrectly_ReturnsTrue()
		{
			Assert.IsTrue(ScimAdaptor.EngineAvailable);
		}

		[Test]
		public void GetActiveKeyboard_ScimIsSetUpAndConfiguredToDefault_ReturnsEnglishKeyboard()
		{
			ResetKeyboardToDefault();
			Assert.AreEqual("English/Keyboard", ScimAdaptor.GetActiveKeyboard());
		}

		[Test]
		public void KeyboardDescriptors_ScimIsSetUpAndConfiguredToDefault_3KeyboardsReturned()
		{
			Assert.AreEqual("English/European", ScimAdaptor.KeyboardDescriptors[0].Name);
			Assert.AreEqual("RAW CODE", ScimAdaptor.KeyboardDescriptors[1].Name);
			Assert.AreEqual("English/Keyboard", ScimAdaptor.KeyboardDescriptors[2].Name);
		}

		[Test]
		public void HasKeyboardNamed_ScimHasKeyboard_ReturnsTrue()
		{
			Assert.IsTrue(ScimAdaptor.HasKeyboardNamed("English/Keyboard"));
		}

		[Test]
		public void HasKeyboardNamed_ScimDoesNotHaveKeyboard_ReturnsFalse()
		{
			Assert.IsFalse(ScimAdaptor.HasKeyboardNamed("Nonexistant Keyboard"));
		}

		[Test]
		public void Deactivate_ScimIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			ScimAdaptor.ActivateKeyboard("English/European");
			ScimAdaptor.Deactivate();
			Assert.AreEqual("English/Keyboard", ScimAdaptor.GetActiveKeyboard());
		}

		[Test]
		public void ActivateKeyBoard_ScimHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			ResetKeyboardToDefault();
			ScimAdaptor.ActivateKeyboard("English/European");
			Assert.AreEqual("English/European", ScimAdaptor.GetActiveKeyboard());
			ResetKeyboardToDefault();
		}

		[Test]
		[ExpectedException( typeof(ArgumentOutOfRangeException))]
		public void ActivateKeyBoard_ScimDoesNotHaveKeyboard_Throws()
		{
			ScimAdaptor.ActivateKeyboard("Nonexistant Keyboard");
		}

		private void ResetKeyboardToDefault()
		{
			ScimAdaptor.Deactivate();
		}

		/*
		[Test]
		public void Deactivate_ScimIsNotRunning_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void ActivateKeyBoard_ScimIsNotRunning_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void KeyboardDescriptors_ScimIsNotRunning_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void GetActiveKeyboard_ScimIsNotRunning_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void EngineAvailable_ScimIsnotRunning_ReturnsFalse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void HasKeyboardNamed_ScimIsNotRunning_Throws()
		{
			throw new NotImplementedException();
		}*/
	}
}
