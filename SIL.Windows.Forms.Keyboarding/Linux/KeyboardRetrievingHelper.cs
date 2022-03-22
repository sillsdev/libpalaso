// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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

		/// <summary>
		/// Convert a GVariant handle that points to a list of strings to a C# string array.
		/// Without leaking memory in the process!
		/// </summary>
		/// <remarks>
		/// No check is made to verify that the input value actually points to a list of strings.
		/// </remarks>
		public static string[] GetStringArrayFromGVariantArray(IntPtr value)
		{
			if (value == IntPtr.Zero)
				return new string[0];
			uint size = Unmanaged.g_variant_n_children(value);
			string[] list = new string[size];
			for (uint i = 0; i < size; ++i)
			{
				IntPtr child = Unmanaged.g_variant_get_child_value(value, i);
				int length;
				// handle must not be freed -- it points into the actual GVariant memory for child!
				IntPtr handle = Unmanaged.g_variant_get_string(child, out length);
				var rawbytes = new byte[length];
				Marshal.Copy(handle, rawbytes, 0, length);
				list[i] = Encoding.UTF8.GetString(rawbytes);
				Unmanaged.g_variant_unref(child);
				//Console.WriteLine("DEBUG GetStringArrayFromGVariant(): list[{0}] = \"{1}\" (length = {2})", i, list[i], length);
			}
			return list;
		}

		/// <summary>
		/// Convert a GVariant handle that points to a list of lists of two strings into
		/// a C# string array.  The original strings in the inner lists are separated by
		/// double semicolons in the output elements of the C# string array.
		/// </summary>
		/// <remarks>
		/// No check is made to verify that the input value actually points to a list of
		/// lists of two strings.
		/// </remarks>
		public static string[] GetStringArrayFromGVariantListArray(IntPtr value)
		{
			if (value == IntPtr.Zero)
				return new string[0];
			uint size = Unmanaged.g_variant_n_children(value);
			string[] list = new string[size];
			for (uint i = 0; i < size; ++i)
			{
				IntPtr duple = Unmanaged.g_variant_get_child_value(value, i);
				var values = GetStringArrayFromGVariantArray(duple);
				Debug.Assert(values.Length == 2);
				list[i] = String.Format("{0};;{1}", values[0], values[1]);
				Unmanaged.g_variant_unref(duple);
				//Console.WriteLine("DEBUG GetStringArrayFromGVariantListArray(): list[{0}] = \"{1}\"", i, list[i]);
			}
			return list;
		}

		internal static string GetKeyboardSetupApplication(out string arguments)
		{
			string prefix = "";
			if (Platform.IsFlatpak)
			{
				prefix = "/run/host";
			}
			// NOTE: if we get false results (e.g. because the user has installed multiple
			// desktop environments) we could check for the currently running desktop
			// (Platform.DesktopEnvironment) and return the matching program
			arguments = string.Empty;
			string program = string.Empty;
			// XFCE
			if (File.Exists($"{prefix}/usr/bin/xfce4-keyboard-settings"))
			{
				program = "/usr/bin/xfce4-keyboard-settings";
			}
			// Cinnamon
			else if (File.Exists($"{prefix}/usr/lib/cinnamon-settings/cinnamon-settings.py")
				&& File.Exists($"{prefix}/usr/bin/python"))
			{
				arguments = "/usr/lib/cinnamon-settings/cinnamon-settings.py " +
							(Platform.DesktopEnvironment == "cinnamon"
								? "region layouts" // Wasta 12
								: "keyboard");     // Wasta 14;
				program = "/usr/bin/python";
			}
			// Cinnamon in Wasta 20.04
			else if (File.Exists($"{prefix}/usr/bin/cinnamon-settings"))
			{
				arguments = "keyboard -t layouts";
				program = "/usr/bin/cinnamon-settings";
			}
			// GNOME
			else if (File.Exists($"{prefix}/usr/bin/gnome-control-center"))
			{
				arguments = "region layouts";
				program = "/usr/bin/gnome-control-center";
			}
			// KDE
			else if (File.Exists($"{prefix}/usr/bin/kcmshell4"))
			{
				arguments = "kcm_keyboard";
				program = "/usr/bin/kcmshell4";
			}
			// Unity
			else if (File.Exists($"{prefix}/usr/bin/unity-control-center"))
			{
				arguments = "region layouts";
				program = "/usr/bin/unity-control-center";
			}
			else
			{
				arguments = null;
				return null;
			}

			if (Platform.IsFlatpak)
			{
				ToFlatpakSpawn(ref program, ref arguments);
			}
			return program;
		}

		/// <summary>
		/// Modify program and arguments to use flatpak-spawn for requesting to launch a program
		/// outside of a flatpak container.
		/// </summary>
		internal static void ToFlatpakSpawn(ref string program, ref string arguments)
		{
			arguments = $"--host --directory=/ {program} {arguments}";
			program = "flatpak-spawn";
		}
	}
}
