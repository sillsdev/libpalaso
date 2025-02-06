// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	/// <summary>
	/// Cancel mode of an action. NonCancelable means that a cancel button is not even shown,
	/// Cancelable means that a cancel button is shown, but it is up to the executing action
	/// to check for the Progress.Mgr.Cancelled, and Abortable means that the action can be
	/// terminated at any time using either ThreadAbortException or ProgressCancelledException
	/// </summary>
	public enum CancelModes
	{
		NonCancelable,
		Cancelable,
		Abortable
	}

	/// <summary>
	/// Utilities for running actions in the background
	/// </summary>
	public static class ProgressUtils
	{
		// FB25127
		public static event Action<Exception> UnhandledThreadException;

		private static volatile ManualResetEvent executeOnSameThreadComplete = new ManualResetEvent(true);

		// Do not access directly
		static volatile SynchronizationContext uiSynchronizationContext;
		static volatile Form uiSynchronizationForm;
		static volatile Thread uiSynchronizationThread;

		/// <summary>
		/// Gets Synchronization context for UI thread
		/// </summary>
		static SynchronizationContext UISynchronizationContext
		{
			get
			{
				// added checks to make sure we don't use a stale context in tests - this can
				// cause tests to hang.
				if (uiSynchronizationContext == null || !uiSynchronizationThread.IsAlive ||
					uiSynchronizationForm.IsDisposed || !uiSynchronizationForm.Visible)
				{
					// Try getting from active form
					uiSynchronizationContext = null;
					uiSynchronizationForm = Form.ActiveForm;
					if (uiSynchronizationForm == null)
					{
						for (int i = 0; i < Application.OpenForms.Count; i++)
							if (!Application.OpenForms[i].IsDisposed && Application.OpenForms[i].Visible)
							{
								uiSynchronizationForm = Application.OpenForms[i];
								break;
							}
					}
					if (uiSynchronizationForm != null)
					{
						ThreadStart getUIThreadDetails = () =>
						{
							uiSynchronizationThread = Thread.CurrentThread;
							uiSynchronizationContext = SynchronizationContext.Current;
						};

						// Since ManagedThreadId isn't fixed on .NET/Windows we use just invoke
						// on windows. (Which should perform a BeginInvoke/EndInvoke if the control was
						// created by the main UI thread)
						// Enhance: if uiSynchronizationForm isn't yet created then this may not work.
						if (Platform.IsDotNet)
						{
							uiSynchronizationForm.Invoke(getUIThreadDetails);
						}
						else
						{
							// This is more reliable than calling InvokeRequired,
							// as InvokeRequired requires the control to be created.
							// Mono's Control.Invoke implementation uses InvokeRequired to determine if
							// The delegate can just be run or if BeginInvoke needs to be called.
							if (Thread.CurrentThread.ManagedThreadId != 1)
							{
								IAsyncResult handle = uiSynchronizationForm.BeginInvoke(getUIThreadDetails);
								uiSynchronizationForm.EndInvoke(handle);
							}
							else
								getUIThreadDetails();
						}
					}
				}
				return uiSynchronizationContext;
			}
		}

		/// <summary>
		/// InvokeLaterOnUIThread reentry protection
		/// </summary>
		static bool runningInvokeLaterOnUIThreadAction;

		/// <summary>
		/// If Reentry occurs while processing a InvokeLaterOnUIThread action, then the second action gets moved
		/// into this List. When the original action is finished then the saved actions are reposted to the
		/// synchronization context, and this list is cleared.
		/// </summary>
		static List<ThreadStart> postponedInvokeLaterOnUIThreadActions = new List<ThreadStart>();

		/// <summary>
		/// Check whether or not we have a UI Synchronization Context, and if so whether or not the current
		/// thread is the main UI thread.
		/// </summary>
		/// <returns>
		/// True if there is no UI Synchronization Context, or if we are on the main (UI) thread.
		/// </returns>
		public static bool OnMainUiThreadOrNoUiContext
		{
			get
			{
				// True if no UI context
				SynchronizationContext context = UISynchronizationContext;
				if (context == null)
					return true;

				if (uiSynchronizationThread != null && uiSynchronizationThread.Equals(Thread.CurrentThread))
					return true;
				// It's the same thread as our UI thread. (Fixes some tests that seem to change the current SynchronizationContext).

				// Note: Assumes that synchronization context of UI thread is only on UI thread
				return context.Equals(SynchronizationContext.Current);
			}
		}

		/// <summary>
		/// Invokes the specified action on the UI thread.  Blocking.
		/// <para>NOTE: For this method to work, there needs to be an active message pump on the
		/// UI thread (i.e. the action will only be run the next time the messages are handled).
		/// Hopefully, if there is a visible form, there will be an active message pump.</para>
		/// </summary>
		/// <param name="action">the action to run on UI thread</param>
		public static void InvokeOnUIThread(ThreadStart action)
		{
			InvokeOnUIThread(action, false);
		}

		/// <summary>
		/// Invokes the specified action on the UI thread.  Blocking.
		/// Does not interrupt calls to ExecuteOnSameThread.  Active message pump required.
		/// </summary>
		/// <param name="action">the action to run on UI thread</param>
		public static void InvokeExclusivelyOnUIThread(ThreadStart action)
		{
			InvokeOnUIThread(action, true);
		}

		/// <summary>
		/// Invokes the specified action on the UI thread.  Blocking.
		/// Active message pump required.
		/// </summary>
		/// <param name="action">the action to run on UI thread</param>
		/// <param name="exclusively">avoid overlap with ExecuteOnSameThread</param>
		private static void InvokeOnUIThread(ThreadStart action, bool exclusively)
		{
			if (action == null)
				return;

			// Run directly if no UI context or if already on UI thread
			if (OnMainUiThreadOrNoUiContext)
			{
				action();
				return;
			}

			Exception caughtException = null;
			bool finished = false;

			// Looping may be necessary to run exclusively
			while (!finished)
			{
				// Wait for the UI Thread
				UISynchronizationContext.Send(delegate
					{
						try
						{
							action();
						}
						catch (Exception exception)
						{
							SaveStackTrace(exception);
							caughtException = exception;
						}
						finally
						{
							finished = true;
						}
					}, null);

				// We can only get here if we are running exclusively and if ExecuteOnSameThread was running.
				// Wait for ExecuteOnSameThread to finish
				if (!finished)
					executeOnSameThreadComplete.WaitOne();
			}

			if (caughtException == null)
				return;

			// FB25127 - if a UnhandledThreadException handler is present use that rather than
			// rethrowing the exception.
			if (UnhandledThreadException != null)
				UnhandledThreadException(caughtException);
			else
				throw caughtException;
		}

		/// <summary>
		/// Invokes the specified action on the UI thread without blocking
		/// </summary>
		/// <param name="action"></param>
		public static void InvokeLaterOnUIThread(ThreadStart action)
		{
			if (action == null)
				return;

			// Run directly if no UI context
			SynchronizationContext context = UISynchronizationContext;
			if (context == null)
			{
				action();
				return;
			}

			// Ignore completely if in unit tests, as the context does not run it on the
			// correct thread.
			if (!(context is WindowsFormsSynchronizationContext))
			{
				return;
			}

			SendOrPostCallback cb = (state) =>
			{
				if (runningInvokeLaterOnUIThreadAction)
				{
					postponedInvokeLaterOnUIThreadActions.Add((ThreadStart)state);
					return;
				}

				// If Application Message pump has quit, Ignore Later actions.
				// Mono: mono is setting Application.MessageLoop to false when showing
				// a modal dialog and not setting it to true when closing the dialog.
				if (Platform.IsDotNet && !Application.MessageLoop)
					return;

				Debug.Assert(Thread.CurrentThread == uiSynchronizationThread, "Invoked on wrong thread");

				runningInvokeLaterOnUIThreadAction = true;
				((ThreadStart)state)();
				runningInvokeLaterOnUIThreadAction = false;

				foreach (var postponedAction in postponedInvokeLaterOnUIThreadActions)
					InvokeLaterOnUIThread(postponedAction);

				postponedInvokeLaterOnUIThreadActions.Clear();
			};

			context.Post(cb, action);
		}

		/// <summary>
		/// Invokes the action later on idle, which will be on the UI thread. Does not block.
		/// </summary>
		/// <param name='action'>
		/// The Action to run on idle.
		/// </param>
		/// <param name='cancelIdleEvent'>
		/// If null, previous idle event isn't cancelled.
		/// Pass return value from previous InvokeLaterOnIdle call to cancel the scheduled event if it hasn't
		/// been executed yet.
		/// </param>
		public static EventHandler InvokeLaterOnIdle(ThreadStart action, EventHandler cancelIdleEvent)
		{
			if (cancelIdleEvent != null)
				Application.Idle -= cancelIdleEvent;

			EventHandler idleAction = null;
			idleAction = (s, e) =>
			{
				Application.Idle -= idleAction;
				if (action != null)
					action();
			};

			Application.Idle += idleAction;
			return idleAction;
		}

		/// <summary>
		/// Convenience method to display a message box on the UI thread.
		/// </summary>
		public static DialogResult DisplayMessageOnUIThread(string message, string caption, MessageBoxButtons buttons,
															MessageBoxIcon icon)
		{
			DialogResult result = DialogResult.OK;
			InvokeOnUIThread(() => result = MessageBox.Show(message, caption, buttons, icon));
			return result;
		}

		public delegate void ExceptionHandlerDelegate(Exception exception);



		/// <summary>
		/// Saves stack trace in the Data property of the exception.
		/// </summary>
		internal static void SaveStackTrace(Exception ex)
		{
			// Some exceptions are created by .Net to be immutable so the Data dictionary is
			// ReadOnly. Want to make sure that we don't cover up the original exception by
			// trying to modify this.
			if (!ex.Data.IsReadOnly)
			{
				// Save old stack trace
				if (!ex.Data.Contains("Stacktrace"))
					ex.Data["Stacktrace"] = "";
				ex.Data["Stacktrace"] = ex.Data["Stacktrace"] + ex.StackTrace;
			}
		}
	}
}
