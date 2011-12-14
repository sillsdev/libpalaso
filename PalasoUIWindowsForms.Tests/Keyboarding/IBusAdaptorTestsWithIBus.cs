#if MONO
using System;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;
using System.Collections.Generic;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{
	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class IBusAdaptorTestsWithIBus
	{

		[Test]
		[Category("IBus")]
		public void EngineAvailable_ReturnsTrue()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.IsTrue(IBusAdaptor.EngineAvailable);
			}
		}

		[Test]
		[Category("IBus")]
		public void KeyboardDescriptors_ListAllKeyboards()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Console.WriteLine("ListAllKeyboards");
				foreach (var keyboard in IBusAdaptor.KeyboardDescriptors)
				{
					Console.WriteLine("Name {0}, Id {1}", keyboard.ShortName, keyboard.Id);
				}
			}
		}

		[Test]
		[Category("IBus")]
		// This will fail because Deactivate does nothing
		public void Deactivate_SwitchesBackToDefaultKeyboard()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				IBusAdaptor.ActivateKeyboard(IBusEnvironmentForTest.OtherKeyboard);
				IBusAdaptor.Deactivate();
				string actual = IBusAdaptor.GetActiveKeyboard();
				Assert.AreEqual(IBusEnvironmentForTest.DefaultKeyboard, actual);
			}
		}

		[Test]
		[Category("IBus")]
		public void ActivateKeyBoard_IBusHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				IBusAdaptor.ActivateKeyboard(IBusEnvironmentForTest.OtherKeyboard);
				string actual = IBusAdaptor.GetActiveKeyboard();
				Assert.AreEqual(IBusEnvironmentForTest.DefaultKeyboard, actual);
			}
		}

		[Test]
		[Category("IBus")]
		public void ActivateKeyBoard_IBusDoesNotHaveKeyboard_Throws()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.Throws<ArgumentOutOfRangeException>(
					() => IBusAdaptor.ActivateKeyboard("Nonexistent Keyboard")
					);
			}
		}

		[Test]
		[Category("IBus")]
		public void DefaultKeyboardName_AfterOpenConnection_ReturnsEnglish()
		{
			IBusAdaptor.EnsureConnection();
			string actual = IBusAdaptor.DefaultKeyboardName;
			Assert.AreEqual("m17n:en:ispell", actual); // An assumption.  Should actually be the zeroth element in the list of available keyboards.
		}
	}
}

#endif