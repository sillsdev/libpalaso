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
using System.Globalization;
using System.Runtime.InteropServices;
using Icu;

namespace X11.XKlavier
{
	/// <summary>
	/// Provides access to the xklavier XKB config registry methods which provide access to
	/// the keyboard layouts.
	/// </summary>
	internal class XklConfigRegistry
	{
		#region struct LayoutDescription
		/// <summary>
		/// XKB keyboard layout description
		/// </summary>
		public struct LayoutDescription
		{
			/// <summary>
			/// Gets or sets the layout identifier.
			/// </summary>
			/// <remarks>The layout identifier consists of the layout name and variant, separated
			/// by a tab character. Example: "us\tintl".</remarks>
			public string LayoutId { get; internal set; }

			/// <summary>
			/// Gets or sets the description of the layout as found in XklConfigItem. It consists
			/// of the country and the variant, separated by a hyphen.
			/// Example:"USA - International".
			/// </summary>
			public string Description { get; internal set; }

			/// <summary>
			/// Gets or sets the keyboard layout variant, e.g. "International".
			/// </summary>
			public string LayoutVariant { get; internal set; }

			/// <summary>
			/// Gets the locale for the current layout. Language and country code are
			/// separated by '-'.
			/// </summary>
			/// <remarks>The ICU documentation says that the components should be separated by
			/// an underscore, but that contradicts the way Windows does it. And ICU seems
			/// to understand the '-' as well.</remarks>
			public string LocaleId
			{
				get { return LanguageCode + "-" + CountryCode; }
			}

			/// <summary>
			/// Gets or sets the 2-letter language abbreviation (mostly ISO 639-1).
			/// </summary>
			public string LanguageCode { get; internal set;}

			/// <summary>
			/// Gets the language name in the culture of the current thread
			/// </summary>
			public string Language
			{
				get
				{
					return new Locale(LocaleId).DisplayLanguage;
				}
			}

			/// <summary>
			/// Gets or sets the country code (mostly 2-letter codes).
			/// </summary>
			public string CountryCode { get; internal set; }

			/// <summary>
			/// Gets the country name in the culture of the current thread
			/// </summary>
			public string Country
			{
				get
				{
					return new Locale(LocaleId).DisplayCountry;
				}
			}

			public override string ToString()
			{
				return string.Format("[LayoutDescription: LayoutId={0}, Description={1}, " +
					"LayoutVariant={2}, Locale={3}, LanguageCode={4}, Language={5}, " +
					"CountryCode={6}, Country={7}]", LayoutId, Description, LayoutVariant,
					LocaleId, LanguageCode, Language, CountryCode, Country);
			}
		}
		#endregion

		#region Alternative language codes
		// ICU uses the ISO 639-3 language codes; xkb has at least some ISO 639-2/B codes.
		// According to http://en.wikipedia.org/wiki/ISO_639-2#B_and_T_codes there are 20 languages
		// that have both B and T codes, so we need to translate those.
		private static Dictionary<string, string> s_AlternateLanguageCodes;

		private Dictionary<string, string> AlternateLanguageCodes
		{
			get
			{
				if (s_AlternateLanguageCodes == null)
				{
					s_AlternateLanguageCodes = new Dictionary<string, string>();
					s_AlternateLanguageCodes["alb"] = "sqi"; // Albanian
					s_AlternateLanguageCodes["arm"] = "hye"; // Armenian
					s_AlternateLanguageCodes["baq"] = "eus"; // Basque
					s_AlternateLanguageCodes["bur"] = "mya"; // Burmese
					s_AlternateLanguageCodes["chi"] = "zho"; // Chinese
					s_AlternateLanguageCodes["cze"] = "ces"; // Czech
					s_AlternateLanguageCodes["dut"] = "nld"; // Dutch, Flemish
					s_AlternateLanguageCodes["fre"] = "fra"; // French
					s_AlternateLanguageCodes["geo"] = "kat"; // Georgian
					s_AlternateLanguageCodes["ger"] = "deu"; // German
					s_AlternateLanguageCodes["gre"] = "ell"; // Modern Greek (1453–)
					s_AlternateLanguageCodes["ice"] = "isl"; // Icelandic
					s_AlternateLanguageCodes["mac"] = "mkd"; // Macedonian
					s_AlternateLanguageCodes["mao"] = "mri"; // Maori
					s_AlternateLanguageCodes["may"] = "msa"; // Malay
					s_AlternateLanguageCodes["per"] = "fas"; // Persian
					s_AlternateLanguageCodes["rum"] = "ron"; // Romanian
					s_AlternateLanguageCodes["slo"] = "slk"; // Slovak
					s_AlternateLanguageCodes["tib"] = "bod"; // Tibetan
					s_AlternateLanguageCodes["wel"] = "cym"; // Welsh
				}
				return s_AlternateLanguageCodes;
			}
		}
		#endregion

