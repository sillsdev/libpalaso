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

		public static void ActivateKeyboard(KeyboardDescriptor keyboard)
		{
			if (keyboard.KeyboardingEngine != Engines.Scim) return;
			ScimPanelController.Singleton.ActivateKeyboard(keyboard.KeyboardName);
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

		public static bool HasKeyboard(KeyboardDescriptor keyboard)
		{
			if (keyboard.KeyboardingEngine != Engines.Scim) return false;
			return ScimPanelController.Singleton.HasKeyboardNamed(keyboard.KeyboardName);
		}

		public static bool HasKeyboardNamed(string name)
		{
			return ScimPanelController.Singleton.HasKeyboardNamed(name);
		}

		public static KeyboardDescriptor GetActiveKeyboardDescriptor()
		{
			KeyboardDescriptor activeKeyboard = null;

			string activeKeyboardName = ScimPanelController.Singleton.GetActiveKeyboard();
			if(!String.IsNullOrEmpty(activeKeyboardName))
			{
				activeKeyboard = new KeyboardDescriptor(activeKeyboardName, Engines.Scim, activeKeyboardName);
			}

			return activeKeyboard;
		}

		public static string GetActiveKeyboard()
		{
			return GetActiveKeyboardDescriptor().KeyboardName;
		}
	}
}