using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using Palaso.Keyboarding;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	internal class ScimAdaptor
	{
		public static void ActivateKeyboard(string name)
		{
			ScimPanelController.Singleton.ActivateKeyboard(name);
		}

		public static List<KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				return ScimPanelController.Singleton.KeyboardDescriptors;
			}
		}

		public static bool EngineAvailable
		{
			get
			{
				return ScimPanelController.Singleton.EngineAvailable;
			}
		}

		public static void Deactivate()
		{
			ScimPanelController.Singleton.Deactivate();
		}

		public static bool HasKeyboardNamed(string name)
		{
			return ScimPanelController.Singleton.HasKeyboardNamed(name);
		}

		public static string GetActiveKeyboard()
		{
			return ScimPanelController.Singleton.GetActiveKeyboard();
		}
	}
}