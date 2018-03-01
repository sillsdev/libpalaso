// Copyright (c) 2013-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// Class for handling Windows system keyboards
	/// </summary>
	internal class WinKeyboardAdaptor : IKeyboardRetrievingAdaptor
	{
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
				IKeyboardDefinition winKeyboard = _keyboardAdaptor.SwitchingAdaptor.ActiveKeyboard;
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

		private WinKeyboardDescription _expectedKeyboard;
		private bool _fSwitchedLanguages;

		private ushort _profileNotifySinkCookie;
		private readonly TfLanguageProfileNotifySink _tfLanguageProfileNotifySink;

		private readonly List<IWindowsLanguageProfileSink> _windowsLanguageProfileSinks = new List<IWindowsLanguageProfileSink>();

		internal ITfInputProcessorProfiles ProcessorProfiles { get; private set; }
		internal ITfInputProcessorProfileMgr ProfileMgr { get; private set; }
		internal ITfSource TfSource { get; private set; }

		public bool IsApplicable => true;

		public IKeyboardSwitchingAdaptor SwitchingAdaptor { get; private set; }

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

			SwitchingAdaptor = new WindowsXPKeyboardSwitchingAdapter(ProcessorProfiles, ProfileMgr);
			_tfLanguageProfileNotifySink = new TfLanguageProfileNotifySink(this);

			TfSource = ProcessorProfiles as ITfSource;
			if (TfSource != null)
			{
				_profileNotifySinkCookie = TfSource.AdviseSink(
					ref Guids.Consts.ITfLanguageProfileNotifySink, _tfLanguageProfileNotifySink);
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
					if (profile.CatId != Guids.Consts.TfcatTipKeyboard)
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

		public void UpdateAvailableKeyboards()
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
						layoutName = WinKeyboardUtils.GetLayoutNameEx(hkl);
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
			// Set each unhanandled keyboard to unavailable
			foreach (var existingKeyboard in curKeyboards.Values)
			{
				existingKeyboard.SetIsAvailable(false);
			}
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

		public void Initialize()
		{
			UpdateAvailableKeyboards();

		}

		private KeyboardDescription GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			return WinKeyboardUtils.GetKeyboardDescription(inputLanguage);
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
			string layout = parts.Length > 1 ? parts[1] : string.Empty;

			string cultureName;
			var inputLanguage = WinKeyboardUtils.GetInputLanguage(locale, layout, out cultureName);

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
