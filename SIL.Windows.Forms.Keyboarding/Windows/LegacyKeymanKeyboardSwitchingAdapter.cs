// Copyright (c) 2013-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using Keyman7Interop;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// KeyboardSwitching adapter for Keyman versions 6-9
	/// </summary>
	internal class LegacyKeymanKeyboardSwitchingAdapter : IKeyboardSwitchingAdaptor
	{
		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			var keymanKbdDesc = (KeymanKeyboardDescription)keyboard;
			if (keymanKbdDesc.IsKeyman6)
			{
				try
				{
					var keymanLink = new KeymanLink.KeymanLink();
					if (!keymanLink.Initialize())
					{
						ErrorReport.NotifyUserOfProblem("Keyman6 could not be activated.");
						return false;
					}
					keymanLink.SelectKeymanKeyboard(keyboard.Id);
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
					var keyman = new TavultesoftKeymanClass();
					int oneBasedIndex = keyman.Keyboards.IndexOf(keyboard.Id);

					if (oneBasedIndex < 1)
					{
						ErrorReport.NotifyUserOfProblem("The keyboard '{0}' could not be activated using Keyman 7.",
							keyboard.Id);
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

			KeyboardController.Instance.ActiveKeyboard = keyboard;
			return true;
		}

		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			try
			{
				if (((KeymanKeyboardDescription)keyboard).IsKeyman6)
				{
					var keymanLink = new KeymanLink.KeymanLink();
					if (keymanLink.Initialize())
						keymanLink.SelectKeymanKeyboard(null, false);
				}
				else
				{
					var keyman = new TavultesoftKeymanClass();
					keyman.Control.ActiveKeyboard = null;
				}
			}
			catch (Exception)
			{
				// Keyman not installed?
			}
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