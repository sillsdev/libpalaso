// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
using System;
using System.Windows.Forms;
using IBusDotNet;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>Normal implementation of IIbusCommunicator</summary>
	internal class IbusCommunicator : IIbusCommunicator
	{
		// see https://github.com/ibus/ibus/blob/1.4.y/src/ibustypes.h for ibus modifier values
		private enum IbusModifiers
		{
			Shift = 1 << 0,
			ShiftLock = 1 << 1,
			Control = 1 << 2,
		}

		#region protected fields

		/// <summary>
		/// stores Dbus Connection to ibus
		/// </summary>
		protected IBusConnection m_connection;

		/// <summary>
		/// the input Context created
		/// </summary>
		protected IInputContext m_inputContext;

		/// <summary>
		/// Ibus helper class
		/// </summary>
		protected InputBus m_ibus;

		#endregion

		/// <summary>
		/// Create a Connection to Ibus. If successfull Connected property is true.
		/// </summary>
		public IbusCommunicator()
		{
			m_connection = IBusConnectionFactory.Create();

			if (m_connection == null)
				return;

			// Prevent hanging on exit issues caused by missing dispose calls, or strange interaction
			// between ComObjects and managed object.
			Application.ThreadExit += (sender, args) =>
								{
									m_connection?.Dispose();
									m_connection = null;
								};

			m_ibus = new InputBus(m_connection);
		}

		#region Disposable stuff
		#if DEBUG
		/// <summary/>
		~IbusCommunicator()
		{
			Dispose(false);
		}
		#endif

		/// <summary/>
		public bool IsDisposed
		{
			get;
			private set;
		}

		/// <summary/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary/>
		protected virtual void Dispose(bool fDisposing)
		{
			System.Diagnostics.Debug.WriteLineIf(!fDisposing, "****** Missing Dispose() call for " + GetType() + ". *******");
			if (fDisposing && !IsDisposed)
			{
				// dispose managed and unmanaged objects
				m_connection?.Dispose();
			}
			m_connection = null;
			IsDisposed = true;
		}
		#endregion

		/// <summary>
		/// Wrap an ibus with protection incase DBus connection is dropped.
		/// </summary>
		protected void ProtectedIBusInvoke(Action action)
		{
			try
			{
				action();
			}
			catch (NDesk.DBus.DBusConectionErrorException)
			{
				m_ibus = null;
				m_inputContext = null;
				NotifyUserOfIBusConnectionDropped();
			}
			catch (System.NullReferenceException)
			{
			}
		}

		/// <summary>
		/// Inform users of IBus problem.
		/// </summary>
		protected void NotifyUserOfIBusConnectionDropped()
		{
			MessageBox.Show(Form.ActiveForm, "Please restart IBus and the application.", "IBus connection has stopped.");
		}

		private int ConvertToIbusModifiers(Keys modifierKeys, char charUserTyped)
		{
			int ibusModifiers = 0;
			if ((modifierKeys & Keys.Shift) != 0)
				ibusModifiers |= (int)IbusModifiers.Shift;
			if ((modifierKeys & Keys.Control) != 0)
				ibusModifiers |= (int)IbusModifiers.Control;
			// modifierKeys don't contain CapsLock and Control.IsKeyLocked(Keys.CapsLock)
			// doesn't work on mono. So we guess the caps state by unicode value and the shift
			// state. This is far from ideal.
			if ((char.IsUpper(charUserTyped) && (modifierKeys & Keys.Shift) == 0) ||
				(char.IsLower(charUserTyped) && (modifierKeys & Keys.Shift) != 0))
				ibusModifiers |= (int)IbusModifiers.ShiftLock;

			return ibusModifiers;
		}


#region IIBusCommunicator Implementation

		/// <summary>
		/// Returns true if we have a connection to Ibus.
		/// </summary>
		public bool Connected => m_connection != null;

		/// <summary>
		/// Gets the connection to IBus.
		/// </summary>
		public IBusConnection Connection => m_connection;

		/// <summary>
		/// If we have a valid inputContext Focus it. Also set the GlobalCachedInputContext.
		/// </summary>
		public void FocusIn()
		{
			if (InputContext == null)
				return;

			ProtectedIBusInvoke(() => m_inputContext.FocusIn());

			// For performance reasons we store the active inputContext
			GlobalCachedInputContext.InputContext = m_inputContext;
		}

		/// <summary>
		/// If we have a valid inputContext call FocusOut ibus method.
		/// </summary>
		public void FocusOut()
		{
			if (m_inputContext == null)
				return;

			ProtectedIBusInvoke(() => m_inputContext.FocusOut());

			GlobalCachedInputContext.Clear();
		}

		/// <summary>
		/// Tells IBus the location and height of the selection
		/// </summary>
		public void NotifySelectionLocationAndHeight(int x, int y, int height)
		{
			if (m_inputContext == null)
				return;

			ProtectedIBusInvoke(() => m_inputContext.SetCursorLocation(x, y, 0, height));
		}

		/// <summary>
		/// Sends a key Event to the ibus current input context. This method will be called by
		/// the IbusKeyboardAdapter on some KeyDown and all KeyPress events.
		/// </summary>
		/// <param name="keySym">The X11 key symbol (for special keys) or the key code</param>
		/// <param name="scanCode">The X11 scan code</param>
		/// <param name="state">The modifier state, i.e. shift key etc.</param>
		/// <returns><c>true</c> if the key event is handled by ibus.</returns>
		/// <seealso cref="IbusKeyboardSwitchingAdaptor.HandleKeyPress"/>
		public bool ProcessKeyEvent(int keySym, int scanCode, Keys state)
		{
			if (m_inputContext == null)
				return false;

			try
			{
				// m_inputContext.IsEnabled() is no longer contained in IBus 1.5. However,
				// ibusdotnet >= 1.9.1 deals with that and always returns true so that we still
				// can use this method.
				if (!m_inputContext.IsEnabled())
					return false;

				var modifiers = ConvertToIbusModifiers(state, (char)keySym);

				return m_inputContext.ProcessKeyEvent(keySym, scanCode, modifiers);
			}
			catch (NDesk.DBus.DBusConectionErrorException e)
			{
				Console.WriteLine("IbusCommunicator.ProcessKeyEvent({0},{1},{2}): caught DBusConectionErrorException: {3}", keySym, scanCode, state, e);
				m_ibus = null;
				m_inputContext = null;
				NotifyUserOfIBusConnectionDropped();
			}
			catch (System.NullReferenceException e)
			{
				Console.WriteLine("IbusCommunicator.ProcessKeyEvent({0},{1},{2}): caught NullReferenceException: {3}", keySym, scanCode, state, e);
			}
			return false;
		}

		/// <summary>
		/// Reset the Current ibus inputContext.
		/// </summary>
		public void Reset()
		{
			if (m_inputContext == null)
				return;

			ProtectedIBusInvoke(m_inputContext.Reset);
		}

		private bool m_contextCreated;

		/// <summary>
		/// Create an input context and setup callback handlers.
		/// </summary>
		public void CreateInputContext()
		{
			System.Diagnostics.Debug.Assert(!m_contextCreated);
			if (m_ibus == null)
			{
				// This seems to be needed for tests on TeamCity.
				// It also seems that it shouldn't be necessary to run IBus to use Linux keyboarding!
				return;
			}

			// A previous version of this code that seems to have worked on Saucy had the following
			// comment: "For IBus 1.5, we must use the current InputContext because there is no
			// way to enable one we create ourselves.  (InputContext.Enable() has turned
			// into a no-op.)  For IBus 1.4, we must create one ourselves because
			// m_ibus.CurrentInputContext() throws an exception."
			// However as of 2015-08-11 this no longer seems to be true: when we use the existing
			// input context then typing with an IPA KMFL keyboard doesn't work in the
			// SIL.Windows.Forms.TestApp on Trusty with unity desktop although the keyboard
			// indicator correctly shows the IPA keyboard. I don't know what changed or why it is
			// suddenly behaving differently.
			//
			//			if (KeyboardController.CombinedKeyboardHandling)
			//			{
			//				var path = m_ibus.CurrentInputContext();
			//				m_inputContext = new InputContext(m_connection, path);
			//			}
			//			else

			m_inputContext = m_ibus.CreateInputContext("IbusCommunicator");

			AttachContextMethods(m_inputContext);
			m_contextCreated = true;
		}

		private void AttachContextMethods(IInputContext context)
		{
			ProtectedIBusInvoke(() =>
			{
				context.SetCapabilities(Capabilities.Focus | Capabilities.PreeditText | Capabilities.SurroundingText);
				context.CommitText += OnCommitText;
				context.UpdatePreeditText += OnUpdatePreeditText;
				context.HidePreeditText += OnHidePreeditText;
				context.ForwardKeyEvent += OnKeyEvent;
				context.DeleteSurroundingText += OnDeleteSurroundingText;
				context.Enable();	// not needed for IBus 1.5, but doesn't hurt.
			});
		}

		/// <summary>
		/// Get the ibus input context, creating it if necessary.
		/// </summary>
		/// <remarks>
		/// This must be called after the KeyboardController.CombinedKeyboardHandling has been set.
		/// </remarks>
		protected IInputContext InputContext
		{
			get
			{
				if (m_inputContext != null)
					return m_inputContext;
				if (m_contextCreated || !Connected)
					return null;		// we must have had an error that cleared m_inputContext
				CreateInputContext();
				return m_inputContext;
			}
		}

		/// <summary>
		/// Return the DBUS 'path' name for the currently focused InputContext
		/// </summary>
		/// <exception cref="System.Exception">Throws: System.Exception with message
		/// 'org.freedesktop.DBus.Error.Failed: No input context focused' if nothing is currently
		/// focused.</exception>
		public string GetFocusedInputContext()
		{
			return m_ibus.CurrentInputContext();
		}

		/// <summary></summary>
		public event Action<object> CommitText;

		/// <summary></summary>
		public event Action<object, int> UpdatePreeditText;

		/// <summary></summary>
		public event Action<int, int> DeleteSurroundingText;

		/// <summary></summary>
		public event Action HidePreeditText;

		/// <summary></summary>
		public event Action<int, int, int> KeyEvent;
		#endregion

#region private methods

		private void OnCommitText(object text)
		{
			CommitText?.Invoke(IBusText.FromObject(text));
		}

		private void OnUpdatePreeditText(object text, uint cursor_pos, bool visible)
		{
			if (UpdatePreeditText != null && visible)
			{
				UpdatePreeditText(IBusText.FromObject(text), (int)cursor_pos);
			}
		}

		private void OnDeleteSurroundingText(int offset, uint nChars)
		{
			DeleteSurroundingText?.Invoke(offset, (int)nChars);
		}

		private void OnHidePreeditText()
		{
			HidePreeditText?.Invoke();
		}

		private void OnKeyEvent(uint keyval, uint keycode, uint modifiers)
		{
			KeyEvent?.Invoke((int)keyval, (int)keycode, (int)modifiers);
		}

		#endregion
	}
}
