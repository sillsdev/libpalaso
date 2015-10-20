// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Microsoft.Win32;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Windows
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Keyboard description for a Keyman keyboard not associated with a windows language
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[SuppressMessage("Gendarme.Rules.Design", "TypesWithNativeFieldsShouldBeDisposableRule",
		Justification = "WindowHandle is a reference to a control")]
	internal class KeymanKeyboardDescription : KeyboardDescription
	{
		private static bool s_keymanKeyboardSwitchingSettingEnabled;

		public bool IsKeyman6 { get; private set; }

		static KeymanKeyboardDescription()
		{
			s_keymanKeyboardSwitchingSettingEnabled = GetEvilKeymanKeyboardSwitchingSetting();
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Windows.KeymanKeyboardDescription"/> class.
		/// </summary>
		public KeymanKeyboardDescription(string layout, bool isKeyman6, IKeyboardSwitchingAdaptor engine)
			: base(engine, KeyboardType.OtherIm)
		{
			InternalName = layout;
			Layout = layout;
			IsKeyman6 = isKeyman6;
		}

		internal KeymanKeyboardDescription(KeymanKeyboardDescription other): base(other)
		{
			IsKeyman6 = other.IsKeyman6;
		}

		/// <summary>
		/// Indicates whether we should pass NFC or NFD data to the keyboard. This implementation
		/// always returns <c>false</c>.
		/// </summary>
		public override bool UseNfcContext { get { return false; } }

		public override IKeyboardDefinition Clone()
		{
			return new KeymanKeyboardDescription(this);
		}
	
		/// <summary>
		/// If the new keyboard is the default windows keyboard then we need to deactivate the Keyman 
		/// keyboard without resetting the windows keyboard. However, if the default keyboard is a Keyman 
		/// keyboard associated with the system default keyboard, then don't reset the Keyman keyboard as
		/// that causes the association to appear as if it's not there due to a Keyman timing issue.
		/// </summary>
		protected override bool DeactivatePreviousKeyboard(IKeyboardDefinition keyboardToActivate)
		{
			return (!s_keymanKeyboardSwitchingSettingEnabled ||
				keyboardToActivate.Equals(((IKeyboardControllerImpl)Keyboard.Controller).DefaultKeyboard));
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
							if (keyVersion != null)
							{
								object value = keyVersion.GetValue("switch language with keyboard");
								if (value != null && (int)value == 0)
									return false; // Setting was found to be off in at least one of the installed versions
							}
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
	}
}
#endif
