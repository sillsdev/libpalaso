// Copyright (c) 2013-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// This class handles switching for normal Windows keyboards, Windows IME keyboards, and Keyman 10 keyboards
	/// </summary>
	internal class WindowsKeyboardSwitchingAdapter : IKeyboardSwitchingAdaptor
	{
		private WinKeyboardAdaptor _adaptor;
		private bool _isSwitchingKeyboards;

		public WindowsKeyboardSwitchingAdapter(WinKeyboardAdaptor adaptor)
		{
			_adaptor = adaptor;
		}

		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			if (KeyboardController.Instance.ActiveKeyboard == keyboard)
			{
				Trace.WriteLine($"[KbdSwitch] ActivateKeyboard: already active, skipping: {keyboard}");
				return true;
			}
			Trace.WriteLine($"[KbdSwitch] ActivateKeyboard: switching from {KeyboardController.Instance.ActiveKeyboard} to {keyboard}");
			return SwitchKeyboard(keyboard);
		}

		private bool SwitchKeyboard(KeyboardDescription winKeyboard)
		{
			var keyboard = winKeyboard as WinKeyboardDescription;
			if (_isSwitchingKeyboards)
			{
				Trace.WriteLine($"[KbdSwitch] SwitchKeyboard: reentrant call blocked for {keyboard?.Name}");
				return true;
			}

			if (keyboard?.InputLanguage?.Culture == null || keyboard.InputProcessorProfile.LangId == 0)
			{
				Trace.WriteLine($"[KbdSwitch] SwitchKeyboard: invalid keyboard description (culture={keyboard?.InputLanguage?.Culture}, langId=0x{keyboard?.InputProcessorProfile.LangId:X4})");
				return false;
			}

			_isSwitchingKeyboards = true;
			try
			{
				return Platform.IsMono || SwitchByProfile(keyboard);
			}
			finally
			{
				_isSwitchingKeyboards = false;
			}
		}

		private bool SwitchByProfile(WinKeyboardDescription keyboard)
		{
			var focusBefore = Win32.GetFocus();
			Trace.WriteLine($"[KbdSwitch] SwitchByProfile START: {keyboard.Name}, LangId=0x{keyboard.InputProcessorProfile.LangId:X4}, ProfileType={keyboard.InputProcessorProfile.ProfileType}, focus=0x{focusBefore:X}, focusClass={GetWindowClassName(focusBefore)}");

			_adaptor.ProcessorProfiles.ChangeCurrentLanguage(keyboard.InputProcessorProfile.LangId);
			Trace.WriteLine($"[KbdSwitch] ChangeCurrentLanguage done, CurrentInputLanguage={InputLanguage.CurrentInputLanguage.Culture.Name}");

			Guid classId = keyboard.InputProcessorProfile.ClsId;
			Guid guidProfile = keyboard.InputProcessorProfile.GuidProfile;
			_adaptor.ProfileManager.ActivateProfile(keyboard.InputProcessorProfile.ProfileType, keyboard.InputProcessorProfile.LangId, ref classId, ref guidProfile,
				keyboard.InputProcessorProfile.Hkl, TfIppMf.ForProcess);

			var focusAfter = Win32.GetFocus();
			var hkl = Win32.GetKeyboardLayout(0);
			Trace.WriteLine($"[KbdSwitch] ActivateProfile done, CurrentInputLanguage={InputLanguage.CurrentInputLanguage.Culture.Name}, focus=0x{focusAfter:X} (focusChanged={focusBefore != focusAfter}), threadHKL=0x{hkl:X}");

			RestoreImeConversionStatus(keyboard);
			TraceImeState("PostSwitch", focusAfter, keyboard);

			Trace.WriteLine("[KbdSwitch] SwitchByProfile END");
			return true;
		}

		/// <summary>
		/// Log detailed IME state for diagnosing intermittent IME activation issues.
		/// </summary>
		private void TraceImeState(string context, IntPtr focusHwnd, WinKeyboardDescription keyboard)
		{
			var windowHandle = new HandleRef(this, focusHwnd);
			var contextPtr = Win32.ImmGetContext(windowHandle);
			if (contextPtr == IntPtr.Zero)
			{
				Trace.WriteLine($"[KbdSwitch] {context}: ImmGetContext=NULL (focus=0x{focusHwnd:X}) — no IME context available");
				return;
			}
			var contextHandle = new HandleRef(this, contextPtr);
			var isOpen = Win32.ImmGetOpenStatus(contextHandle);
			int convMode, sentMode;
			Win32.ImmGetConversionStatus(contextHandle, out convMode, out sentMode);
			Win32.ImmReleaseContext(windowHandle, contextHandle);
			Trace.WriteLine($"[KbdSwitch] {context}: IME open={isOpen}, conversion=0x{convMode:X}, sentence=0x{sentMode:X}, focus=0x{focusHwnd:X}, focusClass={GetWindowClassName(focusHwnd)}");
		}

		private static string GetWindowClassName(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero)
				return "(null)";
			var sb = new StringBuilder(256);
			Win32.GetClassName(hwnd, sb, sb.Capacity);
			return sb.ToString();
		}


		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			Trace.WriteLine($"[KbdSwitch] DeactivateKeyboard: {keyboard}");
			SaveImeConversionStatus((WinKeyboardDescription)keyboard);
		}

		/// <summary>
		/// Save the state of the conversion and sentence mode for the current IME
		/// so that we can restore it later.
		/// </summary>
		private void SaveImeConversionStatus(WinKeyboardDescription winKeyboard)
		{
			if (winKeyboard == null)
			{
				Trace.WriteLine("[KbdSwitch] SaveIME: keyboard is null, skipping");
				return;
			}

			if (InputLanguage.CurrentInputLanguage.Culture.Name != winKeyboard.InputLanguage.Culture.Name)
			{
				Trace.WriteLine($"[KbdSwitch] SaveIME: culture mismatch (current={InputLanguage.CurrentInputLanguage.Culture.Name}, keyboard={winKeyboard.InputLanguage.Culture.Name}), skipping");
				return;
			}

			var focusHwnd = Win32.GetFocus();
			var windowHandle = new HandleRef(this, focusHwnd);
			var contextPtr = Win32.ImmGetContext(windowHandle);
			if (contextPtr == IntPtr.Zero)
			{
				Trace.WriteLine($"[KbdSwitch] SaveIME: ImmGetContext returned NULL (focus=0x{focusHwnd:X}), skipping");
				return;
			}
			var contextHandle = new HandleRef(this, contextPtr);
			int conversionMode;
			int sentenceMode;
			Win32.ImmGetConversionStatus(contextHandle, out conversionMode, out sentenceMode);
			Trace.WriteLine($"[KbdSwitch] SaveIME: {winKeyboard.Name} conversion=0x{conversionMode:X}, sentence=0x{sentenceMode:X}");
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
			var focusHwnd = Win32.GetFocus();
			var windowHandle = new HandleRef(this, focusHwnd);

			// NOTE: Windows uses the same context for all windows of the current thread, so it
			// doesn't really matter which window handle we pass.
			var contextPtr = Win32.ImmGetContext(windowHandle);
			if (contextPtr == IntPtr.Zero)
			{
				Trace.WriteLine($"[KbdSwitch] RestoreIME: ImmGetContext returned NULL (focus=0x{focusHwnd:X}), skipping");
				return;
			}

			// NOTE: Chinese Pinyin IME allows to switch between Chinese and Western punctuation.
			// This can be selected in both Native and Alphanumeric conversion mode. However,
			// when setting the value the punctuation setting doesn't get restored in Alphanumeric
			// conversion mode, not matter what I try. I guess that is because Chinese punctuation
			// doesn't really make sense with Latin characters.
			var contextHandle = new HandleRef(this, contextPtr);
			int currentConversionMode, currentSentenceMode;
			Win32.ImmGetConversionStatus(contextHandle, out currentConversionMode, out currentSentenceMode);
			if (winKeyboard.ConversionMode != currentConversionMode || winKeyboard.SentenceMode != currentSentenceMode)
			{
				var success = Win32.ImmSetConversionStatus(contextHandle, winKeyboard.ConversionMode, winKeyboard.SentenceMode);
				Trace.WriteLine($"[KbdSwitch] RestoreIME: {winKeyboard.Name} set conversion 0x{currentConversionMode:X}->0x{winKeyboard.ConversionMode:X}, sentence 0x{currentSentenceMode:X}->0x{winKeyboard.SentenceMode:X}, success={success}");
			}
			else
			{
				Trace.WriteLine($"[KbdSwitch] RestoreIME: {winKeyboard.Name} modes already match (conversion=0x{currentConversionMode:X}, sentence=0x{currentSentenceMode:X})");
			}
			Win32.ImmReleaseContext(windowHandle, contextHandle);
		}

		public KeyboardDescription DefaultKeyboard => WinKeyboardUtils.GetKeyboardDescription(InputLanguage.DefaultInputLanguage.Interface());

		/// <summary/>
		public KeyboardDescription ActiveKeyboard
		{
			get
			{
				var currentLanguage = InputLanguage.CurrentInputLanguage;
				var availableWinKeyboards = Keyboard.Controller.AvailableKeyboards.OfType<WinKeyboardDescription>();
				return availableWinKeyboards.FirstOrDefault(winKeybd => winKeybd.InputLanguage.Culture.Name == currentLanguage.Culture.Name)
					?? KeyboardController.NullKeyboard;
			}
		}

	}
}