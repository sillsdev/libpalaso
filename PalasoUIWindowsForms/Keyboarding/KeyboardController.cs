using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
			All = 255
		} ;

		public class KeyboardDescriptor
		{
			public string Name;
			public Engines engine;
		}
		public static List<KeyboardDescriptor> GetAvailableKeyboards(Engines engineKinds)
		{
			List<KeyboardDescriptor> keyboards = new List<KeyboardDescriptor>();

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

			return keyboards;
		}

		public static void ActivateKeyboard(string name)
		{
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
			else
			{
				if (!(s_languagesAlreadyShownKeyBoardNotFoundMessages.Contains(name)))
				{
					s_languagesAlreadyShownKeyBoardNotFoundMessages.Add(name, null);
					Palaso.Reporting.ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
						"Could not find a keyboard ime that had a keyboard named '{0}'", name);

				}
			}

		}

		public static string GetActiveKeyboard()
		{
			string name = Keyman6Adaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;

			name = Keyman7Adaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;

			name = WindowsIMEAdaptor.GetActiveKeyboard();
			if (!string.IsNullOrEmpty(name))
				return name;

			return null;
		}

		public static void DeactivateKeyboard()
		{
			Keyman6Adaptor.Deactivate();
			Keyman7Adaptor.Deactivate();
			WindowsIMEAdaptor.Deactivate();
		}

		public static bool EngineAvailable(Engines engine)
		{
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
			Debug.Fail("Unrecognized engine enumeration");
			return false;
		}
	}
}