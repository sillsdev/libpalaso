// Copyright (c) 2011-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling ibus keyboards on Linux.
	/// </summary>
	public class IbusKeyboardSwitchingAdaptor : IKeyboardSwitchingAdaptor
	{
		private readonly IIbusCommunicator _ibusComm;
		private bool _needIMELocation;

		public IbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator)
		{
			_ibusComm = ibusCommunicator;

			if (KeyboardController.Instance != null)
			{
				KeyboardController.Instance.ControlAdded += OnControlRegistered;
				KeyboardController.Instance.ControlRemoving += OnControlRemoving;
			}

			// Don't turn on any Ibus IME keyboard until requested explicitly.
			// If we do nothing, the first Ibus IME keyboard is automatically activated.
			_ibusComm.FocusIn();
			if (GlobalCachedInputContext.InputContext != null)
			{
				try
				{
					var context = GlobalCachedInputContext.InputContext;
					context.Reset();
					GlobalCachedInputContext.Keyboard = null;
					context.SetEngine("");
					context.Disable();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			_ibusComm.FocusOut();
		}

		protected bool CanSetIbusKeyboard()
		{
			if (!_ibusComm.Connected)
				return false;
			_ibusComm.FocusIn();
			return GlobalCachedInputContext.InputContext != null;
		}

		// Has to be internal because IbusKeyboardDescription is only internal
		internal bool IBusKeyboardAlreadySet(IbusKeyboardDescription keyboard)
		{
			if (keyboard?.IBusKeyboardEngine == null)
			{
				UnsetKeyboard();
				return true;
			}

			// check our cached value
			return GlobalCachedInputContext.Keyboard?.Equals(keyboard) ?? false;
		}

		protected static void UnsetKeyboard()
		{
			if (GlobalCachedInputContext.Keyboard == null)
				return;

			var context = GlobalCachedInputContext.InputContext;
			if (context != null)
			{
				context.Reset();
				context.Disable();
			}
			GlobalCachedInputContext.Keyboard = null;
		}

		/// <summary>
		/// Activate the ibus keyboard
		/// </summary>
		/// <remarks>This method will have a different implementation depending on how ibus
		/// keyboards are implemented.</remarks>
		protected virtual void SelectKeyboard(KeyboardDescription keyboard)
		{
			var ibusKeyboard = (IbusKeyboardDescription) keyboard;
			// Set the associated XKB keyboard
			var parentLayout = ibusKeyboard.ParentLayout;
			if (parentLayout == "en")
				parentLayout = "us";
			var xkbKeyboard = KeyboardController.Instance.AvailableKeyboards.FirstOrDefault(kbd => kbd.Layout == parentLayout);
			xkbKeyboard?.Activate();
			// Then set the IBus keyboard
			var context = GlobalCachedInputContext.InputContext;
			context.SetEngine(ibusKeyboard.IBusKeyboardEngine.LongName);
		}

		private void SetImePreEditWindowLocationAndSize(Control control)
		{
			var eventHandler = GetEventHandlerForControl(control);
			if (eventHandler == null)
				return;

			var location = eventHandler.SelectionLocationAndHeight;
			_ibusComm.NotifySelectionLocationAndHeight(location.Left, location.Top,
				location.Height);
		}

		/// <summary>
		/// Synchronize on a commit.
		/// </summary>
		/// <returns><c>true</c> if an open composition got cancelled, otherwise <c>false</c>.
		/// </returns>
		private void ResetAndWaitForCommit(Control control)
		{
			_ibusComm.Reset();

			// This should allow any generated commits to be handled by the message pump.
			// TODO: find a better way to synchronize
			Application.DoEvents();

			var eventHandler = GetEventHandlerForControl(control);
			eventHandler?.CommitOrReset();
		}

		private static IIbusEventHandler GetEventHandlerForControl(Control control)
		{
			if (control == null)
				return null;

			if (!KeyboardController.Instance.EventHandlers.TryGetValue(control, out var handler))
				return null;
			return handler as IIbusEventHandler;
		}

		#region KeyboardController events

		private void OnControlRegistered(object sender, RegisterEventArgs e)
		{
			if (e.Control == null)
				return;

			var eventHandler = e.EventHandler as IIbusEventHandler;
			if (eventHandler == null)
			{
				Debug.Assert(e.Control is TextBox, "Currently only TextBox controls are compatible with the default IBus event handler.");
				eventHandler = new IbusDefaultEventHandler((TextBox)e.Control);
			}
			KeyboardController.Instance.EventHandlers[e.Control] = eventHandler;

			_ibusComm.CommitText += eventHandler.OnCommitText;
			_ibusComm.UpdatePreeditText += eventHandler.OnUpdatePreeditText;
			_ibusComm.HidePreeditText += eventHandler.OnHidePreeditText;
			_ibusComm.KeyEvent += eventHandler.OnIbusKeyPress;
			_ibusComm.DeleteSurroundingText += eventHandler.OnDeleteSurroundingText;

			e.Control.GotFocus += HandleGotFocus;
			e.Control.LostFocus += HandleLostFocus;
			e.Control.MouseDown += HandleMouseDown;
			e.Control.PreviewKeyDown += HandlePreviewKeyDown;
			e.Control.KeyPress += HandleKeyPress;

			if (e.Control is ScrollableControl scrollableControl)
				scrollableControl.Scroll += HandleScroll;
		}

		private void OnControlRemoving(object sender, ControlEventArgs e)
		{
			if (e.Control == null)
				return;

			e.Control.GotFocus -= HandleGotFocus;
			e.Control.LostFocus -= HandleLostFocus;
			e.Control.MouseDown -= HandleMouseDown;
			e.Control.PreviewKeyDown -= HandlePreviewKeyDown;
			e.Control.KeyPress -= HandleKeyPress;
			e.Control.KeyDown -= HandleKeyDownAfterIbusHandledKey;

			if (e.Control is ScrollableControl scrollableControl)
				scrollableControl.Scroll -= HandleScroll;

			var eventHandler = GetEventHandlerForControl(e.Control);
			if (eventHandler == null)
				return;

			_ibusComm.CommitText -= eventHandler.OnCommitText;
			_ibusComm.UpdatePreeditText -= eventHandler.OnUpdatePreeditText;
			_ibusComm.HidePreeditText -= eventHandler.OnHidePreeditText;
			_ibusComm.KeyEvent -= eventHandler.OnIbusKeyPress;
			_ibusComm.DeleteSurroundingText -= eventHandler.OnDeleteSurroundingText;
			KeyboardController.Instance.EventHandlers.Remove(e.Control);
		}

		#endregion

		/// <summary>
		/// Passes the key event to ibus. This method deals with the special keys (Cursor up/down,
		/// backspace, etc.) that usually shouldn't cause a commit.
		/// </summary>
		private bool PassSpecialKeyEventToIbus(Control control, Keys keyChar, Keys modifierKeys)
		{
			var keySym = X11KeyConverter.GetKeySym(keyChar);
			return PassKeyEventToIbus(control, keySym, modifierKeys, false);
		}

		private bool PassKeyEventToIbus(Control control, char keyChar, Keys modifierKeys)
		{
			const char asciiDel = (char)0x7F;
			const char backspace = '\b';

			if (keyChar == asciiDel) // Ctrl+Backspace arrives as DEL; normalize to Backspace
				keyChar = backspace;

			return PassKeyEventToIbus(control, keyChar, modifierKeys, true);
		}

		private bool PassKeyEventToIbus(Control control, int keySym, Keys modifierKeys,
			bool resetIfUnhandled)
		{
			if (!_ibusComm.Connected)
				return false;

			var scancode = X11KeyConverter.GetScanCode(keySym);
			if (scancode > -1)
			{
				if (_ibusComm.ProcessKeyEvent(keySym, scancode, modifierKeys))
				{
					return true;
				}
			}

			if (resetIfUnhandled)
			{
				// If ProcessKeyEvent doesn't consume the key, we need to kill any pre-edits and
				// sync before continuing processing the keypress. We return false so that the
				// control can process the character.
				ResetAndWaitForCommit(control);
			}
			return false;
		}

		#region Event Handler for control

		private void HandleGotFocus(object sender, EventArgs e)
		{
			if (!_ibusComm.Connected)
				return;

			// On FieldWorks we get here twice: once because we intercept the WM_SETFOCUS message
			// and the other time because we have to call the original window proc. However, only
			// the second time will the control report as being focused (or when we not intercept
			// the message then the first time) (see SimpleRootSite.OriginalWndProc).
			if (!(sender is Control control) || !control.Focused)
				return;

			_ibusComm.FocusIn();
			_needIMELocation = true;
		}

		private void HandleLostFocus(object sender, EventArgs e)
		{
			if (!_ibusComm.Connected)
				return;

			_ibusComm.FocusOut();

			var eventHandler = GetEventHandlerForControl(sender as Control);

			eventHandler?.CommitOrReset();
		}

		/// <summary>
		/// Inform input bus of Keydown events
		/// This is useful to get warning of key that should stop the pre-edit
		/// </summary>
		private void HandlePreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (!_ibusComm.Connected)
				return;

			var control = (Control)sender;
			var eventHandler = GetEventHandlerForControl(control);
			if (eventHandler == null)
				return;

			if (_needIMELocation)
			{
				SetImePreEditWindowLocationAndSize(control);
				_needIMELocation = false;
			}

			var key = e.KeyCode;
			if (key == Keys.Escape)
			{
				// These should end a pre-edit, so wait until that has happened
				// before allowing the key to be processed.
				ResetAndWaitForCommit(control);
				return;
			}

			if (IsKeyHandledByIbus(key))
			{
				if (PassSpecialKeyEventToIbus(control, key, e.Modifiers))
				{
					// If IBus handled the key we don't want the control to get it. However,
					// we can't do this in PreviewKeyDown, so we temporarily subscribe to
					// KeyDown and suppress the key event there.
					control.KeyDown += HandleKeyDownAfterIbusHandledKey;
				}
				return;
			}

			// pass function keys onto ibus since they don't appear (on mono at least) as WM_SYSCHAR
			if (key >= Keys.F1 && key <= Keys.F24)
				PassSpecialKeyEventToIbus(control, key, e.Modifiers);
		}

		/// <summary>
		/// Handles a key down. While a pre-edit is active we don't want the control to handle
		/// any of the keys that IBus deals with.
		/// </summary>
		private void HandleKeyDownAfterIbusHandledKey(object sender, KeyEventArgs e)
		{
			if (IsKeyHandledByIbus(e.KeyCode) && sender is Control control)
			{
				var eventHandler = GetEventHandlerForControl(control);
				if (eventHandler != null)
					e.SuppressKeyPress = eventHandler.IsPreeditActive;
				control.KeyDown -= HandleKeyDownAfterIbusHandledKey;
			}
		}

		/// <summary>
		/// Gets whether the given key is expected to be handled by IBus when a pre-edit is active.
		/// </summary>
		/// <remarks>
		/// REVIEW: During pre-edit, I assume that IBus handles both basic and modified navigation
		/// keys like Ctrl+Left. ChatGPT says this is true, but I have not been able to verify this
		/// since I don't know how to set up a Linux/IBus environment where I could test this.
		/// </remarks>
		private bool IsKeyHandledByIbus(Keys key)
		{
			switch (key & Keys.KeyCode)
			{
				case Keys.Up:
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
				case Keys.Delete:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End:
				case Keys.Back:
					return true;
			}

			return false;
		}

		/// <summary>
		/// Handles a key press.
		/// </summary>
		/// <remarks>When the user types a character the control receives the KeyPress event and
		/// this method gets called. We forward the key press to IBus. If IBus swallowed the key
		/// it will return true, so no further handling is done by the control, otherwise the
		/// control will process the key and update the selection.
		/// If IBus swallows the key event, it will either raise the
		/// <see cref="IIbusCommunicator.UpdatePreeditText"/> event, allowing the event handler to
		/// insert the composition as pre-edit (and update the selection), or it will raise the
		/// <see cref="IIbusCommunicator.CommitText"/> event so that the event handler can
		/// remove the pre-edit, replace it with the final composition string and update the
		/// selection. Some IBus keyboards might raise a
		/// <see cref="IBusDotNet.IInputContext.ForwardKeyEvent"/> (handled by
		/// <see cref="IIbusEventHandler.OnIbusKeyPress"/>) prior to raising
		/// <see cref="IIbusCommunicator.CommitText"/> to simulate a key press (e.g. backspace)
		/// so that the event handler can modify the existing text of the control.
		/// IBus might also open a pop-up window at the location we told it
		/// (<see cref="IIbusEventHandler.SelectionLocationAndHeight"/>) to display possible
		/// compositions. However, it will still raise
		/// <see cref="IIbusCommunicator.UpdatePreeditText"/>.</remarks>
		private void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = PassKeyEventToIbus(sender as Control, e.KeyChar, Control.ModifierKeys);
		}

		private void HandleMouseDown(object sender, MouseEventArgs e)
		{
			if (!_ibusComm.Connected)
				return;

			ResetAndWaitForCommit(sender as Control);
			_needIMELocation = true;
		}

		private void HandleScroll(object sender, ScrollEventArgs e)
		{
			if (!_ibusComm.Connected)
				return;

			SetImePreEditWindowLocationAndSize(sender as Control);
		}

		#endregion

		#region IKeyboardSwitchingAdaptor implementation

		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			var ibusKeyboard = keyboard as IbusKeyboardDescription;
			try
			{
				if (!CanSetIbusKeyboard())
					return false;
				if (IBusKeyboardAlreadySet(ibusKeyboard))
					return true;

				SelectKeyboard(ibusKeyboard);

				GlobalCachedInputContext.Keyboard = ibusKeyboard;
				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Changing keyboard failed, is kfml/ibus running? {0}", e);
				return false;
			}
		}

		/// <summary>
		/// Deactivates the specified keyboard.
		/// </summary>
		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			UnsetKeyboard();
		}

		/// <summary>
		/// Implementation is not required because this is not the primary (Type System) adapter.
		/// </summary>
		public virtual KeyboardDescription DefaultKeyboard => throw new NotImplementedException();

		/// <summary>
		/// Implementation is not required because this is not the primary (Type System) adapter.
		/// </summary>
		public virtual KeyboardDescription ActiveKeyboard => throw new NotImplementedException();

		#endregion
	}
}
