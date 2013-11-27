// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Unmanaged.TSF;
using Microsoft.Win32;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Windows
{
	/// <summary>
	/// Class for handling Windows system keyboards
	/// </summary>
	[SuppressMessage("Gendarme.Rules.Design", "TypesWithDisposableFieldsShouldBeDisposableRule",
		Justification = "m_Timer gets disposed in Close() which gets called from KeyboardControllerImpl.Dispose")]
	internal class WinKeyboardAdaptor: IKeyboardAdaptor
	{
		internal class LayoutName
		{
			public LayoutName()
			{
				Name = string.Empty;
				LocalizedName = string.Empty;
			}

			public LayoutName(string layout): this(layout, layout)
			{
			}

			public LayoutName(string layout, string localizedLayout)
			{
				Name = layout;
				LocalizedName = localizedLayout;
			}

			public string Name;
			public string LocalizedName;
		}

		private List<IKeyboardErrorDescription> m_BadLocales;
		private Timer m_Timer;
		private WinKeyboardDescription m_ExpectedKeyboard;
		private bool m_fSwitchedLanguages;
		internal ITfInputProcessorProfiles ProcessorProfiles { get; private set; }
		internal ITfInputProcessorProfileMgr ProfileMgr { get; private set; }

		public WinKeyboardAdaptor()
		{
			try
			{
				ProcessorProfiles = new TfInputProcessorProfilesClass();
			}
			catch (InvalidCastException)
			{
				ProcessorProfiles = null;
				return;
			}

			// ProfileMgr will be null on Windows XP - the interface got introduced in Vista
			ProfileMgr = ProcessorProfiles as ITfInputProcessorProfileMgr;
		}

		protected short[] Languages
		{
			get
			{
				if (ProcessorProfiles == null)
					return new short[0];

				var ptr = IntPtr.Zero;
				try
				{
					var count = ProcessorProfiles.GetLanguageList(out ptr);
					if (count <= 0)
						return new short[0];

					var langIds = new short[count];
					Marshal.Copy(ptr, langIds, 0, count);
					return langIds;
				}
				catch (InvalidCastException)
				{
					// For strange reasons tests on TeamCity failed with InvalidCastException: Unable
					// to cast COM object of type TfInputProcessorProfilesClass to interface type
					// ITfInputProcessorProfiles when trying to call GetLanguageList. Don't know why
					// it wouldn't fail when we create the object. Since it's theoretically possible
					// that this also happens on a users machine we catch the exception here - maybe
					// TSF is not enabled?
					ProcessorProfiles = null;
					return new short[0];
				}
				finally
				{
					if (ptr != IntPtr.Zero)
						Marshal.FreeCoTaskMem(ptr);
				}
			}
		}

		private void GetInputMethodsThroughTsf(short[] languages)
		{
			foreach (var langId in languages)
			{
				var profilesEnumerator = ProfileMgr.EnumProfiles(langId);
				TfInputProcessorProfile profile;
				while (profilesEnumerator.Next(1, out profile) == 1)
				{
					// We only deal with keyboards; skip other input methods
					if (profile.CatId != Guids.TfcatTipKeyboard)
						continue;

					if (profile.ProfileType == TfProfileType.KeyboardLayout || (profile.Flags & TfIppFlags.Enabled) != 0)
					{
						KeyboardController.Manager.RegisterKeyboard(new WinKeyboardDescription(profile, this));
					}
				}
			}
		}

		private void GetInputMethodsThroughWinApi()
		{
			var countKeyboardLayouts = Win32.GetKeyboardLayoutList(0, IntPtr.Zero);
			if (countKeyboardLayouts == 0)
				return;

			var keyboardLayouts = Marshal.AllocCoTaskMem(countKeyboardLayouts * IntPtr.Size);
			try
			{
				Win32.GetKeyboardLayoutList(countKeyboardLayouts, keyboardLayouts);

				var current = keyboardLayouts;
				var elemSize = (ulong)IntPtr.Size;
				for (int i = 0; i < countKeyboardLayouts; i++)
				{
					var hkl = (IntPtr)Marshal.ReadInt32(current);
					KeyboardController.Manager.RegisterKeyboard(new WinKeyboardDescription(hkl, this));
					current = (IntPtr)((ulong)current + elemSize);
				}
			}
			finally
			{
				Marshal.FreeCoTaskMem(keyboardLayouts);
			}
		}

		private void GetInputMethods()
		{
			if (ProfileMgr != null)
				// Windows >= Vista
				GetInputMethodsThroughTsf(Languages);
			else
				// Windows XP
				GetInputMethodsThroughWinApi();
		}

		internal static LayoutName GetLayoutNameEx(IntPtr handle)
		{
			// InputLanguage.LayoutName is not to be trusted, especially where there are mutiple
			// layouts (input methods) associated with a language. This function also provides
			// the additional benefit that it does not matter whether a user switches from using
			// InKey in Portable mode to using it in Installed mode (perhaps as the project is
			// moved from one computer to another), as this function will identify the correct
			// input language regardless, rather than (unhelpfully ) calling an InKey layout in
			// portable mode the "US" layout. The layout is identified soley by the high-word of
			// the HKL (a.k.a. InputLanguage.Handle).  (The low word of the HKL identifies the
			// language.)
			// This function determines an HKL's LayoutName based on the following order of
			// precedence:
			// - Look up HKL in HKCU\\Software\\InKey\\SubstituteLayoutNames
			// - Look up extended layout in HKLM\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts
			// - Look up basic (non-extended) layout in HKLM\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts
			// -Scan for ID of extended layout in HKLM\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts
			var hkl = string.Format("{0:X8}", (int)handle);

			// Get substitute first
			var substituteHkl = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Keyboard Layout\Substitutes", hkl, null);
			if (!string.IsNullOrEmpty(substituteHkl))
				hkl = substituteHkl;

			// Check InKey
			var substituteLayoutName = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\InKey\SubstituteLayoutNames", hkl, null);
			if (!string.IsNullOrEmpty(substituteLayoutName))
				return new LayoutName(substituteLayoutName);

			var layoutName = GetLayoutNameFromKey(string.Concat(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\", hkl));
			if (layoutName != null)
				return layoutName;

			layoutName = GetLayoutNameFromKey(string.Concat(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\0000",
				hkl.Substring(0, 4)));
			if (layoutName != null)
				return layoutName;

			using (var regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Keyboard Layouts"))
			{
				if (regKey == null)
					return new LayoutName();

				string layoutId = "0" + hkl.Substring(1, 3);
				foreach (string subKeyName in regKey.GetSubKeyNames().Reverse())
					// Scan in reverse order for efficiency, as the extended layouts are at the end.
				{
					using (var klid = regKey.OpenSubKey(subKeyName))
					{
						if (klid == null)
							continue;

						var layoutIdSk = ((string) klid.GetValue("Layout ID"));
						if (layoutIdSk != null &&
							layoutIdSk.Equals(layoutId, StringComparison.InvariantCultureIgnoreCase))
						{
							return GetLayoutNameFromKey(klid.Name);
						}
					}
				}
			}

			return new LayoutName();
		}

		private static LayoutName GetLayoutNameFromKey(string key)
		{
			var layoutText = (string)Registry.GetValue(key, "Layout Text", null);
			var displayName = (string)Registry.GetValue(key, "Layout Display Name", null);
			if (string.IsNullOrEmpty(layoutText) && string.IsNullOrEmpty(displayName))
				return null;
			else if (string.IsNullOrEmpty(displayName))
				return new LayoutName(layoutText);
			else
			{
				var bldr = new StringBuilder(100);
				Win32.SHLoadIndirectString(displayName, bldr, 100, IntPtr.Zero);
				return string.IsNullOrEmpty(layoutText) ? new LayoutName(bldr.ToString()) :
					new LayoutName(layoutText, bldr.ToString());
			}
		}

		/// <summary>
		/// Gets the InputLanguage that has the same layout as <paramref name="keyboardDescription"/>.
		/// </summary>
		internal static InputLanguage GetInputLanguage(IKeyboardDefinition keyboardDescription)
		{
			InputLanguage sameLayout = null;
			InputLanguage sameCulture = null;
			foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
			{
				// TODO: write some tests
				try
				{
					if (GetLayoutNameEx(lang.Handle).Name == keyboardDescription.Name)
					{
						if (keyboardDescription.Locale == lang.Culture.Name)
							return lang;
						if (sameLayout == null)
							sameLayout = lang;
					}
					else if (keyboardDescription.Locale == lang.Culture.Name && sameCulture == null)
						sameCulture = lang;
				}
				catch (CultureNotFoundException)
				{
					// we get an exception for non-supported cultures, probably because of a
					// badly applied .NET patch.
					// http://www.ironspeed.com/Designer/3.2.4/WebHelp/Part_VI/Culture_ID__XXX__is_not_a_supported_culture.htm and others
				}
			}
			return sameLayout ?? sameCulture;
		}

		/// <summary>
		/// Gets the keyboard description for the layout of <paramref name="inputLanguage"/>.
		/// </summary>
		private static KeyboardDescription GetKeyboardDescription(IInputLanguage inputLanguage)
		{
			KeyboardDescription sameLayout = null;
			KeyboardDescription sameCulture = null;
			// TODO: write some tests
			var requestedLayout = GetLayoutNameEx(inputLanguage.Handle).Name;
			foreach (KeyboardDescription keyboardDescription in Keyboard.Controller.AllAvailableKeyboards)
			{
				try
				{
					if (requestedLayout == keyboardDescription.Layout)
					{
						if (keyboardDescription.Locale == inputLanguage.Culture.Name)
							return keyboardDescription;
						if (sameLayout == null)
							sameLayout = keyboardDescription;
					}
					else if (keyboardDescription.Locale == inputLanguage.Culture.Name && sameCulture == null)
						sameCulture = keyboardDescription;
				}
				catch (CultureNotFoundException)
				{
					// we get an exception for non-supported cultures, probably because of a
					// badly applied .NET patch.
					// http://www.ironspeed.com/Designer/3.2.4/WebHelp/Part_VI/Culture_ID__XXX__is_not_a_supported_culture.htm and others
				}
			}
			return sameLayout ?? sameCulture;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (m_ExpectedKeyboard == null || !m_fSwitchedLanguages)
				return;

			if (InputLanguage.CurrentInputLanguage.Culture.KeyboardLayoutId == m_ExpectedKeyboard.InputLanguage.Culture.KeyboardLayoutId)
			{
				m_ExpectedKeyboard = null;
				m_fSwitchedLanguages = false;
				m_Timer.Enabled = false;
				return;
			}

			SwitchKeyboard(m_ExpectedKeyboard);
		}

		private bool UseWindowsApiForKeyboardSwitching(WinKeyboardDescription winKeyboard)
		{
			return ProcessorProfiles == null ||
				(ProfileMgr == null && winKeyboard.InputProcessorProfile.Hkl == IntPtr.Zero);
		}

		private void SwitchKeyboard(WinKeyboardDescription winKeyboard)
		{
			((IKeyboardControllerImpl)Keyboard.Controller).ActiveKeyboard = ActivateKeyboard(winKeyboard);
			if (Form.ActiveForm != null)
			{
				// If we activate a keyboard while a particular Form is active, we want to know about
				// input language change calls for that form. The previous -= may help make sure we
				// don't get multiple hookups.
				Form.ActiveForm.InputLanguageChanged -= ActiveFormOnInputLanguageChanged;
				Form.ActiveForm.InputLanguageChanged += ActiveFormOnInputLanguageChanged;
			}

			// If we have a TIP (TSF Input Processor) we don't have a handle. But we do the
			// keyboard switching through TSF so we don't need the workaround below.
			if (!UseWindowsApiForKeyboardSwitching(winKeyboard))
				return;

			m_ExpectedKeyboard = winKeyboard;
			// The following lines help to work around a Windows bug (happens at least on
			// XP-SP3): When you set the current input language (by any method), if there is more
			// than one loaded input language associated with that same culture, Windows may
			// initially go along with your request, and even respond to an immediate query of
			// the current input language with the answer you expect.  However, within a fraction
			// of a second, it often takes the initiative to again change the input language to
			// the _other_ input language having that same culture. We check that the proper
			// input language gets set by enabling a timer so that we can re-set the input
			// language if necessary.
			m_fSwitchedLanguages = true;
			// stop timer first so that the 0.5s interval restarts.
			m_Timer.Stop();
			m_Timer.Start();
		}

		private WinKeyboardDescription ActivateKeyboard(WinKeyboardDescription winKeyboard)
		{
			try
			{
				if (UseWindowsApiForKeyboardSwitching(winKeyboard))
				{
					// Win XP with regular keyboard, or TSF disabled
					Win32.ActivateKeyboardLayout(new HandleRef(this, winKeyboard.InputLanguage.Handle), 0);
					return winKeyboard;
				}

				var profile = winKeyboard.InputProcessorProfile;
				ProcessorProfiles.ChangeCurrentLanguage(profile.LangId);
				if (ProfileMgr == null)
				{
					// Win XP with TIP (TSF Input Processor)
					ProcessorProfiles.ActivateLanguageProfile(ref profile.ClsId, profile.LangId,
						ref profile.GuidProfile);
				}
				else
				{
					// Windows >= Vista with either TIP or regular keyboard
					ProfileMgr.ActivateProfile(profile.ProfileType, profile.LangId,
						ref profile.ClsId, ref profile.GuidProfile, profile.Hkl,
						TfIppMf.DontCareCurrentInputLanguage);
				}
			}
			catch (ArgumentException)
			{
				// throws exception for non-supported culture, though seems to set it OK.
			}
			return winKeyboard;
		}

		/// <summary>
		/// Save the state of the conversion and sentence mode for the current IME
		/// so that we can restore it later.
		/// </summary>
		private void SaveImeConversionStatus(WinKeyboardDescription winKeyboard)
		{
			if (winKeyboard == null)
				return;

			var windowHandle = new HandleRef(this,
				winKeyboard.WindowHandle != IntPtr.Zero ? winKeyboard.WindowHandle : Win32.GetFocus());
			var contextPtr = Win32.ImmGetContext(windowHandle);
			if (contextPtr == IntPtr.Zero)
				return;

			var contextHandle = new HandleRef(this, contextPtr);
			int conversionMode;
			int sentenceMode;
			Win32.ImmGetConversionStatus(contextHandle, out conversionMode, out sentenceMode);
			winKeyboard.ConversionMode = conversionMode;
			winKeyboard.SentenceMode = sentenceMode;
			Win32.ImmReleaseContext(windowHandle, contextHandle);
		}

		/// <summary>
		/// Restore the conversion and sentence mode to the states they had last time
		/// we activated this keyboard (unless we never activated this keyboard since the app
		/// got started, in which case we use sensible default values).
		/// </summary>
		private void RestoreImeConversionStatus(KeyboardDescription keyboard)
		{
			var winKeyboard = keyboard as WinKeyboardDescription;
			if (winKeyboard == null)
				return;

			// Restore the state of the new keyboard to the previous value. If we don't do
			// that e.g. in Chinese IME the input mode will toggle between English and
			// Chinese (LT-7487 et al).
			var windowPtr = winKeyboard.WindowHandle != IntPtr.Zero ? winKeyboard.WindowHandle : Win32.GetFocus();
			var windowHandle = new HandleRef(this, windowPtr);

			// NOTE: Windows uses the same context for all windows of the current thread, so it
			// doesn't really matter which window handle we pass.
			var contextPtr = Win32.ImmGetContext(windowHandle);
			if (contextPtr == IntPtr.Zero)
				return;

			// NOTE: Chinese Pinyin IME allows to switch between Chinese and Western punctuation.
			// This can be selected in both Native and Alphanumeric conversion mode. However,
			// when setting the value the punctuation setting doesn't get restored in Alphanumeric
			// conversion mode, not matter what I try. I guess that is because Chinese punctuation
			// doesn't really make sense with Latin characters.
			var contextHandle = new HandleRef(this, contextPtr);
			Win32.ImmSetConversionStatus(contextHandle, winKeyboard.ConversionMode, winKeyboard.SentenceMode);
			Win32.ImmReleaseContext(windowHandle, contextHandle);
			winKeyboard.WindowHandle = windowPtr;
		}

		private void ActiveFormOnInputLanguageChanged(object sender, InputLanguageChangedEventArgs inputLanguageChangedEventArgs)
		{
			RestoreImeConversionStatus(GetKeyboardDescription(inputLanguageChangedEventArgs.InputLanguage.Interface()));
		}

		#region IKeyboardAdaptor Members

		[SuppressMessage("Gendarme.Rules.Correctness", "EnsureLocalDisposalRule",
			Justification = "m_Timer gets disposed in Close() which gets called from KeyboardControllerImpl.Dispose")]
		public void Initialize()
		{
			m_Timer = new Timer { Interval = 500 };
			m_Timer.Tick += OnTimerTick;

			GetInputMethods();

			// Form.ActiveForm can be null when running unit tests
			if (Form.ActiveForm != null)
				Form.ActiveForm.InputLanguageChanged += ActiveFormOnInputLanguageChanged;
		}

		public void UpdateAvailableKeyboards()
		{
			GetInputMethods();
		}

		public void Close()
		{
			if (m_Timer != null)
			{
				m_Timer.Dispose();
				m_Timer = null;
			}
		}

		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get { return m_BadLocales; }
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			SwitchKeyboard(keyboard as WinKeyboardDescription);
			return true;
		}

		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			var winKeyboard = keyboard as WinKeyboardDescription;
			Debug.Assert(winKeyboard != null);

			SaveImeConversionStatus(winKeyboard);
		}

		public void ActivateDefaultKeyboard()
		{
			var keyboard = DefaultKeyboard;
			if (keyboard == null)
				return;

			keyboard.Activate();
		}

		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			return GetKeyboardDescription(inputLanguage);
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			return new WinKeyboardDescription(locale, layout, this) {IsAvailable = false};
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		public IKeyboardDefinition DefaultKeyboard
		{
			get { return GetKeyboardDescription(InputLanguage.DefaultInputLanguage.Interface()); }
		}

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardType Type
		{
			get { return KeyboardType.System; }
		}
		#endregion
	}
}
#endif
