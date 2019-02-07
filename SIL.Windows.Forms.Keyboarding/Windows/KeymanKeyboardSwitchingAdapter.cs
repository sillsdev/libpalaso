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
			if (keyboard?.InputLanguage?.Culture == null)
			{
				return false;
			}
			foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
			{
				if (keyboard.InputLanguage.Culture.Name == lang.Culture.Name)
				{
					InputLanguage.CurrentInputLanguage = lang;
					return true;
				}
			}
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
				throw new NotImplementedException("Only keyboards of the type KeyboardAdaptorTypeSystem need to return a DefaultKeyboard. KeymanKeyboards are not of that type.");
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