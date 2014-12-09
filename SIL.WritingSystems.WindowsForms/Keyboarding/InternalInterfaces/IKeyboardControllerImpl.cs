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
using System.Windows.Forms;
using SIL.WritingSystems;
using SIL.WritingSystems.WindowsForms.Keyboarding.Types;

namespace SIL.WritingSystems.WindowsForms.Keyboarding.InternalInterfaces
{
	public delegate void RegisterEventHandler(object sender, RegisterEventArgs e);

	/// <summary>
	/// Interface for the implementation of the keyboard controller. Implement this
	/// interface if you want to provide a double for unit testing. Otherwise the default
	/// implementation is sufficient.
	/// </summary>
	public interface IKeyboardControllerImpl: IDisposable
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

        /// <summary>
        /// Gets the default system keyboard.
        /// </summary>
        IKeyboardDefinition DefaultKeyboard { get; }
        
        /// <summary>
		/// Registers the control for keyboarding. Called by KeyboardController when the
		/// application registers a control by calling KeyboardController.Register.
		/// </summary>
		/// <param name="control">The control to register</param>
		/// <param name="eventHandler">An event handler that receives events from the keyboard
		/// adaptors. This gets passed in the ControlAdded event. Currently only IBus makes use
		/// of the eventHandler. The event handler is adaptor specific,
		/// therefore we use a generic object. If <c>null</c> a default handler is used.</param>
		void RegisterControl(Control control, object eventHandler);

		/// <summary>
		/// Unregisters the control from keyboarding.
		/// </summary>
		void UnregisterControl(Control control);

		/// <summary>
		/// Occurs when the application registers the control by calling KeyboardController.Register.
		/// </summary>
		event RegisterEventHandler ControlAdded;

		/// <summary>
		/// Occurs when a control gets removed by calling KeyboardController.Unregister.
		/// </summary>
		event ControlEventHandler ControlRemoving;

		Dictionary<Control, object> EventHandlers { get; }
	}
}
