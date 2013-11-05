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
		/// <summary>
		/// Make this keyboard active.
		/// Review: do we need some argument to indicate which window to make it active for?
		/// </summary>
		/// <param name="keyboard"></param>
		void Activate(IKeyboardDefinition keyboard);

		/// <summary>
		/// Returns everything that is installed on the system and available to be used.
		/// This would typically be used to populate a list of available keyboards in configuring a writing system.
		/// Currently it is also used to evaluate whether a particular keyboard is available to use on this system.
		/// Enhance: the latter task could be done more efficiently with another API function to indicate whether
		/// a particular keyboard is available. Not sure whether the cost of building and searching the list is enough
		/// to make this worthwhile.
		/// </summary>
		IEnumerable<IKeyboardDefinition> AllAvailableKeyboards { get; }

		/// <summary>
		/// Figures out the system default keyboard for the specified writing system (the one to use if we have no available KnownKeyboards).
		/// The implementation may use obsolete fields such as Keyboard
		/// </summary>
		/// <param name="ws"></param>
		/// <returns></returns>
		IKeyboardDefinition DefaultForWritingSystem(IWritingSystemDefinition ws);
	}
}
