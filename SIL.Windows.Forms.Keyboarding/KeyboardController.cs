// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.ObjectModel;
#if __MonoCS__
using SIL.Windows.Forms.Keyboarding.Linux;
#else
using SIL.Windows.Forms.Keyboarding.Windows;
#endif

namespace SIL.Windows.Forms.Keyboarding
{
	/// <summary>
	/// Singleton class with methods for registering different keyboarding engines (e.g. Windows
	/// system, Keyman, XKB, IBus keyboards), and activating keyboards.
	/// Clients have to call KeyboardController.Initialize() before they can start using the
	/// keyboarding functionality, and they have to call KeyboardController.Shutdown() before
	/// the application or the unit test exits.
	/// </summary>
	public sealed class KeyboardController : IKeyboardController
	{
		#region Static methods and properties

		/// <summary>
		/// The null keyboard description
		/// </summary>
		public static readonly IKeyboardDefinition NullKeyboard = new KeyboardDescriptionNull();

		private static KeyboardController _instance;

		internal static KeyboardController Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// Enables keyboarding. This should be called during application startup.
		/// </summary>
		public static void Initialize(params IKeyboardAdaptor[] adaptors)
		{
			// Note: arguably it is undesirable to install this as the public keyboard controller before we initialize it.
			// However, we have not in general attempted thread safety for the keyboarding code; it seems
			// highly unlikely that any but the UI thread wants to manipulate keyboards. Apart from other threads, nothing
			// has a chance to access this before we return. If initialization does not complete successfully, we clear the
			// global.
			try
			{
				if (_instance != null)
					Shutdown();
				_instance = new KeyboardController();
				Keyboard.Controller = _instance;
				if (adaptors.Length == 0)
					_instance.SetDefaultKeyboardAdaptors();
				else
					_instance.SetKeyboardAdaptors(adaptors);
			}
			catch (Exception)
			{
				Keyboard.Controller = null;
				throw;
			}
		}

		/// <summary>
		/// Ends support for keyboarding.
		/// </summary>
		public static void Shutdown()
		{
			if (_instance == null)
				return;

			_instance.Dispose();
			Keyboard.Controller = null;
		}

		/// <summary>
		/// Returns <c>true</c> if KeyboardController.Initialize() got called before.
		/// </summary>
		public static bool IsInitialized { get { return _instance != null; } }

		/// <summary>
		/// Register the control for keyboarding, optionally providing an event handler for
		/// a keyboarding adapter. If <paramref ref="eventHandler"/> is <c>null</c> the
		/// default handler will be used.
		/// The application should call this method for each control that needs IME input before
		/// displaying the control, typically after calling the InitializeComponent() method.
		/// </summary>
		public static void RegisterControl(Control control, object eventHandler = null)
		{
			_instance._eventHandlers[control] = eventHandler;
			if (_instance.ControlAdded != null)
				_instance.ControlAdded(_instance, new RegisterEventArgs(control, eventHandler));
		}

		/// <summary>
		/// Unregister the control from keyboarding. The application should call this method
		/// prior to disposing the control so that the keyboard adapters can release unmanaged
		/// resources.
		/// </summary>
		public static void UnregisterControl(Control control)
		{
			if (_instance.ControlRemoving != null)
				_instance.ControlRemoving(_instance, new ControlEventArgs(control));
			_instance._eventHandlers.Remove(control);
		}

		#if __MonoCS__
		public static bool CombinedKeyboardHandling
		{
			get { return _instance.Adaptors.Any(a => a is CombinedKeyboardAdaptor); }
		}

		public static bool CinnamonKeyboardHandling
		{
			get { return _instance.Adaptors.Any(a => a is CinnamonIbusAdaptor); }
		}
		#endif

		#endregion

		public event EventHandler<RegisterEventArgs> ControlAdded;
		public event ControlEventHandler ControlRemoving;

		private IKeyboardDefinition _activeKeyboard;
		private readonly List<IKeyboardAdaptor> _adaptors;
		private readonly KeyedList<string, KeyboardDescription> _keyboards;
		private readonly Dictionary<Control, object> _eventHandlers;

