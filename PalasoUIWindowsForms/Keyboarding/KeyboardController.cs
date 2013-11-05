using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Palaso.Reporting;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	public class KeyboardController
	{
		static readonly Hashtable s_languagesAlreadyShownKeyBoardNotFoundMessages = new Hashtable();

		public enum Engines
		{
			None = 0,
			Windows = 1,
			Keyman6 = 2,
			Keyman7 = 4,
			Scim = 8,
			IBus = 16,
			All = 255
		} ;

		public class KeyboardDescriptor
		{
			public string ShortName;
			public Engines engine;
			public string Id;
			public string LongName;
			public string Locale;
			public bool Available = true;
		}
		public static List<KeyboardDescriptor> GetAvailableKeyboards(Engines engineKinds)
		{
			List<KeyboardDescriptor> keyboards = new List<KeyboardDescriptor>();

#if MONO
			if ((engineKinds & Engines.IBus) == Engines.IBus)
			{
				keyboards.AddRange(IBusAdaptor.KeyboardDescriptors);
			}
			// Scim no longer supported 2011-01-10 CP
			//if ((engineKinds & Engines.Scim) == Engines.Scim)
			//{
			//    keyboards.AddRange(ScimAdaptor.KeyboardDescriptors);
			//}
#else
			if ((engineKinds & Engines.Windows) == Engines.Windows)
			{
				keyboards.AddRange(WindowsIMEAdaptor.KeyboardDescriptors);
			}
#endif

			return keyboards;
		}

		public static void ActivateKeyboard(string name)
		{
#if MONO
			if (IBusAdaptor.HasKeyboardNamed(name))
			{
				IBusAdaptor.ActivateKeyboard(name);
			}
			// Scim no longer supported 2011-01-10 CP
			//else if (ScimAdaptor.HasKeyboardNamed(name))
			//{
			//    ScimAdaptor.ActivateKeyboard(name);
			//}
#else
			if (WindowsIMEAdaptor.HasKeyboardNamed(name))
			{
				WindowsIMEAdaptor.ActivateKeyboard(name);
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

		public static string GetActiveKeyboard()
		{
#if MONO
			string name = IBusAdaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;

			// Scim no longer supported 2011-01-10 CP
			//name = ScimAdaptor.GetActiveKeyboard();
			//if (!string.IsNullOrEmpty(name))
			//    return name;
#else
			string name = WindowsIMEAdaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;
#endif
			return null;
		}

		public static void DeactivateKeyboard()
		{
#if MONO
			IBusAdaptor.Deactivate();
			// Scim no longer supported 2011-01-10 CP
			//ScimAdaptor.Deactivate();
#else
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
			// Scim no longer supported 2011-01-10 CP
			//if ((engine & Engines.Scim) == Engines.Scim)
			//{
			//    return ScimAdaptor.EngineAvailable;
			//}
#else
			if ((engine & Engines.Windows) == Engines.Windows)
			{
				return WindowsIMEAdaptor.EngineAvailable;
			}
#endif
			Debug.Fail("Unrecognized engine enumeration");
			return false;
		}

		//returns the first keyboard that looks like it handles ipa, or string.empty
		public static string GetIpaKeyboardIfAvailable()
		{
			var keyboard = GetAvailableKeyboards(Engines.All).FirstOrDefault(k => k.Id.ToLower().Contains("ipa"));
			if (keyboard == null)
				return string.Empty;
			return keyboard.Id;
		}
	}
}
