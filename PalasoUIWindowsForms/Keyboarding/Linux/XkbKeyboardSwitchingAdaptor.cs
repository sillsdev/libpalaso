// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

#if __MonoCS__
using System;
using System.Diagnostics;
using System.Linq;
using X11.XKlavier;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling xkb keyboards on Linux
	/// </summary>
	public class XkbKeyboardSwitchingAdaptor: IKeyboardSwitchingAdaptor
	{
		private readonly IXklEngine m_engine;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="Palaso.UI.WindowsForms.Keyboarding.Linux.XkbKeyboardAdaptor"/> class.
		/// </summary>
		public XkbKeyboardSwitchingAdaptor(IXklEngine engine)
		{
			m_engine = engine;
		}

		internal IXklEngine XklEngine
		{
			get { return m_engine; }
		}

		protected virtual void SelectKeyboard(IKeyboardDefinition keyboard)
		{
			Debug.Assert(keyboard is KeyboardDescription);
			Debug.Assert(((KeyboardDescription)keyboard).Engine == this);
			Debug.Assert(keyboard is XkbKeyboardDescription);
			var xkbKeyboard = keyboard as XkbKeyboardDescription;
			if (xkbKeyboard == null)
				throw new ArgumentException();

			if (xkbKeyboard.GroupIndex >= 0)
			{
				m_engine.SetGroup(xkbKeyboard.GroupIndex);
			}
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			SelectKeyboard(keyboard);
			return true;
		}

		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
		}

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardType Type
		{
			get { return KeyboardType.System; }
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		/// <remarks>
		/// For Xkb the default keyboard has GroupIndex set to zero.
		/// Wasta/Cinnamon keyboarding doesn't use XkbKeyboardDescription objects.
		/// </remarks>
		public IKeyboardDefinition DefaultKeyboard
		{
			get
			{
				return Keyboard.Controller.AllAvailableKeyboards.Where(kbd => kbd.Type == KeyboardType.System)
					.FirstOrDefault (x => x is XkbKeyboardDescription && ((XkbKeyboardDescription)x).GroupIndex == 0);
			}
		}

		/// <summary>
		/// Implementation is not required because the default implementation of KeyboardController
		/// is sufficient.
		/// </summary>
		public IKeyboardDefinition ActiveKeyboard
		{
			get { return null; }
		}

	}
}
#endif
