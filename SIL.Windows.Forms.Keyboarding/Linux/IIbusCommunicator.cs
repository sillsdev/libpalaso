// Copyright (c) 2013, SIL International.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
using System;
using System.Windows.Forms;
using IBusDotNet;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Methods that the IbusKeyboardAdaptor calls to communicate with IBus, and events that
	/// are used by IBus to forward events to the adaptor.
	/// These methods and events are implemented by IbusCommunicator, but are extracted in an
	/// interface so that unit tests can create doubles.
	/// IbusCommunicator is a utility class that manages some aspects of the communication with
	/// IBus, e.g. creating and releasing the connection to IBus and dealing with connection
	/// errors.
	/// </summary>
	public interface IIbusCommunicator : IDisposable
	{
		/// <summary>
		/// Returns <c>true</c> if disposed
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// Returns true if connected to IBus.
		/// </summary>
		bool Connected { get; }

		IBusConnection Connection { get; }

		/// <summary>
		/// Informs IBus that our control received focus.
		/// </summary>
		void FocusIn();

		/// <summary>
		/// Informs IBus that our control lost focus.
		/// </summary>
		void FocusOut();

		/// <summary>
		/// Tells IBus the location and height of the selection
		/// </summary>
		void NotifySelectionLocationAndHeight(int x, int y, int height);

		/// <summary>
		/// Sends a key event to the IBus. This method will be called by the IbusKeyboardAdapter
		/// on some KeyDown and all KeyPress events.
		/// </summary>
		/// <param name="keySym">The X11 key symbol (for special keys) or the key code</param>
		/// <param name="scanCode">The X11 scan code</param>
		/// <param name="state">The modifier state, i.e. shift key etc.</param>
		/// <returns><c>true</c> if the key event is handled by ibus and thus the control
		/// shouldn't process it.</returns>
		/// <remarks>The IBus keyboard will either use the <paramref name="keySym"/> or
		/// <paramref name="scanCode"/> and <paramref name="state"/>, depending on wether it
		/// expects a specific keyboard layout or not.
		/// <see href="http://ibus.github.io/docs/ibus-1.5/IBusInputContext.html#ibus-input-context-process-key-event-async"/>
		/// </remarks>
		/// <seealso cref="IbusKeyboardSwitchingAdaptor.HandleKeyPress"/>
		bool ProcessKeyEvent(int keySym, int scanCode, Keys state);

		/// <summary>
		/// Cancels any open compositions and closes the preedit window.
		/// </summary>
		void Reset();

		/// <summary>
		/// Create an input context.
		/// </summary>
		void CreateInputContext();

		/// <summary>Occurs when the composition string gets commited, e.g. after the user pressed
		/// the space bar.</summary>
		event Action<object> CommitText;

		/// <summary>Occurs after the user pressed a key. IBus raises this event while processing
		/// a key event (while we're in ProcessKeyEvent) to update the temporary composition
		/// string that is displayed to the user.</summary>
		event Action<object, int> UpdatePreeditText;

		/// <summary>IBus raises this event while processing a key event to delete surrounding
		/// characters (before or after the current cursor position).</summary>
		/// <seealso href="http://ibus.github.io/docs/ibus-1.5/IBusInputContext.html#IBusInputContext-delete-surrounding-text"/>
		event Action<int, int> DeleteSurroundingText;

		/// <summary>Occurs to remove the temporary composition string without committing it.
		/// A client of IIbusCommunicator will typically call Reset() when this event gets
		/// raised.</summary>
		event Action HidePreeditText;

		/// <summary>IBus raises this event so that the application can act on the key event,
		/// e.g. it will pass Backspace as argument so that the application/control can delete
		/// the character to the left of the IP before IBus commits a new character.</summary>
		/// <seealso cref="SIL.Windows.Forms.Keyboarding.Linux.IIbusEventHandler.OnIbusKeyPress"/>
		event Action<int, int, int> KeyEvent;
	}
}
