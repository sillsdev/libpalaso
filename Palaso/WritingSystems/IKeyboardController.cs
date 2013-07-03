using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// This interface defines the functions of a keyboard controller that are required to implement the keyboard-related
	/// methods of WritingSystem.
	/// Various classes obtain one of these when needed by reading Keyboarding.Controller.
	/// The default implementation of this in the core Palaso DLL has minimal functionality.
	/// Typically clients will set Keyboarding.Controller to some more useful class, such as UI.WindowsForms.KeyboardController.
	/// </summary>
	public interface IKeyboardController
	{
		void Activate(IKeyboardDefinition keyboard);

		IEnumerable<IKeyboardDefinition> AllAvailableKeyboards { get; }
	}
}
