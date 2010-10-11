using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Palaso.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	internal class WindowsIMEAdaptor
	{
		public static void ActivateKeyboard(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return;
			}

			try
			{
				InputLanguage inputLanguage = FindInputLanguage(name);
				if (inputLanguage != null)
				{
					InputLanguage.CurrentInputLanguage = inputLanguage;
				}
				else
				{
					Palaso.Reporting.ProblemNotificationDialog.Show("The keyboard '" + name + "' could not be activated using windows ime.");
				}
			}
			catch (Exception )
			{
				Palaso.Reporting.ProblemNotificationDialog.Show("There was an error trying to access the windows ime.");
			}
		}

		public static bool HasKeyboardNamed(string name)
		{
			return (null != FindInputLanguage(name));
		}

		static private InputLanguage FindInputLanguage(string name)
		{
			if (InputLanguage.InstalledInputLanguages != null) // as is the case on Linux
			{
				foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
				{
					if (l.LayoutName == name)
					{
						return l;
					}
				}
			}
			return null;
		}

		public static List<KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardDescriptor> descriptors = new List<KeyboardDescriptor>();
				try
				{
					foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
					{
						KeyboardDescriptor d = new KeyboardDescriptor(lang.LayoutName, Engines.Windows, lang.Handle.ToString());
						descriptors.Add(d);
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
			get {
				return PlatformID.Win32NT == Environment.OSVersion.Platform
					   || PlatformID.Unix == Environment.OSVersion.Platform;
			}
		}

		static  public void Deactivate()
		{
			try
			{
				InputLanguage.CurrentInputLanguage = InputLanguage.DefaultInputLanguage;
			}
			catch (Exception )
			{
				Palaso.Reporting.ProblemNotificationDialog.Show("There was a problem deactivating windows ime.");
			}
		}

		public static string GetActiveKeyboard()
		{
			try
			{
				InputLanguage lang = InputLanguage.CurrentInputLanguage;
				if (null == lang)
					return null;
				else
					return lang.LayoutName;
			}
			catch (Exception )
			{
				Palaso.Reporting.ProblemNotificationDialog.Show(
					"There was a problem retrieving the active keyboard in from windows ime.");
			}
			return null;
		}
	}
}