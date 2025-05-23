// Copyright (c) 2015-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Helper class that implements common functionality for unity and Gnome keyboard retrieving
	/// </summary>
	internal class GnomeKeyboardRetrievingHelper
	{
		private readonly Func<string[]> _getKeyboards;

		public GnomeKeyboardRetrievingHelper()
		{
			_getKeyboards = GetMyKeyboards;
		}

		// Used in unit tests
		internal GnomeKeyboardRetrievingHelper(Func<string[]> getKeyboards): this()
		{
			_getKeyboards = getKeyboards;
		}


		#region Public methods and properties
		public bool IsApplicable
		{
			get
			{
				var list = _getKeyboards();
				return list != null && list.Length > 0;
			}
		}

		public void InitKeyboards(Func<string, bool> keyboardTypeMatches,
			Action<IDictionary<string, uint>, (string, string)> registerKeyboards)
		{
			var list = _getKeyboards();
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
			return programs.FirstOrDefault(File.Exists);
		}
		#endregion

		/// <summary>
		/// Returns the list of keyboards or <c>null</c> if we can't get the combined keyboards
		/// list.
		/// </summary>
		/// <remarks>protected virtual for unit tests</remarks>
		protected virtual string[] GetMyKeyboards()
		{
			// This is the proper path for the combined keyboard handling, not the path
			// given in the IBus reference documentation.
			const string schema = "org.gnome.desktop.input-sources";
			if (!GlibHelper.SchemaIsInstalled(schema))
				return null;

			var settings = Unmanaged.g_settings_new(schema);
			if (settings == IntPtr.Zero)
				return null;

			var sources = Unmanaged.g_settings_get_value(settings, "sources");
			if (sources == IntPtr.Zero)
				return null;
			var list = GlibHelper.GetStringArrayFromGVariantListArray(sources);
			Unmanaged.g_variant_unref(sources);
			Unmanaged.g_object_unref(settings);

			return list;
		}

		private static void AddMatchingKeyboard(string source, ref uint kbdIndex,
			IDictionary<string, uint> keyboards, Func<string, bool> keyboardTypeMatches)
		{
			var (type, layout) = SplitKeyboardEntry(source);
			if (keyboardTypeMatches(type) && !keyboards.ContainsKey(layout))
				keyboards.Add(layout, kbdIndex);
			++kbdIndex;
		}

		private static (string type, string layout) SplitKeyboardEntry(string source)
		{
			var parts = source.Split(new[]{";;"}, StringSplitOptions.None);
			Debug.Assert(parts.Length == 2);
			return parts.Length != 2 ? (null, null) : (parts[0], parts[1]);
		}

	}
}