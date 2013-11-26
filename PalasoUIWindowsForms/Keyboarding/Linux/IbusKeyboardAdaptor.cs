// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------
#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using IBusDotNet;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling ibus keyboards on Linux. Currently just a wrapper for KeyboardSwitcher.
	/// </summary>
	[CLSCompliant(false)]
	public class IbusKeyboardAdaptor: IKeyboardAdaptor
	{
		private IIbusCommunicator IBusCommunicator;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="Palaso.UI.WindowsForms.Keyboard.Linux.IbusKeyboardAdaptor"/> class.
		/// </summary>
		public IbusKeyboardAdaptor(): this(new IbusCommunicator())
		{
		}

		/// <summary>
		/// Used in unit tests
		/// </summary>
		public IbusKeyboardAdaptor(IIbusCommunicator ibusCommunicator)
		{
			IBusCommunicator = ibusCommunicator;

			if (!IBusCommunicator.Connected)
				return;

			if (KeyboardController.EventProvider != null)
			{
				KeyboardController.EventProvider.ControlAdded += OnControlRegistered;
				KeyboardController.EventProvider.ControlRemoving += OnControlRemoving;
			}

			IBusCommunicator.CreateInputContext();
		}

		protected virtual void InitKeyboards()
		{
			foreach (var ibusKeyboard in GetIBusKeyboards())
			{
				var keyboard = new IbusKeyboardDescription(this, ibusKeyboard);
				KeyboardController.Manager.RegisterKeyboard(keyboard);
			}
		}

		private IBusEngineDesc[] GetIBusKeyboards()
		{
			if (!IBusCommunicator.Connected)
				return null;

			var ibusWrapper = new InputBus(IBusCommunicator.Connection);
			return ibusWrapper.ListActiveEngines();
		}

		private bool SetIMEKeyboard(IbusKeyboardDescription keyboard)
		{
			try
			{
				if (!IBusCommunicator.Connected)
					return false;

				IBusCommunicator.FocusIn();
				if (GlobalCachedInputContext.InputContext == null)
					return false;

				// check our cached value
				if (GlobalCachedInputContext.Keyboard == keyboard)
					return true;

				var context = GlobalCachedInputContext.InputContext;

				if (keyboard == null || keyboard.IBusKeyboardEngine == null)
				{
					context.Reset();
					GlobalCachedInputContext.Keyboard = null;
					context.Disable();
					return true;
				}

				// Set the associated XKB keyboard
				var parentLayout = keyboard.IBusKeyboardEngine.Layout;
				if (parentLayout == "en")
					parentLayout = "us";
				var xkbKeyboard = Keyboard.Controller.AllAvailableKeyboards.FirstOrDefault(kbd => kbd.Layout == parentLayout);
				if (xkbKeyboard != null)
					xkbKeyboard.Activate();

				// Then set the IBus keyboard
				context.SetEngine(keyboard.IBusKeyboardEngine.LongName);

				GlobalCachedInputContext.Keyboard = keyboard;
				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("Changing keyboard failed, is kfml/ibus running? {0}", e));
				return false;
			}
		}

		private void SetImePreeditWindowLocationAndSize(Control control)
		{
			var eventHandler = GetEventHandlerForControl(control);
			if (eventHandler != null)
			{
				var location = eventHandler.SelectionLocationAndHeight;
				IBusCommunicator.NotifySelectionLocationAndHeight(location.Left, location.Top,
					location.Height);
			}
		}

		/// <summary>
		/// Synchronize on a commit.
		/// </summary>
		/// <returns><c>true</c> if an open composition got cancelled, otherwise <c>false</c>.
		/// </returns>
		private bool ResetAndWaitForCommit(Control control)
		{
			IBusCommunicator.Reset();

			// This should allow any generated commits to be handled by the message pump.
			// TODO: find a better way to synchronize
			Application.DoEvents();

			var eventHandler = GetEventHandlerForControl(control);
			if (eventHandler != null)
				return eventHandler.CommitOrReset();
			return false;
		}

		private static IIbusEventHandler GetEventHandlerForControl(Control control)
		{
			if (control == null)
				return null;

			object handler;
			if (!KeyboardController.EventProvider.EventHandlers.TryGetValue(control, out handler))
				return null;
			return handler as IIbusEventHandler;
		}

		#region KeyboardController events
		private void OnControlRegistered(object sender, RegisterEventArgs e)
		{
			if (e.Control != null)
			{
				var eventHandler = e.EventHandler as IIbusEventHandler;
				if (eventHandler == null)
				{
					Debug.Assert(e.Control is TextBox, "Currently only TextBox controls are compatible with the default IBus event handler.");
					eventHandler = new IbusDefaultEventHandler((TextBox)e.Control);
				}
				KeyboardController.EventProvider.EventHandlers[e.Control] = eventHandler;

				IBusCommunicator.CommitText += eventHandler.OnCommitText;
				IBusCommunicator.UpdatePreeditText += eventHandler.OnUpdatePreeditText;
				IBusCommunicator.HidePreeditText += eventHandler.OnHidePreeditText;
				IBusCommunicator.KeyEvent += eventHandler.OnIbusKeyPress;
				IBusCommunicator.DeleteSurroundingText += eventHandler.OnDeleteSurroundingText;

				e.Control.GotFocus += HandleGotFocus;
				e.Control.LostFocus += HandleLostFocus;
				e.Control.MouseDown += HandleMouseDown;
				e.Control.PreviewKeyDown += HandlePreviewKeyDown;
				e.Control.KeyPress += HandleKeyPress;
			}
		}

		private void OnControlRemoving(object sender, ControlEventArgs e)
		{
			if (e.Control != null)
			{
				e.Control.GotFocus -= HandleGotFocus;
				e.Control.LostFocus -= HandleLostFocus;
				e.Control.MouseDown -= HandleMouseDown;
				e.Control.PreviewKeyDown -= HandlePreviewKeyDown;
				e.Control.KeyPress -= HandleKeyPress;

				var eventHandler = GetEventHandlerForControl(e.Control);
				if (eventHandler != null)
				{
					IBusCommunicator.CommitText -= eventHandler.OnCommitText;
					IBusCommunicator.UpdatePreeditText -= eventHandler.OnUpdatePreeditText;
					IBusCommunicator.HidePreeditText -= eventHandler.OnHidePreeditText;
					IBusCommunicator.KeyEvent -= eventHandler.OnIbusKeyPress;
					IBusCommunicator.DeleteSurroundingText -= eventHandler.OnDeleteSurroundingText;
					KeyboardController.EventProvider.EventHandlers.Remove(e.Control);
				}

			}
		}
		#endregion

		private bool PassKeyEventToIbus(Control control, char keyChar, Keys modifierKeys)
		{
			if (!IBusCommunicator.Connected)
				return false;

			// modifierKeys don't contain CapsLock and Control.IsKeyLocked(Keys.CapsLock)
			// doesn't work on mono. So we guess the caps state by unicode value and the shift
			// state. This is far from ideal.
			if (char.IsUpper(keyChar) && (modifierKeys & Keys.Shift) == 0)
				modifierKeys |= Keys.CapsLock;
			else if (char.IsLower(keyChar) && (modifierKeys & Keys.Shift) != 0)
				modifierKeys |= Keys.CapsLock;

			int scancode = X11KeyConverter.GetScanCode(keyChar);
			if (scancode > -1)
			{
				if (IBusCommunicator.ProcessKeyEvent(keyChar, scancode, modifierKeys))
				{
					return true;
				}
			}
			// If ProcessKeyEvent doesn't consume the key, we need to kill any preedits and
			// sync before continuing processing the keypress. If it's a backspace we return
			// true because it is consumed by removing the pre-edit. For other characters we
			// return false so that the control can process the character.
			if (ResetAndWaitForCommit(control) && keyChar == '\b')
				return true;
			return false;
		}

		#region Event Handler for control
		private void HandleGotFocus(object sender, EventArgs e)
		{
			if (!IBusCommunicator.Connected)
				return;

			IBusCommunicator.FocusIn();
			SetImePreeditWindowLocationAndSize(sender as Control);
		}

		private void HandleLostFocus(object sender, EventArgs e)
		{
			if (!IBusCommunicator.Connected)
				return;

			IBusCommunicator.FocusOut();

			var eventHandler = GetEventHandlerForControl(sender as Control);
			if (eventHandler == null)
				return;

			eventHandler.CommitOrReset();
		}

		/// <summary>
		/// Inform input bus of Keydown events
		/// This is useful to get warning of key that should stop the preedit
		/// </summary>
		private void HandlePreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (!IBusCommunicator.Connected)
				return;

			var eventHandler = GetEventHandlerForControl(sender as Control);
			if (eventHandler == null)
				return;

			var key = e.KeyCode;
			switch (key)
			{
				case Keys.Escape:
					// These should end a preedit, so wait until that has happened
					// before allowing the key to be processed.
					ResetAndWaitForCommit(sender as Control);
					return;
				case Keys.Up:
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
				case Keys.Delete:
					// pass cursor keys to ibus
					PassKeyEventToIbus(sender as Control, (char)X11KeyConverter.GetKeySym(key), Control.ModifierKeys);
					return;
				case Keys.Back:
					// we'll get a WM_CHAR for this and have ibus handle it then
					return;
			}
			// pass function keys onto ibus since they don't appear (on mono at least) as WM_SYSCHAR
			if (key >= Keys.F1 && key <= Keys.F24)
				PassKeyEventToIbus(sender as Control, (char)X11KeyConverter.GetKeySym(key), Control.ModifierKeys);
		}

		/// <summary>
		/// Handles a key press.
		/// </summary>
		/// <remarks>When the user types a character the control receives the KeyPress event and
		/// this method gets called. We forward the key press to IBus. If IBus swallowed the key
		/// it will return true, so no further handling is done by the control, otherwise the
		/// control will process the key and update the selection.
		/// If IBus swallows the key event, it will either raise the UpdatePreeditText event,
		/// allowing the event handler to insert the composition as preedit (and update the
		/// selection), or it will raise the CommitText event so that the event handler can
		/// remove the preedit, replace it with the final composition string and update the
		/// selection. Some IBus keyboards might raise a ForwardKeyEvent (handled by
		/// <see cref="IIbusEventHandler.OnIbusKeyPress"/>) prior to calling CommitText to
		/// simulate a key press (e.g. backspace) so that the event handler can modify the
		/// existing text of the control.
		/// IBus might also open a pop-up window at the location we told it
		/// (<see cref="IIbusEventHandler.SelectionLocationAndHeight"/>) to display possible
		/// compositions. However, it will still call UpdatePreeditText.</remarks>
		private void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = PassKeyEventToIbus(sender as Control, e.KeyChar, Control.ModifierKeys);
		}

		private void HandleMouseDown(object sender, MouseEventArgs e)
		{
			if (!IBusCommunicator.Connected)
				return;

			ResetAndWaitForCommit(sender as Control);
			SetImePreeditWindowLocationAndSize(sender as Control);
		}
		#endregion

		#region IKeyboardAdaptor implementation
		/// <summary>
		/// Initialize the installed keyboards
		/// </summary>
		public void Initialize()
		{
			InitKeyboards();
			// Don't turn on any Ibus IME keyboard until requested explicitly.
			// If we do nothing, the first Ibus IME keyboard is automatically activated.
			IBusCommunicator.FocusIn();
			if (GlobalCachedInputContext.InputContext != null)
			{
				var context = GlobalCachedInputContext.InputContext;
				context.Reset();
				GlobalCachedInputContext.Keyboard = null;
				context.SetEngine("");
				context.Disable();
			}
			IBusCommunicator.FocusOut();
		}

		public void UpdateAvailableKeyboards()
		{
			InitKeyboards();
		}

		/// <summary/>
		public void Close()
		{
			if (!IBusCommunicator.IsDisposed)
			{
				IBusCommunicator.Dispose();
			}
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			var ibusKeyboard = keyboard as IbusKeyboardDescription;
			return SetIMEKeyboard(ibusKeyboard);
		}

		/// <summary>
		/// Deactivates the specified keyboard.
		/// </summary>
		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			SetIMEKeyboard(null);
		}

		/// <summary>
		/// List of keyboard layouts that either gave an exception or other error trying to
		/// get more information. We don't have enough information for these keyboard layouts
		/// to include them in the list of installed keyboards.
		/// </summary>
		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get
			{
				return new List<IKeyboardErrorDescription>();
			}
		}

		// Currently we expect this to only be useful on Windows.
		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardType Type
		{
			get { return KeyboardType.OtherIm; }
		}

		/// <summary>
		/// Implemenation is not required because this is not the primary (Type System) adapter.
		/// </summary>
		public IKeyboardDefinition DefaultKeyboard
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Only the primary (Type=System) adapter is required to implement this method. This one makes keyboards
		/// during Initialize, but is not used to make an unavailable keyboard to match an LDML file.
		/// </summary>
		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
#endif
