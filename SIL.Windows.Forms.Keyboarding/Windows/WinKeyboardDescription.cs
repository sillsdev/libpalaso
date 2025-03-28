// Copyright (c) 2013-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using Keyman10Interop;
using System.Runtime.InteropServices;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Keyboard description for a Windows system keyboard
	/// </summary>
	/// <remarks>Holds information about a specific keyboard, especially for IMEs (e.g. whether
	/// English input mode is selected) in addition to the default keyboard description. This
	/// is necessary to restore the current setting when switching between fields with
	/// differing keyboards. The user expects that a keyboard keeps its state between fields.
	/// </remarks>
	/// ----------------------------------------------------------------------------------------
	internal class WinKeyboardDescription : KeyboardDescription
	{
		private string _localizedName;
		private readonly bool _useNfcContext;

		internal WinKeyboardDescription(string keyboardId, string localizedKeyboardName,
			string inputLanguageLayoutName, string cultureName, bool isAvailable,
			IInputLanguage inputLanguage, WinKeyboardAdaptor engine,
			TfInputProcessorProfile profile)
			: base(keyboardId, localizedKeyboardName, inputLanguageLayoutName, cultureName,
				isAvailable, engine.SwitchingAdaptor)
		{
			InputLanguage = inputLanguage;
			_localizedName = localizedKeyboardName;
			ConversionMode = (int)(Win32.IME_CMODE.NATIVE | Win32.IME_CMODE.SYMBOL);
			_useNfcContext = IsKeymanKeyboard(cultureName);
			InputProcessorProfile = profile;
		}

		private static bool IsKeymanKeyboard(string cultureName)
		{
			try
			{
				var kmn = new KeymanClass();
				if (kmn.Languages.Count > 0)
				{
					foreach (IKeymanKeyboard kb in kmn.Keyboards)
					{
						if (kb.DefaultWindowsLanguages != null && kb.DefaultWindowsLanguages.Contains(cultureName))
						{
							return true;
						}
					}
				}
			}
			catch (COMException)
			{
				// Not a keyman keyboard
			}
			return false;
		}

		/// <summary>
		/// Indicates whether we should pass NFC or NFD data to the keyboard. This implementation
		/// returns <c>false</c> for Keyman keyboards and <c>true</c> for other keyboards.
		/// </summary>
		public override bool UseNfcContext => _useNfcContext;

		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		public override string LocalizedName => _localizedName;

		internal int ConversionMode { get; set; }
		internal int SentenceMode { get; set; }
		internal IntPtr WindowHandle { get; set; }

		public TfInputProcessorProfile InputProcessorProfile { get; set; }

		internal void SetIsAvailable(bool isAvailable)
		{
			IsAvailable = isAvailable;
		}

		internal void SetLocalizedName(string localizedName)
		{
			_localizedName = localizedName;
		}

		public override string ToString()
		{
			return Id;
		}
	}
}
