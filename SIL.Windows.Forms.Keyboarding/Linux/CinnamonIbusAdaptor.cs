// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IBusDotNet;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Wasta versions >= 14 (aka
	/// Mint 17/Cinnamon).  Wasta 12 worked the same as Precise (Ubuntu 12.04), and default
	/// Wasta 14 without IBus appears to also work the same as Precise with XKB keyboards only.
	/// Starting with Wasta 14, if IBus is used for keyboard inputs, things are joined together,
	/// but not the same as the combined keyboard processing in Trusty (Ubuntu 14.04).
	/// </summary>
	internal class CinnamonIbusAdaptor : CommonBaseAdaptor
	{
		private static bool DetermineIsRequired()
		{
			IntPtr client = IntPtr.Zero;
			try
			{
				try
				{
					client = dconf_client_new();
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

				// This tells us whether we're running under Wasta 14 (Mint 17/Cinnamon).
				IntPtr cinnamon = dconf_client_read(client, "/org/cinnamon/number-workspaces");
				if (cinnamon == IntPtr.Zero)
					return false;
				g_variant_unref(cinnamon);

				// This is the proper path for the combined keyboard handling on Cinnamon with IBus.
				IntPtr sources = dconf_client_read(client, "/desktop/ibus/general/preload-engines");
				if (sources == IntPtr.Zero)
					return false;
				g_variant_unref(sources);
			}
			finally
			{
				if (client != IntPtr.Zero)
					g_object_unref(client);
			}

			// Maybe the user experimented with cinnamon, but isn't really using it?
			string desktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
			if (!String.IsNullOrEmpty(desktop) && !desktop.ToLowerInvariant().Contains("cinnamon"))
				return false;

			// Maybe the user experimented with ibus, then removed it??
			if (!System.IO.File.Exists("/usr/bin/ibus-setup"))
				return false;

			return true;
		}
		private static bool? _isRequired;

		public static bool IsRequired
		{
			get
			{
				// only compute this once
				if (_isRequired == null)
					_isRequired = DetermineIsRequired();
				return (bool) _isRequired;
			}
		}


		private IIbusCommunicator _ibuscom;

		// These should not change while the program is running, and they're expensive to obtain.
		// So we've made them static.
		static string _defaultLayout;
		static string _defaultVariant;
		static string _defaultOption;
		static string[] _latinLayouts;
		static bool _use_xmodmap;

		private readonly IbusKeyboardAdaptor _ibusAdaptor;
		private readonly Dictionary<string,int> _ibusKeyboards;

		public CinnamonIbusAdaptor()
		{
			_ibusAdaptor = new IbusKeyboardAdaptor();
			_ibusKeyboards = new Dictionary<string, int>();
		}

		private void RegisterIbusKeyboards()
		{
			if (_ibusKeyboards.Count == 0)
				return;

			Dictionary<string, IbusKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			foreach (IBusEngineDesc ibusKeyboard in _ibusAdaptor.GetAllIBusKeyboards())
			{
				if (_ibusKeyboards.ContainsKey(ibusKeyboard.LongName)
					|| (_ibusKeyboards.ContainsKey(ibusKeyboard.Name) && ibusKeyboard.Name.StartsWith("xkb:", StringComparison.Ordinal)))
				{
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
						keyboard = new IbusKeyboardDescription(id, ibusKeyboard, this);
						KeyboardController.Instance.Keyboards.Add(keyboard);
					}
					keyboard.SystemIndex = _ibusKeyboards[ibusKeyboard.LongName];
				}
			}

			foreach (IbusKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);
		}

		protected override void AddAllKeyboards(string[] list)
		{
			_ibusKeyboards.Clear();
			// e.g., "pinyin", "xkb:us::eng", "xkb:fr::fra", "xkb:de::ger", "/usr/share/kmfl/IPA14.kmn", "xkb:es::spa"
			int kbdIndex = 0;
			foreach (string item in list)
			{
				_ibusKeyboards.Add(item, kbdIndex);
				++kbdIndex;
			}
			RegisterIbusKeyboards();
		}

		protected override string GSettingsSchema { get { return "org.freedesktop.ibus.general"; } }

		protected override string[] GetMyKeyboards(IntPtr client, IntPtr settings)
		{
			// This is the proper path for the combined keyboard handling on Cinnamon with IBus.
			IntPtr sources = dconf_client_read(client, "/desktop/ibus/general/preload-engines");
			if (sources == IntPtr.Zero)
				return null;
			string[] list = GetStringArrayFromGVariantArray(sources);
			g_variant_unref(sources);

			// Call these only once per run of the program.
			if (_defaultLayout == null)
				LoadDefaultXkbSettings();
			if (_latinLayouts == null)
				LoadLatinLayouts(settings);

			return list;
		}

		#region IKeyboardAdaptor implementation

		public override bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			Debug.Assert(keyboard.Engine == this);
			//Console.WriteLine("DEBUG CinnamonIbusAdaptor.ActivateKeyboard({0})", keyboard);
			var ibusKeyboard = keyboard as IbusKeyboardDescription;
			if (ibusKeyboard != null)
			{
				try
				{
					if (_ibuscom == null)
						_ibuscom = new IbusCommunicator();
					_ibuscom.FocusIn();
					if (!_ibusAdaptor.CanSetIbusKeyboard())
						return false;
					if (_ibusAdaptor.IBusKeyboardAlreadySet(ibusKeyboard))
						return true;

					// Set the associated XKB layout
					SetLayout(ibusKeyboard);

					// Then set the IBus keyboard (may be an XKB keyboard in actuality)
					IInputContext context = GlobalCachedInputContext.InputContext;
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

		public override void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			//Console.WriteLine ("DEBUG deactivating {0}", keyboard);
			if (keyboard is IbusKeyboardDescription)
			{
				if (_ibusAdaptor.CanSetIbusKeyboard())
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

		public override KeyboardDescription CreateKeyboardDefinition(string id)
		{
			string[] parts = id.Split('_');
			string locale = parts[0];
			string layout = parts[1];
			return new IbusKeyboardDescription(id, layout, locale, this); 
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
			string layout = ibusKeyboard.ParentLayout;
			if (layout == "en")
				layout = "us";	// layout is a country code, not a language code!
			string variant = ibusKeyboard.IBusKeyboardEngine.LayoutVariant;
			string option = ibusKeyboard.IBusKeyboardEngine.LayoutOption;
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

		protected override void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				if (_ibuscom != null)
				{
					_ibuscom.Dispose ();
					_ibuscom = null;
				}
				_ibusAdaptor.Dispose();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			base.Dispose(disposing);
		}

		private KeyboardDescription _defaultKeyboard;

		/// <summary>
		/// This adaptor doesn't make use of XkbKeyboardDefinition objects, so we have to
		/// figure out the default keyboard here, searching the available keyboards for the
		/// best match to _defaultLayout.  An explicit xkb: keyboard is preferred, but we
		/// settle for another match (or anything at all) if we need to.
		/// </summary>
		public override KeyboardDescription DefaultKeyboard
		{
			get
			{
				if (_defaultKeyboard == null)
				{
					var desired = String.Format("xkb:{0}:", _defaultLayout);
					if (!String.IsNullOrEmpty (_defaultVariant))
						desired = String.Format ("xkb:{0}\\{1}:", _defaultLayout, _defaultVariant);
					var pattern = String.Format("[^A-Za-z]{0}[^A-Za-z]|^{0}[^A-Za-z]|.*[^A-Za-z]{0}$", _defaultLayout);
					var regex = new System.Text.RegularExpressions.Regex(pattern);
					KeyboardDescription first = null;
					foreach (KeyboardDescription kbd in Keyboard.Controller.AllAvailableKeyboards)
					{
						if (first == null)
							first = kbd;	// last-ditch value if all else fails
						if (kbd.Layout.StartsWith(desired) || kbd.Layout == _defaultLayout)
						{
							_defaultKeyboard = kbd;
							break;
						}
						if (regex.IsMatch(kbd.Layout))
						{
							_defaultKeyboard = kbd;
						}
					}
					if (_defaultKeyboard == null)
						_defaultKeyboard = first;
				}
				return _defaultKeyboard;
			}
		}
	}
}
#endif
