// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Linq;
using X11.XKlavier;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling xkb keyboards on Linux
	/// </summary>
	public class XkbKeyboardSwitchingAdaptor : IKeyboardSwitchingAdaptor
	{
		private readonly IXklEngine _engine;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="SIL.Windows.Forms.Keyboarding.Linux.XkbKeyboardSwitchingAdaptor"/> class.
		/// </summary>
		public XkbKeyboardSwitchingAdaptor(IXklEngine engine)
		{
			_engine = engine;
		}

		internal IXklEngine XklEngine => _engine;

		protected virtual void SelectKeyboard(KeyboardDescription keyboard)
		{
			Debug.Assert(keyboard.Engine == this);
			Debug.Assert(keyboard is XkbKeyboardDescription);
			var xkbKeyboard = keyboard as XkbKeyboardDescription;
			if (xkbKeyboard == null)
				throw new ArgumentException();

			if (xkbKeyboard.GroupIndex >= 0)
			{
				_engine.SetGroup(xkbKeyboard.GroupIndex);
			}
		}

		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			SelectKeyboard(keyboard);
			return true;
		}

		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		/// <remarks>
		/// For Xkb the default keyboard has GroupIndex set to zero.
		/// Wasta/Cinnamon keyboarding doesn't use XkbKeyboardDescription objects.
		/// </remarks>
		public KeyboardDescription DefaultKeyboard
		{
			get
			{
				return KeyboardController.Instance.AvailableKeyboards.OfType<XkbKeyboardDescription>().FirstOrDefault(kbd => kbd.GroupIndex == 0);
			}
		}

		/// <summary>
		/// Implementation is not required because the default implementation of KeyboardController
		/// is sufficient.
		/// </summary>
		public KeyboardDescription ActiveKeyboard => null;
	}
}
