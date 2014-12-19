// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Unmanaged.TSF;

namespace SIL.WritingSystems.WindowsForms.Keyboarding.Windows
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
	[SuppressMessage("Gendarme.Rules.Design", "TypesWithNativeFieldsShouldBeDisposableRule",
		Justification = "WindowHandle is a reference to a control")]
	internal class WinKeyboardDescription : KeyboardDescription
	{
		private string _localizedName;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Windows.WinKeyboardDescription"/> class.
		/// </summary>
		internal WinKeyboardDescription(string id, string name, string layout, string locale, bool isAvailable,
			IInputLanguage inputLanguage, WinKeyboardAdaptor engine, string localizedName, TfInputProcessorProfile profile)
			: base(id, name, layout, locale, isAvailable, engine)
		{
			InputLanguage = inputLanguage;
			_localizedName = localizedName;
			InputProcessorProfile = profile;
			ConversionMode = (int) (Win32.IME_CMODE.NATIVE | Win32.IME_CMODE.SYMBOL);
		}

		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		public override string LocalizedName
		{
			get { return _localizedName; }
		}

		internal int ConversionMode { get; set; }
		internal int SentenceMode { get; set; }
		internal IntPtr WindowHandle { get; set; }

		internal TfInputProcessorProfile InputProcessorProfile { get; set; }

		internal void SetIsAvailable(bool isAvailable)
		{
			IsAvailable = isAvailable;
		}

		internal void SetLocalizedName(string localizedName)
		{
			_localizedName = localizedName;
		}
	}
}
#endif
