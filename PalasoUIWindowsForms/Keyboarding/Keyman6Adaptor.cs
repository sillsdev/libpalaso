using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	internal class Keyman6Adaptor
	{
		public static void ActivateKeyboard(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return;
			}

			if(Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return;
			}
			TryActivateKeyman6Keyboard();
		}

		// this needs to be in a separate method or the Mono Jit will fail
		private static void TryActivateKeyman6Keyboard() {
#if !MONO
			try
			{
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (!keymanLink.Initialize(false))
				{
					Palaso.Reporting.NonFatalErrorDialog.Show("Keyman6 could not be activated.");
					return;
				}
				keymanLink.SelectKeymanKeyboard(name, true);

				//Wanted to fail fast if that didn't work, but it turns out that it takes a turn through
				//Application.DoEvents before the keyboard is actually active, and it is unsafe of us
				//to call that here.  Unit tests, however, do need to call that to ensure this meathod works.
			}
			catch (Exception )
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("The keyboard '" + name + "' could not be activated using Keyman 6.");
			}
#endif
		}

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardController.KeyboardDescriptor> descriptors = new List<KeyboardController.KeyboardDescriptor>();
				if (Environment.OSVersion.Platform == PlatformID.Unix)
				{
					return descriptors;
				}
TryGetKeyboardDescriptors();
				return descriptors;
			}
		}

		// this needs to be in a separate method or the Mono Jit will fail
		private static void TryGetKeyboardDescriptors()
		{
#if !MONO
				try
				{
					KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
					if (keymanLink.Initialize(false))
					{
						foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in
							keymanLink.Keyboards)
						{
							KeyboardController.KeyboardDescriptor d = new KeyboardController.KeyboardDescriptor();
							d.Name = keyboard.KbdName;
							d.engine = KeyboardController.Engines.Keyman6;
							descriptors.Add(d);
						}
					}
				}
				catch (Exception err)
				{
					Debug.Fail(err.Message);
				}
#endif
		}

		public static bool EngineAvailable
		{
			get
			{
				if (Environment.OSVersion.Platform == PlatformID.Unix)
				{
					return false;
				}

				IsEngineAvailable();
				return false;
			}
		}

		// this needs to be in a separate method or the Mono Jit will fail
		private static void IsEngineAvailable()
		{
#if !MONO
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				return keymanLink.Initialize(false);
#endif
		}

		static public void Deactivate()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return;
			}

			TryDeactivate();
		}

		// this needs to be in a separate method or the Mono Jit will fail
		private static void TryDeactivate()
		{
#if !MONO
			try
			{
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize(false))
				{
					keymanLink.SelectKeymanKeyboard(null, false);
				}
			}
			catch (Exception )
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("There was a problem deactivating keyman 6.");
			}
#endif
		}

		public static bool HasKeyboardNamed(string name)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return false;
			}
			TryHasKeyboardNamed();
			return false;
		}

		// this needs to be in a separate method or the Mono Jit will fail
		private static void TryHasKeyboardNamed()
		{
#if !MONO
			try
			{
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (!keymanLink.Initialize(false))
				{
					return false;
				}
				foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in keymanLink.Keyboards)
				{
					if(keyboard.KbdName == name)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception )
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("There was a problem looking for a keybaord in keyman 6.");
			}
#endif
		}

		public static string GetActiveKeyboard()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return null;
			}
			TryGetActiveKeyboard();
			return null;
		}

		// this needs to be in a separate method or the Mono Jit will fail
		private static void TryGetActiveKeyboard()
		{
#if !MONO
			KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
			if (!keymanLink.Initialize(false))
			{
				return null;
			}
			try
			{
				KeymanLink.KeymanLink.KeymanKeyboard keyboard = keymanLink.ActiveKeymanKeyboard();
				if(null == keyboard)
					return null;
				else
					return keyboard.KbdName;
			}
			catch (Exception )
			{
				Palaso.Reporting.NonFatalErrorDialog.Show(
					"There was a problem retrieving the active keyboard in keyman 6.");
			}
#endif
		}
	}
}