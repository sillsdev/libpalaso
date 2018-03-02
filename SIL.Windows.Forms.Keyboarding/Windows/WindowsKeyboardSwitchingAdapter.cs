// Copyright (c) 2013-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// This keyboard switching adapter handles windows keyboard switching using the modern api as well as falling back to old win API for XP.
	/// </summary>
	internal class WindowsKeyboardSwitchingAdapter : IKeyboardSwitchingAdaptor, IDisposable
	{
		public Timer Timer { get; private set; }

		private KeyboardDescription _expectedKeyboard;
		private bool HasSwitchedLanguages { get; set; }
		private bool IsSwitchingKeyboards { get; set; }
		private ITfInputProcessorProfiles ProcessorProfiles { get; }
		private ITfInputProcessorProfileMgr ProfileMgr { get; }
		internal ITfSource TfSource { get; private set; }

		private ushort _profileNotifySinkCookie;
		private readonly TfLanguageProfileNotifySink _tfLanguageProfileNotifySink;

		private readonly List<IWindowsLanguageProfileSink> _windowsLanguageProfileSinks = new List<IWindowsLanguageProfileSink>();

		public WindowsKeyboardSwitchingAdapter(ITfInputProcessorProfiles processorProfiles, ITfInputProcessorProfileMgr profileMgr)
		{
			ProfileMgr = profileMgr;
			ProcessorProfiles = processorProfiles;
			_tfLanguageProfileNotifySink = new TfLanguageProfileNotifySink(this);

			TfSource = ProcessorProfiles as ITfSource;
			if (TfSource != null)
			{
				_profileNotifySinkCookie = TfSource.AdviseSink(
					ref Guids.Consts.ITfLanguageProfileNotifySink, _tfLanguageProfileNotifySink);
			}

			Timer = new Timer { Interval = 500 };
			Timer.Tick += OnTimerTick;

			// Form.ActiveForm can be null when running unit tests
			if (Form.ActiveForm != null)
			{
				Form.ActiveForm.InputLanguageChanged += ActiveFormOnInputLanguageChanged;
			}

			if (KeyboardController.Instance != null)
			{
				KeyboardController.Instance.ControlAdded += OnControlRegistered;
				KeyboardController.Instance.ControlRemoving += OnControlRemoving;
			}
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_expectedKeyboard == null || HasSwitchedLanguages)
			{
				return;
			}

			// This code gets only called if TSF is not available(e.g. Windows XP)
			if (InputLanguage.CurrentInputLanguage.Culture.KeyboardLayoutId == _expectedKeyboard.InputLanguage.Culture.KeyboardLayoutId)
			{
				_expectedKeyboard = null;
				HasSwitchedLanguages = false;
				Timer.Enabled = false;
				return;
			}

			SwitchKeyboard(_expectedKeyboard);
		}

		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			SwitchKeyboard(keyboard);
			return true;
		}

		private void SwitchKeyboard(KeyboardDescription winKeyboard)
		{
			if (IsSwitchingKeyboards)
				return;

			IsSwitchingKeyboards = true;
			try
			{
				KeyboardController.Instance.ActiveKeyboard = ActivateWinKeyboard(winKeyboard);
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
				if (!UseWindowsApiForKeyboardSwitching((WinKeyboardDescription)winKeyboard))
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
				HasSwitchedLanguages = true;
				// stop timer first so that the 0.5s interval restarts.
				Timer.Stop();
				Timer.Start();
			}
			finally
			{
				IsSwitchingKeyboards = false;
			}
		}

		private bool UseWindowsApiForKeyboardSwitching(WinKeyboardDescription winKeyboard)
		{
			return ProcessorProfiles == null || ProfileMgr == null && winKeyboard.InputProcessorProfile.Hkl == IntPtr.Zero;
		}

		private void ActiveFormOnInputLanguageChanged(object sender, InputLanguageChangedEventArgs inputLanguageChangedEventArgs)
		{
			RestoreImeConversionStatus(WinKeyboardUtils.GetKeyboardDescription(inputLanguageChangedEventArgs.InputLanguage.Interface()));
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

		private KeyboardDescription ActivateWinKeyboard(KeyboardDescription keyboard)
		{
			var winKeyboard = (WinKeyboardDescription)keyboard;
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

		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
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

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		public KeyboardDescription DefaultKeyboard => WinKeyboardUtils.GetKeyboardDescription(InputLanguage.DefaultInputLanguage.Interface());

		/// <summary>
		///  Gets the currently active keyboard.
		/// </summary>
		public KeyboardDescription ActiveKeyboard
		{
			get
			{
				if (ProfileMgr != null)
				{
					var profile = ProfileMgr.GetActiveProfile(ref Guids.Consts.TfcatTipKeyboard);
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

		internal void OnControlRegistered(object sender, RegisterEventArgs e)
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

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				if (_profileNotifySinkCookie > 0)
				{
					if (TfSource != null)
						TfSource.UnadviseSink(_profileNotifySinkCookie);
					_profileNotifySinkCookie = 0;
				}
			}

			Timer?.Dispose();
			IsDisposed = true;
		}

		~WindowsKeyboardSwitchingAdapter()
		{
			Dispose(false);
		}

		/// <summary>
		/// This class receives notifications from TSF when the input method changes.
		/// It also implements a fallback to Windows messages if TSF isn't available,
		/// e.g. on Windows XP.
		/// </summary>
		private class TfLanguageProfileNotifySink : ITfLanguageProfileNotifySink
		{
			private readonly WindowsKeyboardSwitchingAdapter _keyboardSwitcher;
			private readonly List<Form> _toplevelForms = new List<Form>();

			public TfLanguageProfileNotifySink(WindowsKeyboardSwitchingAdapter keyboardSwitcher)
			{
				_keyboardSwitcher = keyboardSwitcher;
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
				IKeyboardDefinition winKeyboard = _keyboardSwitcher.ActiveKeyboard;
				Debug.WriteLine("Language changed from {0} to {1}",
					Keyboard.Controller.ActiveKeyboard != null ? Keyboard.Controller.ActiveKeyboard.Layout : "<null>",
					winKeyboard != null ? winKeyboard.Layout : "<null>");

				_keyboardSwitcher._windowsLanguageProfileSinks.ForEach(
					sink => sink.OnInputLanguageChanged(Keyboard.Controller.ActiveKeyboard, winKeyboard));
			}
			#endregion

			#region Fallback if TSF isn't available

			// The WinKeyboardAdaptor will subscribe to the Form's InputLanguageChanged event
			// only if TSF is not available. Otherwise this code won't be executed.

			private void OnWindowsMessageInputLanguageChanged(object sender,
				InputLanguageChangedEventArgs inputLanguageChangedEventArgs)
			{
				Debug.Assert(_keyboardSwitcher._profileNotifySinkCookie == 0);

				KeyboardDescription winKeyboard = WinKeyboardUtils.GetKeyboardDescription(inputLanguageChangedEventArgs.InputLanguage.Interface());

				_keyboardSwitcher._windowsLanguageProfileSinks.ForEach(
					sink => sink.OnInputLanguageChanged(Keyboard.Controller.ActiveKeyboard, winKeyboard));
			}

			public void RegisterWindowsMessageHandler(Control control)
			{
				Debug.Assert(_keyboardSwitcher._profileNotifySinkCookie == 0);

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
	}
}