// Copyright (c) 2013-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.Keyboarding;
using Timer = System.Windows.Forms.Timer;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// This class handles switching for normal Windows keyboards, Windows IME keyboards, and Keyman 10 keyboards
	/// </summary>
	internal class WindowsKeyboardSwitchingAdapter : IKeyboardSwitchingAdaptor, IDisposable
	{
		#region Variables used for windows IME Mode switching hack
		public Timer Timer { get; private set; }
		private KeyboardDescription _expectedKeyboard;
		private WinKeyboardAdaptor _adaptor;
		private bool HasSwitchedLanguages { get; set; }
		public bool IsSwitchingKeyboards { get; set; }
		#endregion

		public WindowsKeyboardSwitchingAdapter(WinKeyboardAdaptor adaptor)
		{
			_adaptor = adaptor;
			Timer = new Timer { Interval = 500 };
			Timer.Tick += OnTimerTick;
			Timer.Enabled = true;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_expectedKeyboard == null || HasSwitchedLanguages)
			{
				return;
			}
			RestoreImeConversionStatus(_expectedKeyboard);
			IsSwitchingKeyboards = false;
			Timer.Stop();
		}

		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			if (KeyboardController.Instance.ActiveKeyboard == keyboard)
				return true;
			Timer.Start();
			return SwitchKeyboard(keyboard);
		}

		private bool SwitchKeyboard(KeyboardDescription winKeyboard)
		{
			var keyboard = winKeyboard as WinKeyboardDescription;
			if (IsSwitchingKeyboards)
				return true;

			IsSwitchingKeyboards = true;
			try
			{
				if (keyboard?.InputLanguage?.Culture == null)
				{
					return false;
				}

				_expectedKeyboard = keyboard;
#if !MONO
				return SwitchByProfile(keyboard);
#endif
			}
			finally
			{
				IsSwitchingKeyboards = false;
			}
			return true;
		}

		private bool SwitchByProfile(WinKeyboardDescription keyboard)
		{
			_adaptor.ProcessorProfiles.ChangeCurrentLanguage(keyboard.InputProcessorProfile.LangId);

			Guid classId = keyboard.InputProcessorProfile.ClsId;
			Guid guidProfile = keyboard.InputProcessorProfile.GuidProfile;
			_adaptor.ProfileManager.ActivateProfile(keyboard.InputProcessorProfile.ProfileType, keyboard.InputProcessorProfile.LangId, ref classId, ref guidProfile,
				keyboard.InputProcessorProfile.Hkl, TfIppMf.ForProcess);

			RestoreImeConversionStatus(
				keyboard); // Restore it even though sometimes windows will ignore us
			Timer.Stop();
			Timer.Start(); // Start the timer for restoring IME status for when windows ignores us.

			return true;
		}


		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			Timer.Stop();
			SaveImeConversionStatus((WinKeyboardDescription)keyboard);
		}

		/// <summary>
		/// Save the state of the conversion and sentence mode for the current IME
		/// so that we can restore it later.
		/// </summary>
		private void SaveImeConversionStatus(WinKeyboardDescription winKeyboard)
		{
			if (winKeyboard == null)
				return;

			if (InputLanguage.CurrentInputLanguage.Culture.Name != winKeyboard.InputLanguage.Culture.Name)
			{
				// Users can switch the keyboards without switching fields, don't save unrelated IME status
				return;
			}

			var windowHandle = new HandleRef(this, Win32.GetFocus());
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
			var windowHandle = new HandleRef(this, Win32.GetFocus());

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
			int currentConversionMode, currentSentenceMode;
			Win32.ImmGetConversionStatus(contextHandle, out currentConversionMode, out currentSentenceMode);
			if (winKeyboard.ConversionMode != currentConversionMode || winKeyboard.SentenceMode != currentSentenceMode)
			{
				Win32.ImmSetConversionStatus(contextHandle, winKeyboard.ConversionMode, winKeyboard.SentenceMode);
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

		~WindowsKeyboardSwitchingAdapter()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary/>
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

		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			Timer?.Dispose();
			Timer = null;
		}
	}
}