		private KeyboardController()
		{
			_keyboards = new KeyedList<string, KeyboardDescription>(kd => kd.Id);
			_eventHandlers = new Dictionary<Control, object>();
			_adaptors = new List<IKeyboardAdaptor>();
		}

		private void SetDefaultKeyboardAdaptors()
		{
			var adaptors = new List<IKeyboardAdaptor>();
#if __MonoCS__
			if (CombinedKeyboardAdaptor.IsRequired)
			{
				adaptors.Add(new CombinedKeyboardAdaptor());
			}
			else if (CinnamonIbusAdaptor.IsRequired)
			{
				adaptors.Add(new CinnamonIbusAdaptor());
			}
			else
			{
				adaptors.Add(new XkbKeyboardAdaptor());
				adaptors.Add(new IbusKeyboardAdaptor());
			}
#else
			adaptors.Add(new WinKeyboardAdaptor());
			adaptors.Add(new KeymanKeyboardAdaptor());
#endif
			SetKeyboardAdaptors(adaptors);
		}

		private void SetKeyboardAdaptors(IEnumerable<IKeyboardAdaptor> adaptors)
		{
			_keyboards.Clear();
			foreach (IKeyboardAdaptor adaptor in _adaptors)
				adaptor.Dispose();
			_adaptors.Clear();
			_adaptors.AddRange(adaptors);
			// this will also populate m_keyboards
			foreach (IKeyboardAdaptor adaptor in _adaptors)
				adaptor.Initialize();
		}

		public void UpdateAvailableKeyboards()
		{
			foreach (IKeyboardAdaptor adapter in _adaptors)
				adapter.UpdateAvailableKeyboards();
		}

		internal IKeyedCollection<string, KeyboardDescription> Keyboards
		{
			get { return _keyboards; }
		}

		internal IList<IKeyboardAdaptor> Adaptors
		{
			get { return _adaptors; }
		}

		internal IDictionary<Control, object> EventHandlers
		{
			get { return _eventHandlers; }
		}

		#region Disposable stuff
		/// <summary/>
		~KeyboardController()
		{
			Dispose(false);
		}

		/// <summary/>
		public bool IsDisposed { get; private set; }

		/// <summary/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary/>
		private void Dispose(bool fDisposing)
		{
			System.Diagnostics.Debug.WriteLineIf(!fDisposing,
				"****** Missing Dispose() call for " + GetType() + ". *******");
			if (fDisposing && !IsDisposed)
			{
				// dispose managed and unmanaged objects
				foreach (IKeyboardAdaptor adaptor in _adaptors)
					adaptor.Dispose();
				_adaptors.Clear();
			}
			IsDisposed = true;
		}
		#endregion

		public IKeyboardDefinition DefaultKeyboard
		{
			get
			{
				KeyboardDescription defaultKeyboard = _adaptors.First(adaptor => adaptor.Type == KeyboardAdaptorType.System).DefaultKeyboard;
#if __MonoCS__
				if (defaultKeyboard == null)
				{
					CinnamonIbusAdaptor cinnamonAdaptor = _instance.Adaptors.OfType<CinnamonIbusAdaptor>().FirstOrDefault();
					if (cinnamonAdaptor != null)
						defaultKeyboard = cinnamonAdaptor.DefaultKeyboard;
				}
#endif
				return defaultKeyboard;
			}
		}

