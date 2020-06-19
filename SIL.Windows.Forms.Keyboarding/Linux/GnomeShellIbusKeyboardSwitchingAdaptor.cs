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
	public class GnomeShellIbusKeyboardSwitchingAdaptor: UnityIbusKeyboardSwitchingAdaptor, IUnityKeyboardSwitchingAdaptor
	{
		public GnomeShellIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator)
			: base(ibusCommunicator)
		{
		}

		void IUnityKeyboardSwitchingAdaptor.SelectKeyboard(uint index)
		{
			var okay = false;
			// https://askubuntu.com/a/1039964
			try
			{
				using (var proc = new Process {
					EnableRaisingEvents = false,
					StartInfo = {
						FileName = "/usr/bin/gdbus",
						Arguments = "call --session --dest org.gnome.Shell --object-path /org/gnome/Shell " +
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
					var msg = $"GnomeShellIbusKeyboardSwitchingAdaptor.SelectKeyboard({index}) failed";
					Console.WriteLine(msg);
					Logger.WriteEvent(msg);
				}
			}
		}

	}
}