		private Dictionary<string, List<LayoutDescription>> m_Layouts;

		public static XklConfigRegistry Create(IXklEngine engine)
		{
			var configRegistry = xkl_config_registry_get_instance(engine.Engine);
			if (!xkl_config_registry_load(configRegistry, true))
				throw new ApplicationException("Got error trying to load config registry: " + engine.LastError);
			return new XklConfigRegistry(configRegistry);
		}

		internal IntPtr ConfigRegistry { get; private set; }

		private XklConfigRegistry(IntPtr configRegistry)
		{
			ConfigRegistry = configRegistry;
		}

		/// <summary>
		/// Gets all possible keyboard layouts defined in the system (though not necessarily
		/// installed).
		/// </summary>
		public Dictionary<string, List<LayoutDescription>> Layouts
		{
			get
			{
				if (m_Layouts == null)
				{
					m_Layouts = new Dictionary<string, List<LayoutDescription>>();
					xkl_config_registry_foreach_layout(ConfigRegistry,
						ProcessMainLayout, IntPtr.Zero);
				}
				return m_Layouts;
			}
		}

		private string Get2LetterLanguageCode(string langCode3letter)
		{
			string newLangCode;
			if (AlternateLanguageCodes.TryGetValue(langCode3letter, out newLangCode))
				langCode3letter = newLangCode;
			return new Locale(langCode3letter).Language;
		}

		private void ProcessMainLayout(IntPtr configRegistry, ref XklConfigItem item, IntPtr data)
		{
			IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(item));
			try
			{
				StoreLayoutInfo(item, data);
				Marshal.StructureToPtr(item, dataPtr, false);
				xkl_config_registry_foreach_layout_variant(configRegistry, item.Name,
					ProcessLayoutVariant, dataPtr);
			}
			finally
			{
				Marshal.FreeHGlobal(dataPtr);
			}
		}

		private void ProcessLayoutVariant(IntPtr configRegistry, ref XklConfigItem item, IntPtr data)
		{
			StoreLayoutInfo(item, data);
		}

		void StoreLayoutInfo(XklConfigItem item, IntPtr data)
		{
			var description = item.Description;
			List<LayoutDescription> layouts;
			if (m_Layouts.ContainsKey(description))
			{
				layouts = m_Layouts[description];
			}
			else
			{
				layouts = new List<LayoutDescription>();
				m_Layouts[description] = layouts;
			}
			var newLayout = new LayoutDescription {
				LayoutId = item.Name,
				Description = description,
				LayoutVariant = String.Empty,
				CountryCode = item.Name.ToUpper()
			};
			if (data != IntPtr.Zero)
			{
				XklConfigItem layout = (XklConfigItem)Marshal.PtrToStructure(data, typeof(XklConfigItem));
				newLayout.LanguageCode = Get2LetterLanguageCode(layout.Name);
			}
			layouts.Add(newLayout);
		}

		#region p/invoke related
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
		private struct XklConfigItem
		{
			private const int XKL_MAX_CI_NAME_LENGTH = 32;
			private const int XKL_MAX_CI_SHORT_DESC_LENGTH = 10;
			private const int XKL_MAX_CI_DESC_LENGTH = 192;

			[StructLayout(LayoutKind.Sequential)]
			public struct GObject
			{
				public IntPtr Class;
				public IntPtr RefCount;
				public IntPtr Data;
			}

			public GObject Parent;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=XKL_MAX_CI_NAME_LENGTH)]
			public string Name;
			// Setting the length to XKL_MAX_CI_DESC_LENGTH looks like a bug in the header file
			// (/usr/include/libxklavier/xkl_config_item.h)
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=XKL_MAX_CI_DESC_LENGTH)]
			public string Short_Description;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=XKL_MAX_CI_DESC_LENGTH)]
			public string Description;
		}

		private delegate void ConfigItemProcessFunc(IntPtr configRegistry, ref XklConfigItem item, IntPtr data);

		[DllImport("libxklavier")]
		private extern static IntPtr xkl_config_registry_get_instance(IntPtr engine);

		[DllImport("libxklavier")]
		private extern static bool xkl_config_registry_load(IntPtr configRegistry, bool fExtrasNeeded);

		[DllImport("libxklavier")]
		private extern static void xkl_config_registry_foreach_layout(IntPtr configRegistry,
			ConfigItemProcessFunc func, IntPtr data);

		[DllImport("libxklavier")]
		private extern static void xkl_config_registry_foreach_layout_variant(IntPtr configRegistry,
			string layoutCode, ConfigItemProcessFunc func, IntPtr data);
		#endregion
	}
}
#endif
