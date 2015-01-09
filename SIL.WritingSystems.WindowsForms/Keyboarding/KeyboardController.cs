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
using Palaso.Reporting;
using Palaso.Extensions;
#if __MonoCS__
using SIL.WritingSystems.WindowsForms.Keyboarding.Linux;
#else
using SIL.WritingSystems.WindowsForms.Keyboarding.Windows;
#endif

namespace SIL.WritingSystems.WindowsForms.Keyboarding
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

		private static KeyboardController _instance;

		public static KeyboardController Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// Enables the PalasoUIWinForms keyboarding. This should be called during application startup.
		/// </summary>
		public static void Initialize()
		{
			// Note: arguably it is undesirable to install this as the public keyboard controller before we initialize it.
			// However, we have not in general attempted thread safety for the keyboarding code; it seems
			// highly unlikely that any but the UI thread wants to manipulate keyboards. Apart from other threads, nothing
			// has a chance to access this before we return. If initialization does not complete successfully, we clear the
			// global.
			try
			{
				_instance = new KeyboardController();
				Keyboard.Controller = _instance;
				_instance.SetDefaultAdaptors();
			}
			catch (Exception)
			{
				Keyboard.Controller = null;
				throw;
			}
		}

		/// <summary>
		/// Ends support for the PalasoUIWinForms keyboarding
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

		#endregion

		public event EventHandler<RegisterEventArgs> ControlAdded;
		public event ControlEventHandler ControlRemoving;

		private readonly List<string> _languagesAlreadyShownKeyboardNotFoundMessages;
		private IKeyboardDefinition _activeKeyboard;
		private readonly List<IKeyboardAdaptor> _adaptors;
		private readonly KeyboardDescriptionCollection _keyboards;
		private readonly Dictionary<Control, object> _eventHandlers;

		private KeyboardController()
		{
			_keyboards = new KeyboardDescriptionCollection();
			_eventHandlers = new Dictionary<Control, object>();
			_languagesAlreadyShownKeyboardNotFoundMessages = new List<string>();
			_adaptors = new List<IKeyboardAdaptor>();
		}

		private void SetDefaultAdaptors()
		{
#if __MonoCS__
			if (CombinedKeyboardAdaptor.IsRequired)
				SetKeyboardAdaptors(new CombinedKeyboardAdaptor());
			else if (CinnamonIbusAdaptor.IsRequired)
				SetKeyboardAdaptors(new CinnamonIbusAdaptor());
			else
				SetKeyboardAdaptors(new XkbKeyboardAdaptor(), new IbusKeyboardAdaptor());
#else
			SetKeyboardAdaptors(new WinKeyboardAdaptor(), new KeymanKeyboardAdaptor());
#endif
		}

		internal void SetKeyboardAdaptors(params IKeyboardAdaptor[] adaptors)
		{
			_keyboards.Clear();
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

		internal KeyboardDescriptionCollection Keyboards
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
			get { return _adaptors.First(adaptor => adaptor.Type == KeyboardAdaptorType.System).DefaultKeyboard; }
		}

		public IKeyboardDefinition GetKeyboard(string id)
		{
			if (string.IsNullOrEmpty(id))
				return KeyboardDescription.Zero;

			if (_keyboards.Contains(id))
				return _keyboards[id];

			string[] parts = id.Split('|');
			if (parts.Length == 2)
			{
				// This is the way Paratext stored IDs in 7.4-7.5 while there was a temporary bug-fix in place)
				return GetKeyboard(parts[0], parts[1]);
			}

			// Handle old Palaso IDs
			parts = id.Split('-');
			if (parts.Length > 1)
			{
				for (int i = 1; i < parts.Length; i++)
				{
					var kb = GetKeyboard(string.Join("-", parts.Take(i)), string.Join("-", parts.Skip(i)));
					if (!kb.Equals(KeyboardDescription.Zero))
						return kb;
				}
			}

			return KeyboardDescription.Zero;
		}

		public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
		{
			if (string.IsNullOrEmpty(layoutName) && string.IsNullOrEmpty(locale))
				return KeyboardDescription.Zero;

			IKeyboardDefinition result = _keyboards.FirstOrDefault(kd => kd.Layout == layoutName && kd.Locale == locale);
			return result ?? KeyboardDescription.Zero;
		}

		/// <summary>
		/// Tries to get the keyboard for the specified <paramref name="writingSystem"/>.
		/// </summary>
		/// <returns>
		/// Returns <c>KeyboardDescription.Zero</c> if no keyboard can be found.
		/// </returns>
		public IKeyboardDefinition GetKeyboard(WritingSystemDefinition writingSystem)
		{
			if (writingSystem == null)
				return KeyboardDescription.Zero;
			return writingSystem.LocalKeyboard ?? KeyboardDescription.Zero;
		}

		public IKeyboardDefinition GetKeyboard(IInputLanguage language)
		{
			// NOTE: on Windows InputLanguage.LayoutName returns a wrong name in some cases.
			// Therefore we need this overload so that we can identify the keyboard correctly.
			return _keyboards
				.Where(keyboard => keyboard is KeyboardDescription &&
					((KeyboardDescription)keyboard).InputLanguage != null &&
					((KeyboardDescription)keyboard).InputLanguage.Equals(language))
				.DefaultIfEmpty(KeyboardDescription.Zero)
				.First();
		}

		/// <summary>
		/// Sets the keyboard.
		/// </summary>
		/// <param name='id'>Keyboard layout name</param>
		public void SetKeyboard(string id)
		{
			IKeyboardDefinition keyboard = GetKeyboard(id);
			if (keyboard.Equals(KeyboardDescription.Zero))
			{
				if (!_languagesAlreadyShownKeyboardNotFoundMessages.Contains(id))
				{
					_languagesAlreadyShownKeyboardNotFoundMessages.Add(id);
					ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
						"Could not find a keyboard ime that had a keyboard named '{0}'", id);
				}
				return;
			}

			SetKeyboard(keyboard);
		}

		public void SetKeyboard(string layoutName, string locale)
		{
			SetKeyboard(GetKeyboard(layoutName, locale));
		}

		public void SetKeyboard(WritingSystemDefinition writingSystem)
		{
			SetKeyboard(writingSystem.LocalKeyboard);
		}

		public void SetKeyboard(IInputLanguage language)
		{
			SetKeyboard(GetKeyboard(language));
		}

		public void SetKeyboard(IKeyboardDefinition keyboard)
		{
			keyboard.Activate();
		}

		/// <summary>
		/// Activates the keyboard of the default input language
		/// </summary>
		public void ActivateDefaultKeyboard()
		{
			SetKeyboard(DefaultKeyboard);
		}

		/// <summary>
		/// Returns everything that is installed on the system and available to be used.
		/// This would typically be used to populate a list of available keyboards in configuring a writing system.
		/// </summary>
		public IEnumerable<IKeyboardDefinition> AllAvailableKeyboards
		{
			get { return _keyboards.Where(kd => kd.IsAvailable); }
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the ID.
		/// </summary>
		public IKeyboardDefinition CreateKeyboardDefinition(string id, KeyboardFormat format, IEnumerable<string> urls)
		{
			KeyboardDescription keyboard;
			if (!_keyboards.TryGetItem(id, out keyboard))
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
						_activeKeyboard = KeyboardDescription.Zero;
				}
				return _activeKeyboard;
			}
			set { _activeKeyboard = value; }
		}

		/// <summary>
		/// Figures out the system default keyboard for the specified writing system (the one to use if we have no available KnownKeyboards).
		/// The implementation may use obsolete fields such as Keyboard
		/// </summary>
		public IKeyboardDefinition DefaultForWritingSystem(WritingSystemDefinition ws)
		{
			return LegacyForWritingSystem(ws) ?? DefaultKeyboard;
		}

		/// <summary>
		/// Finds a keyboard specified using one of the legacy fields. If such a keyboard is found, it is appropriate to
		/// automatically add it to KnownKeyboards. If one is not, a general DefaultKeyboard should NOT be added.
		/// </summary>
		/// <param name="ws"></param>
		/// <returns></returns>
		public IKeyboardDefinition LegacyForWritingSystem(WritingSystemDefinition ws)
		{
			if (ws == null)
				return null;

			if (!string.IsNullOrEmpty(ws.WindowsLcid))
			{
				IKeyboardDefinition keyboard = HandleFwLegacyKeyboards(ws);
				if (keyboard != null)
					return keyboard;
			}

			if (!string.IsNullOrEmpty(ws.Keyboard))
			{
				if (_keyboards.Contains(ws.Keyboard))
					return _keyboards[ws.Keyboard];

				// Palaso WinIME keyboard
				string locale = GetLocaleName(ws.Keyboard);
				string layout = GetLayoutName(ws.Keyboard);
				IKeyboardDefinition result = GetKeyboard(layout, locale);
				if (result != KeyboardDescription.Zero)
					return result;

				// Palaso Keyman or Ibus keyboard
				if (_keyboards.Contains(layout))
					return _keyboards[layout];
			}

			return null;
		}

		/// <summary>
		/// Register the control for keyboarding, optionally providing an event handler for
		/// a keyboarding adapter. If <paramref ref="eventHandler"/> is <c>null</c> the
		/// default handler will be used.
		/// The application should call this method for each control that needs IME input before
		/// displaying the control, typically after calling the InitializeComponent() method.
		/// </summary>
		public void RegisterControl(Control control, object eventHandler = null)
		{
			_eventHandlers[control] = eventHandler;
			if (ControlAdded != null)
				ControlAdded(this, new RegisterEventArgs(control, eventHandler));
		}

		/// <summary>
		/// Unregister the control from keyboarding. The application should call this method
		/// prior to disposing the control so that the keyboard adapters can release unmanaged
		/// resources.
		/// </summary>
		public void UnregisterControl(Control control)
		{
			if (ControlRemoving != null)
				ControlRemoving(this, new ControlEventArgs(control));
			_eventHandlers.Remove(control);
		}

		#if __MonoCS__
		public bool CombinedKeyboardHandling
		{
			get { return Adaptors.Any(a => a is CombinedKeyboardAdaptor); }
		}

		public bool CinnamonKeyboardHandling
		{
			get { return Adaptors.Any(a => a is CinnamonIbusAdaptor); }
		}
		#endif

		private IKeyboardDefinition HandleFwLegacyKeyboards(WritingSystemDefinition ws)
		{
			int lcid = GetLcid(ws);
			if (lcid >= 0)
			{
				try
				{
					if (string.IsNullOrEmpty(ws.Keyboard))
					{
						// FW system keyboard
						IKeyboardDefinition keyboard = _keyboards.FirstOrDefault(
							kbd =>
							{
								var keyboardDescription = kbd as KeyboardDescription;
								if (keyboardDescription == null || keyboardDescription.InputLanguage == null || keyboardDescription.InputLanguage.Culture == null)
									return false;
								return keyboardDescription.InputLanguage.Culture.LCID == lcid;
							});
						if (keyboard != null)
							return keyboard;
					}
					else
					{
						// FW keyman keyboard
						var culture = new CultureInfo(lcid);
						IKeyboardDefinition keyboard = GetKeyboard(ws.Keyboard, culture.Name);
						if (keyboard != KeyboardDescription.Zero)
							return keyboard;
					}
				}
				catch (CultureNotFoundException)
				{
					// Culture specified by LCID is not supported on current system. Just ignore.
				}
			}
			return null;
		}

		private static int GetLcid(WritingSystemDefinition ws)
		{

			int lcid;
			if (!Int32.TryParse(ws.WindowsLcid, out lcid))
				lcid = -1; // can't convert ws.WindowsLcid to a valid LCID. Just ignore.
			return lcid;
		}

		private static string GetLocaleName(string name)
		{
			var split = name.Split(new[] { '-' });
			string localeName;
			if (split.Length <= 1)
			{
				localeName = string.Empty;
			}
			else if (split.Length > 1 && split.Length <= 3)
			{
				localeName = string.Join("-", split.Skip(1).ToArray());
			}
			else
			{
				localeName = string.Join("-", split.Skip(split.Length - 2).ToArray());
			}
			return localeName;
		}

		private static string GetLayoutName(string name)
		{
			//Just cut off the length of the locale + 1 for the dash
			var locale = GetLocaleName(name);
			if (string.IsNullOrEmpty(locale))
			{
				return name;
			}
			var layoutName = name.Substring(0, name.Length - (locale.Length + 1));
			return layoutName;
		}
	}
}
