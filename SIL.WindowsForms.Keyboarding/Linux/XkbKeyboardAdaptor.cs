// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------

#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Icu;
using X11.XKlavier;
using SIL.Reporting;
using SIL.Keyboarding;

namespace SIL.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling xkb keyboards on Linux
	/// </summary>
	public class XkbKeyboardAdaptor : IKeyboardAdaptor
	{
		private IXklEngine _engine;

		public XkbKeyboardAdaptor(): this(new XklEngine())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Palaso.UI.WindowsForms.Keyboarding.Linux.XkbKeyboardAdaptor"/> class.
		/// This overload is used in unit tests.
		/// </summary>
		public XkbKeyboardAdaptor(IXklEngine engine)
		{
			_engine = engine;
		}

		private string GetLanguageCountry(Icu.Locale locale)
		{
			if (string.IsNullOrEmpty(locale.Country) && string.IsNullOrEmpty(locale.Language))
				return string.Empty;
			return locale.Language + "_" + locale.Country;
		}

		/// <summary>
		/// Gets the IcuLocales by language and country. The 3-letter language and country codes
		/// are concatenated with an underscore in between, e.g. fra_BEL
		/// </summary>
		private Dictionary<string, Icu.Locale> IcuLocalesByLanguageCountry
		{
			get
			{
				var localesByLanguageCountry = new Dictionary<string, Icu.Locale>();
				foreach (var locale in Icu.Locale.AvailableLocales)
				{
					var languageCountry = GetLanguageCountry(locale);
					if (string.IsNullOrEmpty(languageCountry) ||
						localesByLanguageCountry.ContainsKey(languageCountry))
					{
						continue;
					}
					localesByLanguageCountry[languageCountry] = locale;
				}
				return localesByLanguageCountry;
			}
		}

		private static string GetDescription(XklConfigRegistry.LayoutDescription layout)
		{
			return string.Format("{0} - {1} ({2})", layout.Description, layout.Language, layout.Country);
		}

		protected virtual void InitLocales()
		{
			ReinitLocales();
		}

		private void ReinitLocales()
		{
			var configRegistry = XklConfigRegistry.Create(_engine);
			Dictionary<string, List<XklConfigRegistry.LayoutDescription>> layouts = configRegistry.Layouts;

			Dictionary<string, XkbKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<XkbKeyboardDescription>().ToDictionary(kd => kd.Id);
			for (int iGroup = 0; iGroup < _engine.GroupNames.Length; iGroup++)
			{
				// a group in a xkb keyboard is a keyboard layout. This can be used with
				// multiple languages - which language is ambigious. Here we just add all
				// of them.
				// m_engine.GroupNames are not localized, but the layouts are. Before we try
				// to compare them we better localize the group name as well, or we won't find
				// much (FWNX-1388)
				string groupName = _engine.LocalizedGroupNames[iGroup];
				List<XklConfigRegistry.LayoutDescription> layoutList;
				if (!layouts.TryGetValue(groupName, out layoutList))
				{
					// No language in layouts uses the groupName keyboard layout.
					Console.WriteLine("WARNING: Couldn't find layout for {0}.", groupName);
					Logger.WriteEvent("WARNING: Couldn't find layout for {0}.", groupName);
					continue;
				}

				for (int iLayout = 0; iLayout < layoutList.Count; iLayout++)
				{
					XklConfigRegistry.LayoutDescription layout = layoutList[iLayout];
					AddKeyboardForLayout(curKeyboards, layout, iGroup, this);
				}
			}

			foreach (XkbKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);
		}

		internal static void AddKeyboardForLayout(IDictionary<string, XkbKeyboardDescription> curKeyboards, XklConfigRegistry.LayoutDescription layout,
			int iGroup, IKeyboardAdaptor engine)
		{
			string description = GetDescription(layout);
			CultureInfo culture = null;
			try
			{
				culture = new CultureInfo(layout.LocaleId);
			}
			catch (ArgumentException)
			{
				// This can happen if the locale is not supported.
				// TODO: fix mono's list of supported locales. Doesn't support e.g. de-BE.
				// See mono/tools/locale-builder.
			}
			string id = string.Format("{0}_{1}", layout.LocaleId, layout.LayoutId);
			var inputLanguage = new InputLanguageWrapper(culture, IntPtr.Zero, layout.Language);
			XkbKeyboardDescription existingKeyboard;
			if (curKeyboards.TryGetValue(id, out existingKeyboard))
			{
				if (!existingKeyboard.IsAvailable)
				{
					existingKeyboard.SetIsAvailable(true);
					existingKeyboard.SetName(description);
					existingKeyboard.SetInputLanguage(inputLanguage);
					existingKeyboard.GroupIndex = iGroup;
				}
				curKeyboards.Remove(id);
			}
			else
			{
				var keyboard = new XkbKeyboardDescription(id, description, layout.LayoutId, layout.LocaleId, true,
					inputLanguage, engine, iGroup);
				KeyboardController.Instance.Keyboards.Add(keyboard);
			}
		}

		public IXklEngine XklEngine
		{
			get { return _engine; }
		}

		public void Initialize()
		{
			InitLocales();
		}

		public void UpdateAvailableKeyboards()
		{
			ReinitLocales();
		}

		public bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			Debug.Assert(keyboard.Engine == this);
			Debug.Assert(keyboard is XkbKeyboardDescription);
			var xkbKeyboard = keyboard as XkbKeyboardDescription;
			if (xkbKeyboard == null)
				throw new ArgumentException();

			if (xkbKeyboard.GroupIndex >= 0)
			{
				_engine.SetGroup(xkbKeyboard.GroupIndex);
			}
			return true;
		}

		public void DeactivateKeyboard(KeyboardDescription keyboard)
		{
		}

		public KeyboardDescription GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardAdaptorType Type
		{
			get { return KeyboardAdaptorType.System; }
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		/// <remarks>
		/// For Xkb the default keyboard has GroupIndex set to zero.
		/// Wasta/Cinnamon keyboarding doesn't use XkbKeyboardDescription objects.
		/// </remarks>
		public KeyboardDescription DefaultKeyboard
		{
			get
			{
				return Keyboard.Controller.AllAvailableKeyboards.OfType<XkbKeyboardDescription>().FirstOrDefault(kbd => kbd.GroupIndex == 0);
			}
		}
			
		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public KeyboardDescription CreateKeyboardDefinition(string id)
		{
			return CreateKeyboardDefinition(id, this);
		}

		internal static XkbKeyboardDescription CreateKeyboardDefinition(string id, IKeyboardAdaptor engine)
		{
			string[] parts = id.Split('_');
			string locale = parts[0];
			string layout = parts[1];

			string realLocale = locale;
			if (locale == "zh")
			{
				realLocale = "zh-CN";	// Mono doesn't support bare "zh" until version 3 sometime
			}
			else if (locale == "x040F")
			{
				realLocale = "is";			// 0x040F is the numeric code for Icelandic.
			}

			// Don't crash if the locale is unknown to the system.  (It may be that ibus is not running and
			// this particular locale and layout refer to an ibus keyboard.)  Mark the keyboard description
			// as missing, but create an English (US) keyboard underneath.
			if (IsLocaleKnown(realLocale))
				return new XkbKeyboardDescription(id, string.Format("{0} ({1})", locale, layout), layout, locale, false,
					new InputLanguageWrapper(realLocale, IntPtr.Zero, layout), engine, -1);
			string missingKeyboardFmt = L10NSharp.LocalizationManager.GetString("XkbKeyboardAdaptor.MissingKeyboard", "[Missing] {0} ({1})");
			return new XkbKeyboardDescription(id, String.Format(missingKeyboardFmt, locale, layout), layout, locale, false,
				new InputLanguageWrapper("en", IntPtr.Zero, "US"), engine, -1);
		}

		private static HashSet<string> _knownCultures;
		/// <summary>
		/// Check whether the locale is known to the system.
		/// </summary>
		private static bool IsLocaleKnown(string locale)
		{
			if (_knownCultures == null)
			{
				_knownCultures = new HashSet<string>();
				foreach (var ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
					_knownCultures.Add(ci.Name);
			}
			return _knownCultures.Contains(locale);
		}

		public bool CanHandleFormat(KeyboardFormat format)
		{
			return format == KeyboardFormat.Unknown;
		}

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~XkbKeyboardAdaptor()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				if (_engine != null)
				{
					_engine.Close();
					_engine = null;
				}
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			IsDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}
#endif
