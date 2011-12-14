#if MONO
using System;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Keyboarding;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{

	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class IBusAdaptorTestsWithoutIBus
	{

		[Test]
		[Category("IBus")]
		public void EngineAvailable_IBusNotRunning_ReturnsFalse()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.IsFalse(IBusAdaptor.EngineAvailable);
			}
		}

		[Test]
		[Category("IBus")]
		public void GetActiveKeyboard_IBusNotRunning_ThrowsProblemNotificationSentToUser()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
					() => IBusAdaptor.GetActiveKeyboard()
				);
			}
		}

		[Test]
		[Category("IBus")]
		public void OpenConnection_IBusNotRunning_ThrowsProblemNotificationSentToUser()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
					() => IBusAdaptor.OpenConnection()
				);
			}
		}

		[Test]
		[Category("IBus")]
		public void KeyboardDescriptors_IBusNotRunning_DoesNotThrow()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.DoesNotThrow(
					() => { var keyboards = IBusAdaptor.KeyboardDescriptors; }
				);
			}
		}

		[Test]
		[Category("IBus")]
		public void Deactivate_IBusNotRunning_DoesNotThrow()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.DoesNotThrow(
					() => IBusAdaptor.Deactivate()
				);
			}
		}

		[Test]
		[Category("IBus")]
		public void ActivateKeyBoard_IBusNotRunning_ThrowsProblemNotificationSentToUser()
		{
			using (var e = new IBusEnvironmentForTest(true))
			{
				Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
					() => IBusAdaptor.ActivateKeyboard(IBusEnvironmentForTest.OtherKeyboard)
				);
			}
		}
	}
}

#endif