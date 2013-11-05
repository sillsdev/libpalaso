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
using X11.XKlavier;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.UI.WindowsForms.Keyboarding.Types;
using Palaso.WritingSystems;
using Icu;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling xkb keyboards on Linux
	/// </summary>
	internal class XkbKeyboardAdaptor: IKeyboardAdaptor
	{
		private static Type XklEngineType = typeof(XklEngine);

		private List<IKeyboardErrorDescription> m_BadLocales;
		private XklEngine m_engine;

		/// <summary>
		/// Sets the type of the XklEngine. This is useful for unit tests.
		/// </summary>
		internal static void SetXklEngineType<T>() where T: XklEngine
		{
			XklEngineType = typeof(T);
		}

		public XkbKeyboardAdaptor()
		{
			m_engine = Activator.CreateInstance(XklEngineType) as XklEngine;
		}

		private string GetLanguageCountry(Locale locale)
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
				var localesByLanguageCountry = new Dictionary<string, Locale>();
				foreach (var locale in Locale.AvailableLocales)
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

		private void InitLocales()
		{
			if (m_BadLocales != null)
				return;
			ReinitLocales();
		}

		private void ReinitLocales()
		{
			m_BadLocales = new List<IKeyboardErrorDescription>();

			var configRegistry = XklConfigRegistry.Create(m_engine);
			var layouts = configRegistry.Layouts;
			//var icuLocales = IcuLocalesByLanguageCountry;

			for (int iGroup = 0; iGroup < m_engine.GroupNames.Length; iGroup++)
			{
				// a group in a xkb keyboard is a keyboard layout. This can be used with
				// multiple languages - which language is ambigious. Here we just add all
				// of them.
				var groupName = m_engine.GroupNames[iGroup];
				List<XklConfigRegistry.LayoutDescription> layoutList;
				if (!layouts.TryGetValue(groupName, out layoutList))
				{
					// No language in layouts uses the groupName keyboard layout.
					m_BadLocales.Add(new KeyboardErrorDescription(groupName));
					continue;
				}

				for (int iLayout = 0; iLayout < layoutList.Count; iLayout++)
				{
					var layout = layoutList[iLayout];
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
						inputLanguage, this, iGroup);
					KeyboardController.Manager.RegisterKeyboard(keyboard);
				}
			}
		}

		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get
			{
				InitLocales();
				return m_BadLocales;
			}
		}

		public void Initialize()
		{
			InitLocales();
		}

		public void UpdateAvailableKeyboards()
		{
			ReinitLocales();
		}

		public void Close()
		{
			m_engine.Close();
			m_engine = null;
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			Debug.Assert(keyboard is KeyboardDescription);
			Debug.Assert(((KeyboardDescription)keyboard).Engine == this);
			Debug.Assert(keyboard is XkbKeyboardDescription);
			var xkbKeyboard = keyboard as XkbKeyboardDescription;
			if (xkbKeyboard == null)
				throw new ArgumentException();

			if (xkbKeyboard.GroupIndex >= 0)
				m_engine.SetGroup(xkbKeyboard.GroupIndex);
			return true;
		}

		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
		}

		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public KeyboardType Type
		{
			get { return KeyboardType.System; }
		}

		/// <summary>
		/// Gets the default keyboard of the system.
		/// </summary>
		/// <remarks>For Xkb the default keyboard is the first one.</remarks>
		public IKeyboardDefinition DefaultKeyboard
		{
			get
			{
				return Keyboard.Controller.AllAvailableKeyboards.Where(kbd => kbd.Type == KeyboardType.System)
					.FirstOrDefault(x => x is XkbKeyboardDescription && ((XkbKeyboardDescription)x).GroupIndex == 0);
			}
		}

		/// <summary>
		/// Creates and returns a keyboard definition object based on the layout and locale.
		/// Note that this method is used when we do NOT have a matching available keyboard.
		/// Therefore we can presume that the created one is NOT available.
		/// </summary>
		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			return new XkbKeyboardDescription(string.Format("{0} ({1})", locale, layout), layout, locale,
				new InputLanguageWrapper(locale, IntPtr.Zero, layout), this, -1) {IsAvailable = false};
		}
	}
}
#endif
