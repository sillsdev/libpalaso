using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
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

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				var descriptors = new List<KeyboardController.KeyboardDescriptor>();
				try
				{
					foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
					{
						var descriptor = new KeyboardController.KeyboardDescriptor();
						descriptor.ShortName = lang.LayoutName;
						// The below might be better, but really the layout is the thing, and we can't select a
						// particular layout from a locale which has multiple layouts.  Also, different users
						// may have the layout, but under a different locale, so just recording the layoutname as the
						// Id allows us to work with that.
						//descriptor.Id = String.Format("{0}-{1}", lang.Culture.IetfLanguageTag, lang.LayoutName);
						descriptor.Id = lang.LayoutName;
						descriptor.LongName = String.Format("{0} - {1}", lang.Culture.DisplayName, lang.LayoutName);
						descriptor.engine = KeyboardController.Engines.Windows;
						if (descriptors.Find(a => a.Id == descriptor.Id) == null)
						{
							descriptors.Add(descriptor);
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