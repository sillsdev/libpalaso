// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Wasta versions >= 14 (aka
	/// Mint 17/Cinnamon).  Wasta 12 worked the same as Precise (Ubuntu 12.04), and default
	/// Wasta 14 without IBus appears to also work the same as Precise with XKB keyboards only.
	/// Starting with Wasta 14, if IBus is used for keyboard inputs, things are joined together,
	/// but not the same as the combined keyboard processing in Trusty (Ubuntu 14.04).
	/// It also works for other desktop environments that use combined ibus keyboards, e.g.
	/// XFCE.
	/// </summary>
	[CLSCompliant(false)]
	public class CombinedIbusKeyboardRetrievingAdaptor : IbusKeyboardRetrievingAdaptor
	{
		private IntPtr _settingsGeneral;

		public CombinedIbusKeyboardRetrievingAdaptor()
		{
			KeyboardRetrievingHelper.InitGlib();
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="SIL.Windows.Forms.Keyboarding.Linux.CombinedIbusKeyboardRetrievingAdaptor"/> class.
		/// Used in unit tests
		/// </summary>
		/// <param name="ibusCommunicator">Ibus communicator.</param>
		public CombinedIbusKeyboardRetrievingAdaptor(IIbusCommunicator ibusCommunicator): base(ibusCommunicator)
		{
		}

		private static string GSettingsSchema { get { return "org.freedesktop.ibus.general"; } }

		// To find the corresponding schema for a dconf path, e.g. /desktop/ibus/general/preload-engines:
		//     $ gsettings list-schemas | grep desktop.ibus.general
		//     org.freedesktop.ibus.general
		//     org.freedesktop.ibus.general.hotkey
		//     $ gsettings list-keys org.freedesktop.ibus.general | grep preload-engines
		//     preload-engines


		protected string[] GetMyKeyboards(IntPtr settingsGeneral)
		{
			// This is the proper path for the combined keyboard handling on Cinnamon with IBus.
			var sources = Unmanaged.g_settings_get_value(settingsGeneral, "preload-engines");
			if (sources == IntPtr.Zero)
				return null;
			var list = KeyboardRetrievingHelper.GetStringArrayFromGVariantArray(sources);
			Unmanaged.g_variant_unref(sources);

			// Call these only once per run of the program.
			if (CombinedIbusKeyboardSwitchingAdaptor.DefaultLayout == null)
				LoadDefaultXkbSettings();
			if (CombinedIbusKeyboardSwitchingAdaptor.LatinLayouts == null)
				LoadLatinLayouts(settingsGeneral);

			return list;
		}

		/// <summary>
		/// Load the default (current) xkb settings, using the setxkbmap program.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void LoadDefaultXkbSettings()
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = "/usr/bin/setxkbmap";
			startInfo.Arguments = "-query";
			startInfo.RedirectStandardOutput = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			string stdout;
			using (Process process = Process.Start(startInfo))
			{
				stdout = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
			}
			if (!string.IsNullOrEmpty(stdout))
			{
				var lines = stdout.Split(new char[]{ '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < lines.Length; ++i)
				{
					var parts = lines[i].Split(new char[]{ ':' }, StringSplitOptions.None);
					switch (parts[0])
					{
						case "layout":
							CombinedIbusKeyboardSwitchingAdaptor.DefaultLayout = parts[1].Trim();
							break;
						case "variant":
							CombinedIbusKeyboardSwitchingAdaptor.DefaultVariant = parts[1].Trim();
							break;
						case "options":
							CombinedIbusKeyboardSwitchingAdaptor.DefaultOption = parts[1].Trim();
							break;
					}
				}
			}
			//Console.WriteLine("DEBUG _defaultLayout = \"{0}\"", Adaptor.DefaultLayout);
			//Console.WriteLine("DEBUG _defaultVariant = \"{0}\"", Adaptor.DefaultVariant);
			//Console.WriteLine("DEBUG _defaultOption = \"{0}\"", Adaptor.DefaultOption);
		}

		/// <summary>
		/// Load a couple of settings from the GNOME settings system.
		/// </summary>
		private static void LoadLatinLayouts(IntPtr settingsGeneral)
		{
			IntPtr value = Unmanaged.g_settings_get_value(settingsGeneral, "xkb-latin-layouts");
			CombinedIbusKeyboardSwitchingAdaptor.LatinLayouts =
				KeyboardRetrievingHelper.GetStringArrayFromGVariantArray(value);
			Unmanaged.g_variant_unref(value);

			CombinedIbusKeyboardSwitchingAdaptor.UseXmodmap = Unmanaged.g_settings_get_boolean(
				settingsGeneral, "use-xmodmap");
			//Console.WriteLine("DEBUG use-xmodmap = {0}", _use_xmodmap);
			//Console.Write("DEBUG xkb-latin-layouts =");
			//for (int i = 0; i < _latinLayouts.Length; ++i)
			//	Console.Write("  '{0}'", _latinLayouts[i]);
			//Console.WriteLine();
		}

		#region Specific implementations of IKeyboardRetriever

		public override bool IsApplicable
		{
			get
			{
				try
				{
					if (!KeyboardRetrievingHelper.SchemaIsInstalled(GSettingsSchema))
						return false;
					_settingsGeneral = Unmanaged.g_settings_new(GSettingsSchema);
					if (_settingsGeneral == IntPtr.Zero)
						return false;
				}
				catch (DllNotFoundException)
				{
					// Older versions of Linux have a version of the dconf library with a
					// different version number (different from what libdconf.dll gets
					// mapped to in app.config). However, since those Linux versions
					// don't have combined keyboards under IBus this really doesn't
					// matter.
					return false;
				}
				var list = GetMyKeyboards(_settingsGeneral);
				return list != null && list.Length > 0 && Platform.DesktopEnvironment != "unity"
					&& !Platform.DesktopEnvironment.Contains("gnome");
			}
		}

		public override KeyboardAdaptorType Type
		{
			get { return KeyboardAdaptorType.System | KeyboardAdaptorType.OtherIm; }
		}

		public override void Initialize()
		{
			// We come here twice, for both KeyboardType.System and KeyboardType.OtherIm.
			// We want to create a new switching adaptor only the first time otherwise we're
			// getting into trouble
			if (SwitchingAdaptor == null)
			{
				SwitchingAdaptor = new CombinedIbusKeyboardSwitchingAdaptor(IbusCommunicator);
				KeyboardRetrievingHelper.AddIbusVersionAsErrorReportProperty();
				InitKeyboards();
			}
		}

		public override KeyboardDescription CreateKeyboardDefinition(string id)
		{
			return XkbKeyboardRetrievingAdaptor.CreateKeyboardDefinition(id, SwitchingAdaptor);
		}

		#endregion

		protected override void InitKeyboards()
		{
			var keyboards = new Dictionary<string, uint>();
			var list = GetMyKeyboards(_settingsGeneral);
			uint kbdIndex = 0;
			foreach (var item in list)
				keyboards.Add(item, kbdIndex++);

			RegisterKeyboards(keyboards);
		}

		private void RegisterKeyboards(IDictionary<string, uint> keyboards)
		{
			if (keyboards.Count <= 0)
				return;

			Dictionary<string, IbusKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			List<string> missingLayouts = new List<string>(keyboards.Keys);
			foreach (var ibusKeyboard in GetAllIBusKeyboards())
			{
				if (keyboards.ContainsKey(ibusKeyboard.LongName))
					missingLayouts.Remove(ibusKeyboard.LongName);
				else if (keyboards.ContainsKey(ibusKeyboard.Name) &&
					ibusKeyboard.Name.StartsWith("xkb:", StringComparison.InvariantCulture))
				{
					missingLayouts.Remove(ibusKeyboard.Name);
				}
				else
					continue;

				string id = string.Format("{0}_{1}", ibusKeyboard.Language, ibusKeyboard.LongName);
				IbusKeyboardDescription keyboard;
				if (curKeyboards.TryGetValue(id, out keyboard))
				{
					if (!keyboard.IsAvailable)
					{
						keyboard.SetIsAvailable(true);
						keyboard.IBusKeyboardEngine = ibusKeyboard;
					}
					curKeyboards.Remove(id);
				}
				else
				{
					keyboard = new IbusKeyboardDescription(id, ibusKeyboard, SwitchingAdaptor);
					KeyboardController.Instance.Keyboards.Add(keyboard);
				}
				keyboard.SystemIndex = keyboards[ibusKeyboard.LongName];
			}

			foreach (IbusKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);

			foreach (var layout in missingLayouts)
				Console.WriteLine("{0}: Didn't find {1}", GetType().Name, layout);
		}

		protected override void Dispose(bool disposing)
		{
			if (_settingsGeneral != IntPtr.Zero)
			{
				Unmanaged.g_object_unref(_settingsGeneral);
				_settingsGeneral = IntPtr.Zero;
			}
			base.Dispose(disposing);
		}

	}
}
