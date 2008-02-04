using System;
using System.Collections.Generic;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.Keyboarding
{


	internal class Keyman7Adaptor
	{

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				try
				{
					return InnerKeyman7Wrapper.KeyboardDescriptors;
				}
				catch (Exception)
				{
				}
				return new List<KeyboardController.KeyboardDescriptor>();
			}
		}

		public static bool EngineAvailable
		{
			get
			{
				try
				{
					return InnerKeyman7Wrapper.EngineAvailable;
				}
				catch (Exception)
				{
				}
				return false;
			}
		}

		public static void ActivateKeyboard(string name)
		{
		}

		public static void Deactivate()
		{
		}

		public static bool HasKeyboardNamed(string name)
		{
			return false;
		}

		public static string GetActiveKeyboard()
		{
			return null;
		}
	}

	internal class InnerKeyman7Wrapper
	{
		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardController.KeyboardDescriptor> keyboards =
					new List<KeyboardController.KeyboardDescriptor>();
				kmcomapi.TavultesoftKeymanClass keyman = new kmcomapi.TavultesoftKeymanClass();
				foreach (kmcomapi.IKeymanKeyboard keyboard in keyman.Keyboards)
				{
					KeyboardController.KeyboardDescriptor descriptor = new KeyboardController.KeyboardDescriptor();
					descriptor.name = keyboard.Name;
					descriptor.engine = KeyboardController.Engines.Keyman7;
					keyboards.Add(descriptor);
				}
				return keyboards;
			}
		}

		public static bool EngineAvailable
		{
			get { return null != new kmcomapi.TavultesoftKeymanClass(); } //if we were able to load the libarry, we assume it is installed
		}

		public static void ActivateKeyboard(string name)
		{

		}

		public static void Deactivate()
		{

		}
	}
}