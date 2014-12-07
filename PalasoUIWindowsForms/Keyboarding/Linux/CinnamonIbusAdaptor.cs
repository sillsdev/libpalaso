// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using X11.XKlavier;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;
using Palaso.Reporting;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Wasta versions >= 14 (aka
	/// Mint 17/Cinnamon).  Wasta 12 worked the same as Precise (Ubuntu 12.04), and default
	/// Wasta 14 without IBus appears to also work the same as Precise with XKB keyboards only.
	/// Starting with Wasta 14, if IBus is used for keyboard inputs, things are joined together,
	/// but not the same as the combined keyboard processing in Trusty (Ubuntu 14.04).
	/// </summary>
	public class CinnamonIbusAdaptor: CommonBaseAdaptor
	{
		private IIbusCommunicator m_ibuscom;

		// These should not change while the program is running, and they're expensive to obtain.
		// So we've made them static.
		static string _defaultLayout;
		static string _defaultVariant;
		static string _defaultOption;
		static string[] _latinLayouts;
		static bool _use_xmodmap;

		// This starts out true, is set false once and for all the first time the system
		// is probed for this style of keyboarding if it's not appropriate.
		private static bool IsCinnamonWithIbus = true;

		public CinnamonIbusAdaptor() : base()
		{
		}

		private void RegisterIbusKeyboards()
		{
			if (IbusKeyboards.Count <= 0)
				return;

			var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
			List<string> missingLayouts = new List<string>(IbusKeyboards.Keys);
			foreach (var ibusKeyboard in ibusAdaptor.GetAllIBusKeyboards())
			{
				if (IbusKeyboards.ContainsKey(ibusKeyboard.LongName))
				{
					missingLayouts.Remove(ibusKeyboard.LongName);
					var keyboard = new IbusKeyboardDescription(this, ibusKeyboard);
					keyboard.SystemIndex = IbusKeyboards[ibusKeyboard.LongName];
					KeyboardController.Manager.RegisterKeyboard(keyboard);
				}
				else if (IbusKeyboards.ContainsKey(ibusKeyboard.Name) && ibusKeyboard.Name.StartsWith ("xkb:"))
				{
					missingLayouts.Remove(ibusKeyboard.Name);
					var keyboard = new IbusKeyboardDescription(this, ibusKeyboard);
					keyboard.SystemIndex = IbusKeyboards [ibusKeyboard.LongName];
					KeyboardController.Manager.RegisterKeyboard(keyboard);
				}
			}
			foreach (var layout in missingLayouts)
				Console.WriteLine("Didn't find " + layout);
		}

		protected override void AddAllKeyboards(string[] list)
		{
			// e.g., "pinyin", "xkb:us::eng", "xkb:fr::fra", "xkb:de::ger", "/usr/share/kmfl/IPA14.kmn", "xkb:es::spa"
			int kbdIndex = 0;
			foreach (var item in list)
			{
				IbusKeyboards.Add(item, kbdIndex);
				++kbdIndex;
			}
			RegisterIbusKeyboards();
		}

		protected override bool UseThisAdaptor
		{
			get
			{
				return IsCinnamonWithIbus;
			}
			set
			{
				IsCinnamonWithIbus = value;
				KeyboardController.CinnamonKeyboardHandling = value;
			}
		}

		protected override string GSettingsSchema { get { return "org.freedesktop.ibus.general"; } }

		protected override string[] GetMyKeyboards(IntPtr client, IntPtr settingsGeneral)
		{
			// This tells us whether we're running under Wasta 14 (Mint 17/Cinnamon).
			var cinnamon = dconf_client_read(client, "/org/cinnamon/number-workspaces");
			if (cinnamon == IntPtr.Zero)
				return null;
			g_variant_unref(cinnamon);

			// This is the proper path for the combined keyboard handling on Cinnamon with IBus.
			var sources = dconf_client_read(client, "/desktop/ibus/general/preload-engines");
			if (sources == IntPtr.Zero)
				return null;
			var list = GetStringArrayFromGVariantArray(sources);
			g_variant_unref(sources);

			// Maybe the user experimented with cinnamon, but isn't really using it?
			var desktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
			if (!String.IsNullOrEmpty(desktop) && !desktop.ToLowerInvariant().Contains("cinnamon"))
				return null;

			// Maybe the user experimented with ibus, then removed it??
			if (!System.IO.File.Exists("/usr/bin/ibus-setup"))
				return null;

			// Call these only once per run of the program.
			if (_defaultLayout == null)
				LoadDefaultXkbSettings();
			if (_latinLayouts == null)
				LoadLatinLayouts(settingsGeneral);

			return list;
		}

		#region IKeyboardAdaptor implementation
		public override void Close()
		{
			if (m_ibuscom != null)
			{
				m_ibuscom.Dispose();
				m_ibuscom = null;
			}
		}

		public override bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			Debug.Assert(keyboard is KeyboardDescription);
			Debug.Assert(((KeyboardDescription)keyboard).Engine == this);
			//Console.WriteLine("DEBUG CinnamonIbusAdaptor.ActivateKeyboard({0})", keyboard);
			if (keyboard is IbusKeyboardDescription)
			{
				var ibusKeyboard = keyboard as IbusKeyboardDescription;
				try
				{
					if (m_ibuscom == null)
						m_ibuscom = new IbusCommunicator();
					m_ibuscom.FocusIn();
					var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
					if (!ibusAdaptor.CanSetIbusKeyboard())
						return false;
					if (ibusAdaptor.IBusKeyboardAlreadySet(ibusKeyboard))
						return true;

					// Set the associated XKB layout
					SetLayout(ibusKeyboard);

					// Then set the IBus keyboard (may be an XKB keyboard in actuality)
					var context = GlobalCachedInputContext.InputContext;
					//Console.WriteLine ("DEBUG calling context.SetEngine({0})", ibusKeyboard.IBusKeyboardEngine.LongName);
					context.SetEngine(ibusKeyboard.IBusKeyboardEngine.LongName);
					GlobalCachedInputContext.Keyboard = ibusKeyboard;
				}
				catch (Exception e)
				{
					Debug.WriteLine(string.Format("Changing keyboard failed, is kfml/ibus running? {0}", e));
					return false;
				}
			}
			else
			{
				throw new ArgumentException();
			}
			return true;
		}

		public override void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			//Console.WriteLine ("DEBUG deactivating {0}", keyboard);
			if (keyboard is IbusKeyboardDescription)
			{
				var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
				if (ibusAdaptor.CanSetIbusKeyboard())
				{
					var context = GlobalCachedInputContext.InputContext;
					try
					{
							context.SetEngine("");
							context.Reset();
							context.Disable();
					}
					catch (Exception ex)
					{
						// We don't want a random DBus exception to kill the program.
						// If the keyboarding doesn't work quite right, that's still
						// better than dying spontaneously.  (And keyboarding seems
						// to keep working okay according to my limited testing even
						// after exceptions have been caught and ignored here.)
						Console.WriteLine ("DBUS EXCEPTION CAUGHT: {0}", ex.Message);
					}
				}
			}
			GlobalCachedInputContext.Keyboard = null;
		}
		#endregion

		/// <summary>
		/// This is call by the keyboard controller since we don't have any true XKB handlers
		/// around to be the default keyboard.
		/// </summary>
		public void ActivateDefaultKeyboard()
		{
			//Console.WriteLine("DEBUG CinnamonIbusAdaptor.ActivateDefaultKeyboard()");
			if (GlobalCachedInputContext.Keyboard != null)
				DeactivateKeyboard(GlobalCachedInputContext.Keyboard);
			SetLayout(_defaultLayout, _defaultVariant, _defaultOption);
		}

		/// <summary>
		/// Set the XKB layout from the IBus keyboard.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void SetLayout(IbusKeyboardDescription ibusKeyboard)
		{
			var layout = ibusKeyboard.ParentLayout;
			if (layout == "en")
				layout = "us";	// layout is a country code, not a language code!
			var variant = ibusKeyboard.IBusKeyboardEngine.LayoutVariant;
			var option = ibusKeyboard.IBusKeyboardEngine.LayoutOption;
			Debug.Assert(layout != null);

			bool need_us_layout = false;
			foreach (string latinLayout in _latinLayouts)
			{
				if (layout == latinLayout && variant != "eng")
				{
					need_us_layout = true;
					break;
				}
				if (!String.IsNullOrEmpty(variant) && String.Format("{0}({1})", layout, variant) == latinLayout)
				{
					need_us_layout = true;
					break;
				}
			}

			if (String.IsNullOrEmpty(layout) || layout == "default")
			{
				layout = _defaultLayout;
				variant = _defaultVariant;
			}
			if (String.IsNullOrEmpty(variant) || variant == "default")
				variant = null;
			if (String.IsNullOrEmpty(option) || option == "default")
			{
				option = _defaultOption;
			}
			else if (!String.IsNullOrEmpty(_defaultOption))
			{
				if (_defaultOption.Split(',').Contains(option))
					option = _defaultOption;
				else
					option = String.Format("{0},{1}", _defaultOption, option);
			}

			if (need_us_layout)
			{
				layout = layout + ",us";
				// If we have a variant, we need to indicate an empty variant to
				// match the "us" layout.
				if (!String.IsNullOrEmpty(variant))
					variant = variant + ",";
			}

			SetLayout(layout, variant, option);
		}

		/// <summary>
		/// Set the XKB layout to use henceforward using the setxkbmap program.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void SetLayout(string layout, string variant, string option)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = "/usr/bin/setxkbmap";
			StringBuilder bldr = new StringBuilder();
			bldr.AppendFormat ("-layout {0}", layout);
			if (!String.IsNullOrEmpty(variant))
				bldr.AppendFormat (" -variant {0}", variant);
			if (!String.IsNullOrEmpty(option))
				bldr.AppendFormat (" -option {0}", option);
			//Console.WriteLine ("DEBUG: about to call \"{0} {1}\"", startInfo.FileName, bldr.ToString ());
			startInfo.Arguments = bldr.ToString();
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			var process = Process.Start(startInfo);
			process.WaitForExit();
			process.Close();

			if (_use_xmodmap)
				SetXModMap();
		}

		static string[] _knownXModMapFiles = {".xmodmap", ".xmodmaprc", ".Xmodmap", ".Xmodmaprc"};

		/// <summary>
		/// Add the user's modifications to whatever keyboard mapping is active.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void SetXModMap()
		{
			string homedir = Environment.GetEnvironmentVariable("HOME");
			foreach (string file in _knownXModMapFiles)
			{
				string filepath = System.IO.Path.Combine(homedir, file);
				if (!System.IO.File.Exists(filepath))
					continue;
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = "xmodmap";
				startInfo.Arguments = filepath;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				var process = Process.Start(startInfo);
				process.WaitForExit();
				process.Close();
				break;
			}
		}

		/// <summary>
		/// Load the default (current) xkb settings, using the setxkbmap program.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void LoadDefaultXkbSettings()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = "/usr/bin/setxkbmap";
			startInfo.Arguments = "-query";
			startInfo.RedirectStandardOutput = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			var process = Process.Start(startInfo);
			string stdout = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			process.Close();
			if (!String.IsNullOrEmpty(stdout))
			{
				var lines = stdout.Split(new char[]{ '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < lines.Length; ++i)
				{
					var parts = lines[i].Split(new char[]{ ':' }, StringSplitOptions.None);
					switch (parts[0])
					{
					case "layout":    _defaultLayout = parts[1].Trim();    break;
					case "variant":   _defaultVariant = parts[1].Trim();   break;
					case "options":   _defaultOption = parts[1].Trim();    break;
				}
				}
			}
			//Console.WriteLine("DEBUG _defaultLayout = \"{0}\"", _defaultLayout);
			//Console.WriteLine("DEBUG _defaultVariant = \"{0}\"", _defaultVariant);
			//Console.WriteLine("DEBUG _defaultOption = \"{0}\"", _defaultOption);
		}

		/// <summary>
		/// Load a couple of settings from the GNOME settings system.
		/// </summary>
		private static void LoadLatinLayouts(IntPtr settingsGeneral)
		{
			IntPtr value = g_settings_get_value(settingsGeneral, "xkb-latin-layouts");
			_latinLayouts = GetStringArrayFromGVariantArray(value);
			g_variant_unref(value);

			_use_xmodmap = false;
			value = g_settings_get_value(settingsGeneral, "use-xmodmap");
			if (value != IntPtr.Zero)
			{
				_use_xmodmap = g_variant_get_boolean(value);
				g_variant_unref(value);
				value = IntPtr.Zero;
			}
			//Console.WriteLine("DEBUG use-xmodmap = {0}", _use_xmodmap);
			//Console.Write("DEBUG xkb-latin-layouts =");
			//for (int i = 0; i < _latinLayouts.Length; ++i)
			//	Console.Write("  '{0}'", _latinLayouts[i]);
			//Console.WriteLine();
		}
	}
}
#endif
