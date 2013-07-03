using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// This class acts as a central point for writing-system related keyboarding information.
	/// Currently its only function is to provide a home for the active IKeyboardController
	/// </summary>
	public class Keyboarding
	{
		private static IKeyboardController _controller;
		/// <summary>
		/// The active instance of keyboard controller which is used by various writing system methods.
		/// This may be set to a stub for testing, or by clients to a useful controller such as Palaso.UI.WindowsForms.KeyboardController.
		/// If not otherwise set, it returns a DefaultKeyboardController.
		/// </summary>
		public static IKeyboardController Controller
		{
			get
			{
				if (_controller == null)
					_controller = new DefaultKeyboardController();
				return _controller;
			}
			set { _controller = value; }
		}
	}
}
