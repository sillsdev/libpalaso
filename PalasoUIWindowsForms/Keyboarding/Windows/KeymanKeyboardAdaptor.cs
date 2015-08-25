// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;
using Keyman7Interop;

namespace Palaso.UI.WindowsForms.Keyboarding.Windows
{
	/// <summary>
	/// Class for handling Keyman keyboards not associated with a Windows language
	/// </summary>
	internal class KeymanKeyboardAdaptor: IKeyboardRetrievingAdaptor, IKeyboardSwitchingAdaptor
	{
		#region IKeyboardAdaptor Members

		public bool IsApplicable
		{
			get
			{
				// Try the Keyman 7/8 interface
				try
				{
					var keyman = new TavultesoftKeymanClass();
					if (keyman.Keyboards != null && keyman.Keyboards.Count > 0)
					{
						return true;
					}
				}
				catch (Exception)
				{
					// Keyman 7 isn't installed or whatever.
				}

				// Try the Keyman 6 interface
				try
				{
					var keymanLink = new KeymanLink.KeymanLink();
					if (keymanLink.Initialize())
					{
						if (keymanLink.Keyboards != null && keymanLink.Keyboards.Length > 0)
						{
							return true;
						}
					}
				}
				catch (Exception)
				{
					// Keyman 6 isn't installed or whatever.
				}
				return false;
			}
		}

		public IKeyboardSwitchingAdaptor Adaptor
		{
			get { return this; }
		}

		public void Initialize()
		{
			// nothing to do
		}

		public void RegisterAvailableKeyboards()
		{
			UpdateAvailableKeyboards();
		}

		public void UpdateAvailableKeyboards()
		{
			// Try the Keyman 7/8 interface
			try
			{
				var keyman = new TavultesoftKeymanClass();
				foreach (IKeymanKeyboard keyboard in keyman.Keyboards)
					KeyboardController.Manager.RegisterKeyboard(new KeymanKeyboardDescription(keyboard.Name, false, this));
			}
			catch (Exception)
			{				
				// Keyman 7 isn't installed or whatever.
			}

			// Try the Keyman 6 interface
			try
			{
				var keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize())
				{
					foreach (var keyboard in keymanLink.Keyboards)
						KeyboardController.Manager.RegisterKeyboard(new KeymanKeyboardDescription(keyboard.KbdName, true, this));
				}
			}
			catch (Exception)
			{
				// Keyman 6 isn't installed or whatever.
			}
		}

		public void Close()
		{
		}

		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get { return null; }
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			var keymanKbdDesc = (KeymanKeyboardDescription)keyboard;
			if (keymanKbdDesc.IsKeyman6)
			{
				try
				{
					KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
					if (!keymanLink.Initialize())
					{
						Palaso.Reporting.ErrorReport.NotifyUserOfProblem("Keyman6 could not be activated.");
						return false;
					}
					keymanLink.SelectKeymanKeyboard(keyboard.Layout);
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
			{
				try
				{
					TavultesoftKeymanClass keyman = new TavultesoftKeymanClass();
					int oneBasedIndex = keyman.Keyboards.IndexOf(keyboard.Layout);

					if (oneBasedIndex < 1)
					{
						Palaso.Reporting.ErrorReport.NotifyUserOfProblem("The keyboard '{0}' could not be activated using Keyman 7.",
							keyboard.Layout);
						return false;
					}
					keyman.Control.ActiveKeyboard = keyman.Keyboards[oneBasedIndex];
				}
				catch (Exception)
				{
					// Keyman 7 not installed?
					return false;
				}
			}

			((IKeyboardControllerImpl)Keyboard.Controller).ActiveKeyboard = keyboard;
			return true;
		}

		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			try
			{
				if (((KeymanKeyboardDescription) keyboard).IsKeyman6)
				{
					KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
					if (keymanLink.Initialize())
						keymanLink.SelectKeymanKeyboard(null, false);
				}
				else
				{
					TavultesoftKeymanClass keyman = new TavultesoftKeymanClass();
					keyman.Control.ActiveKeyboard = null;
				}
			}
			catch (Exception)
			{
				// Keyman not installed?
			}
		}

		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException("Keyman keyboards that are not associated with a language cannot be looked up by language.");
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			return new KeymanKeyboardDescription(layout, false, this) {IsAvailable = false};
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		public IKeyboardDefinition DefaultKeyboard
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
		public IKeyboardDefinition ActiveKeyboard
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardType Type
		{
			get { return KeyboardType.OtherIm; }
		}
		#endregion
	}
}
#endif
