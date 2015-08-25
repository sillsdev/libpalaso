// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.IO;


#if __MonoCS__
using System;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using X11.XKlavier;
using System.Collections.Generic;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.WritingSystems;
using Palaso.Reporting;
using System.Globalization;
using Palaso.UI.WindowsForms.Keyboarding.Types;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for retrieving Xkb keyboards on Linux
	/// </summary>
	[CLSCompliant(false)]
	public class XkbKeyboardRetrievingAdaptor: IKeyboardRetrievingAdaptor
	{
		protected List<IKeyboardErrorDescription> BadLocales;
		protected IXklEngine _engine;
		protected IKeyboardSwitchingAdaptor _adaptor;
		protected static string _missingKeyboardFmt;
		protected static HashSet<string> _knownCultures;

		public XkbKeyboardRetrievingAdaptor(): this(new XklEngine())
		{
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="Palaso.UI.WindowsForms.Keyboarding.Linux.XkbKeyboardRetriever"/> class.
		/// This overload is used in unit tests.
		/// </summary>
		public XkbKeyboardRetrievingAdaptor(IXklEngine engine)
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
			if (BadLocales != null)
				return;
			ReinitLocales();
		}

		protected virtual void ReinitLocales()
		{
			BadLocales = new List<IKeyboardErrorDescription>();

			var configRegistry = XklConfigRegistry.Create(_engine);
			var layouts = configRegistry.Layouts;

			for (uint iGroup = 0; iGroup < _engine.GroupNames.Length; iGroup++)
			{
				// a group in a xkb keyboard is a keyboard layout. This can be used with
				// multiple languages - which language is ambigious. Here we just add all
				// of them.
				// m_engine.GroupNames are not localized, but the layouts are. Before we try
				// to compare them we better localize the group name as well, or we won't find
				// much (FWNX-1388)
				var groupName = _engine.LocalizedGroupNames[iGroup];
				List<XklConfigRegistry.LayoutDescription> layoutList;
				if (!layouts.TryGetValue(groupName, out layoutList))
				{
					// No language in layouts uses the groupName keyboard layout.
					BadLocales.Add(new KeyboardErrorDescription(groupName));
					Console.WriteLine("WARNING: Couldn't find layout for {0}.", groupName);
					Logger.WriteEvent("WARNING: Couldn't find layout for {0}.", groupName);
					continue;
				}

				for (int iLayout = 0; iLayout < layoutList.Count; iLayout++)
				{
					var layout = layoutList[iLayout];
					AddKeyboardForLayout(layout, iGroup);
				}
			}
		}

		private void AddKeyboardForLayout(XklConfigRegistry.LayoutDescription layout, uint iGroup)
		{
			AddKeyboardForLayout(layout, iGroup, _adaptor);
		}

		internal void AddKeyboardForLayout(XklConfigRegistry.LayoutDescription layout, uint iGroup,
			IKeyboardSwitchingAdaptor engine)
		{
			var description = GetDescription(layout);
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
			var inputLanguage = new InputLanguageWrapper(culture, IntPtr.Zero, layout.Language);
			var keyboard = new XkbKeyboardDescription(description, layout.LayoutId, layout.LocaleId,
				inputLanguage, engine, (int)iGroup);
			KeyboardController.Manager.RegisterKeyboard(keyboard);
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			return CreateKeyboardDefinition(layout, locale, _adaptor);
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		internal static IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale,
			IKeyboardSwitchingAdaptor adaptor)
		{
			var realLocale = locale;
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
				return new XkbKeyboardDescription(string.Format("{0} ({1})", locale, layout), layout, locale,
					new InputLanguageWrapper(realLocale, IntPtr.Zero, layout), adaptor, -1) {IsAvailable = false};
			if (_missingKeyboardFmt == null)
			{
				_missingKeyboardFmt = L10NSharp.LocalizationManager.GetString("XkbKeyboardAdaptor.MissingKeyboard",
					"[Missing] {0} ({1})");
			}
			return new XkbKeyboardDescription(String.Format(_missingKeyboardFmt, locale, layout), layout, locale,
				new InputLanguageWrapper("en", IntPtr.Zero, "US"), adaptor, -1) {IsAvailable = false};
		}

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
		#region IKeyboardRetriever implementation

		/// <summary>
		/// The type of keyboards this retriever handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardType Type
		{
			get { return KeyboardType.System; }
		}

		public virtual bool IsApplicable
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the keyboard adaptor that deals with keyboards that this class retrieves.
		/// </summary>
		public IKeyboardSwitchingAdaptor Adaptor { get { return _adaptor; } }

		public virtual void Initialize()
		{
			_adaptor = new XkbKeyboardSwitchingAdaptor(_engine);
		}

		public virtual void RegisterAvailableKeyboards()
		{
			InitLocales();
		}

		public virtual void UpdateAvailableKeyboards()
		{
			ReinitLocales();
		}

		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get
			{
				InitLocales();
				return BadLocales;
			}
		}

		// Currently we expect this to only be useful on Windows.
		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		public virtual void Close()
		{
			_engine.Close();
			_engine = null;

			_adaptor = null;
		}

		public virtual string GetKeyboardSetupApplication(out string arguments)
		{
			// NOTE: if we get false results (e.g. because the user has installed multiple
			// desktop environments) we could check for the currently running desktop
			// (Platform.DesktopEnvironment) and return the matching program
			arguments = null;
			// XFCE
			if (File.Exists("/usr/bin/xfce4-keyboard-settings"))
				return "/usr/bin/xfce4-keyboard-settings";
			// GNOME
			if (File.Exists("/usr/bin/gnome-control-center"))
			{
				arguments = "region layouts";
				return "/usr/bin/gnome-control-center";
			}
			// Cinnamon
			if (File.Exists("/usr/lib/cinnamon-settings/cinnamon-settings.py") && File.Exists("/usr/bin/python"))
			{
				arguments = "/usr/lib/cinnamon-settings/cinnamon-settings.py keyboard";
				return "/usr/bin/python";
			}
			// KDE
			if (File.Exists("/usr/bin/kcmshell4"))
			{
				arguments = "kcm_keyboard";
				return "/usr/bin/kcmshell4";
			}
			return null;
		}

		public bool IsSecondaryKeyboardSetupApplication
		{
			get { return false; }
		}

		#endregion

	}
}
#endif