// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// KeyboardSwitchingAdapter supporting Keyman 10
	/// </summary>
	internal class KeymanKeyboardSwitchingAdapter : IKeyboardSwitchingAdaptor
	{
		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
#if !MONO
			foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
			{
				if (keyboard.InputLanguage.Culture.EnglishName == lang.Culture.EnglishName)
				{
					InputLanguage.CurrentInputLanguage = lang;
					return true;
				}
			}
#endif
			return false;
		}

		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			// noop
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		public KeyboardDescription DefaultKeyboard
		{
			get
			{
				throw new NotImplementedException(
					"Keyman keyboards that are not associated with a language are never the system default.");
			}
		}

		/// <summary>
		/// Implementation is not required because this is not the primary (Type System) adapter.
		/// </summary>
		public KeyboardDescription ActiveKeyboard
		{
			get { throw new NotImplementedException(); }
		}
	}
}