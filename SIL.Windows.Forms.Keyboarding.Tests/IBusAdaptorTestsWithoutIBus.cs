#if __MonoCS__
using System;
using NUnit.Framework;
using SIL.Reporting;
using SIL.Windows.Forms.Keyboarding;
using System.Windows.Forms;
using System.Collections.Generic;
#if WANT_PORT
namespace SIL.Windows.Forms.Keyboarding.Tests
{

	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class IBusAdaptorTestsWithoutIBus
	{

		[Test]
		[Category("IBus")]
		public void EngineAvailable_IBusNotRunning_ReturnsFalse()
		{
			using (var e = new IBusEnvironmentForTest(true, false))
			{
				Assert.IsFalse(IBusAdaptor.EngineAvailable);
			}
		}

		[Test]
		[Category("IBus")]
		public void GetActiveKeyboard_IBusNotRunning_ThrowsProblemNotificationSentToUser()
		{
			using (var e = new IBusEnvironmentForTest(true, false))
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
			using (var e = new IBusEnvironmentForTest(true, false))
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
			using (var e = new IBusEnvironmentForTest(true, false))
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
			using (var e = new IBusEnvironmentForTest(true, false))
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
			using (var e = new IBusEnvironmentForTest(true, false))
			{
				Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(
					() => IBusAdaptor.ActivateKeyboard(IBusEnvironmentForTest.OtherKeyboard)
				);
			}
		}
	}
}
#endif
#endif
