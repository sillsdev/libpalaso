// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Unmanaged.TSF;
using SIL.WritingSystems.WindowsForms.Keyboarding.Types;

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
		private readonly string _internalLocalizedName;
		private readonly TfInputProcessorProfile _profile;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Windows.WinKeyboardDescription"/> class.
		/// </summary>
		internal WinKeyboardDescription(TfInputProcessorProfile profile, string displayName, WinKeyboardAdaptor.LayoutName layoutName, string locale,
			IInputLanguage inputLanguage, WinKeyboardAdaptor engine, bool isAvailable)
			: base(GetDisplayName(displayName, layoutName.Name), layoutName.Name, locale, inputLanguage, engine, KeyboardType.System, isAvailable)
		{
			_profile = profile;
			ConversionMode = (int)(Win32.IME_CMODE.NATIVE | Win32.IME_CMODE.SYMBOL);
			_internalLocalizedName = GetDisplayName(displayName, layoutName.LocalizedName);
		}

		#region Overrides of DefaultKeyboardDefinition
		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		public override string LocalizedName
		{
			get { return _internalLocalizedName; }
		}
		#endregion

		internal int ConversionMode { get; set; }
		internal int SentenceMode { get; set; }
		internal IntPtr WindowHandle { get; set; }

		public TfInputProcessorProfile InputProcessorProfile
		{
			get { return _profile; }
		}
	}
}
#endif
