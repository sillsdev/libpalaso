using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.Extensions;

namespace SIL.Windows.Forms.Tests.ControlExtensionsTests
{
	[TestFixture]
	public class ControlExtensionsTests
	{
		private Exception _threadException;
		private Control _control;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			Application.ThreadException += ApplicationOnThreadException;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Application.ThreadException -= ApplicationOnThreadException;
		}

		[SetUp]
		public void Setup()
		{
			_threadException = null;
			_control = new Control();
		}

		[TearDown]
		public void Teardown()
		{
			if (!_control.IsDisposed)
				_control.Dispose();
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		[TestCase(ControlExtensions.ErrorHandlingAction.Throw)]
		public void SafeInvoke_NullControl_ThrowsArgumentNullException(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			var ex = Assert.Throws<ArgumentNullException>(() => { ControlExtensions.SafeInvoke(null, () => { }, "NullControlTest", errorHandling); });
			Assert.AreEqual("control", ex.ParamName);
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		[TestCase(ControlExtensions.ErrorHandlingAction.Throw)]
		public void SafeInvoke_NullAction_ThrowsArgumentNullException(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			_control.CreateControl();
			var ex = Assert.Throws<ArgumentNullException>(() => { _control.SafeInvoke(null, "NullActionTest", errorHandling); });
			Assert.AreEqual("action", ex.ParamName);
		}

		[Test]
		public void SafeInvoke_OnUiThread_HandleNotCreated_IgnoreAll_ReturnsWithoutInvokingOrThrowing()
		{
			int i = 0;
			_control.SafeInvoke(() => { i++; }, "HandleNotCreatedTest", ControlExtensions.ErrorHandlingAction.IgnoreAll);
			Assert.AreEqual(0, i);
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		[TestCase(ControlExtensions.ErrorHandlingAction.Throw)]
		public void SafeInvoke_OnUiThread_HandleNotCreated_NotIgnoreAll_ThrowsInvalidOperationException(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			var ex = Assert.Throws<InvalidOperationException>(() => { _control.SafeInvoke(() => { }, "HandleNotCreatedTest", errorHandling); });
			Assert.AreEqual("SafeInvoke called before the control's handle was created. (HandleNotCreatedTest)", ex.Message);
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		public void SafeInvoke_OnUiThread_Disposed_Ignore_ReturnsWithoutInvokingOrThrowing(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			var i = 0;
			_control.CreateControl();
			_control.Dispose();
			_control.SafeInvoke(() => { i++; }, "DisposedTest", errorHandling);
			Assert.AreEqual(0, i);
		}

		[Test]
		public void SafeInvoke_OnUiThread_Disposed_Default_ThrowsObjectDisposedException()
		{
			var i = 0;
			_control.CreateControl();
			_control.Dispose();
			var ex = Assert.Throws<ObjectDisposedException>(() => { _control.SafeInvoke(() => { i++; }, "DisposedTest"); });
			Assert.AreEqual("SafeInvoke called after the control was disposed. (DisposedTest)", ex.ObjectName);
			Assert.AreEqual(0, i);
		}

		/// <summary>
		/// We thought that SafeInvoke would need to handle the edge case where an action is invoked asynchronously on a control, which is
		/// subsequently disposed (i.e., before the action can actually be performed), but it turns out that when the control's handle is
		/// destroyed, the action is dequeued, and an ObjectDisposedException is registered in the entry. That exception is available via
		/// EndInvoke, but since SafeInvoke does not return the IAsyncResult, this test cannot show that.
		/// Note that the first SafeInvoke in this test also tests the normal case of an action invoked asynchronously on a non-UI thread.
		/// </summary>
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		[TestCase(ControlExtensions.ErrorHandlingAction.Throw)]
		public void SafeInvoke_DisposedAfterInvokingOnUiThread_InvokedActionDequeued(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			string confirmationMessage = null;
			_control.CreateControl();
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				Trace.WriteLine("SafeInvoke 1");
				ctrl.SafeInvoke(() =>
				{
					Trace.WriteLine("invoking 1");
					confirmationMessage = "First action got invoked";
					Assert.IsFalse(ctrl.InvokeRequired);
					ctrl.Dispose();
				}, "Getting initial value, resetting control text, and disposing control.");
				Trace.WriteLine("SafeInvoke 2");
				ctrl.SafeInvoke(() =>
				{
					Assert.Fail("This should have been de-queued when the control was disposed.");
				}, "DisposedAfterInvokingOnUiThread", errorHandling);
				Trace.WriteLine("About to sleep on worker thread : 50 ms");
				Thread.Sleep(50);
			};
			worker.RunWorkerAsync(_control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.AreEqual("First action got invoked", confirmationMessage);
		}

		[Test]
		public void SafeInvoke_Asynchronous_InvokedAsynchronously()
		{
			string textFromControl = null;
			_control.CreateControl();
			_control.Text = "Initial Text";
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				Trace.WriteLine("SafeInvoke 1");
				ctrl.SafeInvoke(() =>
				{
					// Since this code is going to be executed on the UI thread, it is guaranteed not to execute until
					// the next time messages are "pumped", which in the case of this test happens during the call to
					// Application.DoEvents below.
					Trace.WriteLine("invoking 1");
					textFromControl = ctrl.Text;
					ctrl.Text = "Final Text";
				}, "Getting initial value and resetting control text.");
				Assert.IsNull(textFromControl);
			};
			worker.RunWorkerAsync(_control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.AreEqual("Initial Text", textFromControl);
			Assert.AreEqual("Final Text", _control.Text);
		}

		[Test]
		public void SafeInvoke_ForceSynchronous_InvokedSynchronously()
		{
			string textFromControl = null;
			_control.CreateControl();
			_control.Text = "Initial Text";
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				Trace.WriteLine("SafeInvoke 1");
				ctrl.SafeInvoke(() =>
				{
					// The calls to Thread.Sleep are sprinkled in here just to PROVE that this is being done synchronously and therefore
					// essentially blocks any further execution on either thread.
					Thread.Sleep(10);
					textFromControl = ctrl.Text;
					Thread.Sleep(10);
					ctrl.Text = "Final Text";
					Thread.Sleep(10);
				}, "Force synchronous.", forceSynchronous: true);
				Assert.AreEqual("Initial Text", textFromControl, "Since the invoke was done synchronously, this should have been set before SafeInvoke returned.");
				Assert.AreEqual("Final Text", ctrl.Text);
			};
			worker.RunWorkerAsync(_control);
			while (worker.IsBusy)
				Application.DoEvents(); // Even though the SafeInvoke call is synchronous, the worker thread obviously isn't, so we need to wait for it to finish.
			Assert.AreEqual("Final Text", _control.Text, "This just proves that we don't get here without having executed the worker thread's code.");
		}

		[TestCase(true)]
		[TestCase(false)]
		public void SafeInvoke_OnUiThread_Normal_ActionInvokedSynchronously(bool forceSynchronous)
		{
			var i = 0;
			_control.CreateControl();
			_control.SafeInvoke(() => { i++; }, "CalledOnUiThread_Normal", forceSynchronous: forceSynchronous);
			Assert.AreEqual(1, i);
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		public void SafeInvoke_OnNonUiThread_Disposed_Ignore_ReturnsWithoutInvokingOrThrowing(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			var i = 0;
			_control.CreateControl();
			_control.Dispose();
			_control.SafeInvoke(() => { i++; }, "DisposedTest", errorHandling);
			Assert.AreEqual(0, i);
		}

		[Test]
		public void SafeInvoke_OnNonUiThread_Disposed_Default_ThrowsObjectDisposedException()
		{
			Exception exception = null;
			string textFromControl = null;
			_control.CreateControl();
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				try
				{
					ctrl.SafeInvoke(() => { textFromControl = ctrl.Text; }, "OnNonUiThread_Disposed");
				}
				catch (Exception e)
				{
					exception = e;
				}
			};
			_control.Dispose();
			worker.RunWorkerAsync(_control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.IsNull(textFromControl);
			Assert.IsTrue(exception is ObjectDisposedException);
		}

		[Test]
		public void SafeInvoke_AsynchronousActionThrowsException_ExceptionHandledByApplicationThreadExceptionHandler()
		{
			Exception exceptionThrownBySafeInvoke = null;
			_control.CreateControl();
			bool actionWasInvoked = false;
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				try
				{
					ctrl.SafeInvoke(() =>
					{
						actionWasInvoked = true;
						throw new InvalidOperationException("Blah");
					}, "AsynchronousActionThrowsException");
				}
				catch (Exception e)
				{
					exceptionThrownBySafeInvoke = e;
				}
				Thread.Sleep(50); // Ensure that the UI thread has a chance to pump messages and perform the action
			};
			worker.RunWorkerAsync(_control);
			while (worker.IsBusy)
				Application.DoEvents();
			Application.DoEvents(); // Just for good measure. We're desperate!

			Assert.IsTrue(actionWasInvoked);
			Assert.IsNull(exceptionThrownBySafeInvoke);
			Assert.IsNotNull(_threadException);
			Assert.AreEqual(typeof(InvalidOperationException), _threadException.GetType());
		}

		private void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
		{
			_threadException = threadExceptionEventArgs.Exception;
		}

		[Test]
		public void SafeInvoke_SynchronousActionThrowsException_ExceptionThrownOutOfSafeInvoke()
		{
			Exception exceptionThrownBySafeInvoke = null;
			_control.CreateControl();
			bool finished = false;
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				try
				{
					ctrl.SafeInvoke(() => { throw new InvalidOperationException("Blah"); }, "SynchronousActionThrowsException", forceSynchronous:true);
				}
				catch (Exception e)
				{
					exceptionThrownBySafeInvoke = e;
				}
				finished = true;
			};
			worker.RunWorkerAsync(_control);
			// The next two lines are not *required*, but they help to prove that the action was really performed synchronously.
			Thread.Sleep(30); // Nothing should hapen on the non-UI-thread until we call DoEvents.
			Assert.IsFalse(finished);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.IsNull(_threadException);
			Assert.IsNotNull(exceptionThrownBySafeInvoke);
			string nestedExceptions = "";
			Exception ex = exceptionThrownBySafeInvoke;
			while (ex != null && !(ex is InvalidOperationException))
			{
				nestedExceptions += ex.GetType() + ": " + ex.Message + Environment.NewLine;
				ex = ex.InnerException;
			}
			Assert.IsNotNull(ex, nestedExceptions);
			Assert.IsTrue(ex is InvalidOperationException, nestedExceptions);
			Assert.AreEqual("Blah", ex.Message);
		}
	}
}
