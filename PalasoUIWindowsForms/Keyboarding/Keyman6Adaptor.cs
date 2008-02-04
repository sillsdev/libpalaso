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

			try
			{
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (!keymanLink.Initialize(false))
				{
					Palaso.Reporting.NonFatalErrorDialog.Show("Keyman6 could not be activated.");
					return;
				}
				keymanLink.SelectKeymanKeyboard(name, true);

				//fail fast if that didn't work
				KeymanLink.KeymanLink.KeymanKeyboard keyboard = keymanLink.ActiveKeymanKeyboard();
				if (keyboard.KbdName != name)
				{
					throw new ApplicationException();
				}
			}
			catch (Exception err)
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("The keyboard '" + name + "' could not be activated using Keyman 6.");
			}
		}

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardController.KeyboardDescriptor> descriptors = new List<KeyboardController.KeyboardDescriptor>();
				try
				{
					KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
					if (keymanLink.Initialize(false))
					{
						foreach (KeymanLink.KeymanLink.KeymanKeyboard keyboard in
							keymanLink.Keyboards)
						{
							KeyboardController.KeyboardDescriptor d = new KeyboardController.KeyboardDescriptor();
							d.name = keyboard.KbdName;
							d.engine = KeyboardController.Engines.Keyman6;
							descriptors.Add(d);
						}
					}
				}
				catch (Exception err)
				{
					Debug.Fail(err.Message);
				}
				return descriptors;
			}
		}

		public static bool EngineAvailable
		{
			get
			{
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				return keymanLink.Initialize(false);
			}
		}

		static public void Deactivate()
		{
			try
			{
				KeymanLink.KeymanLink keymanLink = new KeymanLink.KeymanLink();
				if (keymanLink.Initialize(false))
				{
					keymanLink.SelectKeymanKeyboard(null, false);
				}
			}
			catch (Exception err)
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("There was a problem deactivating keyman 6.");
			}
		}

		public static bool HasKeyboardNamed(string name)
		{
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
			catch (Exception err)
			{
				Palaso.Reporting.NonFatalErrorDialog.Show("There was a problem looking for a keybaord in keyman 6.");
			}
			return false;
		}

		public static string GetActiveKeyboard()
		{
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
			catch (Exception err)
			{
				Palaso.Reporting.NonFatalErrorDialog.Show(
					"There was a problem retrieving the active keyboard in keyman 6.");
			}
			return null;
		}
	}
}