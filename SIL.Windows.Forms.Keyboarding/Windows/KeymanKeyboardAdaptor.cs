// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
#if !MONO
using Keyman7Interop;
using Keyman10Interop;
#endif
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
#if !MONO
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
#endif
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
#if !MONO
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
#endif
			return KeymanVersion.NotInstalled;
		}

		public void UpdateAvailableKeyboards()
		{
			CheckDisposed();
			Dictionary<string, KeymanKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<KeymanKeyboardDescription>().ToDictionary(kd => kd.Id);
#if !MONO
			switch (InstalledKeymanVersion)
			{
				case KeymanVersion.Keyman10:
					var keyman10 = new KeymanClass();
					var langs = keyman10.Languages;
					UpdateKeyboards(curKeyboards, keyman10.Keyboards.OfType<Keyman10Interop.IKeymanKeyboard>(), langs);
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
#endif
		}

#if !MONO
		private void UpdateKeyboards(Dictionary<string, KeymanKeyboardDescription> curKeyboards, IEnumerable<Keyman10Interop.IKeymanKeyboard> availableKeyboards, Keyman10Interop.IKeymanLanguages languages)
		{
			var langAssocKeyboards = new Dictionary<string, string>();
			foreach (Keyman10Interop.IKeymanLanguage lang in languages)
			{
				if (lang.KeymanKeyboardLanguage != null)
				{
					langAssocKeyboards[lang.LayoutName] = lang.KeymanKeyboardLanguage.BCP47Code;
				}
			}
			foreach (Keyman10Interop.IKeymanKeyboard keyboard in availableKeyboards)
			{
				var keyboardName = keyboard.Name;
				KeymanKeyboardDescription existingKeyboard;
				if (curKeyboards.TryGetValue(keyboardName, out existingKeyboard))
				{
					if (!existingKeyboard.IsAvailable)
					{
						existingKeyboard.SetIsAvailable(true);
						existingKeyboard.IsKeyman6 = false;
						if (existingKeyboard.Format == KeyboardFormat.Unknown)
						{
							existingKeyboard.Format = KeyboardFormat.CompiledKeyman;
						}
					}
					curKeyboards.Remove(keyboardName);
				}
				else
				{
					if (!langAssocKeyboards.ContainsKey(keyboardName))
					{
						continue; // a KeymanKeyboard that is not associated with a language can not be used
					}
					var langId = langAssocKeyboards[keyboardName];

					string layout, locale;
					KeyboardController.GetLayoutAndLocaleFromLanguageId(langId, out layout, out locale);

					string cultureName;
					var inputLanguage = WinKeyboardUtils.GetInputLanguage(locale, layout, out cultureName);
					KeyboardController.Instance.Keyboards.Add(new KeymanKeyboardDescription(keyboardName, false, this, true)
					{
						Format = KeyboardFormat.CompiledKeyman, InputLanguage = inputLanguage
					});
				}
			}
		}
#endif

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
					// We would like to use the COM API for Keyman 10 but it will take an API change:
					// Code that we would use:
					//try
					//{
					//	var keymanComObject = new KeymanClass();
					//	keymanComObject.Control.OpenConfiguration();
					//}
					//catch (COMException)
					//{
					//	// Keyman is not installed
					//}
					// From Marc Durdin (7/16/09):
					// Re LT-9902, in Keyman 6, you could launch the configuration dialog reliably by running kmshell.exe.
					// However, Keyman 7 works slightly differently.  The recommended approach is to use the COM API:
					// http://www.tavultesoft.com/keymandev/documentation/70/comapi_interface_IKeymanProduct_OpenConfiguration.html
					// Sample code:
					//	dim kmcom, product
					//	Set kmcom = CreateObject("kmcomapi.TavultesoftKeyman")
					//	rem  Pro = ProductID 1; Light = ProductID 8
					//	rem  Following line will raise exception if product is not installed, so try/catch it
					//	Set product = kmcom.Products.ItemsByProductID(1)
					//	Product.OpenConfiguration
					// But if that is not going to be workable for you, then use the parameter  "-c" to start configuration.
					// Without a parameter, the action is to start Keyman Desktop itself; v7.0 would fire configuration if restarted,
					// v7.1 just flags to the user that Keyman is running and where to find it.  This change was due to feedback that
					// users would repeatedly try to start Keyman when it was already running, and get confused when they got the
					// Configuration dialog.  Sorry for the unannounced change... 9
					// The -c parameter will not work with Keyman 6, so you would need to test for the specific version.  For what it's worth, the
					// COM API is static and should not change, while the command line parameters are not guaranteed to change from version to version.
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
		/// <returns></returns>
		private static string GetKeymanRegistryValue(string key, ref int version)
		{
			using (var keyman10 = Registry.LocalMachine.OpenSubKey(@"Software\Keyman\Keyman Desktop", false))
			using (var keyman6to9 = Registry.LocalMachine.OpenSubKey(@"Software\Tavultesoft\Keyman", false))
			using (var keyman10_32 = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Keyman\Keyman Desktop", false))
			using (var keyman6to9_32 = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Tavultesoft\Keyman", false))
			{
				var keymanKey = keyman10 ?? keyman6to9 ?? keyman10_32 ?? keyman6to9_32;
				if (keymanKey == null)
					return null;

				int[] versions = {10, 9, 8, 7, 6, 5};
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

		public bool IsSecondaryKeyboardSetupApplication => true;

		public bool CanHandleFormat(KeyboardFormat format)
		{
			CheckDisposed();
			switch (format)
			{
				case KeyboardFormat.CompiledKeyman:
				case KeyboardFormat.Keyman:
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
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			IsDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}
