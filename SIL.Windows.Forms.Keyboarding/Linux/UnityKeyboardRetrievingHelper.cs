// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Helper class that implements common functionality for unity keyboard retrieving
	/// </summary>
	public class UnityKeyboardRetrievingHelper
	{
		public UnityKeyboardRetrievingHelper()
		{
			KeyboardRetrievingHelper.InitGlib();
		}

		#region Public methods and properties
		public bool IsApplicable
		{
			get
			{
				var list = GetMyKeyboards();
				return list != null && list.Length > 0;
			}
		}

		public void InitKeyboards(Func<string, bool> keyboardTypeMatches,
			Action<IDictionary<string, uint>> registerKeyboards)
		{
			var keyboards = new Dictionary<string, uint>();
			var list = GetMyKeyboards();
			uint kbdIndex = 0;
			foreach (var t in list)
				AddKeyboard(t, ref kbdIndex, keyboards, keyboardTypeMatches);

			registerKeyboards(keyboards);
		}

		public string GetKeyboardSetupApplication(out string arguments)
		{
			arguments = "region layouts";
			if (File.Exists("/usr/bin/unity-control-center"))
				return "/usr/bin/unity-control-center";
			if (File.Exists("/usr/bin/gnome-control-center"))
				return "/usr/bin/gnome-control-center";
			return null;
		}
		#endregion

		/// <summary>
		/// Returns the list of keyboards or <c>null</c> if we can't get the combined keyboards
		/// list.
		/// </summary>
		private string[] GetMyKeyboards()
		{
			// This is the proper path for the combined keyboard handling, not the path
			// given in the IBus reference documentation.
			const string schema = "org.gnome.desktop.input-sources";
			if (!KeyboardRetrievingHelper.SchemaIsInstalled(schema))
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

		private static void AddKeyboard(string source, ref uint kbdIndex,
			IDictionary<string, uint> keyboards, Func<string, bool> keyboardTypeMatches)
		{
			var parts = source.Split(new[]{";;"}, StringSplitOptions.None);
			Debug.Assert(parts.Length == 2);
			if (parts.Length != 2)
				return;
			var type = parts[0];
			var layout = parts[1];
			if (keyboardTypeMatches(type))
				keyboards.Add(layout, kbdIndex);
			++kbdIndex;
		}
	}
}