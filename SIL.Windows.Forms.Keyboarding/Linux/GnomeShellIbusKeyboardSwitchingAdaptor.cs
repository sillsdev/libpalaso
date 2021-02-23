// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for dealing with ibus keyboards on Gnome Shell (as found in Ubuntu Bionic >= 18.04)
	/// </summary>
	public class GnomeShellIbusKeyboardSwitchingAdaptor : IbusKeyboardSwitchingAdaptor
	{
		private KeyboardDescription _defaultKeyboard;

		public GnomeShellIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator)
			: base(ibusCommunicator)
		{
		}

		protected override void SelectKeyboard(KeyboardDescription keyboard)
		{
			var ibusKeyboard = (IbusKeyboardDescription) keyboard;
			var index = ibusKeyboard.SystemIndex;

			var okay = false;
			// https://askubuntu.com/a/1039964
			try
			{
				using (var proc = new Process {
					EnableRaisingEvents = false,
					StartInfo = {
						FileName = "/usr/bin/gdbus",
						Arguments =
							"call --session --dest org.gnome.Shell --object-path /org/gnome/Shell " +
							"--method org.gnome.Shell.Eval " +
							$"\"imports.ui.status.keyboard.getInputSourceManager().inputSources[{index}].activate()\"",
						UseShellExecute = false
					}
				})
				{
					proc.Start();
					proc.WaitForExit();
					okay = proc.ExitCode == 0;
				}
			}
			finally
			{
				if (!okay)
				{
					var msg =
						$"GnomeShellIbusKeyboardSwitchingAdaptor.SelectKeyboard({index}) failed";
					Console.WriteLine(msg);
					Logger.WriteEvent(msg);
				}
			}
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
	}
}