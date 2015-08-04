// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces
{
	/// <summary>
	/// Interface implemented by some helper classes used by KeyboardController, which
	/// maintains a list of keyboard adapters, one for each type of keyboard on the current platform
	/// which require different treatment.  In particular a keyboard switching adapter is
	/// responsible to activate one of the keyboards of its type when we think the user wants to
	/// type with it.
	/// </summary>
	public interface IKeyboardSwitchingAdaptor
	{
		bool ActivateKeyboard(IKeyboardDefinition keyboard);

		/// <summary>
		/// Called to allow state to be saved when a different keyboard is being activated or the window is being deactivated.
		/// Does not change the active keyboard.
		/// </summary>
		/// <param name="keyboard"></param>
		void DeactivateKeyboard(IKeyboardDefinition keyboard);

		/// <summary>
		/// Gets the default keyboard of the system. This only needs to be implemented by the (first) adapter of type system.
		/// </summary>
		IKeyboardDefinition DefaultKeyboard { get; }

		/// <summary>
		/// Gets the currently active keyboard. This only needs to be implemented by the (first) adapter of
		/// type system, and only if the implementation in KeyboardControllerImpl (which uses layoutname
		/// and culturename based on the current input language) isn't sufficient.
		/// </summary>
		IKeyboardDefinition ActiveKeyboard { get; }

		/// <summary>
		/// Gets the type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		KeyboardType Type { get; }
	}
}
