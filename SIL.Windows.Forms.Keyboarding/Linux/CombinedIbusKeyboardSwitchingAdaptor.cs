// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Extensions;

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

		public CombinedIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator) : base(ibusCommunicator)
		{
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
			startInfo.FileName = "/usr/bin/setxkbmap";
			var bldr = new StringBuilder();
			bldr.AppendFormat ("-layout {0}", layout);
			if (!string.IsNullOrEmpty(variant))
				bldr.AppendFormat (" -variant {0}", variant);
			if (!string.IsNullOrEmpty(option))
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
			var homedir = Environment.GetEnvironmentVariable("HOME");
			foreach (var file in knownXModMapFiles)
			{
				var filepath = System.IO.Path.Combine(homedir, file);
				if (!System.IO.File.Exists(filepath))
					continue;
				var startInfo = new ProcessStartInfo();
				startInfo.FileName = "xmodmap";
				startInfo.Arguments = filepath;
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

			var need_us_layout = false;
			foreach (var latinLayout in LatinLayouts)
			{
				if (parentLayout == latinLayout && variant != "eng")
				{
					need_us_layout = true;
					break;
				}
				if (!string.IsNullOrEmpty(variant) && $"{parentLayout}({variant})" == latinLayout)
				{
					need_us_layout = true;
					break;
				}
			}

			if (string.IsNullOrEmpty(parentLayout) || parentLayout == "default")
			{
				parentLayout = DefaultLayout;
				variant = DefaultVariant;
			}
			if (string.IsNullOrEmpty(variant) || variant == "default")
				variant = null;
			if (string.IsNullOrEmpty(option) || option == "default")
			{
				option = DefaultOption;
			}
			else if (!string.IsNullOrEmpty(DefaultOption))
			{
				option = DefaultOption.Split(',').Contains(option, StringComparison.InvariantCulture)
					? DefaultOption
					: $"{DefaultOption},{option}";
			}

			if (need_us_layout)
			{
				parentLayout += ",us";
				// If we have a variant, we need to indicate an empty variant to
				// match the "us" layout.
				if (!string.IsNullOrEmpty(variant))
					variant += ",";
			}

			SetXkbLayout(parentLayout, variant, option);

			if (ibusKeyboard.Name.StartsWith("xkb:", StringComparison.InvariantCulture))
				return;

			// Set the IBus keyboard
			var context = GlobalCachedInputContext.InputContext;
			context.SetEngine(ibusKeyboard.IBusKeyboardEngine.LongName);
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
					var desired = $"xkb:{DefaultLayout}:";
					if (!string.IsNullOrEmpty (DefaultVariant))
						desired = $"xkb:{DefaultLayout}:{DefaultVariant}:";
					var pattern = string.Format("[^A-Za-z]{0}[^A-Za-z]|^{0}[^A-Za-z]|.*[^A-Za-z]{0}$",
						DefaultLayout);
					var regex = new Regex(pattern);
					KeyboardDescription first = null;
					foreach (var kbd in KeyboardController.Instance.AvailableKeyboards.OfType<KeyboardDescription>())
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
		public override KeyboardDescription ActiveKeyboard => null;
	}
}
