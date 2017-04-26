// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// Class for handling Windows system keyboards
	/// </summary>
	internal class WinKeyboardAdaptor : IKeyboardRetrievingAdaptor, IKeyboardSwitchingAdaptor
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

		/// <summary>
		/// This class receives notifications from TSF when the input method changes.
		/// It also implements a fallback to Windows messages if TSF isn't available,
		/// e.g. on Windows XP.
		/// </summary>
		private class TfLanguageProfileNotifySink : ITfLanguageProfileNotifySink
		{
			private readonly WinKeyboardAdaptor _keyboardAdaptor;
			private readonly List<Form> _toplevelForms = new List<Form>();

			public TfLanguageProfileNotifySink(WinKeyboardAdaptor keyboardAdaptor)
			{
				_keyboardAdaptor = keyboardAdaptor;
			}

			#region ITfLanguageProfileNotifySink Members

			public bool OnLanguageChange(ushort langid)
			{
				// In my tests we never hit this method (Windows 8.1). I don't know if the
				// method signature is wrong or why that is.

				// Return true to allow the language profile change
				return true;
			}

			public void OnLanguageChanged()
			{
				IKeyboardDefinition winKeyboard = _keyboardAdaptor.ActiveKeyboard;
				Debug.WriteLine("Language changed from {0} to {1}",
					Keyboard.Controller.ActiveKeyboard != null ? Keyboard.Controller.ActiveKeyboard.Layout : "<null>",
					winKeyboard != null ? winKeyboard.Layout : "<null>");

				_keyboardAdaptor._windowsLanguageProfileSinks.ForEach(
					sink => sink.OnInputLanguageChanged(Keyboard.Controller.ActiveKeyboard, winKeyboard));
			}
			#endregion

			#region Fallback if TSF isn't available

			// The WinKeyboardAdaptor will subscribe to the Form's InputLanguageChanged event
			// only if TSF is not available. Otherwise this code won't be executed.

			private void OnWindowsMessageInputLanguageChanged(object sender,
				InputLanguageChangedEventArgs inputLanguageChangedEventArgs)
			{
				Debug.Assert(_keyboardAdaptor._profileNotifySinkCookie == 0);

				KeyboardDescription winKeyboard = _keyboardAdaptor.GetKeyboardForInputLanguage(
					inputLanguageChangedEventArgs.InputLanguage.Interface());

				_keyboardAdaptor._windowsLanguageProfileSinks.ForEach(
					sink => sink.OnInputLanguageChanged(Keyboard.Controller.ActiveKeyboard, winKeyboard));
			}

			public void RegisterWindowsMessageHandler(Control control)
			{
				Debug.Assert(_keyboardAdaptor._profileNotifySinkCookie == 0);

				var topForm = control.FindForm();
				if (topForm == null || _toplevelForms.Contains(topForm))
					return;

				_toplevelForms.Add(topForm);
				topForm.InputLanguageChanged += OnWindowsMessageInputLanguageChanged;
			}

			public void UnregisterWindowsMessageHandler(Control control)
			{
				var topForm = control.FindForm();
				if (topForm == null || !_toplevelForms.Contains(topForm))
					return;

				topForm.InputLanguageChanged -= OnWindowsMessageInputLanguageChanged;
				_toplevelForms.Remove(topForm);
			}
			#endregion

		}

		private Timer _timer;
		private WinKeyboardDescription _expectedKeyboard;
		private bool _fSwitchedLanguages;
		/// <summary>Used to prevent re-entrancy. <c>true</c> while we're in the middle of switching keyboards.</summary>
		private bool _fSwitchingKeyboards;

		private ushort _profileNotifySinkCookie;
		private readonly TfLanguageProfileNotifySink _tfLanguageProfileNotifySink;

		private readonly List<IWindowsLanguageProfileSink> _windowsLanguageProfileSinks = new List<IWindowsLanguageProfileSink>();

		internal ITfInputProcessorProfiles ProcessorProfiles { get; private set; }
		internal ITfInputProcessorProfileMgr ProfileMgr { get; private set; }
		internal ITfSource TfSource { get; private set; }

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

			_tfLanguageProfileNotifySink = new TfLanguageProfileNotifySink(this);

			TfSource = ProcessorProfiles as ITfSource;
			if (TfSource != null)
			{
				_profileNotifySinkCookie = TfSource.AdviseSink(Guids.ITfLanguageProfileNotifySink,
					_tfLanguageProfileNotifySink);
			}

			if (KeyboardController.Instance != null)
			{
				KeyboardController.Instance.ControlAdded += OnControlRegistered;
				KeyboardController.Instance.ControlRemoving += OnControlRemoving;
			}
		}

		private void OnControlRegistered(object sender, RegisterEventArgs e)
		{
			var windowsLanguageProfileSink = e.EventHandler as IWindowsLanguageProfileSink;
			if (windowsLanguageProfileSink != null && !_windowsLanguageProfileSinks.Contains(windowsLanguageProfileSink))
				_windowsLanguageProfileSinks.Add(windowsLanguageProfileSink);

			if (_profileNotifySinkCookie != 0)
				return;

			// TSF disabled, so we have to fall back to Windows messages
			_tfLanguageProfileNotifySink.RegisterWindowsMessageHandler(e.Control);
		}

		private void OnControlRemoving(object sender, ControlEventArgs e)
		{
			if (_profileNotifySinkCookie != 0)
				return;

			// TSF disabled, so we have to fall back to Windows messages
			_tfLanguageProfileNotifySink.UnregisterWindowsMessageHandler(e.Control);
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

		private IEnumerable<Tuple<TfInputProcessorProfile, ushort, IntPtr>> GetInputMethodsThroughTsf()
		{
			foreach (short langId in Languages)
			{
				IEnumTfInputProcessorProfiles profilesEnumerator = ProfileMgr.EnumProfiles(langId);
				TfInputProcessorProfile profile;
				while (profilesEnumerator.Next(1, out profile) == 1)
				{
					// We only deal with keyboards; skip other input methods
					if (profile.CatId != Guids.TfcatTipKeyboard)
						continue;

					if ((profile.Flags & TfIppFlags.Enabled) == 0)
						continue;

					yield return Tuple.Create(profile, profile.LangId, profile.Hkl);
				}
			}
		}

		private IEnumerable<Tuple<TfInputProcessorProfile, ushort, IntPtr>> GetInputMethodsThroughWinApi()
		{
			int countKeyboardLayouts = Win32.GetKeyboardLayoutList(0, IntPtr.Zero);
			if (countKeyboardLayouts == 0)
				yield break;

			IntPtr keyboardLayouts = Marshal.AllocCoTaskMem(countKeyboardLayouts * IntPtr.Size);
			try
			{
				Win32.GetKeyboardLayoutList(countKeyboardLayouts, keyboardLayouts);

				IntPtr current = keyboardLayouts;
				var elemSize = (ulong) IntPtr.Size;
				for (int i = 0; i < countKeyboardLayouts; i++)
				{
					var hkl = (IntPtr) Marshal.ReadInt32(current);
					yield return Tuple.Create(new TfInputProcessorProfile(), HklToLangId(hkl), hkl);
					current = (IntPtr) ((ulong)current + elemSize);
				}
			}
			finally
			{
				Marshal.FreeCoTaskMem(keyboardLayouts);
			}
		}

		private static ushort HklToLangId(IntPtr hkl)
		{
			return (ushort)((uint)hkl & 0xffff);
		}

		private static string GetId(string layout, string locale)
		{
			return String.Format("{0}_{1}", locale, layout);
		}

		private static string GetDisplayName(string layout, string locale)
		{
			return string.Format("{0} - {1}", layout, locale);
		}

		private void GetInputMethods()
		{
			IEnumerable<Tuple<TfInputProcessorProfile, ushort, IntPtr>> imes;
			if (ProfileMgr != null)
				// Windows >= Vista
				imes = GetInputMethodsThroughTsf();
			else
				// Windows XP
				imes = GetInputMethodsThroughWinApi();

			var allKeyboards = KeyboardController.Instance.Keyboards;
			Dictionary<string, WinKeyboardDescription> curKeyboards = allKeyboards.OfType<WinKeyboardDescription>().ToDictionary(kd => kd.Id);
			foreach (Tuple<TfInputProcessorProfile, ushort, IntPtr> ime in imes)
			{
				TfInputProcessorProfile profile = ime.Item1;
				ushort langId = ime.Item2;
				IntPtr hkl = ime.Item3;

				CultureInfo culture;
				string locale;
				string cultureName;
				try
				{
					culture = new CultureInfo(langId);
					cultureName = culture.DisplayName;
					locale = culture.Name;
				}
				catch (CultureNotFoundException)
				{
					// This can happen for old versions of Keyman that created a custom culture that is invalid to .Net.
					// Also see http://stackoverflow.com/a/24820530/4953232
					culture = new CultureInfo("en-US");
					cultureName = "[Unknown Language]";
					locale = "en-US";
				}

				try
				{
					LayoutName layoutName;
					if (profile.Hkl == IntPtr.Zero && profile.ProfileType != TfProfileType.Illegal)
					{
						layoutName = new LayoutName(ProcessorProfiles.GetLanguageProfileDescription(
							ref profile.ClsId, profile.LangId, ref profile.GuidProfile));
					}
					else
					{
						layoutName = GetLayoutNameEx(hkl);
					}

					string id = GetId(layoutName.Name, locale);
					WinKeyboardDescription existingKeyboard;
					if (curKeyboards.TryGetValue(id, out existingKeyboard))
					{
						if (!existingKeyboard.IsAvailable)
						{
							existingKeyboard.SetIsAvailable(true);
							existingKeyboard.InputProcessorProfile = profile;
							existingKeyboard.SetLocalizedName(GetDisplayName(layoutName.LocalizedName, cultureName));
						}
						curKeyboards.Remove(id);
					}
					else
					{
						// Prevent a keyboard with this id from being registered again.
						// Potentially, id's are duplicated. e.g. A Keyman keyboard linked to a windows one.
						// For now we simply ignore this second registration.
						// A future enhancement would be to include knowledge of the driver in the Keyboard definition so
						// we could choose the best one to register.
						KeyboardDescription keyboard;
						if (!allKeyboards.TryGet(id, out keyboard))
						{
							KeyboardController.Instance.Keyboards.Add(
								new WinKeyboardDescription(id, GetDisplayName(layoutName.Name, cultureName),
									layoutName.Name, locale, true, new InputLanguageWrapper(culture, hkl, layoutName.Name), this,
									GetDisplayName(layoutName.LocalizedName, cultureName), profile));
						}
					}
				}
				catch (COMException)
				{
					// this can happen when the user changes the language associated with a
					// Keyman keyboard (LT-16172)
				}
			}

			foreach (WinKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);

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
			var hkl = string.Format("{0:X8}", (long)handle);

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

			if (string.IsNullOrEmpty(displayName))
				return new LayoutName(layoutText);

			var bldr = new StringBuilder(100);
			Win32.SHLoadIndirectString(displayName, bldr, 100, IntPtr.Zero);
			return string.IsNullOrEmpty(layoutText) ? new LayoutName(bldr.ToString()) :
				new LayoutName(layoutText, bldr.ToString());
		}

		/// <summary>
		/// Gets the InputLanguage that has the same layout as <paramref name="keyboardDescription"/>.
		/// </summary>
		internal static InputLanguage GetInputLanguage(WinKeyboardDescription keyboardDescription)
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
			KeyboardDescription sameLayout = KeyboardController.NullKeyboard;
			KeyboardDescription sameCulture = KeyboardController.NullKeyboard;
			// TODO: write some tests
			string requestedLayout = GetLayoutNameEx(inputLanguage.Handle).Name;
			foreach (WinKeyboardDescription keyboardDescription in KeyboardController.Instance.AvailableKeyboards.OfType<WinKeyboardDescription>())
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
			if (_expectedKeyboard == null || !_fSwitchedLanguages)
				return;

			// This code gets only called if TSF is not available(e.g. Windows XP)
			if (InputLanguage.CurrentInputLanguage.Culture.KeyboardLayoutId == _expectedKeyboard.InputLanguage.Culture.KeyboardLayoutId)
			{
				_expectedKeyboard = null;
				_fSwitchedLanguages = false;
				_timer.Enabled = false;
				return;
			}

			SwitchKeyboard(_expectedKeyboard);
		}

		private bool UseWindowsApiForKeyboardSwitching(WinKeyboardDescription winKeyboard)
		{
			return ProcessorProfiles == null ||
				(ProfileMgr == null && winKeyboard.InputProcessorProfile.Hkl == IntPtr.Zero);
		}

		private void SwitchKeyboard(WinKeyboardDescription winKeyboard)
		{
			if (_fSwitchingKeyboards)
				return;

			_fSwitchingKeyboards = true;
			try
			{
				KeyboardController.Instance.ActiveKeyboard = ActivateKeyboard(winKeyboard);
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

				_expectedKeyboard = winKeyboard;
				// The following lines help to work around a Windows bug (happens at least on
				// XP-SP3): When you set the current input language (by any method), if there is more
				// than one loaded input language associated with that same culture, Windows may
				// initially go along with your request, and even respond to an immediate query of
				// the current input language with the answer you expect.  However, within a fraction
				// of a second, it often takes the initiative to again change the input language to
				// the _other_ input language having that same culture. We check that the proper
				// input language gets set by enabling a timer so that we can re-set the input
				// language if necessary.
				_fSwitchedLanguages = true;
				// stop timer first so that the 0.5s interval restarts.
				_timer.Stop();
				_timer.Start();
			}
			finally
			{
				_fSwitchingKeyboards = false;
			}
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
				if ((profile.Flags & TfIppFlags.Enabled) == 0)
					return winKeyboard;

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
			catch (COMException e)
			{
				var profile = winKeyboard.InputProcessorProfile;
				var msg = string.Format("Got COM exception trying to activate IM:" + Environment.NewLine +
					"LangId={0}, clsId={1}, hkl={2}, guidProfile={3}, flags={4}, type={5}, catId={6}",
					profile.LangId, profile.ClsId, profile.Hkl, profile.GuidProfile, profile.Flags, profile.ProfileType, profile.CatId);
				throw new ApplicationException(msg, e);
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
		private void RestoreImeConversionStatus(IKeyboardDefinition keyboard)
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

		private static bool InputProcessorProfilesEqual(TfInputProcessorProfile profile1, TfInputProcessorProfile profile2)
		{
			// Don't compare Flags - they can be different and it's still the same profile
			return profile1.ProfileType == profile2.ProfileType &&
				profile1.LangId == profile2.LangId &&
				profile1.ClsId == profile2.ClsId &&
				profile1.GuidProfile == profile2.GuidProfile &&
				profile1.CatId == profile2.CatId &&
				profile1.HklSubstitute == profile2.HklSubstitute &&
				profile1.Caps == profile2.Caps &&
				profile1.Hkl == profile2.Hkl;
		}

		#region IKeyboardRetrievingAdaptor Members

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardAdaptorType Type
		{
			get
			{
				CheckDisposed();
				return KeyboardAdaptorType.System;
			}
		}

		public bool IsApplicable
		{
			get { return true; }
		}

		public IKeyboardSwitchingAdaptor SwitchingAdaptor
		{
			get { return this; }
		}

		[SuppressMessage("Gendarme.Rules.Correctness", "EnsureLocalDisposalRule",
			Justification = "m_Timer gets disposed in Close() which gets called from KeyboardControllerImpl.Dispose")]
		public void Initialize()
		{
			_timer = new Timer { Interval = 500 };
			_timer.Tick += OnTimerTick;

			GetInputMethods();

			// Form.ActiveForm can be null when running unit tests
			if (Form.ActiveForm != null)
				Form.ActiveForm.InputLanguageChanged += ActiveFormOnInputLanguageChanged;
		}

		public void UpdateAvailableKeyboards()
		{
			GetInputMethods();
		}

		private KeyboardDescription GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			return GetKeyboardDescription(inputLanguage);
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the ID.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public KeyboardDescription CreateKeyboardDefinition(string id)
		{
			CheckDisposed();

			string[] parts = id.Split('_');
			string locale = parts[0];
			string layout = parts[1];

			IInputLanguage inputLanguage;
			string cultureName;
			try
			{
				var ci = new CultureInfo(locale);
				cultureName = ci.DisplayName;
				inputLanguage = new InputLanguageWrapper(ci, IntPtr.Zero, layout);
			}
			catch (CultureNotFoundException)
			{
				// ignore if we can't find a culture (this can happen e.g. when a language gets
				// removed that was previously assigned to a WS) - see LT-15333
				inputLanguage = null;
				cultureName = "[Unknown Language]";
			}

			return new WinKeyboardDescription(id, GetDisplayName(layout, cultureName), layout, locale, false, inputLanguage, this,
				GetDisplayName(layout, cultureName), new TfInputProcessorProfile());
		}

		public bool CanHandleFormat(KeyboardFormat format)
		{
			CheckDisposed();
			switch (format)
			{
				case KeyboardFormat.Msklc:
				case KeyboardFormat.Unknown:
					return true;
			}
			return false;
		}

		public string GetKeyboardSetupApplication(out string arguments)
		{
			arguments = @"input.dll";
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.System), @"control.exe");
		}

		public bool IsSecondaryKeyboardSetupApplication
		{
			get { return false; }
		}

		#endregion

		#region IKeyboardSwitchingAdaptor Members

		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			CheckDisposed();
			SwitchKeyboard((WinKeyboardDescription) keyboard);
			return true;
		}

		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			CheckDisposed();
			SaveImeConversionStatus((WinKeyboardDescription) keyboard);
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		public KeyboardDescription DefaultKeyboard
		{
			get
			{
				CheckDisposed();
				return GetKeyboardDescription(InputLanguage.DefaultInputLanguage.Interface());
			}
		}

		/// <summary>
		///  Gets the currently active keyboard.
		/// </summary>
		public KeyboardDescription ActiveKeyboard
		{
			get
			{
				if (ProfileMgr != null)
				{
					var profile = ProfileMgr.GetActiveProfile(Guids.TfcatTipKeyboard);
					return Keyboard.Controller.AvailableKeyboards.OfType<WinKeyboardDescription>()
						.FirstOrDefault(winKeybd => InputProcessorProfilesEqual(profile, winKeybd.InputProcessorProfile)) ??
						KeyboardController.NullKeyboard;
				}

				// Probably Windows XP where we don't have ProfileMgr
				var lang = ProcessorProfiles.GetCurrentLanguage();
				return Keyboard.Controller.AvailableKeyboards.OfType<WinKeyboardDescription>()
					.FirstOrDefault(winKeybd => winKeybd.InputProcessorProfile.LangId == lang) ??
					KeyboardController.NullKeyboard;
			}
		}

		#endregion

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~WinKeyboardAdaptor()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				if (_timer != null)
				{
					_timer.Dispose();
					_timer = null;
				}

				if (_profileNotifySinkCookie > 0)
				{
					if (TfSource != null)
						TfSource.UnadviseSink(_profileNotifySinkCookie);
					_profileNotifySinkCookie = 0;
				}
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			IsDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}
#endif
