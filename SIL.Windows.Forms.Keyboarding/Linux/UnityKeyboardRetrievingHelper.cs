// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.PlatformUtilities;
using SIL.Progress;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Helper class that implements common functionality for unity keyboard retrieving
	/// </summary>
	internal class UnityKeyboardRetrievingHelper
	{
		internal class IbusKeyboardEntry
		{
			public string Type;
			public string Layout;
		}

		private Func<string[]> _GetKeyboards;

		public UnityKeyboardRetrievingHelper()
		{
			_GetKeyboards = GetMyKeyboards;
		}

		// Used in unit tests
		internal UnityKeyboardRetrievingHelper(Func<string[]> getKeyboards): this()
		{
			_GetKeyboards = getKeyboards;
		}

		#region Public methods and properties
		public bool IsApplicable
		{
			get
			{
				var list = _GetKeyboards();
				return list != null && list.Length > 0;
			}
		}

		public void InitKeyboards(Func<string, bool> keyboardTypeMatches,
			Action<IDictionary<string, uint>, IbusKeyboardEntry> registerKeyboards)
		{
			var list = _GetKeyboards();
			if (list == null || list.Length < 1)
				return;

			var installedKeyboards = new Dictionary<string, uint>();
			uint kbdIndex = 0;
			foreach (var keyboardEntry in list)
				AddMatchingKeyboard(keyboardEntry, ref kbdIndex, installedKeyboards, keyboardTypeMatches);

			var firstKeyboard = SplitKeyboardEntry(list[0]);

			registerKeyboards(installedKeyboards, firstKeyboard);
		}

		public static string GetKeyboardSetupApplication(out string arguments)
		{
			string prefix = "";
			if (Platform.IsFlatpak)
			{
				prefix = "/run/host";
			}

			string program = null;
			arguments = "region layouts";
			var programs = Platform.IsGnomeShell
				? new[] {
					"/usr/bin/gnome-control-center",
					"/usr/bin/unity-control-center"
				}
				: new[] {
					"/usr/bin/unity-control-center",
					"/usr/bin/gnome-control-center"
				};
			program = programs.FirstOrDefault((string path) => File.Exists($"{prefix}{path}"));
			if (program != null && Platform.IsFlatpak)
			{
				KeyboardRetrievingHelper.ToFlatpakSpawn(ref program, ref arguments);
			}
			return program;
		}
		#endregion

		/// <summary>
		/// Returns the list of keyboards or <c>null</c> if we can't get the combined keyboards
		/// list.
		/// </summary>
		internal static string[] GetMyKeyboards()
		{
			if (Platform.IsFlatpak)
			{
				// Querying gsettings from within flatpak needs to go thru a flatpak portal.
				return GnomeInputSourcesViaFlatpakPortal();
			}

			// This is the proper path for the combined keyboard handling, not the path
			// given in the IBus reference documentation.
			const string schema = "org.gnome.desktop.input-sources";
			if (!GlibHelper.SchemaIsInstalled(schema))
				return null;

			var settings = Unmanaged.g_settings_new(schema);
			if (settings == IntPtr.Zero)
				return null;

			// Commandline equivalent: gsettings get org.gnome.desktop.input-sources sources
			var sources = Unmanaged.g_settings_get_value(settings, "sources");
			if (sources == IntPtr.Zero)
				return null;
			string[] list = KeyboardRetrievingHelper.GetStringArrayFromGVariantListArray(sources);

			Unmanaged.g_variant_unref(sources);
			Unmanaged.g_object_unref(settings);
			return list;
		}

		/// <summary>
		/// Return list of GNOME input sources, as queried of gsettings by way of a dbus Flatpak portal.
		/// https://flatpak.github.io/xdg-desktop-portal/#gdbus-org.freedesktop.portal.Settings
		/// </summary>
		internal static string[] GnomeInputSourcesViaFlatpakPortal()
		{
			string dest = "org.freedesktop.portal.Desktop";
			string objectPath = "/org/freedesktop/portal/desktop";
			string method = "org.freedesktop.portal.Settings.Read";
			string gsettingsNamespaceAndKey = "org.gnome.desktop.input-sources sources";
			var result = SIL.CommandLineProcessing.CommandLineRunner.Run(
				"gdbus",
				$"call --session --dest {dest} --object-path {objectPath} --method {method} {gsettingsNamespaceAndKey}",
				"/", 10, new StringBuilderProgress());
			return (ParseGDBusKeyboardList(result.StandardOutput)).ToArray<string>();
		}

		/// <summary>
		/// Parses a keyboard list from gdbus into a list of keyboards.
		/// For example, parses "(<<[('xkb', 'us'), ('ibus', 'table:thai')]>>,)\n" to
		/// the list { "xkb;;us", "ibus;;table:thai" }.
		/// </summary>
		internal static List<string> ParseGDBusKeyboardList(string keyboardList)
		{
			string[] keyboards = keyboardList.Split(new string[] {"), "},
				StringSplitOptions.RemoveEmptyEntries);
			if (keyboards.Length < 1)
			{
				return new List<string>();
			}
			List<string> output = new List<string>();
			foreach(var keyboard in keyboards) {
				string[] keyboardDesignationTuple = keyboard
					.Trim("(<[']>),\n".ToCharArray())
					.Split(new string[] {"', '"}, StringSplitOptions.RemoveEmptyEntries);
				if (keyboardDesignationTuple.Length != 2)
				{
					continue;
				}
				var inputFramework = keyboardDesignationTuple[0];
				var engine = keyboardDesignationTuple[1];
				output.Add($"{inputFramework};;{engine}");
			}
			return output;
		}
		private static void AddMatchingKeyboard(string source, ref uint kbdIndex,
			IDictionary<string, uint> keyboards, Func<string, bool> keyboardTypeMatches)
		{
			var keyboardEntry = SplitKeyboardEntry(source);
			if (keyboardTypeMatches(keyboardEntry.Type) && !keyboards.ContainsKey(keyboardEntry.Layout))
				keyboards.Add(keyboardEntry.Layout, kbdIndex);
			++kbdIndex;
		}

		/// <summary>
		/// Split the keyboard entry into type and layout. If <paramref name="source"/> isn't in
		/// the expected format a default IbusKeyboardEntry is returned.
		/// </summary>
		private static IbusKeyboardEntry SplitKeyboardEntry(string source)
		{
			var parts = source.Split(new[] { ";;" }, StringSplitOptions.None);
			Debug.Assert(parts.Length == 2);
			return parts.Length != 2 ? new IbusKeyboardEntry() : new IbusKeyboardEntry { Type = parts[0], Layout = parts[1] };
		}
	}
}