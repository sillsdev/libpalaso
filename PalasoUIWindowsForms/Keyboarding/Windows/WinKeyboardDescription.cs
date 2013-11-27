// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Unmanaged.TSF;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Windows
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
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Windows.WinKeyboardDescription"/> class.
		/// </summary>
		public WinKeyboardDescription(TfInputProcessorProfile profile, IKeyboardAdaptor engine)
			: this(profile, profile.LangId, profile.Hkl, engine)
		{
		}

		public WinKeyboardDescription(IntPtr hkl, IKeyboardAdaptor engine)
			: this(new TfInputProcessorProfile(), HklToLangId(hkl), hkl, engine)
		{
		}

		private WinKeyboardDescription(TfInputProcessorProfile profile, ushort langId, IntPtr hkl, IKeyboardAdaptor engine)
			: base(engine, KeyboardType.System)
		{
			var winEngine = engine as WinKeyboardAdaptor;
			Debug.Assert(winEngine != null);

			InputProcessorProfile = profile;

			var culture = new CultureInfo(langId);
			string locale;
			string cultureName;
			try
			{
				cultureName = culture.DisplayName;
				locale = culture.Name;
			}
			catch (CultureNotFoundException)
			{
				// we get an exception for non-supported cultures, probably because of a
				// badly applied .NET patch.
				// http://www.ironspeed.com/Designer/3.2.4/WebHelp/Part_VI/Culture_ID__XXX__is_not_a_supported_culture.htm and others
				cultureName = "[Unknown Language]";
				locale = "en-US";
			}
			WinKeyboardAdaptor.LayoutName layoutName;
			if (profile.Hkl == IntPtr.Zero && profile.ProfileType != TfProfileType.Illegal)
			{
				layoutName = new WinKeyboardAdaptor.LayoutName(winEngine.ProcessorProfiles.GetLanguageProfileDescription(
					ref profile.ClsId, profile.LangId, ref profile.GuidProfile));
			}
			else
				layoutName = WinKeyboardAdaptor.GetLayoutNameEx(hkl);

			Initialize(cultureName, layoutName, locale,
				new InputLanguageWrapper(culture, hkl, layoutName.Name));
		}

		public WinKeyboardDescription(string locale, string layout, IKeyboardAdaptor engine)
			: base(engine, KeyboardType.System)
		{
			Initialize(locale, new WinKeyboardAdaptor.LayoutName(layout), locale,
				new InputLanguageWrapper(new CultureInfo(locale), IntPtr.Zero, layout));
		}

		internal WinKeyboardDescription(WinKeyboardDescription other): base(other)
		{
			ConversionMode = other.ConversionMode;
			SentenceMode = other.SentenceMode;
			WindowHandle = other.WindowHandle;
			InternalLocalizedName = other.InternalLocalizedName;
		}

		public override IKeyboardDefinition Clone()
		{
			return new WinKeyboardDescription(this);
		}

		private static ushort HklToLangId(IntPtr hkl)
		{
			return (ushort)((uint)hkl & 0xffff);
		}

		private void Initialize(string displayName, WinKeyboardAdaptor.LayoutName layoutName, string locale, IInputLanguage inputLanguage)
		{
			ConversionMode = (int)(Win32.IME_CMODE.NATIVE | Win32.IME_CMODE.SYMBOL);
			InternalName = GetDisplayName(displayName, layoutName.Name);
			InternalLocalizedName = GetDisplayName(displayName, layoutName.LocalizedName);
			Layout = layoutName.Name;
			Locale = locale;
			SetInputLanguage(inputLanguage);
		}

		#region Overrides of DefaultKeyboardDefinition
		/// <summary>
		/// Gets a localized human-readable name of the input language.
		/// </summary>
		public override string LocalizedName
		{
			get { return InternalLocalizedName; }
		}
		#endregion

		private string InternalLocalizedName { get; set; }
		public int ConversionMode { get; set; }
		public int SentenceMode { get; set; }
		public IntPtr WindowHandle { get; set; }
		public TfInputProcessorProfile InputProcessorProfile { get; private set; }


	}
}
#endif
