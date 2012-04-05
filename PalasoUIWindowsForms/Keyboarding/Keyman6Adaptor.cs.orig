using System;
using System.Collections.Generic;
using System.Diagnostics;
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

			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return;
			}

			try
			{
				InnerKeyman6Wrapper.ActivateKeyboard(name);
			}
			catch (Reporting.ErrorReport.NonFatalMessageSentToUserException)
			{
				throw; // needed for tests to know that a message box would have been shown
			}
			catch (Exception)
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("The keyboard '" + name + "' could not be activated using Keyman 6.");
			}
		}

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
				{
					try
					{
						return InnerKeyman6Wrapper.KeyboardDescriptors;
					}
					catch (Exception err)
					{
						 Debug.Fail(err.Message);
					}
				}
				return new List<KeyboardController.KeyboardDescriptor>();
			}
		}


		public static bool EngineAvailable
		{
			get
			{
				if (Environment.OSVersion.Platform == PlatformID.Unix)
				{
					return false;
				}
				try
				{
					return InnerKeyman6Wrapper.EngineAvailable;
				}
				catch (Exception) {}

				return false;
			}
		}

		static public void Deactivate()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return;
			}

			try
			{
				InnerKeyman6Wrapper.Deactivate();
			}
			catch (Exception e)
			{
				string error = string.Format("There was a problem deactivating keyman 6.\r\n{0}", e);
				Palaso.Reporting.NonFatalErrorDialog.Show(error);
			}
		}

		public static bool HasKeyboardNamed(string name)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return false;
			}

			try
			{
				return InnerKeyman6Wrapper.HasKeyboardNamed(name);
			}
			catch (Exception)
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("There was a problem looking for a keybaord in keyman 6.");
			}
			return false;
		}

		public static string GetActiveKeyboard()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return null;
			}

			try
			{
				return InnerKeyman6Wrapper.GetActiveKeyboard();
			}
			catch (Exception)
			{
				Palaso.Reporting.NonFatalErrorDialog.Show(
					"There was a problem retrieving the active keyboard in keyman 6.");
			}
			return null;
		}
	}
	internal class InnerKeyman6Wrapper
	{
		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardController.KeyboardDescriptor> keyboards =
					new List<KeyboardController.KeyboardDescriptor>();

#if !MONO
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize(false))
				{
					foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in
						keymanLink.Keyboards)
					{
						KeyboardController.KeyboardDescriptor d = new KeyboardController.KeyboardDescriptor();
						d.Name = keyboard.KbdName;
						d.engine = KeyboardController.Engines.Keyman6;
						keyboards.Add(d);
					}
				}
#endif
				return keyboards;
			}
		}

		public static bool EngineAvailable
		{
			get
			{
#if MONO
				return false;
#else
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				return keymanLink.Initialize(false);
#endif
			}
		}

		public static void ActivateKeyboard(string name)
		{
#if !MONO
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
#endif
		}

		public static void Deactivate()
		{
#if !MONO
			KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
			if (keymanLink.Initialize(false))
			{
				keymanLink.SelectKeymanKeyboard(null, false);
			}
#endif
		}

		public static bool HasKeyboardNamed(string name)
		{
#if MONO
			return false;
#else
			KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (!keymanLink.Initialize(false))
				{
					return false;
				}
				foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in keymanLink.Keyboards)
				{
					if (keyboard.KbdName == name)
					{
						return true;
					}
				}
				return false;
#endif
		}

		public static string GetActiveKeyboard()
		{
#if !MONO
			KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
			if (!keymanLink.Initialize(false))
			{
				return null;
			}

			KeymanLink.KeymanLink.KeymanKeyboard keyboard = keymanLink.ActiveKeymanKeyboard();
			if (null != keyboard)
				return keyboard.KbdName;
#endif
			return null;
		}
	}
}