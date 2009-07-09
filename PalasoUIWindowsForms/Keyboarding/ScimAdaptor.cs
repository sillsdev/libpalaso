using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	public class ScimAdaptor
	{
		const int MAXNUMBEROFSUPPORTEDKEYBOARDS = 50;

		public static void ActivateKeyboard(string name)
		{
			if(HasKeyboardNamed(name))
			{
					foreach(ScimPanelControllerWrapper.KeyboardProperties keyboard in GetAvailableKeyboardsAsKeyBoardProperties())
					{
						if(keyboard.name == name)
						{
							if(ScimPanelControllerWrapper.OpenConnectionToScimPanel())
							{
								ScimPanelControllerWrapper.SetKeyboard(keyboard.uuid);
								ScimPanelControllerWrapper.CloseConnectionToScimPanel();
							}
						}
					}
			}
			else
			{
				throw new ArgumentOutOfRangeException("There is no Keyboard with the given name.");
			}
		}

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<ScimPanelControllerWrapper.KeyboardProperties> availableKeyboards = GetAvailableKeyboardsAsKeyBoardProperties();
				return CopyKeyboardPropertiesListIntoKeyBoardDescriptorList(availableKeyboards);
			}
		}

		public static bool EngineAvailable
		{
			get
			{
				bool connectionToScimPanelSucceeded = false;

				if (Environment.OSVersion.Platform != PlatformID.Unix){return false;}

				connectionToScimPanelSucceeded = TryToOpenConnectionToScimPanel();
				ScimPanelControllerWrapper.CloseConnectionToScimPanel();
				return connectionToScimPanelSucceeded;
			}
		}

		public static void Deactivate()
		{
			if(ScimPanelControllerWrapper.OpenConnectionToScimPanel())
			{
				ScimPanelControllerWrapper.SetKeyboard("");
				ScimPanelControllerWrapper.CloseConnectionToScimPanel();
			}
		}

		public static bool HasKeyboardNamed(string name)
		{
			bool keyBoardNameExists = false;
			foreach(ScimPanelControllerWrapper.KeyboardProperties keyboard in GetAvailableKeyboardsAsKeyBoardProperties())
			{
				if(keyboard.name == name)
				{
					keyBoardNameExists = true;
				}
			}
			return keyBoardNameExists;
		}

		public static string GetActiveKeyboard()
		{
			if(!EngineAvailable)
			{
				return "";
			}
			if(ScimPanelControllerWrapper.OpenConnectionToScimPanel()){
				ScimPanelControllerWrapper.KeyboardProperties currentKeyboard = new ScimPanelControllerWrapper.KeyboardProperties();
				ScimPanelControllerWrapper.GetCurrentKeyboard(ref currentKeyboard);
				ScimPanelControllerWrapper.CloseConnectionToScimPanel();
				return currentKeyboard.name;
			}
			else
			{
				return "";
			}
		}

		private static List<ScimPanelControllerWrapper.KeyboardProperties> GetAvailableKeyboardsAsKeyBoardProperties()
		{
			List<ScimPanelControllerWrapper.KeyboardProperties> availableKeyboards = new List<ScimPanelControllerWrapper.KeyboardProperties>();

			if(ScimPanelControllerWrapper.OpenConnectionToScimPanel())
			{
				ScimPanelControllerWrapper.KeyboardProperties[] availableKeyboardsArray =
					new ScimPanelControllerWrapper.KeyboardProperties[MAXNUMBEROFSUPPORTEDKEYBOARDS];
				IntPtr numSupportedKeyboards;

				ScimPanelControllerWrapper.GetListOfSupportedKeyboards(availableKeyboardsArray,
																   availableKeyboardsArray.Length,
																   out numSupportedKeyboards);


				for(int i=0; i<numSupportedKeyboards.ToInt32(); ++i)
				{
					availableKeyboards.Add(availableKeyboardsArray[i]);
				}

				ScimPanelControllerWrapper.CloseConnectionToScimPanel();
			}

			return availableKeyboards;
		}

		private static List<KeyboardController.KeyboardDescriptor> CopyKeyboardPropertiesListIntoKeyBoardDescriptorList(List<ScimPanelControllerWrapper.KeyboardProperties> keyboardProperties)
		{
			List<KeyboardController.KeyboardDescriptor> keyboardDescriptors = new List<KeyboardController.KeyboardDescriptor>();
			foreach(ScimPanelControllerWrapper.KeyboardProperties keyboard in keyboardProperties)
			{
				KeyboardController.KeyboardDescriptor keyboardDescriptor = new KeyboardController.KeyboardDescriptor();
				keyboardDescriptor.Name= keyboard.name;
				keyboardDescriptor.engine = KeyboardController.Engines.Scim;
				keyboardDescriptors.Add(keyboardDescriptor);
			}
			return keyboardDescriptors;
		}

		private static bool TryToOpenConnectionToScimPanel()
		{
			bool connectionToScimPanelSucceeded = false;
			try
			{
				connectionToScimPanelSucceeded = ScimPanelControllerWrapper.OpenConnectionToScimPanel();
			}
			catch(DllNotFoundException dllNotFound)
			{
				connectionToScimPanelSucceeded = false;
				return connectionToScimPanelSucceeded;
			}
			catch(EntryPointNotFoundException entryPointNotFound)
			{
				connectionToScimPanelSucceeded = false;
				return connectionToScimPanelSucceeded;
			}
			return connectionToScimPanelSucceeded;
		}

		private static bool ConnectionToScimPanelISOpen()
		{
			return ScimPanelControllerWrapper.ConnectionToScimPanelIsOpen();
		}

		private class ScimPanelControllerWrapper
		{
			const int MAXSTRINGLENGTH = 100;

			/*
			struct KeyboardProperties
			{
				char uuid[MAXSTRINGLENGTH];
				char name[MAXSTRINGLENGTH];
				char language[MAXSTRINGLENGTH];
				char pathToIcon[MAXSTRINGLENGTH];
			};
			*/
			public struct KeyboardProperties {
				[MarshalAs (UnmanagedType.ByValTStr, SizeConst=MAXSTRINGLENGTH)]
				public string uuid;

				[MarshalAs (UnmanagedType.ByValTStr, SizeConst=MAXSTRINGLENGTH)]
				public string name;

				[MarshalAs (UnmanagedType.ByValTStr, SizeConst=MAXSTRINGLENGTH)]
				public string language;

				[MarshalAs (UnmanagedType.ByValTStr, SizeConst=MAXSTRINGLENGTH)]
				public string pathToIcon;

			 }

			[DllImport ("scimpanelcontroller")]
			public static extern bool OpenConnectionToScimPanel();

			[DllImport ("scimpanelcontroller")]
			public static extern void CloseConnectionToScimPanel();

			[DllImport ("scimpanelcontroller")]
			public static extern bool SetKeyboard(string keyboardToBeSelected);

			[DllImport ("scimpanelcontroller")]
			public static extern bool MarshalTest(string testString);

			[DllImport ("scimpanelcontroller")]
			public static extern int GetCurrentKeyboard(ref KeyboardProperties currentKeyboard);

			[DllImport ("scimpanelcontroller")]
			public static extern bool ConnectionToScimPanelIsOpen();

			[DllImport ("scimpanelcontroller")]
			public static extern int GetListOfSupportedKeyboards
				([In, Out]KeyboardProperties[] supportedKeyboards, int maxNumberOfKeyboards, out IntPtr numberOfReturnedKeyboards);
		}
	}


}
