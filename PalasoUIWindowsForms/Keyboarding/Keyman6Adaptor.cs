using System;
using System.Collections.Generic;
using System.Diagnostics;
using Palaso.Keyboarding;
using Palaso.Reporting;

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
			catch (ErrorReport.ProblemNotificationSentToUserException)
			{
				throw; // needed for tests to know that a message box would have been shown
			}
			catch (Exception)
			{
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), "The keyboard '{0}' could not be activated using Keyman 6.", name);
			}
		}

		public static void ActivateKeyboard(KeyboardDescriptor keyboard)
		{
			if(keyboard.KeyboardingEngine != Engines.Keyman6) return;
			ActivateKeyboard(keyboard.KeyboardName);
		}

		public static List<KeyboardDescriptor> KeyboardDescriptors
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
				return new List<KeyboardDescriptor>();
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
			catch (AccessViolationException)
			{
				// review: Maybe we have a problem with the compatability if the interop with some versions of keyman.
				// for now, discard this exception.
				//Palaso.Reporting.NonFatalErrorDialog.Show("There was a problem deactivating keyman 6.");
			}
			catch (Exception)
			{
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), "The keyboard could not be deactivated using Keyman 6.");

				// review: When in Rome...
				Palaso.Reporting.ProblemNotificationDialog.Show("There was a problem deactivating keyman 6.");
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
				Palaso.Reporting.ProblemNotificationDialog.Show("There was a problem looking for a keybaord in keyman 6.");
			}
			return false;
		}

		public static bool HasKeyboard(KeyboardDescriptor keyboard)
		{
			if(keyboard.KeyboardingEngine != Engines.Keyman6) return false;
			return HasKeyboardNamed(keyboard.KeyboardName);
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
				Palaso.Reporting.ProblemNotificationDialog.Show(
					"There was a problem retrieving the active keyboard in keyman 6.");
			}
			return null;
		}

		public static KeyboardDescriptor GetActiveKeyboardDescriptor()
		{
			KeyboardDescriptor activeKeyboard = null;
			string activeKeyboardName = GetActiveKeyboard();
			if(!String.IsNullOrEmpty(activeKeyboardName))
			{
				return new KeyboardDescriptor(activeKeyboardName, Engines.Keyman6, activeKeyboardName);
			}
			return activeKeyboard;
		}
	}
	internal class InnerKeyman6Wrapper
	{
		public static List<KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardDescriptor> keyboards =
					new List<KeyboardDescriptor>();

#if !MONO
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize(false))
				{
					foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in
						keymanLink.Keyboards)
					{
						KeyboardDescriptor d = new KeyboardDescriptor(keyboard.KbdName, Engines.Keyman6, keyboard.KbdName);
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
				Palaso.Reporting.ProblemNotificationDialog.Show("Keyman6 could not be activated.");
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