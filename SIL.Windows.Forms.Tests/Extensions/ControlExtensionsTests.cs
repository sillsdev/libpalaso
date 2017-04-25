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
		Exception _threadException;

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
			using (var control = new Control())
			{
				control.CreateControl();
				var ex = Assert.Throws<ArgumentNullException>(() => { control.SafeInvoke(null, "NullActionTest", errorHandling); });
				Assert.AreEqual("action", ex.ParamName);
			}
		}

		[Test]
		public void SafeInvoke_OnUiThread_HandleNotCreated_IgnoreAll_ReturnsWithoutInvokingOrThrowing()
		{
			int i = 0;
			using (var control = new Control())
			{
				control.SafeInvoke(() => { i++; }, "HandleNotCreatedTest", ControlExtensions.ErrorHandlingAction.IgnoreAll);
			}
			Assert.AreEqual(0, i);
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		[TestCase(ControlExtensions.ErrorHandlingAction.Throw)]
		public void SafeInvoke_OnUiThread_HandleNotCreated_NotIgnoreAll_ThrowsInvalidOperationException(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			using (var control = new Control())
			{
				var ex = Assert.Throws<InvalidOperationException>(() => { control.SafeInvoke(() => { }, "HandleNotCreatedTest", errorHandling); });
				Assert.AreEqual("SafeInvoke called before the control's handle was created. (HandleNotCreatedTest)", ex.Message);
			}
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		public void SafeInvoke_OnUiThread_Disposed_Ignore_ReturnsWithoutInvokingOrThrowing(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			var control = new Control();
			var i = 0;
			control.CreateControl();
			control.Dispose();
			control.SafeInvoke(() => { i++; }, "DisposedTest", errorHandling);
			Assert.AreEqual(0, i);
		}

		[Test]
		public void SafeInvoke_OnUiThread_Disposed_Default_ThrowsObjectDisposedException()
		{
			var control = new Control();
			var i = 0;
			control.CreateControl();
			control.Dispose();
			var ex = Assert.Throws<ObjectDisposedException>(() => { control.SafeInvoke(() => { i++; }, "DisposedTest"); });
			Assert.AreEqual("SafeInvoke called after the control was disposed. (DisposedTest)", ex.ObjectName);
			Assert.AreEqual(0, i);
		}

		/// <summary>
		/// We thought that SafeInvoke would need to handle the edge case where an action is invoked asynchronously on a control, which is
		/// subsequently disposed (i.e., before the action can actually be performed), but it turns out that when the control's handle is
		/// destroyed, the action is dequeued, and an ObjectDisposedException is registered in the entry. That exception is available via
		/// EndInvoke, but since SafeInvoke does not return the IAsyncResult, this test cannot show that.
		/// Not that the first SafeInvoke in this test also tests the normal case of an action invoked asynchronously on a non-UI thread.
		/// </summary>
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		[TestCase(ControlExtensions.ErrorHandlingAction.Throw)]
		public void SafeInvoke_DisposedAfterInvokingOnUiThread_InvokedActionDequeued(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			var control = new Control();
			string textFromControl = null;
			control.CreateControl();
			control.Text = "Initial Text";
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				Trace.WriteLine("SafeInvoke 1");
				ctrl.SafeInvoke(() => { Trace.WriteLine("invoking 1"); textFromControl = ctrl.Text; ctrl.Text = "Final Text"; ctrl.Dispose(); }, "Getting initial value and resetting control text.");
				Trace.WriteLine("SafeInvoke 2");
				ctrl.SafeInvoke(() => { Trace.WriteLine("invoking 2"); textFromControl = ctrl.Text; }, "DisposedAfterInvokingOnUiThread", errorHandling);
				Trace.WriteLine("About to sleep on worker thread : 50 ms");
				Thread.Sleep(50);
			};
			worker.RunWorkerAsync(control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.AreEqual("Initial Text", textFromControl);
		}

		[Test]
		public void SafeInvoke_ForceSynchronous_InvokedSynchronously()
		{
			var control = new Control();
			string textFromControl = null;
			control.CreateControl();
			control.Text = "Initial Text";
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				Trace.WriteLine("SafeInvoke 1");
				ctrl.SafeInvoke(() => { textFromControl = ctrl.Text; ctrl.Text = "Final Text"; }, "Force synchronous.", forceSynchronous: true);
				Assert.AreEqual("Initial Text", textFromControl);
			};
			worker.RunWorkerAsync(control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.AreEqual("Final Text", control.Text);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void SafeInvoke_OnUiThread_Normal_ActionInvokedSynchronously(bool forceSynchronous)
		{
			var control = new Control();
			var i = 0;
			control.CreateControl();
			control.SafeInvoke(() => { i++; }, "CalledOnUiThread_Normal", forceSynchronous:forceSynchronous);
			Assert.AreEqual(1, i);
		}

		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreAll)]
		[TestCase(ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed)]
		public void SafeInvoke_OnNonUiThread_Disposed_Ignore_ReturnsWithoutInvokingOrThrowing(ControlExtensions.ErrorHandlingAction errorHandling)
		{
			var control = new Control();
			var i = 0;
			control.CreateControl();
			control.Dispose();
			control.SafeInvoke(() => { i++; }, "DisposedTest", errorHandling);
			Assert.AreEqual(0, i);
		}

		[Test]
		public void SafeInvoke_OnNonUiThread_Disposed_Default_ThrowsObjectDisposedException()
		{
			var control = new Control();
			Exception exception = null;
			string textFromControl = null;
			control.CreateControl();
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				try
				{
					ctrl.SafeInvoke(() => { textFromControl = ctrl.Text; }, "OnNonUiThread_Disposed");
				}
				catch(Exception e)
				{
					exception = e;
				}
			};
			control.Dispose();
			worker.RunWorkerAsync(control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.IsNull(textFromControl);
			Assert.IsTrue(exception is ObjectDisposedException);
		}

		[Test]
		public void SafeInvoke_AsynchronousActionThrowsException_ExceptionHandledByApplicationThreadExceptionHandler()
		{
			_threadException = null;
			var control = new Control();
			Exception exceptionThrownBySafeInvoke = null;
			control.CreateControl();
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				try
				{
					ctrl.SafeInvoke(() => { throw new InvalidOperationException("Blah"); }, "AsynchronousActionThrowsException");
				}
				catch (Exception e)
				{
					exceptionThrownBySafeInvoke = e;
				}
			};
			worker.RunWorkerAsync(control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.IsNull(exceptionThrownBySafeInvoke);
			Assert.IsTrue(_threadException is InvalidOperationException);
		}

		private void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
		{
			_threadException = threadExceptionEventArgs.Exception;
		}

		[Test]
		public void SafeInvoke_SynchronousActionThrowsException_Hmmmmmm()
		{
			var control = new Control();
			Exception exceptionThrownBySafeInvoke = null;
			control.CreateControl();
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, args) =>
			{
				var ctrl = (Control)args.Argument;
				try
				{
					ctrl.SafeInvoke(() => { throw new InvalidOperationException("Blah"); }, "AsynchronousActionThrowsException", forceSynchronous:true);
				}
				catch (Exception e)
				{
					exceptionThrownBySafeInvoke = e;
				}
			};
			worker.RunWorkerAsync(control);
			while (worker.IsBusy)
				Application.DoEvents();
			Assert.IsNull(_threadException);
			Assert.IsTrue(exceptionThrownBySafeInvoke is InvalidOperationException);
			Assert.AreEqual("Blah", exceptionThrownBySafeInvoke.Message);
		}
	}
}
