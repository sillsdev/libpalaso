// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Miscellaneous
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class provides two different ways to show and hide a wait cursor:
	/// a) WaitCursor.Show() and WaitCursor.Hide()
	/// b) using (new WaitCursor(this))
	///    {
	///        // code that should be run while wait cursor is visible
	///    }
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class WaitCursor
	{
		/// ------------------------------------------------------------------------------------
		public static void Show()
		{
			ToggleWaitCursorState(true);
		}

		/// ------------------------------------------------------------------------------------
		public static void Hide()
		{
			ToggleWaitCursorState(false);
		}

		/// ------------------------------------------------------------------------------------
		private static void ToggleWaitCursorState(bool turnOn)
		{
			Application.UseWaitCursor = turnOn;

			foreach (var frm in Application.OpenForms.Cast<Form>().ToList())
			{
				Form form = frm; // Avoid resharper message about accessing foreach variable in closure.
				try
				{
					if (form.InvokeRequired)
						form.Invoke(new Action(() => form.Cursor = (turnOn ? Cursors.WaitCursor : Cursors.Default)));
					else
						form.Cursor = (turnOn ? Cursors.WaitCursor : Cursors.Default);
				}
				catch
				{
					// Form may have closed and been disposed. Oh, well.
				}
			}

			try
			{
				// I hate doing this, but setting the cursor property in .Net
				// often doesn't otherwise take effect until it's too late.
				Application.DoEvents();
			}
			catch { }
		}

		#if DEBUG_WAITCURSOR
		static int s_depth = 0;		// counter used for debugging/tracing nested uses.
		#endif
		private Cursor _oldCursor;
		private Control _parent;
		private bool _restoreWaitCursorOnDispose;
		private delegate void VoidMethodWithBool(bool f);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="T:WaitCursor"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public WaitCursor() : this(null, false)
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Parent control</param>
		/// ------------------------------------------------------------------------------------
		public WaitCursor(Control parent) : this(parent, false)
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Parent control (can be null if displaying a normal wait cursor)</param>
		/// <param name="showBusyCursor">True to show a busy cursor (arrow with an
		/// hourglass) instead of the hourglass by itself.</param>
		/// ------------------------------------------------------------------------------------
		public WaitCursor(Control parent, bool showBusyCursor)
		{
			if (parent == null && showBusyCursor)
				throw new ArgumentException("Can't show a busy cursor without having a parent control");
			_parent = parent;
			SetWaitCursor(showBusyCursor);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the wait cursor.
		/// </summary>
		/// <param name="showBusyCursor">set to <c>true</c> to display the busy cursor,
		/// set to <c>false</c> to display the normal wait cursor.</param>
		/// ------------------------------------------------------------------------------------
		private void SetWaitCursor(bool showBusyCursor)
		{
			if (_parent != null && _parent.InvokeRequired)
			{
				_parent.Invoke(new VoidMethodWithBool(SetWaitCursor), showBusyCursor);
				return;
			}

			if (_parent != null)
			{
				_oldCursor = _parent.Cursor;
				_parent.Cursor = showBusyCursor ? Cursors.AppStarting : Cursors.WaitCursor;
			}
			else
			{
				_restoreWaitCursorOnDispose = Application.UseWaitCursor;
				Application.UseWaitCursor = true;
				// A comment on the web indicates that the following triggers the cursor to actually change.
				// This appears to be true.  (A related post used the Win32 API SendMessage to achieve this.)
				Cursor.Position = Cursor.Position;
			}
			#if DEBUG_WAITCURSOR
			++s_depth;
			Debug.WriteLine(String.Format("{0}: WaitCursor.Create({1}): _parent={2}; _oldCursor={3}; _restoreWaitCursorOnDispose={4}",
				s_depth, showBusyCursor, _parent, _oldCursor, _restoreWaitCursorOnDispose));
			#endif
		}

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		/// True, if the object has been disposed.
		/// </summary>
		private bool _isDisposed = false;

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~WaitCursor()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			System.Diagnostics.Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			// Must not be run more than once.
			if (_isDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				Restore();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.
			_parent = null;
			_oldCursor = null;

			_isDisposed = true;
		}

		#endregion IDisposable & Co. implementation

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Restore the previous cursor
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Restore()
		{
			CheckDisposed();

			if (_parent != null && _parent.InvokeRequired)
			{
				_parent.Invoke(new MethodInvoker(Restore));
				return;
			}

			#if DEBUG_WAITCURSOR
			Debug.WriteLine(String.Format("{0}: WaitCursor.Dispose(): _parent={1}; _oldCursor={2}; _restoreWaitCursorOnDispose={3}",
				s_depth, _parent, _oldCursor, _restoreWaitCursorOnDispose));
			--s_depth;
			#endif
			if (_oldCursor != null)
			{
				_parent.Cursor = _oldCursor;
			}
			else
			{
				Application.UseWaitCursor = _restoreWaitCursorOnDispose;
				// A comment on the web indicates that the following helps trigger the cursor to actually change.
				// This appears to be true.  (A related post used the Win32 API SendMessage to achieve this.)
				Cursor.Position = Cursor.Position;
			}
		}
	}
}
