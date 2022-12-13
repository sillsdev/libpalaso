// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SIL.PlatformUtilities;
using SIL.Progress;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for dealing with ibus keyboards on Gnome Shell (as found in Ubuntu Bionic >= 18.04)
	/// </summary>
	public class GnomeShellIbusKeyboardSwitchingAdaptor: IbusKeyboardSwitchingAdaptor
	{
		private KeyboardDescription _defaultKeyboard;

		public GnomeShellIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator)
			: base(ibusCommunicator)
		{
		}

		protected override void SelectKeyboard(KeyboardDescription keyboard)
		{
			var ibusKeyboard = (IbusKeyboardDescription) keyboard;
			SetGnomeKeyboard(ibusKeyboard);
		}

		/// <summary>
		/// This adaptor doesn't make use of XkbKeyboardDefinition objects, so we have to
		/// figure out the default keyboard ourself. The default keyboard is the first keyboard
		/// in the input-sources list.
		/// </summary>
		public override KeyboardDescription DefaultKeyboard => _defaultKeyboard;

		/// <summary>
		/// Implementation is not required because the default implementation of KeyboardController
		/// is sufficient.
		/// </summary>
		public override KeyboardDescription ActiveKeyboard => null;

		public void SetDefaultKeyboard(KeyboardDescription keyboard)
		{
			_defaultKeyboard = keyboard;
		}

		/// <summary>
		/// Change the keyboard input method by limiting the scope of available input methods to
		/// the chosen one, and then restore the original list. In Gnome 42, this updates the
		/// panel icon for keyboard input methods, as well as setting the keyboard.
		/// </summary>
		/// <remarks>
		/// In Gnome 41, the ability to run org.gnome.Shell.Eval was changed, so we can not run
		/// getInputSourceManager().inputSources[n].activate(). In addition to setting
		/// input-sources, other solutions could include using a Gnome extension (eg
		/// https://askubuntu.com/a/1428946 , https://github.com/ramottamado/eval-gjs), or
		/// changing the engine by running ibus (eg `ibus engine table:thai`). Running ibus
		/// works, but doesn't update the Gnome panel, and may result in other poor
		/// experiences.
		/// </remarks>
		private void SetGnomeKeyboard(IbusKeyboardDescription keyboard)
		{
			string[] configuredInputSources = UnityKeyboardRetrievingHelper.GetMyKeyboards();
			// If we set only the desired keyboard, the panel keyboarding icon is removed,
			// causing a noticeable flicker when it disappears and a new one reappears.
			// Instead, set the reduced scope list to the desired keyboard first, and an
			// additional keyboard. The additional keyboard can't just be a duplicate of
			// the first to still affect.
			string desiredKeyboardAfront = $"[{keyboard.GnomeInputSourceIdentifier}, ('xkb', 'ie')]";
			string program = "gsettings";
			string arguments = $"set org.gnome.desktop.input-sources sources";
			KeyboardRetrievingHelper.RunOnHostEvenIfFlatpak(program,
				$"{arguments} \"{desiredKeyboardAfront}\"");
			// Set keyboards back. Note that one source (https://unix.stackexchange.com/a/711918) suggested to
			// briefly pause between the two settings. This does not appear to be needed in Ubuntu 22.04.
			KeyboardRetrievingHelper.RunOnHostEvenIfFlatpak(program,
				$"{arguments} \"{ToInputSourcesFormat(configuredInputSources)}\"");
		}

		/// <summary>
		/// Transform a list of keyboards into the a(ss) format expected by
		/// input-sources. For example,
		/// ["xkb;;us", "ibus;;table:thai"] becomes
		/// "[('xkb', 'us'), ('ibus', 'table:thai')]".
		/// </summary>
		internal static string ToInputSourcesFormat(string[] input)
		{
			if (input == null || input.Length < 1)
				return "[]";
			IEnumerable<string> keyboards = input.Select((string item) =>
			{
				string[] parts = item.Split(new string[]{";;"}, StringSplitOptions.None);
				return $"('{parts[0]}', '{parts[1]}')";
			});
			return $"[{string.Join(", ", keyboards)}]";
		}
	}
}