		public IKeyboardDefinition GetKeyboard(string id)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(id, out keyboard))
				return keyboard;
			return NullKeyboard;
		}

		public bool TryGetKeyboard(string id, out IKeyboardDefinition keyboard)
		{
			if (string.IsNullOrEmpty(id))
			{
				keyboard = null;
				return false;
			}

			KeyboardDescription kd;
			if (_keyboards.TryGet(id, out kd))
			{
				keyboard = kd;
				return true;
			}

			string[] parts = id.Split('|');
			if (parts.Length == 2)
			{
				// This is the way Paratext stored IDs in 7.4-7.5 while there was a temporary bug-fix in place)
				keyboard = GetKeyboard(parts[0], parts[1]);
				if (keyboard != NullKeyboard)
					return true;

				keyboard = null;
				return false;
			}

			// Handle old Palaso IDs
			parts = id.Split('-');
			if (parts.Length > 1)
			{
				for (int i = 1; i < parts.Length; i++)
				{
					keyboard = GetKeyboard(string.Join((string) "-", (IEnumerable<string>) parts.Take(i)), string.Join((string) "-", (IEnumerable<string>) parts.Skip(i)));
					if (keyboard != NullKeyboard)
						return true;
				}
			}

			keyboard = null;
			return false;
		}

		public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(layoutName, locale, out keyboard))
				return keyboard;
			return NullKeyboard;
		}

		public bool TryGetKeyboard(string layoutName, string locale, out IKeyboardDefinition keyboard)
		{
			if (string.IsNullOrEmpty(layoutName) && string.IsNullOrEmpty(locale))
			{
				keyboard = null;
				return false;
			}

			keyboard = _keyboards.FirstOrDefault(kd => kd.Layout == layoutName && kd.Locale == locale);
			return keyboard != null;
		}

		public IKeyboardDefinition GetKeyboard(IInputLanguage language)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(language, out keyboard))
				return keyboard;
			return NullKeyboard;
		}

		public bool TryGetKeyboard(IInputLanguage language, out IKeyboardDefinition keyboard)
		{
			// NOTE: on Windows InputLanguage.LayoutName returns a wrong name in some cases.
			// Therefore we need this overload so that we can identify the keyboard correctly.
			keyboard = _keyboards.FirstOrDefault(kd => kd.InputLanguage != null && kd.InputLanguage.Equals(language));
			return keyboard != null;
		}

		public IKeyboardDefinition GetKeyboard(int windowsLcid)
		{
			IKeyboardDefinition keyboard;
			if (TryGetKeyboard(windowsLcid, out keyboard))
				return keyboard;
			return NullKeyboard;
		}

		public bool TryGetKeyboard(int windowsLcid, out IKeyboardDefinition keyboard)
		{
			keyboard = _keyboards.FirstOrDefault(
				kbd =>
				{
					if (kbd.InputLanguage == null || kbd.InputLanguage.Culture == null)
						return false;
					return kbd.InputLanguage.Culture.LCID == windowsLcid;
				});
			return keyboard != null;
		}

		/// <summary>
		/// Activates the keyboard of the default input language
		/// </summary>
		public void ActivateDefaultKeyboard()
		{
#if __MonoCS__
			CinnamonIbusAdaptor cinnamonAdaptor = _instance.Adaptors.OfType<CinnamonIbusAdaptor>().FirstOrDefault();
			if (cinnamonAdaptor != null)
			{
				cinnamonAdaptor.ActivateDefaultKeyboard();
				return;
			}
#endif
			DefaultKeyboard.Activate();
		}

		/// <summary>
		/// Returns everything that is installed on the system and available to be used.
		/// This would typically be used to populate a list of available keyboards in configuring a writing system.
		/// </summary>
		public IEnumerable<IKeyboardDefinition> AvailableKeyboards
		{
			get { return _keyboards.Where(kd => kd.IsAvailable); }
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the ID.
		/// </summary>
		public IKeyboardDefinition CreateKeyboard(string id, KeyboardFormat format, IEnumerable<string> urls)
		{
			KeyboardDescription keyboard;
			if (!_keyboards.TryGet(id, out keyboard))
			{
				keyboard = _adaptors.First(adaptor => adaptor.CanHandleFormat(format)).CreateKeyboardDefinition(id);
				_keyboards.Add(keyboard);
			}

			keyboard.Format = format;
			if (urls != null)
			{
				foreach (string url in urls)
				{
					keyboard.Urls.Add(url);
				}
			}
			return keyboard;
		}

		/// <summary>
		/// Gets or sets the currently active keyboard
		/// </summary>
		public IKeyboardDefinition ActiveKeyboard
		{
			get
			{
				if (_activeKeyboard == null)
				{
					try
					{
						InputLanguage lang = InputLanguage.CurrentInputLanguage;
						_activeKeyboard = GetKeyboard(lang.LayoutName, lang.Culture.Name);
					}
					catch (CultureNotFoundException)
					{
					}
					if (_activeKeyboard == null)
						_activeKeyboard = NullKeyboard;
				}
				return _activeKeyboard;
			}
			set { _activeKeyboard = value; }
		}
	}
}
