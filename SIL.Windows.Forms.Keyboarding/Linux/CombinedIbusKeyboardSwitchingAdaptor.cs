// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using SIL.Extensions;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles switching of keyboards on Wasta versions >= 14 (aka
	/// Mint 17/Cinnamon).  Wasta 12 worked the same as Precise (Ubuntu 12.04), and default
	/// Wasta 14 without IBus appears to also work the same as Precise with XKB keyboards only.
	/// Starting with Wasta 14, if IBus is used for keyboard inputs, things are joined together,
	/// but not the same as the combined keyboard processing in Trusty (Ubuntu 14.04).
	/// It also works for other desktop environments that use combined ibus keyboards, e.g.
	/// XFCE.
	/// </summary>
	public class CombinedIbusKeyboardSwitchingAdaptor : IbusKeyboardSwitchingAdaptor
	{
		// These should not change while the program is running, and they're expensive to obtain.
		// So we've made them static.
		internal static string DefaultLayout { get; set; }
		internal static string DefaultVariant { get; set; }
		internal static string DefaultOption { get; set; }
		internal static string[] LatinLayouts { get; set; }
		internal static bool UseXmodmap { get; set; }
		private static readonly string[] knownXModMapFiles = {".xmodmap", ".xmodmaprc", ".Xmodmap", ".Xmodmaprc"};

		private KeyboardDescription _defaultKeyboard;
		private int _ibusToXkbDelayMs = 0;

		public CombinedIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator) : base(ibusCommunicator)
		{
			// Wasta panel keyboard icon switching can be delayed to ensure the correct
			// icon is shown when moving from an ibus field to an xkb field. The necessary
			// delay is likely CPU dependent, and can be configured with this environment
			// variable. For example,
			//     SIL_KEYBOARDING_IBUS_XKB_DELAY=368 flatpak run org.sil.FieldWorks
			// The problem is only cosmetic, so the delay can be let at 0 and a wrong
			// keyboard icon can be ignored.
			int.TryParse(System.Environment.GetEnvironmentVariable("SIL_KEYBOARDING_IBUS_XKB_DELAY"), out _ibusToXkbDelayMs);
		}

		/// <summary>
		/// Absolute path to setxkbmap command.
		/// </summary>
		internal static string SetxkbmapPath
		{
			get
			{
				// Use command from flatpak /app prefix if present.
				string prefix = "/app";
				string restOfCommandPath = "bin/setxkbmap";
				if (!File.Exists(Path.Combine(prefix, restOfCommandPath)))
				{
					prefix = "/usr";
				}
				return Path.Combine(prefix, restOfCommandPath);
			}
		}

		/// <summary>
		/// Set the XKB layout to use henceforward using the setxkbmap program.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void SetXkbLayout(string layout, string variant, string option)
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = SetxkbmapPath;
			var bldr = new StringBuilder();
			bldr.AppendFormat ("-layout {0}", layout);
			if (!String.IsNullOrEmpty(variant))
				bldr.AppendFormat (" -variant {0}", variant);
			if (!String.IsNullOrEmpty(option))
				bldr.AppendFormat (" -option {0}", option);
			//Console.WriteLine("DEBUG SetXkbLayout({0},{1},{2}): about to call \"{3} {4}\"", layout, variant, option, startInfo.FileName, bldr.ToString());
			startInfo.Arguments = bldr.ToString();
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			using (var process = Process.Start(startInfo))
			{
				// Allow 0.3 seconds for the process -- measured at less than 0.1 seconds from command line.
				// (See https://jira.sil.org/browse/LT-17012 for why we need a timeout here.)  Keyboard
				// switching appears to work okay even when the process hangs and the timeout takes place.
				process.WaitForExit(300);
				process.Close();
			}

			if (UseXmodmap)
				SetXModMap();
		}

		/// <summary>
		/// Add the user's modifications to whatever keyboard mapping is active.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void SetXModMap()
		{
			string homedir = Environment.GetEnvironmentVariable("HOME");
			foreach (string file in knownXModMapFiles)
			{
				string filepath = System.IO.Path.Combine(homedir, file);
				if (!System.IO.File.Exists(filepath))
					continue;
				string program = "xmodmap";
				string args = filepath;
				if (Platform.IsFlatpak)
				{
					KeyboardRetrievingHelper.ToFlatpakSpawn(ref program, ref args);
				}
				var startInfo = new ProcessStartInfo();
				startInfo.FileName = program;
				startInfo.Arguments = args;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				using (var process = Process.Start(startInfo))
				{
					process.WaitForExit();
					process.Close();
				}
				break;
			}
		}

		/// <summary>
		/// Set the XKB layout from the IBus keyboard.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		protected override void SelectKeyboard(KeyboardDescription keyboard)
		{
			var ibusKeyboard = keyboard as IbusKeyboardDescription;

			var parentLayout = ibusKeyboard.ParentLayout;
			if (parentLayout == "en")
				parentLayout = "us";	// layout is a country code, not a language code!
			var variant = ibusKeyboard.IBusKeyboardEngine.LayoutVariant;
			var option = ibusKeyboard.IBusKeyboardEngine.LayoutOption;
			Debug.Assert(parentLayout != null);

			bool need_us_layout = false;
			foreach (string latinLayout in LatinLayouts)
			{
				if (parentLayout == latinLayout && variant != "eng")
				{
					need_us_layout = true;
					break;
				}
				if (!String.IsNullOrEmpty(variant) && String.Format("{0}({1})", parentLayout, variant) == latinLayout)
				{
					need_us_layout = true;
					break;
				}
			}

			if (String.IsNullOrEmpty(parentLayout) || parentLayout == "default")
			{
				parentLayout = DefaultLayout;
				variant = DefaultVariant;
			}
			if (String.IsNullOrEmpty(variant) || variant == "default")
				variant = null;
			if (String.IsNullOrEmpty(option) || option == "default")
			{
				option = DefaultOption;
			}
			else if (!string.IsNullOrEmpty(DefaultOption))
			{
				if (DefaultOption.Split(',').Contains(option, StringComparison.InvariantCulture))
					option = DefaultOption;
				else
					option = String.Format("{0},{1}", DefaultOption, option);
			}

			if (need_us_layout)
			{
				parentLayout = parentLayout + ",us";
				// If we have a variant, we need to indicate an empty variant to
				// match the "us" layout.
				if (!String.IsNullOrEmpty(variant))
					variant = variant + ",";
			}

			SetXkbLayout(parentLayout, variant, option);

			// In Wasta 20.04 and 22.04, when you move from an ibus input area to an xkb
			// input area, in some situations this can result in the old ibus icon still
			// showing in the Cinnamon panel, after the new xkb icon briefly appears, even
			// though the keyboard is now typing with the new xkb keyboard. It may relate
			// to the ibus icon flipping between different icons for the same ibus
			// keyboard. To work around this, we can pause for a moment before setting the
			// xkb keyboard using ibus again. This results in the xkb keyboard icon showing
			// and staying in the panel. How long to pause may be related to CPU speed,
			// which means no value is necessarily right. The icon in the panel usually
			// corresponds to the first item in the list from
			// `gsettings get org.freedesktop.ibus.general engines-order`, but not always
			// so after the icon is wrong.
			// The problem is presumably related to other areas of our keyboard handling,
			// and so more investigation into what is happening may relieve the situation
			// without the need for a sleep.
			// The problem is merely cosmetic and may cause confusion, but does not have a
			// behavioural problem and can be ignored without needing to delay.

			bool switchingFromXkb = false;

			if (_ibusToXkbDelayMs > 0)
			{
				string output = KeyboardRetrievingHelper
					.RunOnHostEvenIfFlatpak("gsettings", $"get org.freedesktop.ibus.general engines-order")
					.StandardOutput;
				string[] panelKeyboardList = KeyboardRetrievingHelper.ToStringArray(output);
				switchingFromXkb = panelKeyboardList.Count() > 0
					&& panelKeyboardList.First().StartsWith("xkb:", StringComparison.InvariantCulture);
			}

			// Instruct IBus to set the xkb or IBus keyboard
			var context = GlobalCachedInputContext.InputContext;
			string desiredKeyboard = ibusKeyboard.IBusKeyboardEngine.LongName;
			context.SetEngine(desiredKeyboard);

			if (_ibusToXkbDelayMs > 0)
			{
				bool switchingToXkb = desiredKeyboard.StartsWith("xkb:", StringComparison.InvariantCulture);
				if (!switchingFromXkb && switchingToXkb)
				{
					// Help the ibus icon also show the correct keyboard icon in the
					// Cinnamon panel.
					System.Threading.Thread.Sleep(_ibusToXkbDelayMs);
					context.SetEngine(desiredKeyboard);
				}
			}
		}

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
					var desired = String.Format("xkb:{0}:", DefaultLayout);
					if (!String.IsNullOrEmpty (DefaultVariant))
						desired = String.Format ("xkb:{0}:{1}:", DefaultLayout, DefaultVariant);
					var pattern = String.Format("[^A-Za-z]{0}[^A-Za-z]|^{0}[^A-Za-z]|.*[^A-Za-z]{0}$",
						DefaultLayout);
					var regex = new System.Text.RegularExpressions.Regex(pattern);
					KeyboardDescription first = null;
					foreach (KeyboardDescription kbd in KeyboardController.Instance.AvailableKeyboards.OfType<KeyboardDescription>())
					{
						if (first == null)
							first = kbd;	// last-ditch value if all else fails
						if (kbd.Layout.StartsWith(desired, StringComparison.InvariantCulture)
							|| kbd.Layout == DefaultLayout)
						{
							_defaultKeyboard = kbd;
							break;
						}
						// REVIEW (EberhardB): it is unclear if we can ever get into a situation
						// where regex would match. That would require an xkb keyboard reported by
						// ibus that doesn't start with 'xkb:'. Can this happen? If it can, do we
						// have to take DefaultVariant into account as well?
						if (regex.IsMatch(kbd.Layout))
						{
							_defaultKeyboard = kbd;
						}
					}
					if (_defaultKeyboard == null)
						_defaultKeyboard = first ?? KeyboardController.NullKeyboard;
				}
				return _defaultKeyboard;
			}
		}

		/// <summary>
		/// Implementation is not required because the default implementation of KeyboardController
		/// is sufficient.
		/// </summary>
		public override KeyboardDescription ActiveKeyboard
		{
			get { return null; }
		}
	}
}
