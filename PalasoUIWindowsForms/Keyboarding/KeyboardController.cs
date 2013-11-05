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
using Palaso.Reporting;
using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
#if __MonoCS__
using Palaso.UI.WindowsForms.Keyboarding.Linux;
#else
using Palaso.UI.WindowsForms.Keyboarding.Windows;
#endif
using Palaso.UI.WindowsForms.Keyboarding.Types;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	/// <summary>
	/// Singleton class with methods for registering different keyboarding engines (e.g. Windows
	/// system, Keyman, XKB, IBus keyboards), and activating keyboards.
	/// Clients have to call KeyboardController.Initialize() before they can start using the
	/// keyboarding functionality, and they have to call KeyboardController.Shutdown() before
	/// the application or the unit test exits.
	/// </summary>
	public static class KeyboardController
	{
		#region Nested Manager class
		/// <summary>
		/// Allows setting different keyboard adapters which is needed for tests. Also allows
		/// registering keyboard layouts.
		/// </summary>
		internal static class Manager
		{
			/// <summary>
			/// Sets the available keyboard adaptors. Note that if this is called more than once, the adapters
			/// installed previously will be closed and no longer useable. Do not pass adapter instances that have been
			/// previously passed. At least one adapter must be of type System.
			/// </summary>
			internal static void SetKeyboardAdaptors(IKeyboardAdaptor[] adaptors)
			{
				Instance.Keyboards.Clear(); // InitializeAdaptors below will fill it in again.
				if (Adaptors != null)
				{
					foreach (var adaptor in Adaptors)
						adaptor.Close();
				}

				Adaptors = adaptors;

				InitializeAdaptors();
			}

			/// <summary>
			/// Resets the keyboard adaptors to the default ones.
			/// </summary>
			internal static void Reset()
			{
				SetKeyboardAdaptors(new IKeyboardAdaptor[] {
#if __MonoCS__
					new XkbKeyboardAdaptor(), new IbusKeyboardAdaptor()
#else
					new WinKeyboardAdaptor()
#endif
				});
			}

			internal static void InitializeAdaptors()
			{
				// this will also populate m_keyboards
				foreach (var adaptor in Adaptors)
					adaptor.Initialize();
			}

			/// <summary>
			/// Adds a keyboard to the list of installed keyboards
			/// </summary>
			/// <param name='description'>Keyboard description object</param>
			public static void RegisterKeyboard(IKeyboardDefinition description)
			{
				if (!Instance.Keyboards.Contains(description))
					Instance.Keyboards.Add(description);
			}
		}
		#endregion

		#region Class KeyboardControllerImpl
		private sealed class KeyboardControllerImpl : IKeyboardController, IKeyboardControllerImpl, IDisposable
		{
			private List<string> LanguagesAlreadyShownKeyboardNotFoundMessages { get; set; }
			public KeyboardCollection Keyboards { get; private set; }

			public KeyboardControllerImpl()
			{
				Keyboards = new KeyboardCollection();
				ActiveKeyboard = new KeyboardDescriptionNull();
				LanguagesAlreadyShownKeyboardNotFoundMessages = new List<string>();
			}

			public void UpdateAvailableKeyboards()
			{
				Keyboards.Clear();
				foreach (var adapter in Adaptors)
					adapter.UpdateAvailableKeyboards();
			}

			#region Disposable stuff
#if DEBUG
			/// <summary/>
			~KeyboardControllerImpl()
			{
				Dispose(false);
			}
#endif

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
					if (Adaptors != null)
					{
						foreach (var adaptor in Adaptors)
							adaptor.Close();
						Adaptors = null;
					}
				}
				IsDisposed = true;
			}
			#endregion

			private IKeyboardDefinition DefaultKeyboard
			{
				get { return Adaptors.First(adaptor => adaptor.Type == KeyboardType.System).DefaultKeyboard; }
			}

			public IKeyboardDefinition GetKeyboard(string layoutNameWithLocale)
			{
				if (string.IsNullOrEmpty(layoutNameWithLocale))
					return KeyboardDescription.Zero;

				return Keyboards.Contains(layoutNameWithLocale) ?
					Keyboards[layoutNameWithLocale] : KeyboardDescription.Zero;
			}

			public IKeyboardDefinition GetKeyboard(string layoutName, string locale)
			{
				if (string.IsNullOrEmpty(layoutName) && string.IsNullOrEmpty(locale))
					return KeyboardDescription.Zero;

				return Keyboards.Contains(layoutName, locale) ?
					Keyboards[layoutName, locale] : KeyboardDescription.Zero;
			}

			/// <summary>
			/// Tries to get the keyboard for the specified <paramref name="writingSystem"/>.
			/// </summary>
			/// <returns>
			/// Returns <c>KeyboardDescription.Zero</c> if no keyboard can be found.
			/// </returns>
			public IKeyboardDefinition GetKeyboard(IWritingSystemDefinition writingSystem)
			{
				if (writingSystem == null)
					return KeyboardDescription.Zero;
				return writingSystem.LocalKeyboard ?? KeyboardDescription.Zero;
			}

			public IKeyboardDefinition GetKeyboard(IInputLanguage language)
			{
				// NOTE: on Windows InputLanguage.LayoutName returns a wrong name in some cases.
				// Therefore we need this overload so that we can identify the keyboard correctly.
				return Keyboards
					.Where(keyboard =>keyboard is KeyboardDescription && ((KeyboardDescription)keyboard).InputLanguage.Equals(language))
					.DefaultIfEmpty(KeyboardDescription.Zero)
					.First();
			}

			/// <summary>
			/// Sets the keyboard.
			/// </summary>
			/// <param name='layoutName'>Keyboard layout name</param>
			public void SetKeyboard(string layoutName)
			{
				var keyboard = GetKeyboard(layoutName);
				if (keyboard.Equals(KeyboardDescription.Zero))
				{
					if (!LanguagesAlreadyShownKeyboardNotFoundMessages.Contains(layoutName))
					{
						LanguagesAlreadyShownKeyboardNotFoundMessages.Add(layoutName);
						ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
							"Could not find a keyboard ime that had a keyboard named '{0}'", layoutName);
					}
					return;
				}

				SetKeyboard(keyboard);
			}

			public void SetKeyboard(string layoutName, string locale)
			{
				SetKeyboard(GetKeyboard(layoutName, locale));
			}

			public void SetKeyboard(IWritingSystemDefinition writingSystem)
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
				get { return Keyboards; }
			}

			/// <summary>
			/// Creates and returns a keyboard definition object based on the layout and locale.
			/// </summary>
			public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
			{
			var existingKeyboard = AllAvailableKeyboards.FirstOrDefault(keyboard => keyboard.Layout == layout && keyboard.Locale == locale);
			return existingKeyboard ??
				Adaptors.First(adaptor => adaptor.Type == KeyboardType.System)
					.CreateKeyboardDefinition(layout, locale);
			}

			/// <summary>
			/// Gets or sets the currently active keyboard
			/// </summary>
			public IKeyboardDefinition ActiveKeyboard { get; set; }

			/// <summary>
			/// Figures out the system default keyboard for the specified writing system (the one to use if we have no available KnownKeyboards).
			/// The implementation may use obsolete fields such as Keyboard
			/// </summary>
			public IKeyboardDefinition DefaultForWritingSystem(IWritingSystemDefinition ws)
			{
				return LegacyForWritingSystem(ws) ?? DefaultKeyboard;
			}

			/// <summary>
			/// Finds a keyboard specified using one of the legacy fields. If such a keyboard is found, it is appropriate to
			/// automatically add it to KnownKeyboards. If one is not, a general DefaultKeyboard should NOT be added.
			/// </summary>
			/// <param name="ws"></param>
			/// <returns></returns>
			public IKeyboardDefinition LegacyForWritingSystem(IWritingSystemDefinition ws)
			{
				var legacyWs = ws as ILegacyWritingSystemDefinition;
				if (legacyWs == null)
					return DefaultKeyboard;

				return LegacyKeyboardHandling.GetKeyboardFromLegacyWritingSystem(legacyWs, this);
			}

			#region Legacy keyboard handling

			private static class LegacyKeyboardHandling
			{
				public static IKeyboardDefinition GetKeyboardFromLegacyWritingSystem(ILegacyWritingSystemDefinition ws,
					KeyboardControllerImpl controller)
				{
					if (!string.IsNullOrEmpty(ws.WindowsLcid))
					{
						var keyboard = HandleFwLegacyKeyboards(ws, controller);
						if (keyboard != null)
							return keyboard;
					}

					if (!string.IsNullOrEmpty(ws.Keyboard))
					{
						if (controller.Keyboards.Contains(ws.Keyboard))
							return controller.Keyboards[ws.Keyboard];

						// Palaso WinIME keyboard
						var locale = GetLocaleName(ws.Keyboard);
						var layout = GetLayoutName(ws.Keyboard);
						if (controller.Keyboards.Contains(layout, locale))
							return controller.Keyboards[layout, locale];

						// Palaso Keyman or Ibus keyboard
						var keyboard = controller.Keyboards.FirstOrDefault(kbd => kbd.Layout == layout);
						if (keyboard != null)
							return keyboard;
					}

					return null;
				}

				private static IKeyboardDefinition HandleFwLegacyKeyboards(ILegacyWritingSystemDefinition ws,
					KeyboardControllerImpl controller)
				{
					var lcid = GetLcid(ws);
					if (lcid >= 0)
					{
						try
						{
							if (string.IsNullOrEmpty(ws.Keyboard))
							{
								// FW system keyboard
								var keyboard = controller.Keyboards.FirstOrDefault(
									kbd =>
									{
										var keyboardDescription = kbd as KeyboardDescription;
										if (keyboardDescription == null || keyboardDescription.InputLanguage == null)
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
								if (controller.Keyboards.Contains(ws.Keyboard, culture.Name))
									return controller.Keyboards[ws.Keyboard, culture.Name];
							}
						}
						catch (CultureNotFoundException)
						{
							// Culture specified by LCID is not supported on current system. Just ignore.
						}
					}
					return null;
				}

				private static int GetLcid(ILegacyWritingSystemDefinition ws)
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
			#endregion
		}
		#endregion

		#region Static methods and properties

		/// <summary>
		/// Gets the current keyboard controller singleton.
		/// </summary>
		private static IKeyboardControllerImpl Instance
		{
			get
			{
				return Keyboard.Controller as IKeyboardControllerImpl;
			}
		}

		/// <summary>
		/// Enables the PalasoUIWinForms keyboarding. This should be called during application startup.
		/// </summary>
		public static void Initialize()
		{
			// Note: arguably it is undesirable to install this as the public keyboard controller before we initialize it
			// (the Reset call). However, we have not in general attempted thread safety for the keyboarding code; it seems
			// highly unlikely that any but the UI thread wants to manipulate keyboards. Apart from other threads, nothing
			// has a chance to access this before we return. If initialization does not complete successfully, we clear the
			// global.
			try
			{
				Keyboard.Controller = new KeyboardControllerImpl();
				Manager.Reset();
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
			if (Instance == null)
				return;

			Instance.Dispose();
			Keyboard.Controller = null;
		}

		/// <summary>
		/// Gets or sets the available keyboard adaptors.
		/// </summary>
		internal static IKeyboardAdaptor[] Adaptors { get; private set; }

		/// <summary>
		/// Gets the currently active keyboard
		/// </summary>
		public static IKeyboardDefinition ActiveKeyboard
		{
			get { return Instance.ActiveKeyboard; }
		}

		#endregion

	}
}
