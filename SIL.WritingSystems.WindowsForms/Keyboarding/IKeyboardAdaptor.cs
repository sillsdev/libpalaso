// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------

using System;

namespace SIL.WritingSystems.WindowsForms.Keyboarding
{
	/// <summary>
	/// The different keyboard types we're supporting.
	/// </summary>
	public enum KeyboardAdaptorType
	{
		/// <summary>
		/// System keyboard like Windows API or xkb
		/// </summary>
		System,
		/// <summary>
		/// Other input method like Keyman, InKey or ibus
		/// </summary>
		OtherIm
	}

	/// <summary>
	/// Interface implemented by some helper classes used by KeyboardController, which
	/// maintains a list of keyboard adapters, one for each type of keyboard on the current platform
	/// which require different treatment.  In particular a keyboard adapter is responsible to figure out which keyboards of the type
	/// it handles are installed, and to activate one of them when we think the user wants to type with it.
	/// </summary>
	public interface IKeyboardAdaptor : IDisposable
	{
		/// <summary>
		/// Initialize the installed keyboards: add to the master list the available keyboards recognized by this adapter.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Add to the master list the (currently) available keyboards recognized by this adapter. This is called when
		/// we need the list to be up-to-date (e.g., when displaying a chooser). The controller first empties the list.
		/// </summary>
		void UpdateAvailableKeyboards();

		bool ActivateKeyboard(KeyboardDescription keyboard);

		/// <summary>
		/// Called to allow state to be saved when a different keyboard is being activated or the window is being deactivated.
		/// Does not change the active keyboard.
		/// </summary>
		/// <param name="keyboard"></param>
		void DeactivateKeyboard(KeyboardDescription keyboard);

		KeyboardDescription GetKeyboardForInputLanguage(IInputLanguage inputLanguage);

		/// <summary>
		/// Creates and returns a keyboard definition object of the type needed by this adapter (and hooked to it)
		/// based on the ID. However, since this method is used (at least by external code) to create
		/// definitions for unavailable keyboards, the expectation is that this keyboard cannot be successfully
		/// activated.
		/// <remarks>This only needs to be implemented by the (first) adapter of type System. It will never be called
		/// on other adapters and may be unimplemented by them, unless the adapter uses it internally.
		/// (Each adapter is given a chance to create all the available keyboards of its type in the course of
		/// executing its Initialize() or UpdateAvailableKeyboards() methods. CreateKeyboardDefinition is called later,
		/// when we need a keyboard definition for a keyboard that is NOT available, such as one found in an LDML file
		/// that does not match anything available on this system. Since it isn't available, it's arbitrary which
		/// adapter creates a keyboard for it, so we arbitrarily pick the first of type System.</remarks>
		/// </summary>
		KeyboardDescription CreateKeyboardDefinition(string id);

		/// <summary>
		/// Gets the default keyboard of the system. This only needs to be implemented by the (first) adapter of type system.
		/// </summary>
		KeyboardDescription DefaultKeyboard { get; }

		/// <summary>
		/// Gets the type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		KeyboardAdaptorType Type { get; }

		/// <summary>
		/// Determines whether this adaptor can handle the specified keyboard format.
		/// </summary>
		bool CanHandleFormat(KeyboardFormat format);
	}
}
