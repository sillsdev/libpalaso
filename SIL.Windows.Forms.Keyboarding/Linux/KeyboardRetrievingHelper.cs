// Copyright (c) 2015-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	public static class KeyboardRetrievingHelper
	{
		public static void AddIbusVersionAsErrorReportProperty()
		{
			var settingsGeneral = IntPtr.Zero;
			try
			{
				const string ibusSchema = "org.freedesktop.ibus.general";
				if (!GlibHelper.SchemaIsInstalled(ibusSchema))
					return;

				settingsGeneral = Unmanaged.g_settings_new(ibusSchema);
				if (settingsGeneral == IntPtr.Zero)
					return;

				var version = Unmanaged.g_settings_get_string(settingsGeneral, "version");
				ErrorReport.AddProperty("IbusVersion", version);
			}
			catch
			{
				// Ignore any error we might get
			}
			finally
			{
				if (settingsGeneral != IntPtr.Zero)
					Unmanaged.g_object_unref(settingsGeneral);
			}
		}

		internal static string GetKeyboardSetupApplication(out string arguments)
		{
			// NOTE: if we get false results (e.g. because the user has installed multiple
			// desktop environments) we could check for the currently running desktop
			// (Platform.DesktopEnvironment) and return the matching program
			arguments = null;
			// XFCE
			if (File.Exists("/usr/bin/xfce4-keyboard-settings"))
				return "/usr/bin/xfce4-keyboard-settings";

			// Cinnamon
			if (File.Exists("/usr/lib/cinnamon-settings/cinnamon-settings.py") &&
				File.Exists("/usr/bin/python"))
			{
				arguments = "/usr/lib/cinnamon-settings/cinnamon-settings.py " +
							(Platform.DesktopEnvironment == "cinnamon"
								? "region layouts" // Wasta 12
								: "keyboard");     // Wasta 14;
				return "/usr/bin/python";
			}

			// Cinnamon in Wasta 20.04
			if (File.Exists("/usr/bin/cinnamon-settings"))
			{
				arguments = "keyboard -t layouts";
				return "/usr/bin/cinnamon-settings";
			}

			// GNOME
			if (File.Exists("/usr/bin/gnome-control-center"))
			{
				arguments = "region layouts";
				return "/usr/bin/gnome-control-center";
			}

			// KDE
			if (File.Exists("/usr/bin/kcmshell4"))
			{
				arguments = "kcm_keyboard";
				return "/usr/bin/kcmshell4";
			}

			// Unity
			if (File.Exists("/usr/bin/unity-control-center"))
			{
				arguments = "region layouts";
				return "/usr/bin/unity-control-center";
			}

			return null;
		}
	}
}
