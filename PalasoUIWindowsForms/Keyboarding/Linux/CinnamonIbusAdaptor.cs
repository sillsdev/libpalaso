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
	public class CinnamonIbusAdaptor: IKeyboardAdaptor
	{
		private static bool IsCinnamonWithIbus = true;
		private Dictionary<string,int> IbusKeyboards;
		private Dictionary<string,int> XkbKeyboards;

		#region P/Invoke imports for glib and dconf
		// NOTE: we directly use glib/dconf methods here since we don't want to
		// introduce an otherwise unnecessary dependency on gconf-sharp/gnome-sharp.
		[DllImport("libgobject-2.0.so")]
		private extern static void g_type_init();

		[DllImport("libgobject-2.0.so")]
		private extern static IntPtr g_variant_new_uint32(UInt32 value);

		[DllImport("libgobject-2.0.so")]
		private extern static void g_object_unref(IntPtr obj);

		[DllImport("libgio-2.0.so")]
		private extern static IntPtr g_settings_new(string schema_id);

		[DllImport("libgio-2.0.so")]
		private extern static IntPtr g_settings_get_value(IntPtr settings, string key);

		[DllImport("libglib-2.0.so")]
		private extern static void g_variant_unref(IntPtr value);

		[DllImport("libglib-2.0.so")]
		private extern static uint g_variant_n_children(IntPtr value);

		[DllImport("libglib-2.0.so")]
		private extern static IntPtr g_variant_get_child_value(IntPtr value, uint index_);

		[DllImport("libglib-2.0.so")]
		private extern static IntPtr g_variant_get_string(IntPtr value, out int length);

		[DllImport("libglib-2.0.so")]
		private extern static bool g_variant_get_boolean(IntPtr value);

		[DllImport("libdconf.dll")]
		private extern static IntPtr dconf_client_new();

		[DllImport("libdconf.dll")]
		private extern static IntPtr dconf_client_read(IntPtr client, string key);

		[DllImport("libdconf.dll")]
		private extern static bool dconf_client_write_sync(IntPtr client, string key, IntPtr value,
			ref string tag, IntPtr cancellable, out IntPtr error);
		#endregion

		public CinnamonIbusAdaptor()
		{
			// g_type_init() is needed for Precise, but deprecated for Trusty.
			// Remove this (and the DllImport above) when we stop supporting Precise.
			g_type_init();
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

		private static T GetAdaptor<T>() where T: class
		{
			foreach (var adaptor in KeyboardController.Adaptors)
			{
				var tAdaptor = adaptor as T;
				if (tAdaptor != default(T))
					return tAdaptor;
			}
			return default(T);
		}

		private void AddAllKeyboards(string[] list)
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

		#region IKeyboardAdaptor implementation
		public void Initialize()
		{
			if (!IsCinnamonWithIbus)
				return;

			IntPtr client = IntPtr.Zero;
			IntPtr settingsGeneral = IntPtr.Zero;

			try
			{
				IsCinnamonWithIbus = false;
				KeyboardController.CinnamonKeyboardHandling = false;
				IbusKeyboards = new Dictionary<string, int>();
				XkbKeyboards = new Dictionary<string, int>();

				try
				{
					client = dconf_client_new();
					settingsGeneral = g_settings_new("org.freedesktop.ibus.general");
				}
				catch (DllNotFoundException)
				{
					// Older Wasta (Mint) versions have a version of the dconf library with a
					// different version number (different from what libdconf.dll gets
					// mapped to in app.config). However, since those Wasta (Mint) versions
					// don't have combined keyboards under IBus this really doesn't
					// matter.
					return;
				}

				// This tells us whether we're running under Wasta 14 (Mint 17/Cinnamon).
				var cinnamon = dconf_client_read(client, "/org/cinnamon/number-workspaces");
				if (cinnamon == IntPtr.Zero)
					return;
				g_variant_unref(cinnamon);

				// This is the proper path for the combined keyboard handling on Cinnamon with IBus.
				var sources = dconf_client_read(client, "/desktop/ibus/general/preload-engines");
				if (sources == IntPtr.Zero)
					return;

				// Maybe the user experimented with cinnamon, but isn't really using it?
				var desktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
				if (!String.IsNullOrEmpty(desktop) && !desktop.ToLowerInvariant().Contains("cinnamon"))
					return;

				// Maybe the user experimented with ibus, then removed it??
				if (!System.IO.File.Exists("/usr/bin/ibus-setup"))
					return;

				IsCinnamonWithIbus = true;
				KeyboardController.CinnamonKeyboardHandling = true;
				KeyboardController.Manager.ClearAllKeyboards();

				var list = GetStringArrayFromGVariant(sources);
				AddAllKeyboards(list);
				g_variant_unref(sources);

				// Call these only once per run of the program.
				if (_defaultLayout == null)
					LoadDefaultXkbSettings();
				if (_latinLayouts == null)
					LoadLatinLayouts(settingsGeneral);
			}
			finally
			{
				if (client != IntPtr.Zero)
					g_object_unref(client);
				if (settingsGeneral != IntPtr.Zero)
					g_object_unref(settingsGeneral);
			}
		}

		public void UpdateAvailableKeyboards()
		{
			Initialize();
		}

		public void Close()
		{
			if (m_ibuscom != null)
			{
				m_ibuscom.Dispose();
				m_ibuscom = null;
			}
		}

		private IIbusCommunicator m_ibuscom;

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
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

		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			//Console.WriteLine ("DEBUG deactivating {0}", keyboard);
			if (keyboard is IbusKeyboardDescription)
			{
				var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
				if (ibusAdaptor.CanSetIbusKeyboard())
				{
					var context = GlobalCachedInputContext.InputContext;
					context.SetEngine("");
					context.Reset();
					context.Disable();
				}
			}
			GlobalCachedInputContext.Keyboard = null;
		}

		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			throw new NotImplementedException();
		}

		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IKeyboardDefinition DefaultKeyboard
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public KeyboardType Type
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		// These should not change while the program is running, and they're expensive to obtain.
		// So we've made them static.
		static string _defaultLayout;
		static string _defaultVariant;
		static string _defaultOption;
		static string[] _latinLayouts;
		static bool _use_xmodmap;

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
		private void SetLayout(IbusKeyboardDescription ibusKeyboard)
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
		static void SetXModMap()
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
		void LoadLatinLayouts(IntPtr settingsGeneral)
		{
			IntPtr value = g_settings_get_value(settingsGeneral, "xkb-latin-layouts");
			_latinLayouts = GetStringArrayFromGVariant(value);
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

		/// <summary>
		/// Convert a GVariant handle that points to a list of strings to a C# string array.
		/// Without leaking memory in the process!
		/// </summary>
		/// <remarks>
		/// No check is made to verify that the input value actually points to an array of strings.
		/// </remarks>
		public static string[] GetStringArrayFromGVariant(IntPtr value)
		{
			if (value == IntPtr.Zero)
				return new string[0];
			uint size = g_variant_n_children(value);
			string[] list = new string[size];
			for (uint i = 0; i < size; ++i)
			{
				IntPtr child = g_variant_get_child_value(value, i);
				int length;
				// handle must not be freed -- it points into the actual GVariant memory for child!
				IntPtr handle = g_variant_get_string(child, out length);
				byte[] rawbytes = new byte[length];
				Marshal.Copy(handle, rawbytes, 0, length);
				list[i] = Encoding.UTF8.GetString(rawbytes);
				g_variant_unref(child);
				//Console.WriteLine("DEBUG GetStringArrayFromGVariant(): list[{0}] = \"{1}\" (length = {2})", i, list[i], length);
			}
			return list;
		}
	}
}
#endif
