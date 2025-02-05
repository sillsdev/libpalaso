// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Keyman7Interop;
using Keyman10Interop;
using Microsoft.Win32;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// Class for handling Keyman keyboards not associated with a Windows language
	/// </summary>
	internal class KeymanKeyboardAdaptor : IKeyboardRetrievingAdaptor
	{
		private enum KeymanVersion
		{
			NotInstalled,
			Keyman6,
			Keyman7to9,
			Keyman10
		}

		public KeymanKeyboardAdaptor()
		{
			InstalledKeymanVersion = GetInstalledKeymanVersion();
		}

		private KeymanVersion InstalledKeymanVersion { get; set; }

		#region IKeyboardRetrievingAdaptor Members

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardAdaptorType Type
		{
			get
			{
				CheckDisposed();
				return KeyboardAdaptorType.OtherIm;
			}
		}

		public bool IsApplicable
		{
			get
			{
				switch (InstalledKeymanVersion)
				{
					case KeymanVersion.Keyman10:
						var keyman10 = new KeymanClass();
						if (keyman10.Keyboards != null && keyman10.Keyboards.Count > 0)
						{
							return true;
						}
						break;
					case KeymanVersion.Keyman7to9:
						var keyman = new TavultesoftKeymanClass();
						if (keyman.Keyboards != null && keyman.Keyboards.Count > 0)
						{
							return true;
						}
						break;
					case KeymanVersion.Keyman6:
						var keymanLink = new KeymanLink.KeymanLink();
							if (keymanLink.Initialize())
							{
								if (keymanLink.Keyboards != null && keymanLink.Keyboards.Length > 0)
								{
									return true;
								}
							}
							break;
					case KeymanVersion.NotInstalled:
						return false;
					default:
						throw new NotSupportedException($"{InstalledKeymanVersion} not yet supported in IsApplicable");

				}
				return false;
			}
		}

		public IKeyboardSwitchingAdaptor SwitchingAdaptor { get; set; }

		public void Initialize()
		{
			CheckDisposed();
			SwitchingAdaptor = GetKeymanSwitchingAdapter(InstalledKeymanVersion);
			UpdateAvailableKeyboards();
		}

		private IKeyboardSwitchingAdaptor GetKeymanSwitchingAdapter(KeymanVersion installedKeymanVersion)
		{
			switch (installedKeymanVersion)
			{
				case KeymanVersion.Keyman10:
					return new KeymanKeyboardSwitchingAdapter();
				case KeymanVersion.Keyman6:
				case KeymanVersion.Keyman7to9:
					return new LegacyKeymanKeyboardSwitchingAdapter();
			}
			return null;
		}

		private KeymanVersion GetInstalledKeymanVersion()
		{
			// limit the COMException catching by determining the current version once and assuming it for the
			// rest of the adaptor's lifetime
			try
			{
				var keyman10 = new KeymanClass();
				return KeymanVersion.Keyman10;
			}
			catch (COMException)
			{
				// not 10
			}
			try
			{
				var keyman = new TavultesoftKeymanClass();
				return KeymanVersion.Keyman7to9;
			}
			catch (COMException)
			{
				// Not 7-9
			}
			try
			{
				var keymanLink = new KeymanLink.KeymanLink();
				return KeymanVersion.Keyman6;
			}
			catch (COMException)
			{
			}
			return KeymanVersion.NotInstalled;
		}

		public void UpdateAvailableKeyboards()
		{
			CheckDisposed();
			Dictionary<string, KeymanKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<KeymanKeyboardDescription>().ToDictionary(kd => kd.Id);
			switch (InstalledKeymanVersion)
			{
				case KeymanVersion.Keyman10:
					// Keyman10 keyboards are handled by the WinKeyboardAdapter and WindowsKeyboardSwitchingAdapter
					break;
				case KeymanVersion.Keyman7to9:
					var keyman = new TavultesoftKeymanClass();
					UpdateKeyboards(curKeyboards, keyman.Keyboards.OfType<Keyman7Interop.IKeymanKeyboard>().Select(kb => kb.Name), false);
					break;
				case KeymanVersion.Keyman6:
					var keymanLink = new KeymanLink.KeymanLink();
					if (keymanLink.Initialize())
					{
						UpdateKeyboards(curKeyboards, keymanLink.Keyboards.Select(kb => kb.KbdName), true);
					}
					break;
			}
		}

		private void UpdateKeyboards(Dictionary<string, KeymanKeyboardDescription> curKeyboards, IEnumerable<string> availableKeyboardNames, bool isKeyman6)
		{
			foreach (string keyboardName in availableKeyboardNames)
			{
				KeymanKeyboardDescription existingKeyboard;
				if (curKeyboards.TryGetValue(keyboardName, out existingKeyboard))
				{
					if (!existingKeyboard.IsAvailable)
					{
						existingKeyboard.SetIsAvailable(true);
						existingKeyboard.IsKeyman6 = isKeyman6;
						if (existingKeyboard.Format == KeyboardFormat.Unknown)
							existingKeyboard.Format = KeyboardFormat.CompiledKeyman;
					}
					curKeyboards.Remove(keyboardName);
				}
				else
				{
					KeyboardController.Instance.Keyboards.Add(new KeymanKeyboardDescription(keyboardName, isKeyman6, this, true) { Format = KeyboardFormat.CompiledKeyman });
				}
			}
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the ID.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public KeyboardDescription CreateKeyboardDefinition(string id)
		{
			CheckDisposed();
			switch (InstalledKeymanVersion)
			{
				case KeymanVersion.Keyman10:
					string layout, locale;
					KeyboardController.GetLayoutAndLocaleFromLanguageId(id, out layout, out locale);

					string cultureName;
					var inputLanguage = WinKeyboardUtils.GetInputLanguage(locale, layout, out cultureName);
					return new KeymanKeyboardDescription(id, false, this, false) { InputLanguage = inputLanguage };
				default:
					return new KeymanKeyboardDescription(id, false, this, false);
			}
		}

		public string GetKeyboardSetupApplication(out string arguments)
		{
			arguments = null;
			string keyman;
			int version = 0;
			string keymanPath = GetKeymanRegistryValue(@"root path", ref version);
			if (keymanPath != null)
			{
				keyman = Path.Combine(keymanPath, @"kmshell.exe");
				if (File.Exists(keyman))
				{
					arguments = @"";
					if (version > 6)
						arguments = @"-c";
					return keyman;
				}
			}
			return null;
		}

		/// <summary>
		/// This method returns the path to Keyman Configuration if it is installed. Otherwise it returns null.
		/// It also sets the version of Keyman that it found.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="version">The version.</param>
		private static string GetKeymanRegistryValue(string key, ref int version)
		{
			using (var olderKeyman = Registry.LocalMachine.OpenSubKey(@"Software\Tavultesoft\Keyman", false))
			using (var olderKeyman32 = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Tavultesoft\Keyman", false))
			{
				var keymanKey = olderKeyman32 ?? olderKeyman;
				if (keymanKey == null)
					return null;

				int[] versions = {9, 8, 7, 6, 5};
				foreach (var vers in versions)
				{
					using (var rkApplication = keymanKey.OpenSubKey($"{vers}.0", false))
					{
						if (rkApplication != null)
						{
							foreach (var sKey in rkApplication.GetValueNames())
							{
								if (sKey == key)
								{
									version = vers;
									return (string) rkApplication.GetValue(sKey);
								}
							}
						}
					}
				}
			}

			return null;
		}

		public Action GetKeyboardSetupAction()
		{
			switch (InstalledKeymanVersion)
			{
				case KeymanVersion.Keyman10:
					return () =>
					{
						var keymanClass = new KeymanClass();
						keymanClass.Control.OpenConfiguration();
					};
				case KeymanVersion.Keyman7to9:
				case KeymanVersion.Keyman6:
				return () =>
				{
					string args;
					var setupApp = GetKeyboardSetupApplication(out args);
					Process.Start(setupApp, args);
				};
				default:
					throw new NotSupportedException($"No keyboard setup action defined for keyman version {InstalledKeymanVersion}");
			}
		}

		public Action GetSecondaryKeyboardSetupAction() => GetKeyboardSetupAction();

		public bool IsSecondaryKeyboardSetupApplication => true;

		public bool CanHandleFormat(KeyboardFormat format)
		{
			CheckDisposed();
			switch (format)
			{
				case KeyboardFormat.CompiledKeyman:
				case KeyboardFormat.Keyman:
				case KeyboardFormat.KeymanPackage:
					return true;
			}
			return false;
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
		~KeymanKeyboardAdaptor()
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
			// Therefore, you should call GC.SuppressFinalize to
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
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			IsDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}
