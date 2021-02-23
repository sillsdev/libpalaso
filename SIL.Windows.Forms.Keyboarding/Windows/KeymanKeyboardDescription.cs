// Copyright (c) 2014-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Security;
using Microsoft.Win32;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Keyboard description for a Keyman keyboard not associated with a windows language
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	internal class KeymanKeyboardDescription : KeyboardDescription
	{
		private static readonly bool KeymanKeyboardSwitchingSettingEnabled;

		static KeymanKeyboardDescription()
		{
			KeymanKeyboardSwitchingSettingEnabled = GetEvilKeymanKeyboardSwitchingSetting();
		}

		/// <summary>
		/// Keyman has an option to change the system keyboard with a keyman keyboard change.
		/// The default for this setting is on (which is what Paratext expects). However,
		/// FieldWorks doesn't work correctly with this setting on and actually turns it off
		/// each time it is run. What this means is that when this setting is off, we need
		/// to explicitly turn off any keyboard engines not switched. This is slightly different
		/// from our normal behavior which is to just turn off other engines when switching to the
		/// default keyboard (FB-28320)
		/// </summary>
		/// <returns>True if the setting is enabled, false if the setting is disabled for any version of
		/// Keyman that might be installed (or have been installed).</returns>
		private static bool GetEvilKeymanKeyboardSwitchingSetting()
		{
			try
			{
				using (RegistryKey engineKey = Registry.CurrentUser.OpenSubKey(@"Software\Tavultesoft\Keyman Engine", false))
				{
					if (engineKey == null)
						return true; // The default setting if it's installed, but it's not installed...

					foreach (string version in engineKey.GetSubKeyNames())
					{
						using (RegistryKey keyVersion = engineKey.OpenSubKey(version, false))
						{
							object value = keyVersion?.GetValue("switch language with keyboard");
							if (value != null && (int)value == 0)
								return false; // Setting was found to be off in at least one of the installed versions
						}
					}
				}
			}
			catch (SecurityException)
			{
				// We shouldn't get this since we are opening for read-only, but you never know...
			}
			return true;
		}

		/// <summary/>
		internal KeymanKeyboardDescription(string layout, bool isKeyman6, KeymanKeyboardAdaptor engine, bool isAvailable)
			: base(layout, layout, layout, string.Empty, isAvailable, engine.SwitchingAdaptor)
		{
			IsKeyman6 = isKeyman6;
		}

		internal bool IsKeyman6 { get; set; }

		/// <summary>
		/// Indicates whether we should pass NFC or NFD data to the keyboard. This implementation
		/// always returns <c>false</c>.
		/// </summary>
		public override bool UseNfcContext => false;

		/// <summary>
		/// If the new keyboard is the default windows keyboard then we need to deactivate the Keyman
		/// keyboard without resetting the windows keyboard. However, if the default keyboard is a Keyman
		/// keyboard associated with the system default keyboard, then don't reset the Keyman keyboard as
		/// that causes the association to appear as if it's not there due to a Keyman timing issue.
		/// </summary>
		protected override bool DeactivatePreviousKeyboard(IKeyboardDefinition keyboardToActivate)
		{
			return !KeymanKeyboardSwitchingSettingEnabled || keyboardToActivate == KeyboardController.Instance.DefaultKeyboard;
		}

		internal void SetIsAvailable(bool isAvailable)
		{
			IsAvailable = isAvailable;
		}
	}
}
