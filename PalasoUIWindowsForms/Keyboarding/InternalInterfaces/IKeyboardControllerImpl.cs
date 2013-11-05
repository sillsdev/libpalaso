// --------------------------------------------------------------------------------------------
// <copyright from='2012' to='2012' company='SIL International'>
// 	Copyright (c) 2012, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;

namespace Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces
{
	/// <summary>
	/// Internal interface for the implementation of the keyboard controller. Implement this
	/// interface if you want to provide a double for unit testing. Otherwise the default
	/// implementation is sufficient.
	/// </summary>
	internal interface IKeyboardControllerImpl: IDisposable
	{
		/// <summary>
		/// Gets the available keyboards
		/// </summary>
		KeyboardCollection Keyboards { get; }

		/// <summary>
		/// Gets or sets the currently active keyboard. This is just a place to record it; setting it does not
		/// change the keyboard behavior.
		/// </summary>
		IKeyboardDefinition ActiveKeyboard { get; set; }
	}
}
