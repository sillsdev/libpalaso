// Copyright (c) 2011-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.ObjectModel;
using SIL.PlatformUtilities;
using SIL.Reporting;
#if NETFRAMEWORK
using SIL.Windows.Forms.Keyboarding.Linux;
#endif
using SIL.Windows.Forms.Keyboarding.Windows;

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
		public static readonly KeyboardDescription NullKeyboard = new NullKeyboardDescription();
		internal static KeyboardController Instance { get; private set; }

		/// <summary>
		/// Enables keyboarding. This should be called during application startup.
		/// </summary>
		public static void Initialize(params IKeyboardRetrievingAdaptor[] adaptors)
		{
			// Note: arguably it is undesirable to install this as the public keyboard controller before we initialize it.
			// However, we have not in general attempted thread safety for the keyboarding code; it seems
			// highly unlikely that any but the UI thread wants to manipulate keyboards. Apart from other threads, nothing
			// has a chance to access this before we return. If initialization does not complete successfully, we clear the
			// global.
			try
			{
				if (Instance != null)
					Shutdown();
				Instance = new KeyboardController();
				Keyboard.Controller = Instance;
				if (adaptors.Length == 0)
					Instance.SetDefaultKeyboardAdaptors();
				else
					Instance.SetKeyboardAdaptors(adaptors);
			}
			catch (Exception e)
			{
				Console.WriteLine("Got exception {0} initializing keyboard controller", e.GetType());
				Console.WriteLine(e.StackTrace);
				Logger.WriteEvent("Got exception {0} initializing keyboard controller", e.GetType());
				Logger.WriteEvent(e.StackTrace);

				if (Keyboard.Controller != null)
					Keyboard.Controller.Dispose();
				Keyboard.Controller = null;
				throw;
			}
		}

		/// <summary>
		/// Ends support for keyboarding.
		/// </summary>
		public static void Shutdown()
		{
			if (Instance == null)
				return;

			Instance.Dispose();
			Keyboard.Controller = null;
		}

		/// <summary>
		/// Returns <c>true</c> if KeyboardController.Initialize() got called before.
		/// </summary>
		public static bool IsInitialized => Instance != null;

		/// <summary>
		/// Register the control for keyboarding, optionally providing an event handler for
		/// a keyboarding adapter. If <paramref ref="eventHandler"/> is <c>null</c> the
		/// default handler will be used.
		/// The application should call this method for each control that needs IME input before
		/// displaying the control, typically after calling the InitializeComponent() method.
		/// </summary>
		public static void RegisterControl(Control control, object eventHandler = null)
		{
			Instance._eventHandlers[control] = eventHandler;
			Instance.ControlAdded?.Invoke(Instance, new RegisterEventArgs(control, eventHandler));
		}

		/// <summary>
		/// Unregister the control from keyboarding. The application should call this method
		/// prior to disposing the control so that the keyboard adapters can release unmanaged
		/// resources.
		/// </summary>
		public static void UnregisterControl(Control control)
		{
			Instance.ControlRemoving?.Invoke(Instance, new ControlEventArgs(control));
			Instance._eventHandlers.Remove(control);
		}
		/// <summary/>
		/// <returns>An action that will bring up the keyboard setup application dialog</returns>
		public static Action GetKeyboardSetupApplication()
		{
			Action program = null;
			if (!HasSecondaryKeyboardSetupApplication && Instance.Adaptors.ContainsKey(KeyboardAdaptorType.OtherIm))
				program = Instance.Adaptors[KeyboardAdaptorType.OtherIm].GetKeyboardSetupAction();

			return program ?? Instance.Adaptors[KeyboardAdaptorType.System].GetKeyboardSetupAction();
		}

		public static Action GetSecondaryKeyboardSetupApplication()
		{
			Action program = null;
			if (HasSecondaryKeyboardSetupApplication && Instance.Adaptors.ContainsKey(KeyboardAdaptorType.OtherIm))
				program = Instance.Adaptors[KeyboardAdaptorType.OtherIm].GetSecondaryKeyboardSetupAction();

			return program;
		}

		/// <summary>
		/// Returns <c>true</c> if there is a secondary keyboard application available, e.g.
		/// Keyman setup dialog on Windows.
		/// </summary>
		public static bool HasSecondaryKeyboardSetupApplication =>
			Instance.Adaptors.ContainsKey(KeyboardAdaptorType.OtherIm) &&
			Instance.Adaptors[KeyboardAdaptorType.OtherIm].IsSecondaryKeyboardSetupApplication;

		// delegate used to detect input processor, like KeyMan
		private delegate bool IsUsingInputProcessorDelegate();
		private static IsUsingInputProcessorDelegate _isUsingInputProcessor;

		/// <summary>
		/// Returns true if the current input device is an Input Processor, like KeyMan.
		/// </summary>
		public static bool IsFormUsingInputProcessor(Form frm)
		{
			bool usingIP;
			if (frm.InvokeRequired)
			{
				// Set up a delegate for the invoke
				if (_isUsingInputProcessor == null)
					_isUsingInputProcessor = IsUsingInputProcessor;

				usingIP = (bool)frm.Invoke(_isUsingInputProcessor);
			}
			else
			{
				usingIP = IsUsingInputProcessor();
			}

			return usingIP;
		}


		private static bool IsUsingInputProcessor()
		{
			if (!Platform.IsWindows)
			{
				// not yet implemented on Linux
				return false;
			}

			TfInputProcessorProfilesClass inputProcessor;
			try
			{
				inputProcessor = new TfInputProcessorProfilesClass();
			}
			catch (InvalidCastException)
			{
				return false;
			}

			var profileMgr = inputProcessor as ITfInputProcessorProfileMgr;

			if (profileMgr == null) return false;

			var profile = profileMgr.GetActiveProfile(ref Guids.Consts.TfcatTipKeyboard);
			return profile.ProfileType == TfProfileType.InputProcessor;
		}

		#endregion

		public event EventHandler<RegisterEventArgs> ControlAdded;
		public event ControlEventHandler ControlRemoving;

		private IKeyboardDefinition _activeKeyboard;
		private readonly Dictionary<KeyboardAdaptorType, IKeyboardRetrievingAdaptor> _adaptors;
		private readonly KeyedList<string, KeyboardDescription> _keyboards;
		private readonly Dictionary<Control, object> _eventHandlers;

		private KeyboardController()
		{
			_keyboards = new KeyedList<string, KeyboardDescription>(kd => kd.Id);
			_eventHandlers = new Dictionary<Control, object>();
			_adaptors = new Dictionary<KeyboardAdaptorType, IKeyboardRetrievingAdaptor>();
		}

		private void SetDefaultKeyboardAdaptors()
		{
			SetKeyboardAdaptors(
				Platform.IsWindows
					? new IKeyboardRetrievingAdaptor[]
					{
						new WinKeyboardAdaptor(), new KeymanKeyboardAdaptor()
					}
					: new IKeyboardRetrievingAdaptor[]
					{
#if NETFRAMEWORK
						new XkbKeyboardRetrievingAdaptor(), new IbusKeyboardRetrievingAdaptor(),
						new UnityXkbKeyboardRetrievingAdaptor(), new UnityIbusKeyboardRetrievingAdaptor(),
						new CombinedIbusKeyboardRetrievingAdaptor(),
						new GnomeShellIbusKeyboardRetrievingAdaptor()
#endif
					}
			);
		}

		private void SetKeyboardAdaptors(IKeyboardRetrievingAdaptor[] adaptors)
		{
			_keyboards.Clear();
			foreach (var adaptor in _adaptors.Values)
				adaptor.Dispose();
			_adaptors.Clear();

			foreach (var adaptor in adaptors)
			{
				if (!adaptor.IsApplicable)
					continue;

				if ((adaptor.Type & KeyboardAdaptorType.System) == KeyboardAdaptorType.System)
					_adaptors[KeyboardAdaptorType.System] = adaptor;
				if ((adaptor.Type & KeyboardAdaptorType.OtherIm) == KeyboardAdaptorType.OtherIm)
					_adaptors[KeyboardAdaptorType.OtherIm] = adaptor;
			}

			foreach (var adaptor in adaptors)
			{
				if (!_adaptors.ContainsValue(adaptor))
					adaptor.Dispose();
			}

			// Now that we know who can deal with the keyboards we can retrieve all available
			// keyboards, as well as add the used keyboard adaptors as error report property.
			var bldr = new StringBuilder();
			foreach (var adaptor in _adaptors.Values)
			{
				adaptor.Initialize();

				if (bldr.Length > 0)
					bldr.Append(", ");
				bldr.Append(adaptor.GetType().Name);
			}
			ErrorReport.AddProperty("KeyboardAdaptors", bldr.ToString());
			Logger.WriteEvent("Keyboard adaptors in use: {0}", bldr.ToString());
		}

		public void UpdateAvailableKeyboards()
		{
			foreach (var adaptor in _adaptors.Values)
				adaptor.UpdateAvailableKeyboards();
		}

		internal IKeyedCollection<string, KeyboardDescription> Keyboards => _keyboards;

		internal IDictionary<KeyboardAdaptorType, IKeyboardRetrievingAdaptor> Adaptors => _adaptors;

		internal IDictionary<Control, object> EventHandlers => _eventHandlers;

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
				foreach (var adaptor in _adaptors.Values)
					adaptor.Dispose();
				_adaptors.Clear();
			}
			IsDisposed = true;
		}
		#endregion

		public IKeyboardDefinition DefaultKeyboard => _adaptors[KeyboardAdaptorType.System].SwitchingAdaptor.DefaultKeyboard;

		public IKeyboardDefinition GetKeyboard(string id)
		{
			return TryGetKeyboard(id, out var keyboard) ? keyboard : NullKeyboard;
		}

		public bool TryGetKeyboard(string id, out IKeyboardDefinition keyboard)
		{
			if (string.IsNullOrEmpty(id))
			{
				keyboard = null;
				return false;
			}

			if (_keyboards.TryGet(id, out var kd))
			{
				keyboard = kd;
				return true;
			}

			var parts = id.Split('|');
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
				for (var i = 1; i < parts.Length; i++)
				{
					keyboard = GetKeyboard(string.Join("-", parts.Take(i)), string.Join("-", parts.Skip(i)));
					if (keyboard != NullKeyboard)
						return true;
				}
			}

			keyboard = null;
			return false;
		}

		public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
		{
			return TryGetKeyboard(layoutName, locale, out var keyboard) ? keyboard : NullKeyboard;
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
			return TryGetKeyboard(language, out var keyboard) ? keyboard : NullKeyboard;
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
			return TryGetKeyboard(windowsLcid, out var keyboard) ? keyboard : NullKeyboard;
		}

		public bool TryGetKeyboard(int windowsLcid, out IKeyboardDefinition keyboard)
		{
			keyboard = _keyboards.FirstOrDefault(
				kbd =>
				{
					if (kbd.InputLanguage?.Culture == null)
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
			DefaultKeyboard?.Activate();
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
			if (!_keyboards.TryGet(id, out var keyboard))
			{
				var firstCompatibleAdapter = _adaptors.Values.FirstOrDefault(adaptor => adaptor.CanHandleFormat(format));
				if (firstCompatibleAdapter == null)
				{
					Debug.Fail($"Could not load keyboard for {id}. Did not find {format} in {_adaptors.Count} adapters");
					return new UnsupportedKeyboardDefinition(id);
				}
				keyboard = firstCompatibleAdapter.CreateKeyboardDefinition(id);
				_keyboards.Add(keyboard);
			}

			keyboard.Format = format;
			if (urls == null)
				return keyboard;

			foreach (var url in urls)
			{
				keyboard.Urls.Add(url);
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
				if (_activeKeyboard != null)
					return _activeKeyboard;

				_activeKeyboard = Adaptors[KeyboardAdaptorType.System].SwitchingAdaptor.ActiveKeyboard;
				if (_activeKeyboard == null || _activeKeyboard == NullKeyboard)
				{
					try
					{
						var lang = InputLanguage.CurrentInputLanguage;
						_activeKeyboard = GetKeyboard(lang.LayoutName, lang.Culture.Name);
					}
					catch (CultureNotFoundException)
					{
					}
				}

				return _activeKeyboard ?? (_activeKeyboard = NullKeyboard);
			}
			set => _activeKeyboard = value;
		}

		internal static void GetLayoutAndLocaleFromLanguageId(string id, out string layout, out string locale)
		{
			var parts = id.Split('_', '-');
			locale = parts[0];
			layout = parts.Length > 1 ? parts[parts.Length - 1] : String.Empty;
		}
	}
}
