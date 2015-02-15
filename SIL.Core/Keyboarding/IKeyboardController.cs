using System;
using System.Collections.Generic;

namespace SIL.Keyboarding
{
	/// <summary>
	/// This interface defines the functions of a keyboard controller.
	/// Various classes obtain one of these when needed by reading Keyboard.Controller.
	/// The default implementation of this in the core SIL.Core DLL has minimal functionality.
	/// Typically clients will set Keyboard.Controller to some more useful class, such as SIL.Windows.Forms.Keyboarding.KeyboardController.
	/// </summary>
	public interface IKeyboardController: IDisposable
	{
		/// <summary>
		/// Gets the keyboard for the default input language.
		/// </summary>
		IKeyboardDefinition DefaultKeyboard { get; }

		/// <summary>
		/// Gets the keyboard with the specified <paramref name="id"/>.
		/// If the keyboard doesn't exist, a no-op keyboard is returned.
		/// </summary>
		IKeyboardDefinition GetKeyboard(string id);

		/// <summary>
		/// Tries to get the keyboard with the specified <paramref name="id"/>.
		/// </summary>
		bool TryGetKeyboard(string id, out IKeyboardDefinition keyboard);

		/// <summary>
		/// Gets the keyboard with the specified <paramref name="layoutName"/> and <paramref name="locale"/>.
		/// If the keyboard doesn't exist, a no-op keyboard is returned.
		/// </summary>
		IKeyboardDefinition GetKeyboard(string layoutName, string locale);

		/// <summary>
		/// Tries to get the keyboard with the specified <paramref name="layoutName"/> and <paramref name="locale"/>.
		/// </summary>
		bool TryGetKeyboard(string layoutName, string locale, out IKeyboardDefinition keyboard);

		/// <summary>
		/// Gets the keyboard for the specified <paramref name="language"/>.
		/// If the keyboard doesn't exist, a no-op keyboard is returned.
		/// </summary>
		IKeyboardDefinition GetKeyboard(IInputLanguage language);

		/// <summary>
		/// Tries to get the keyboard for the specified <paramref name="language"/>.
		/// </summary>
		bool TryGetKeyboard(IInputLanguage language, out IKeyboardDefinition keyboard);

		/// <summary>
		/// Gets the keyboard with the specified <paramref name="windowsLcid"/>.
		/// If the keyboard doesn't exist, a no-op keyboard is returned.
		/// </summary>
		IKeyboardDefinition GetKeyboard(int windowsLcid);

		/// <summary>
		/// Tries to get the keyboard with the specified <paramref name="windowsLcid"/>.
		/// </summary>
		bool TryGetKeyboard(int windowsLcid, out IKeyboardDefinition keyboard);

		/// <summary>
		/// Activates the keyboard of the default input language.
		/// </summary>
		void ActivateDefaultKeyboard();

		/// <summary>
		/// Returns everything that is installed on the system and available to be used.
		/// This would typically be used to populate a list of available keyboards in configuring a writing system.
		/// Currently it is also used to evaluate whether a particular keyboard is available to use on this system.
		/// Enhance: the latter task could be done more efficiently with another API function to indicate whether
		/// a particular keyboard is available. Not sure whether the cost of building and searching the list is enough
		/// to make this worthwhile.
		/// </summary>
		IEnumerable<IKeyboardDefinition> AvailableKeyboards { get; }

		/// <summary>
		/// Call when something may have changed in the external world that should affect AllAvailableKeyboards
		/// (e.g., when activating a window that needs an accurate current list).
		/// </summary>
		void UpdateAvailableKeyboards();

		/// <summary>
		/// Gets or creates a keyboard definition based on the ID.
		/// </summary>
		IKeyboardDefinition CreateKeyboard(string id, KeyboardFormat format, IEnumerable<string> urls);

		/// <summary>
		/// Gets or sets the currently active keyboard
		/// </summary>
		IKeyboardDefinition ActiveKeyboard { get; set; }
	}
}
