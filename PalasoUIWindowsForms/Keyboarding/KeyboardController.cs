using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Palaso.Keyboarding;
using Palaso.Reporting;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	public class KeyboardController
	{
		static readonly Hashtable s_languagesAlreadyShownKeyBoardNotFoundMessages = new Hashtable();

		public static List<KeyboardDescriptor> GetAvailableKeyboards(Engines engineKinds)
		{
			List<KeyboardDescriptor> keyboards = new List<KeyboardDescriptor>();

#if MONO
			if ((engineKinds & Engines.IBus) == Engines.IBus)
			{
				keyboards.AddRange(IBusAdaptor.KeyboardDescriptors);
			}
			if ((engineKinds & Engines.Scim) == Engines.Scim)
			{
				keyboards.AddRange(ScimAdaptor.KeyboardDescriptors);
			}
#else
			if ((engineKinds & Engines.Windows) == Engines.Windows)
			{
				keyboards.AddRange(WindowsIMEAdaptor.KeyboardDescriptors);
			}
			if ((engineKinds & Engines.Keyman6) == Engines.Keyman6)
			{
				keyboards.AddRange(Keyman6Adaptor.KeyboardDescriptors);
			}
			if ((engineKinds & Engines.Keyman7) == Engines.Keyman7)
			{
				keyboards.AddRange(Keyman7Adaptor.KeyboardDescriptors);
			}
#endif

			return keyboards;
		}

		public static void ActivateKeyboard(KeyboardDescriptor keyboard)
		{
#if MONO
			if (IBusAdaptor.HasKeyboard(keyboard))
			{
				IBusAdaptor.ActivateKeyboard(keyboard);
			}
			else if (ScimAdaptor.HasKeyboard(keyboard))
			{
				ScimAdaptor.ActivateKeyboard(keyboard);
			}
#else
			if (WindowsIMEAdaptor.HasKeyboard(keyboard))
			{
				WindowsIMEAdaptor.ActivateKeyboard(keyboard);
			}
			else if (Keyman6Adaptor.HasKeyboard(keyboard))
			{
				Keyman6Adaptor.ActivateKeyboard(keyboard);
			}
			else if (Keyman7Adaptor.HasKeyboard(keyboard))
			{
				Keyman7Adaptor.ActivateKeyboard(keyboard);
			}
#endif
			else
			{
				if (!(s_languagesAlreadyShownKeyBoardNotFoundMessages.Contains(keyboard)))
				{
					s_languagesAlreadyShownKeyBoardNotFoundMessages.Add(keyboard, null);
					ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
						"Could not find a keyboard named '{0}' in the {1} engine.", keyboard.KeyboardName, keyboard.KeyboardingEngine);

				}
			}

		}

		[Obsolete("Using keyboard descriptors rather than keyboard names will result in more accurate error messages and avoid a improve keyboard control across localized versions of windows.")]
		public static void ActivateKeyboard(string name)
		{
#if MONO
			if (IBusAdaptor.HasKeyboardNamed(name))
			{
				IBusAdaptor.ActivateKeyboard(name);
			}
			else if (ScimAdaptor.HasKeyboardNamed(name))
			{
				ScimAdaptor.ActivateKeyboard(name);
			}
#else
			if (WindowsIMEAdaptor.HasKeyboardNamed(name))
			{
				WindowsIMEAdaptor.ActivateKeyboard(name);
			}
			else if (Keyman6Adaptor.HasKeyboardNamed(name))
			{
				Keyman6Adaptor.ActivateKeyboard(name);
			}
			else if (Keyman7Adaptor.HasKeyboardNamed(name))
			{
				Keyman7Adaptor.ActivateKeyboard(name);
			}
#endif
			else
			{
				if (!(s_languagesAlreadyShownKeyBoardNotFoundMessages.Contains(name)))
				{
					s_languagesAlreadyShownKeyBoardNotFoundMessages.Add(name, null);
					ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
						"Could not find a keyboard ime that had a keyboard named '{0}'", name);

				}
			}

		}

		[Obsolete("Using keyboard descriptors rather than keyboard names will result in more accurate error messages and avoid a improve keyboard control across localized versions of windows. Please use GetActiveKeyboardDescriptor() instead.")]
		public static string GetActiveKeyboard()
		{
#if MONO
			string name = IBusAdaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;

			name = ScimAdaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;
#else
			string name = Keyman6Adaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;

			name = Keyman7Adaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;

			name = WindowsIMEAdaptor.GetActiveKeyboardDescriptor().KeyboardName;
			if (!string.IsNullOrEmpty(name))
				return name;
#endif
			return null;
		}

		public static KeyboardDescriptor GetActiveKeyboardDescriptor()
		{
#if MONO
			KeyboardDescriptor keyboard = IBusAdaptor.GetActiveKeyboardDescriptor();
			if (keyboard != null)
				return keyboard;

			keyboard = ScimAdaptor.GetActiveKeyboardDescriptor();
			if (keyboard != null)
				return keyboard;
#else
			KeyboardDescriptor keyboard = Keyman6Adaptor.GetActiveKeyboardDescriptor();
			if (keyboard != null)
				return keyboard;

			keyboard = Keyman7Adaptor.GetActiveKeyboardDescriptor();
			if (keyboard != null)
				return keyboard;

			keyboard = WindowsIMEAdaptor.GetActiveKeyboardDescriptor();
			if (keyboard != null)
				return keyboard;
#endif
			return null;
		}

		public static void DeactivateKeyboard()
		{
#if MONO
			IBusAdaptor.Deactivate();
			ScimAdaptor.Deactivate();
#else
			Keyman6Adaptor.Deactivate();
			Keyman7Adaptor.Deactivate();
			WindowsIMEAdaptor.Deactivate();
#endif
		}

		public static bool EngineAvailable(Engines engine)
		{
#if MONO
			if ((engine & Engines.IBus) == Engines.IBus)
			{
				return IBusAdaptor.EngineAvailable;
			}
			if ((engine & Engines.Scim) == Engines.Scim)
			{
				return ScimAdaptor.EngineAvailable;
			}
#else
			if ((engine & Engines.Windows) == Engines.Windows)
			{
				return WindowsIMEAdaptor.EngineAvailable;
			}
			if ((engine & Engines.Keyman6) == Engines.Keyman6)
			{
				return Keyman6Adaptor.EngineAvailable;
			}
			if ((engine & Engines.Keyman7) == Engines.Keyman7)
			{
				return Keyman7Adaptor.EngineAvailable;
			}
#endif
			Debug.Fail("Unrecognized engine enumeration");
			return false;
		}

		//returns the first keyboard that looks like it handles ipa, or string.empty
		public static string GetIpaKeyboardIfAvailable()
		{
			var keyboard = GetAvailableKeyboards(Engines.All).FirstOrDefault(k => k.KeyboardName.ToLower().Contains("ipa"));
			if (keyboard == null)
				return string.Empty;
			return keyboard.KeyboardName;
		}
	}
}
