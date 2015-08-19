// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces
{
	/// <summary>
	/// Interface implemented by some helper classes used by KeyboardController, which
	/// maintains a list of keyboard retriever adapters, one for each type of keyboard on the
	/// current platform which require different treatment.  In particular a keyboard retrieving
	/// adapter is responsible to figure out which keyboards of the type it handles are
	/// installed.
	/// </summary>
	public interface IKeyboardRetrievingAdaptor
	{
		/// <summary>
		/// Gets the type of keyboards this retriever handles: system or other (like Keyman, ibus...)
		/// or both.
		/// </summary>
		KeyboardType Type { get; }

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
		IKeyboardSwitchingAdaptor Adaptor { get; }

		/// <summary>
		/// Initialize this keyboard retriever
		/// </summary>
		void Initialize();

		/// <summary>
		/// Retrieve and register the available keyboards
		/// </summary>
		void RegisterAvailableKeyboards();

		/// <summary>
		/// Update the available keyboards
		/// </summary>
		void UpdateAvailableKeyboards();

		/// <summary>
		/// List of keyboard layouts that either gave an exception or other error trying to
		/// get more information. We don't have enough information for these keyboard layouts
		/// to include them in the list of installed keyboards.
		/// </summary>
		/// <returns>List of IKeyboardErrorDescription objects, or an empty list.</returns>
		List<IKeyboardErrorDescription> ErrorKeyboards { get; }

		IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage);

		/// <summary>
		/// Creates and returns a keyboard definition object of the type needed by this adapter (and hooked to it)
		/// based on the layout and locale. However, since this method is used (at least by external code) to create
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
		IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale);

		/// <summary>
		/// Shutdown this instance and prevent futher use
		/// </summary>
		void Close();

		/// <summary>
		/// Gets the keyboard setup application and the arguments needed to call it.
		/// </summary>
		string GetKeyboardSetupApplication(out string arguments);

		/// <summary>
		/// Returns <c>true</c> if this is the secondary keyboard application, e.g.
		/// Keyman setup dialog on Windows.
		/// </summary>
		bool IsSecondaryKeyboardSetupApplication { get; }
	}
}

