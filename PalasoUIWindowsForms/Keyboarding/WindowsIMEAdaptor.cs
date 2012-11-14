using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Reporting;

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
					ProblemNotificationDialog.Show("The keyboard '" + name + "' could not be activated using windows ime.");
				}
			}
			catch (Exception )
			{
				ProblemNotificationDialog.Show("There was an error trying to access the windows ime.");
			}
		}

		public static bool HasKeyboardNamed(string name)
		{
			return (null != FindInputLanguage(name));
		}

		static private InputLanguage FindInputLanguage(string name)
		{
			var split = name.Split(new[]{'-'}, 2);
			string layoutName = split[0];
			string localeName = split.Length >= 2 ? split[1] : String.Empty;
			var possibles = new List<InputLanguage>();
			if (InputLanguage.InstalledInputLanguages != null) // as is the case on Linux
			{
				foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
				{
					if (l.LayoutName == layoutName)
					{
						if (l.Culture.IetfLanguageTag == localeName)
						{
							return l;
						}
						possibles.Add(l);
					}
				}
			}
			if (possibles.Count > 0)
			{
				return possibles[0];
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
						//descriptor.Id = lang.LayoutName;
						descriptor.Id = MakeDescriptorId(lang);
						descriptor.LongName = String.Format("{0} - {1}", lang.LayoutName, lang.Culture.DisplayName);
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

		private static string MakeDescriptorId(InputLanguage lang)
		{
			return String.Format("{0}-{1}", lang.LayoutName, lang.Culture.IetfLanguageTag);
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
				ProblemNotificationDialog.Show("There was a problem deactivating windows ime.");
			}
		}

		public static string GetActiveKeyboard()
		{
			try
			{
				InputLanguage lang = InputLanguage.CurrentInputLanguage;
				if (null == lang)
				{
					return null;
				}
				return MakeDescriptorId(lang);
			}
			catch (Exception )
			{
				ProblemNotificationDialog.Show(
					"There was a problem retrieving the active keyboard in from windows ime."
				);
			}
			return null;
		}
	}
}