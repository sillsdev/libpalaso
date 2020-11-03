// Copyright (c) 2015 SIL International
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
		private static string[] GetMyKeyboards()
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
			var list = KeyboardRetrievingHelper.GetStringArrayFromGVariantListArray(sources);
			Unmanaged.g_variant_unref(sources);
			Unmanaged.g_object_unref(settings);

			return list;
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