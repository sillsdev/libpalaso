// Copyright (c) 2011-2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Icu;
using SIL.Windows.Forms.Keyboarding.Linux;

// ReSharper disable once CheckNamespace
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
			public string LocaleId => string.IsNullOrEmpty(CountryCode) ? LanguageCode : LanguageCode + "-" + CountryCode;

			/// <summary>
			/// Gets or sets the 2-letter language abbreviation (mostly ISO 639-1).
			/// </summary>
			public string LanguageCode { get; internal set;}

			/// <summary>
			/// Gets the language name in the culture of the current thread
			/// </summary>
			public string Language => new Locale(LocaleId).DisplayLanguage;

			/// <summary>
			/// Gets or sets the country code (mostly 2-letter codes).
			/// </summary>
			public string CountryCode { get; internal set; }

			/// <summary>
			/// Gets the country name in the culture of the current thread
			/// </summary>
			public string Country => new Locale(LocaleId).DisplayCountry;

			public override string ToString()
			{
				return $"[LayoutDescription: LayoutId={LayoutId}, Description={Description}, " +
						$"LayoutVariant={LayoutVariant}, Locale={LocaleId}, LanguageCode={LanguageCode}, Language={Language}, " +
						$"CountryCode={CountryCode}, Country={Country}]";
			}
		}
		#endregion

		private Dictionary<string, List<LayoutDescription>> _layouts;

		public static XklConfigRegistry Create(IXklEngine engine)
		{
			var configRegistry = xkl_config_registry_get_instance(engine.Engine);
			if (!xkl_config_registry_load(configRegistry, true))
				throw new ApplicationException($"Got error trying to load config registry: {engine.LastError}");
			return new XklConfigRegistry(configRegistry);
		}

		private IntPtr ConfigRegistry { get; }

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
				if (_layouts == null)
				{
					_layouts = new Dictionary<string, List<LayoutDescription>>();
					// add layouts with standard language code (in /usr/share/locale)
					xkl_config_registry_foreach_language(ConfigRegistry,
						ProcessLanguage, IntPtr.Zero);
					// add layouts with nonstandard language code, but standard country code
					xkl_config_registry_foreach_country(ConfigRegistry,
						ProcessCountry, IntPtr.Zero);
					// add layouts with nonstandard language code and nonstandard country code
					xkl_config_registry_foreach_layout(ConfigRegistry,
						ProcessMainLayout, IntPtr.Zero);
				}
				return _layouts;
			}
		}

		private static string Get2LetterLanguageCode(string langCode3Letter)
		{
			return new Locale(AlternateLanguageCodes.GetLanguageCode(langCode3Letter)).Language;
		}

		private void ProcessLanguage(IntPtr configRegistry, ref XklConfigItem item, IntPtr unused)
		{
			var dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(item));
			try
			{
				Marshal.StructureToPtr(item, dataPtr, false);
				xkl_config_registry_foreach_language_variant(configRegistry, item.Name,
					ProcessOneLayoutForLanguage, dataPtr);
			}
			finally
			{
				Marshal.FreeHGlobal(dataPtr);
			}
		}

		private void ProcessOneLayoutForLanguage(IntPtr configRegistry, ref XklConfigItem item,
			ref XklConfigItem subitem, IntPtr data)
		{
			var subitemIsNull = subitem.Parent.RefCount == IntPtr.Zero;
			var language = (XklConfigItem)Marshal.PtrToStructure(data, typeof(XklConfigItem));
			var description = subitemIsNull ? item.Description : subitem.Description;
			var name = subitemIsNull ? item.Name : subitem.Name;
			var variant = subitemIsNull ? string.Empty : subitem.Description;
			var layouts = GetLayoutList(description);
			var newLayout = new LayoutDescription {
				LayoutId = name,
				Description = description,
				LayoutVariant = variant,
				LanguageCode = Get2LetterLanguageCode(language.Name),
				CountryCode = item.Name.ToUpper()
			};
			layouts.Add(newLayout);
		}

		private void ProcessCountry(IntPtr configRegistry, ref XklConfigItem item, IntPtr unused)
		{
			var dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(item));
			try
			{
				Marshal.StructureToPtr(item, dataPtr, false);
				xkl_config_registry_foreach_country_variant(configRegistry, item.Name,
					ProcessOneLayoutForCountry, dataPtr);
			}
			finally
			{
				Marshal.FreeHGlobal(dataPtr);
			}
		}

		private void ProcessOneLayoutForCountry(IntPtr configRegistry, ref XklConfigItem item,
			ref XklConfigItem subitem, IntPtr data)
		{
			var subitemIsNull = subitem.Parent.RefCount == IntPtr.Zero;
			var country = (XklConfigItem)Marshal.PtrToStructure(data, typeof(XklConfigItem));
			var description = subitemIsNull ? item.Description : subitem.Description;
			var name = subitemIsNull ? item.Name : subitem.Name;
			var variant = subitemIsNull ? string.Empty : subitem.Description;
			var layouts = GetLayoutList(description);
			if (layouts.Any(desc => desc.LayoutId == name && desc.Description == description && desc.LayoutVariant == variant))
				return;

			var langCode = subitemIsNull ? item.Short_Description : subitem.Short_Description;
			if (string.IsNullOrEmpty(langCode))
				langCode = subitemIsNull ? item.Name : subitem.Name;
			var newLayout = new LayoutDescription {
				LayoutId = name,
				Description = description,
				LayoutVariant = variant,
				LanguageCode = Get2LetterLanguageCode(langCode),
				CountryCode = country.Name
			};
			layouts.Add(newLayout);
		}

		private void ProcessMainLayout(IntPtr configRegistry, ref XklConfigItem item, IntPtr data)
		{
			var dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(item));
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

		private void StoreLayoutInfo(XklConfigItem item, IntPtr data)
		{
			var description = item.Description;
			var variant = data != IntPtr.Zero ? description : string.Empty;
			var layouts = GetLayoutList(description);
			if (layouts.Any(desc => desc.LayoutId == item.Name && desc.Description == description && desc.LayoutVariant == variant))
				return;

			var newLayout = new LayoutDescription {
				LayoutId = item.Name,
				Description = description,
				LayoutVariant = variant
			};
			if (data != IntPtr.Zero)
			{
				var parent = (XklConfigItem)Marshal.PtrToStructure(data, typeof(XklConfigItem));
				var langCode = string.IsNullOrEmpty(item.Short_Description) ? parent.Short_Description : item.Short_Description;
				if (string.IsNullOrEmpty(langCode))
					langCode = string.IsNullOrEmpty(item.Name) ? parent.Name : item.Name;
				newLayout.LanguageCode = Get2LetterLanguageCode(langCode);
				if (parent.Name.Length == 2 || item.Name != item.Short_Description)
					newLayout.CountryCode = parent.Name.ToUpper();
			}
			else
			{
				newLayout.LanguageCode = Get2LetterLanguageCode(string.IsNullOrEmpty(item.Short_Description) ? item.Name : item.Short_Description);
				if (item.Name.Length == 2 || item.Name != item.Short_Description)
					newLayout.CountryCode = item.Name.ToUpper();
			}
			layouts.Add(newLayout);
		}

		private List<LayoutDescription> GetLayoutList(string description)
		{
			List<LayoutDescription> layouts;
			if (_layouts.ContainsKey(description))
			{
				layouts = _layouts[description];
			}
			else
			{
				layouts = new List<LayoutDescription>();
				_layouts[description] = layouts;
			}
			return layouts;
		}

		#region p/invoke related
		// ReSharper disable FieldCanBeMadeReadOnly.Local
		// ReSharper disable MemberCanBePrivate.Local
		// ReSharper disable InconsistentNaming
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
		private struct XklConfigItem
		{
			private const int XKL_MAX_CI_NAME_LENGTH = 32;
			// private const int XKL_MAX_CI_SHORT_DESC_LENGTH = 10;
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
		// ReSharper restore FieldCanBeMadeReadOnly.Local
		// ReSharper restore MemberCanBePrivate.Local
		// ReSharper restore InconsistentNaming

		private delegate void ConfigItemProcessFunc(IntPtr configRegistry, ref XklConfigItem item, IntPtr data);
		private delegate void TwoConfigItemsProcessFunc(IntPtr configRegistry,
			ref XklConfigItem item, ref XklConfigItem subitem, IntPtr data);


		[DllImport("libxklavier")]
		private static extern IntPtr xkl_config_registry_get_instance(IntPtr engine);

		[DllImport("libxklavier")]
		private static extern bool xkl_config_registry_load(IntPtr configRegistry, bool fExtrasNeeded);

		[DllImport("libxklavier")]
		private static extern void xkl_config_registry_foreach_language(IntPtr configRegistry,
			ConfigItemProcessFunc func, IntPtr data);

		[DllImport("libxklavier")]
		private static extern void xkl_config_registry_foreach_language_variant(IntPtr configRegistry,
			string languageCode, TwoConfigItemsProcessFunc func, IntPtr data);

		[DllImport("libxklavier")]
		private static extern void xkl_config_registry_foreach_country(IntPtr configRegistry,
			ConfigItemProcessFunc func, IntPtr data);

		[DllImport("libxklavier")]
		private static extern void xkl_config_registry_foreach_country_variant(IntPtr configRegistry,
			string countryCode, TwoConfigItemsProcessFunc func, IntPtr data);

		[DllImport("libxklavier")]
		private static extern void xkl_config_registry_foreach_layout(IntPtr configRegistry,
			ConfigItemProcessFunc func, IntPtr data);

		[DllImport("libxklavier")]
		private static extern void xkl_config_registry_foreach_layout_variant(IntPtr configRegistry,
			string layoutCode, ConfigItemProcessFunc func, IntPtr data);
		#endregion
	}
}
