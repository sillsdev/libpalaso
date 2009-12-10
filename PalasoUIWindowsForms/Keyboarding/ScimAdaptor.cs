using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	internal class ScimAdaptor
	{
		//As of 3-Aug-2009 mono has a bug where the X input context is actually switched
		//AFTER the OnEnter and OnFocused events are fired. Thus switching the keyboard
		//in the respective event handlers actually switches it for the LAST context
		//which naturally leads to some unexpected behavior.
		//As a result we instead start a timer that waits 1 ms and then fires an event whose
		//handler then actually does the switching.
		public static void ActivateKeyboard(string name)
		{
			ScimPanelController.Singleton.ActivateKeyboard(name);
		}

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
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