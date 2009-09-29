using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	internal class ScimPanelController
	{
		//This nested class is part of the singleton pattern as implemented at
		//http://www.yoda.arachsys.com/csharp/singleton.html
		//This implementation supplies thread safety and lazy loading
		class Nested
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static Nested()
			{
			}

			internal static readonly ScimPanelController singleton = new ScimPanelController();
		}


		private const int MAXNUMBEROFSUPPORTEDKEYBOARDS = 50;
		private static Timer _timerToOvercomeContextSwitchingBug = new Timer();
		static string _keyboardNameForTimerToSwitchTo = "";

		public struct ContextInfo {
			public int frontendClient;
			public int context;
		};

		public static ScimPanelController Singleton
		{
			get
			{
				return Nested.singleton;
			}
		}



		private ScimPanelController()
		{
			_timerToOvercomeContextSwitchingBug.Interval = 1;
			_timerToOvercomeContextSwitchingBug.Tick += TimerActivateKeyboard;
		}

		private void OpenConnectionIfNecassary()
		{
			if(!ConnectionToScimPanelIsOpen())
			{
				ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
			}
		}

		//As of 3-Aug-2009 mono has a bug where the X input context is actually switched
		//AFTER the OnEnter and OnFocused events are fired. Thus switching the keyboard
		//in the respective event handlers actually switches it for the LAST context
		//which naturally leads to some unexpected behavior.
		//As a result we instead start a timer that waits 1 ms and then fires an event whose
		//handler then actually does the switching.
		public void ActivateKeyboard(string name)
		{
			if(!HasKeyboardNamed(name))
			{
				throw new ArgumentOutOfRangeException("Scim does not have a Keyboard with that name!" + name);
			}
			_keyboardNameForTimerToSwitchTo = name;
			if(!_timerToOvercomeContextSwitchingBug.Enabled)
			{
				_timerToOvercomeContextSwitchingBug.Start();
			}
		}

		public void TimerActivateKeyboard(Object sender, EventArgs e)
		{
			_timerToOvercomeContextSwitchingBug.Stop();
			if(HasKeyboardNamed(_keyboardNameForTimerToSwitchTo))
			{
				ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
				foreach(ScimPanelControllerWrapper.KeyboardProperties keyboard in ScimPanelControllerWrapper.GetListOfSupportedKeyboardsWrapped())
				{
					if(keyboard.name == _keyboardNameForTimerToSwitchTo)
					{
							ScimPanelControllerWrapper.SetKeyboardWrapped(keyboard.uuid);
					}
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException("Scim does not have a Keyboard with that name!" + _keyboardNameForTimerToSwitchTo);
			}
		}

		public List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardController.KeyboardDescriptor> availableKeyboardsAsDescriptors = new List<KeyboardController.KeyboardDescriptor>();
				List<ScimPanelControllerWrapper.KeyboardProperties> availableKeyboards = new List<ScimPanelControllerWrapper.KeyboardProperties>();

				try {OpenConnectionIfNecassary();}
				catch{return availableKeyboardsAsDescriptors;}

				availableKeyboards = ScimPanelControllerWrapper.GetListOfSupportedKeyboardsWrapped();
				availableKeyboardsAsDescriptors = CopyKeyboardPropertiesListIntoKeyBoardDescriptorList(availableKeyboards);
				return availableKeyboardsAsDescriptors;
			}
		}

		public bool EngineAvailable
		{
			get
			{
				return ScimIsRunning;
			}
		}

		public void Deactivate()
		{
			try {OpenConnectionIfNecassary();}
			catch{return;}
			ScimPanelControllerWrapper.SetKeyboardWrapped("");
		}

		public bool HasKeyboardNamed(string name)
		{
			bool keyBoardNameExists = false;

			try {OpenConnectionIfNecassary();}
			catch{return false;	}

			foreach(ScimPanelControllerWrapper.KeyboardProperties keyboard in ScimPanelControllerWrapper.GetListOfSupportedKeyboardsWrapped())
			{
				if(keyboard.name == name)
				{
					keyBoardNameExists = true;
				}
			}

			return keyBoardNameExists;
		}

		public string GetActiveKeyboard()
		{
			try {OpenConnectionIfNecassary();}
			catch{return "";}

			ScimPanelControllerWrapper.KeyboardProperties currentKeyboard = ScimPanelControllerWrapper.GetCurrentKeyboardWrapped();
			return currentKeyboard.name;
		}

		public ContextInfo GetCurrentInputContext()
		{
			try {OpenConnectionIfNecassary();}
			catch{throw new InvalidOperationException("Scim does not seem to be running! Please turn on Scim!");}

			ScimPanelControllerWrapper.OpenConnectionToScimPanelWrapped();
			ContextInfo currentContext;
			ScimPanelControllerWrapper.ContextInfo currentContextFromWrapper =
				ScimPanelControllerWrapper.GetCurrentInputContextWrapped();
			currentContext.frontendClient = currentContextFromWrapper.frontendClient;
			currentContext.context = currentContextFromWrapper.context;
			return currentContext;
		}

		private List<KeyboardController.KeyboardDescriptor> CopyKeyboardPropertiesListIntoKeyBoardDescriptorList(List<ScimPanelControllerWrapper.KeyboardProperties> keyboardProperties)
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

		private bool ConnectionToScimPanelIsOpen()
		{
			return ScimPanelControllerWrapper.ConnectionToScimPanelIsOpenWrapped();
		}

		private bool ScimIsRunning
		{
			get
			{
				bool connectionToScimPanelSucceeded = false;

				if (Environment.OSVersion.Platform != PlatformID.Unix){return false;}

				if(ConnectionToScimPanelIsOpen()){return true;}

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

			public struct ContextInfo {
				public int frontendClient;
				public int context;
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
				int return_status = CloseConnectionToScimPanel();
				throwExceptionIfNeeded(return_status);
			}

			public static void SetKeyboardWrapped(string keyboardToBeSelected)
			{
				int return_status = SetKeyboard(keyboardToBeSelected);
				throwExceptionIfNeeded(return_status);
			}

			public static KeyboardProperties GetCurrentKeyboardWrapped()
			{
				KeyboardProperties currentKeyboard;
				int return_status = GetCurrentKeyboard(out currentKeyboard);
				throwExceptionIfNeeded(return_status);
				return currentKeyboard;
			}

			public static bool ConnectionToScimPanelIsOpenWrapped()
			{
				return ConnectionToScimPanelIsOpen();
			}

			public static ContextInfo GetCurrentInputContextWrapped()
			{
				ContextInfo currentContext;
				int return_status = GetCurrentInputContext(out currentContext);
				throwExceptionIfNeeded(return_status);
				return currentContext;
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
					case 6:
						throw new IOException("There was a problem communicating with the Scim Panel! We recieved a corrupted packet from Scim.");
					case 7:
						throw new InvalidOperationException("No input context focused. You can not change the keyboard unless the current window has an input context.");
					default:
						throw new ApplicationException("An unknown status was returned by the ScimPanelController.");
				}
			}


			//These are the marshalled library calls
			[DllImport ("scimpanelcontroller-1.0")]
			private static extern bool OpenConnectionToScimPanel();

			[DllImport ("scimpanelcontroller-1.0")]
			private static extern int CloseConnectionToScimPanel();

			[DllImport ("scimpanelcontroller-1.0")]
			private static extern int SetKeyboard(string keyboardToBeSelected);

			[DllImport ("scimpanelcontroller-1.0")]
			private static extern int GetCurrentKeyboard(out KeyboardProperties currentKeyboard);

			[DllImport ("scimpanelcontroller-1.0")]
			private static extern bool ConnectionToScimPanelIsOpen();

			[DllImport ("scimpanelcontroller-1.0")]
			private static extern int GetListOfSupportedKeyboards
				([In, Out]KeyboardProperties[] supportedKeyboards, int maxNumberOfKeyboards, out IntPtr numberOfReturnedKeyboards);

			[DllImport ("scimpanelcontroller-1.0")]
			private static extern int GetCurrentInputContext (out ContextInfo currentContext);
		}
	}


}
