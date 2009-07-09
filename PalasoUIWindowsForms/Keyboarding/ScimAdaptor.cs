using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	public class ScimAdaptor
	{
		const int MAXNUMBEROFSUPPORTEDKEYBOARDS = 50;

		public static void ActivateKeyboard(string name)
		{
			if(HasKeyboardNamed(name))
			{
				ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
				foreach(ScimPanelControllerWrapper.KeyboardProperties keyboard in ScimPanelControllerWrapper.GetListOfSupportedKeyboardsWrapped())
				{
					if(keyboard.name == name)
					{
							ScimPanelControllerWrapper.SetKeyboardWrapped(keyboard.uuid);
					}
				}
				ScimPanelControllerWrapper.CloseConnectionToScimPanelWrapped();
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
				List<ScimPanelControllerWrapper.KeyboardProperties> availableKeyboards = new List<ScimPanelControllerWrapper.KeyboardProperties>();
				ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
				availableKeyboards = ScimPanelControllerWrapper.GetListOfSupportedKeyboardsWrapped();
				ScimPanelControllerWrapper.CloseConnectionToScimPanelWrapped();
				return CopyKeyboardPropertiesListIntoKeyBoardDescriptorList(availableKeyboards);
			}
		}

		public static bool EngineAvailable
		{
			get
			{
				bool connectionToScimPanelSucceeded = false;

				if (Environment.OSVersion.Platform != PlatformID.Unix){return false;}

				try
				{
					ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
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
				catch(IOException ioException)
				{
					connectionToScimPanelSucceeded = false;
					return connectionToScimPanelSucceeded;
				}
				connectionToScimPanelSucceeded = true;
				ScimPanelControllerWrapper.CloseConnectionToScimPanelWrapped();

				return connectionToScimPanelSucceeded;
			}
		}

		public static void Deactivate()
		{
			ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
			ScimPanelControllerWrapper.SetKeyboardWrapped("");
			ScimPanelControllerWrapper.CloseConnectionToScimPanelWrapped();
		}

		public static bool HasKeyboardNamed(string name)
		{
			bool keyBoardNameExists = false;
			ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
			foreach(ScimPanelControllerWrapper.KeyboardProperties keyboard in ScimPanelControllerWrapper.GetListOfSupportedKeyboardsWrapped())
			{
				if(keyboard.name == name)
				{
					keyBoardNameExists = true;
				}
			}
			ScimPanelControllerWrapper.CloseConnectionToScimPanelWrapped();

			return keyBoardNameExists;
		}

		public static string GetActiveKeyboard()
		{
			if(!EngineAvailable)
			{
				return "";
			}

			ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
			ScimPanelControllerWrapper.KeyboardProperties currentKeyboard = ScimPanelControllerWrapper.GetCurrentKeyboardWrapped();
			ScimPanelControllerWrapper.CloseConnectionToScimPanelWrapped();
			return currentKeyboard.name;
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

		private static bool ConnectionToScimPanelISOpen()
		{
			return ScimPanelControllerWrapper.ConnectionToScimPanelIsOpenWrapped();
		}

		private class ScimPanelControllerWrapper
		{
			const int MAXSTRINGLENGTH = 100;

			/*
			This is the unmanaged structure that is being wrapped below.
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


			//These are the methods that wrap the marshalled library calls.
			//These methods provide a prettier C# interface to the C function signatures
			//and also throw appropriate exceptions for potential error codes.
			public static void OpenConnectionToScimPanelWrapped()
			{
				bool connectionOpened = OpenConnectionToScimPanel();
				if(!connectionOpened)
				{
					throw new IOException("Could not open a connection to the Scim Panel!");
				}
			}

			public static void CloseConnectionToScimPanelWrapped()
			{
				return_status = CloseConnectionToScimPanel();
				throwExceptionIfNeeded(return_status);
			}

			public static void SetKeyboardWrapped(string keyboardToBeSelected)
			{
				int return_status = SetKeyboard(keyboardToBeSelected);
				throwExceptionIfNeeded(return_status);
			}

			public static KeyboardProperties GetCurrentKeyboardWrapped()
			{
				KeyboardProperties currentKeyboard = new KeyboardProperties();
				int return_status = GetCurrentKeyboard(ref currentKeyboard);
				throwExceptionIfNeeded(return_status);
				return currentKeyboard;
			}

			public static bool ConnectionToScimPanelIsOpenWrapped()
			{
				return ConnectionToScimPanelIsOpen();
			}

			public static List<KeyboardProperties> GetListOfSupportedKeyboardsWrapped()
			{
				List<KeyboardProperties> availableKeyboards = new List<KeyboardProperties>();

				KeyboardProperties[] availableKeyboardsArray =	new KeyboardProperties[MAXNUMBEROFSUPPORTEDKEYBOARDS];
				IntPtr numSupportedKeyboards;

				int return_status = ScimPanelControllerWrapper.GetListOfSupportedKeyboards(availableKeyboardsArray,
																   availableKeyboardsArray.Length,
																   out numSupportedKeyboards);


				for(int i=0; i<numSupportedKeyboards.ToInt32(); ++i)
				{
					availableKeyboards.Add(availableKeyboardsArray[i]);
				}
				throwExceptionIfNeeded(return_status);
				return availableKeyboards;
			}

			private static void throwExceptionIfNeeded(int status)
			{
				switch(status)
				{
					case 0:
						// all is well!
						break;
					case 1:
						throw new IOException("There was a problem communicating with the Scim Panel! Please make sure Scim is running correctly.");
					case 2:
						throw new IOException("There was a problem communicating with the Scim Panel! We timed out while waiting for a response from Scim.");
					case 3:
						throw new IOException("There was a problem communicating with the Scim Panel! We recieved an incorrect packet from Scim.");
					case 4:
						throw new ArgumentOutOfRangeException("Scim does not contain a keyboard with this name!");
					case 5:
						throw new InvalidOperationException("There is no connection to the Scim Panel! Please call OpenConnectionToScimPanelWrapped before using this method");
					default:
						throw new ApplicationException("An unknown status was returned by the ScimPanelController.");
				}
			}


			//These are the marshalled library calls
			[DllImport ("scimpanelcontroller")]
			private static extern bool OpenConnectionToScimPanel();

			[DllImport ("scimpanelcontroller")]
			private static extern int CloseConnectionToScimPanel();

			[DllImport ("scimpanelcontroller")]
			private static extern int SetKeyboard(string keyboardToBeSelected);

			[DllImport ("scimpanelcontroller")]
			private static extern int GetCurrentKeyboard(ref KeyboardProperties currentKeyboard);

			[DllImport ("scimpanelcontroller")]
			private static extern bool ConnectionToScimPanelIsOpen();

			[DllImport ("scimpanelcontroller")]
			private static extern int GetListOfSupportedKeyboards
				([In, Out]KeyboardProperties[] supportedKeyboards, int maxNumberOfKeyboards, out IntPtr numberOfReturnedKeyboards);
		}
	}


}
