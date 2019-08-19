// Copyright (c) 2013-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary>
	/// Static methods used with keyboard adapters and switching adapters on windows
	/// </summary>
	internal class WinKeyboardUtils
	{
		internal static LayoutName GetLayoutNameEx(IntPtr handle)
		{
			// InputLanguage.LayoutName is not to be trusted, especially where there are mutiple
			// layouts (input methods) associated with a language. This function also provides
			// the additional benefit that it does not matter whether a user switches from using
			// InKey in Portable mode to using it in Installed mode (perhaps as the project is
			// moved from one computer to another), as this function will identify the correct
			// input language regardless, rather than (unhelpfully ) calling an InKey layout in
			// portable mode the "US" layout. The layout is identified soley by the high-word of
			// the HKL (a.k.a. InputLanguage.Handle).  (The low word of the HKL identifies the
			// language.)
			// This function determines an HKL's LayoutName based on the following order of
			// precedence:
			// - Look up HKL in HKCU\\Software\\InKey\\SubstituteLayoutNames
			// - Look up extended layout in HKLM\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts
			// - Look up basic (non-extended) layout in HKLM\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts
			// -Scan for ID of extended layout in HKLM\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts
			var hkl = string.Format("{0:X8}", (ulong)handle & 0x000000000fffffffUL);

			// Get substitute first
			var substituteHkl = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Keyboard Layout\Substitutes", hkl, null);
			if (!string.IsNullOrEmpty(substituteHkl))
				hkl = substituteHkl;

			// Check InKey
			var substituteLayoutName = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\InKey\SubstituteLayoutNames", hkl, null);
			if (!string.IsNullOrEmpty(substituteLayoutName))
				return new LayoutName(substituteLayoutName);

			var layoutName = GetLayoutNameFromKey(string.Concat(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\", hkl));
			if (layoutName != null)
				return layoutName;

			layoutName = GetLayoutNameFromKey(string.Concat(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\0000",
				hkl.Substring(0, 4)));
			if (layoutName != null)
				return layoutName;

			using (var regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Keyboard Layouts"))
			{
				if (regKey == null)
					return new LayoutName();

				string layoutId = "0" + hkl.Substring(1, 3);
				foreach (string subKeyName in regKey.GetSubKeyNames().Reverse())
					// Scan in reverse order for efficiency, as the extended layouts are at the end.
				{
					using (var klid = regKey.OpenSubKey(subKeyName))
					{
						if (klid == null)
							continue;

						var layoutIdSk = ((string)klid.GetValue("Layout ID"));
						if (layoutIdSk != null &&
						    layoutIdSk.Equals(layoutId, StringComparison.InvariantCultureIgnoreCase))
						{
							return GetLayoutNameFromKey(klid.Name);
						}
					}
				}
			}

			return new LayoutName();
		}

		private static LayoutName GetLayoutNameFromKey(string key)
		{
			var layoutText = (string)Registry.GetValue(key, "Layout Text", null);
			var displayName = (string)Registry.GetValue(key, "Layout Display Name", null);
			if (string.IsNullOrEmpty(layoutText) && string.IsNullOrEmpty(displayName))
				return null;

			if (string.IsNullOrEmpty(displayName))
				return new LayoutName(layoutText);

			var bldr = new StringBuilder(100);
			Win32.SHLoadIndirectString(displayName, bldr, 100, IntPtr.Zero);
			return string.IsNullOrEmpty(layoutText) ? new LayoutName(bldr.ToString()) :
				new LayoutName(layoutText, bldr.ToString());
		}

		/// <summary>
		/// Gets the keyboard description for the layout of <paramref name="inputLanguage"/>.
		/// </summary>
		internal static KeyboardDescription GetKeyboardDescription(IInputLanguage inputLanguage)
		{
			KeyboardDescription sameLayout = KeyboardController.NullKeyboard;
			KeyboardDescription sameCulture = KeyboardController.NullKeyboard;
			// TODO: write some tests
			string requestedLayout = GetLayoutNameEx(inputLanguage.Handle).Name;
			foreach (WinKeyboardDescription keyboardDescription in KeyboardController.Instance.AvailableKeyboards.OfType<WinKeyboardDescription>())
			{
				try
				{
					if (requestedLayout == keyboardDescription.Layout)
					{
						if (keyboardDescription.Locale == inputLanguage.Culture.Name)
							return keyboardDescription;
						if (sameLayout == null)
							sameLayout = keyboardDescription;
					}
					else if (keyboardDescription.Locale == inputLanguage.Culture.Name && sameCulture == null)
						sameCulture = keyboardDescription;
				}
				catch (CultureNotFoundException)
				{
					// we get an exception for non-supported cultures, probably because of a
					// badly applied .NET patch.
					// http://www.ironspeed.com/Designer/3.2.4/WebHelp/Part_VI/Culture_ID__XXX__is_not_a_supported_culture.htm and others
				}
			}
			return sameLayout ?? sameCulture;
		}

		/// <summary>
		/// Gets the InputLanguage that has the same layout as <paramref name="keyboardDescription"/>.
		/// </summary>
		internal static InputLanguage GetInputLanguage(WinKeyboardDescription keyboardDescription)
		{
			InputLanguage sameLayout = null;
			InputLanguage sameCulture = null;
			foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
			{
				// TODO: write some tests
				try
				{
					if (GetLayoutNameEx(lang.Handle).Name == keyboardDescription.Name)
					{
						if (keyboardDescription.Locale == lang.Culture.Name)
							return lang;
						if (sameLayout == null)
							sameLayout = lang;
					}
					else if (keyboardDescription.Locale == lang.Culture.Name && sameCulture == null)
						sameCulture = lang;
				}
				catch (CultureNotFoundException)
				{
					// we get an exception for non-supported cultures, probably because of a
					// badly applied .NET patch.
					// http://www.ironspeed.com/Designer/3.2.4/WebHelp/Part_VI/Culture_ID__XXX__is_not_a_supported_culture.htm and others
				}
			}
			return sameLayout ?? sameCulture;
		}

		public static IInputLanguage GetInputLanguage(string locale, string layout,
			out string cultureName)
		{
			IInputLanguage inputLanguage;
			try
			{
				var ci = new CultureInfo(locale);
				cultureName = ci.DisplayName;
				inputLanguage = new InputLanguageWrapper(ci, IntPtr.Zero, layout);
			}
			catch (CultureNotFoundException)
			{
				// ignore if we can't find a culture (this can happen e.g. when a language gets
				// removed that was previously assigned to a WS) - see LT-15333
				inputLanguage = null;
				cultureName = "[Unknown Language]";
			}
			return inputLanguage;
		}
	}
}