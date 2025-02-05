// Copyright (c) 2011-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding
{
	/// <summary>
	/// The different keyboard types we're supporting.
	/// </summary>
	[Flags]
	public enum KeyboardAdaptorType
	{
		/// <summary>
		/// System keyboard like Windows API or xkb
		/// </summary>
		System = 1,
		/// <summary>
		/// Other input method like Keyman, InKey or ibus
		/// </summary>
		OtherIm = 2
	}

	/// <summary>
	/// Interface implemented by some helper classes used by KeyboardController, which
	/// maintains a list of keyboard retriever adapters, one for each type of keyboard on the
	/// current platform which require different treatment.  In particular a keyboard retrieving
	/// adapter is responsible to figure out which keyboards of the type it handles are
	/// installed.
	/// </summary>
	public interface IKeyboardRetrievingAdaptor : IDisposable
	{
		/// <summary>
		/// Gets the type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		KeyboardAdaptorType Type { get; }

		/// <summary>
		/// Checks whether this keyboard retriever can get keyboards. Different desktop
		/// environments use differing APIs to get the available keyboards. If this class is
		/// able to find the available keyboards this property will return <c>true</c>,
		/// otherwise <c>false</c>.
		/// </summary>
		bool IsApplicable { get; }

		/// <summary>
		/// Gets the keyboard adaptor that deals with keyboards that this class retrieves.
		/// </summary>
		IKeyboardSwitchingAdaptor SwitchingAdaptor { get; }

		/// <summary>
		/// Initialize the installed keyboards: add to the master list the available keyboards recognized by this adapter.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Add to the master list the (currently) available keyboards recognized by this adapter. This is called when
		/// we need the list to be up-to-date (e.g., when displaying a chooser). The controller first empties the list.
		/// </summary>
		void UpdateAvailableKeyboards();

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
		/// Determines whether this adaptor can handle the specified keyboard format.
		/// </summary>
		bool CanHandleFormat(KeyboardFormat format);

		/// <summary>
		/// Gets an action that when executed will launch the keyboard setup application
		/// </summary>
		Action GetKeyboardSetupAction();

		/// <summary>
		/// Gets an action that when executed will launch the secondary keyboard setup
		/// application, or null if this adaptor doesn't support secondary keyboard setup
		/// applications
		/// </summary>
		Action GetSecondaryKeyboardSetupAction();

		/// <summary>
		/// Returns <c>true</c> if this is the secondary keyboard application, e.g.
		/// Keyman setup dialog on Windows.
		/// </summary>
		bool IsSecondaryKeyboardSetupApplication { get; }
	}
}
