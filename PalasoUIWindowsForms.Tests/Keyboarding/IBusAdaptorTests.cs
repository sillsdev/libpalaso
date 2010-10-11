#if MONO
using System;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PalasoUIWindowsForms.Tests.Keyboarding
{

	[TestFixture]
	public class IBusAdaptorTests
	{
		private Form _window;

		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
		}

		private void RequiresWindow()
		{
			_window = new Form();
			TextBox box = new TextBox();
			box.Dock = DockStyle.Fill;
			_window.Controls.Add(box);

			_window.Show();
			box.Select();
			Application.DoEvents();
		}

		[Test]
		[NUnit.Framework.Category("IBus")]
		public void EngineAvailable_IBusIsSetUpAndConfiguredCorrectly_ReturnsTrue()
		{
			// needed for focus
			RequiresWindow();

			Assert.IsTrue(IBusAdaptor.EngineAvailable);
		}

		[Test]
		[NUnit.Framework.Category("IBus")]
		public void GetActiveKeyboard_IBusIsSetUpAndConfiguredToDefault_ReturnsEnglishKeyboard()
		{
			// needed for focus
			RequiresWindow();

			IBusAdaptor.Deactivate();
			Assert.Throws<Palaso.Reporting.ErrorReport.ProblemNotificationSentToUserException>(
				() => IBusAdaptor.GetActiveKeyboard()
			);
		}

		[Test]
		[NUnit.Framework.Category("IBus")]
		public void KeyboardDescriptors_IBusIsSetUpAndConfiguredToDefault_0KeyboardsReturned()
		{
			// needed for focus
			RequiresWindow();

			List<KeyboardDescriptor> availableKeyboards = IBusAdaptor.KeyboardDescriptors;

			// Assuming default ibus install doesn't have any active keyboards
			Assert.AreEqual(0, availableKeyboards.Count);
		}

		[Test]
		[NUnit.Framework.Category("IBus")]
		public void Deactivate_IBusIsRunning_GetCurrentKeyboardReturnsEnglishKeyboard()
		{
			// needed for focus
			RequiresWindow();

			IBusAdaptor.ActivateKeyboard("am:sera");
			IBusAdaptor.Deactivate();
			Assert.AreEqual("am:sera", IBusAdaptor.GetActiveKeyboard());
		}

		[Test]
		[NUnit.Framework.Category("IBus")]
		public void ActivateKeyBoard_IBusHasKeyboard_GetCurrentKeyboardReturnsActivatedKeyboard()
		{
			// needed for focus
			RequiresWindow();

			IBusAdaptor.Deactivate();
			IBusAdaptor.ActivateKeyboard("am:sera");
			Assert.AreEqual("am:sera", KeyboardController.GetActiveKeyboard());
			IBusAdaptor.Deactivate();
		}

		[Test]
		[NUnit.Framework.Category("IBus")]
		public void ActivateKeyBoard_IBusDoesNotHaveKeyboard_Throws()
		{
			// needed for focus
			RequiresWindow();

			Assert.Throws<ArgumentOutOfRangeException>(
				() => IBusAdaptor.ActivateKeyboard("Nonexistant Keyboard")
			);
		}

		[Test]
		[NUnit.Framework.Category("IBus not Running")]
		public void EngineAvailable_IBusNotRunning_ReturnsFalse()
		{
			// needed for focus
			RequiresWindow();

			Assert.IsFalse(IBusAdaptor.EngineAvailable);
		}

		[Test]
		[NUnit.Framework.Category("IBus not Running")]
		public void GetActiveKeyboard_IBusNotRunning_ReturnsEmptyString()
		{
			// needed for focus
			RequiresWindow();

			Assert.IsEmpty(IBusAdaptor.GetActiveKeyboard());
		}

		[Test]
		[NUnit.Framework.Category("IBus not Running")]
		public void KeyboardDescriptors_IBusNotRunning_EmptyListReturned()
		{
			// needed for focus
			RequiresWindow();

			List<KeyboardDescriptor> availableKeyboards = IBusAdaptor.KeyboardDescriptors;

			Assert.AreEqual(0, availableKeyboards.Count);
		}

		[Test]
		[NUnit.Framework.Category("IBus not Running")]
		public void Deactivate_IBusNotRunning_DoesNotThrow()
		{
			// needed for focus
			RequiresWindow();

			IBusAdaptor.Deactivate();
		}

		[Test]
		[NUnit.Framework.Category("IBus not Running")]
		public void ActivateKeyBoard_IBusNotRunning_DoesNotThrow()
		{
			// needed for focus
			RequiresWindow();
			IBusAdaptor.ActivateKeyboard("am:sera");
		}
	}
}
#endif