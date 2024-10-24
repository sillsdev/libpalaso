// Copyright (c) 2024, SIL Global.
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using X11;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Utility class to convert WinForms keys to X11 key symbols and scan codes.
	/// </summary>
	/// <remarks>When the user presses a key on the keyboard the X11 system generates an XEvent
	/// which the XplatUIX11 adapter of Mono's WinForms implementation handles. It translates
	/// the X11 key symbol in corresponding WinForms keys. Unfortunately there is no reliable way
	/// to hook into the system at that point, so we have to deal with key presses during the
	/// WinForms events. When we want to pass the key press to IBus we have to convert it back
	/// to X11 key symbols and scan codes since there is no way to directly access that
	/// information in the event.</remarks>
	internal class X11KeyConverter
	{
		private static X11KeyConverter Instance;

		/// <summary>
		/// Maps X11 keySyms to scan codes
		/// </summary>
		private Dictionary<int, int> KeyMap;

		/// <summary>
		/// Gets the X11 scan code for the passed in X11 key symbol, or -1 if we don't know
		/// the scan code.
		/// </summary>
		internal static int GetScanCode(int keySym)
		{
			if (Instance == null)
				Instance = new X11KeyConverter();
			return Instance.Convert(keySym);
		}

		/// <summary>
		/// For the function keys and certain cursor keys this method gets the X11 key symbol for
		/// the passed in WinForms key. For all other keys an InvalidProgramException will be
		/// thrown.
		/// </summary>
		internal static int GetKeySym(Keys key)
		{
			// These values are from keysymdef.h
			if (key >= Keys.F1 && key <= Keys.F24)
			{
				return 0xffbe + (key - Keys.F1);
			}
			switch (key)
			{
				case Keys.Back:
					return 0xff08; // XK_BackSpace
				case Keys.Enter:
					return 0xff0d; // XK_Return
				case Keys.Home:
					return 0xff50; // XK_Home
				case Keys.Left:
					return 0xff51; // XK_Left
				case Keys.Up:
					return 0xff52; // XK_Up
				case Keys.Right:
					return 0xff53; // XK_Right
				case Keys.Down:
					return 0xff54; // XK_Down
				case Keys.PageUp:
					return 0xff55; // XK_Page_Up
				case Keys.PageDown:
					return 0xff56; // XK_Page_Down
				case Keys.End:
					return 0xff57; // XK_End
				case Keys.Delete:
					return 0xffff; // XK_Delete
			}
			return (int)key;
		}

		private X11KeyConverter()
		{
			EnsureInitialized();
		}

		/// <summary>
		/// If necessary initializes the keySym-to-scancode map.
		/// </summary>
		/// <remarks>The keySym-to-scancode map gets initialized by iterating over the
		/// keyc2scan field of XplatUIX11 and then calling an X11 function to convert the
		/// iterated keyCode to a keySym. This is done for the unshifted and shifted case of the
		/// keycode.</remarks>
		private void EnsureInitialized()
		{
			if (KeyMap != null)
				return;

			var swfAssembly = Assembly.GetAssembly(typeof(System.Windows.Forms.Form));
			var xplatUIX11Type = swfAssembly.GetType("System.Windows.Forms.XplatUIX11");
			var x11KeyboardInfo = xplatUIX11Type.GetField("Keyboard", BindingFlags.Static | BindingFlags.NonPublic);
			Debug.Assert(x11KeyboardInfo != null, "Can't access XplatUIX11.Keyboard through reflection. Did the Mono implementation change?");
			var x11KeyboardObj = x11KeyboardInfo.GetValue(null);
			var keyc2scanInfo = x11KeyboardObj.GetType().GetField("keyc2scan", BindingFlags.Instance | BindingFlags.NonPublic);
			Debug.Assert(keyc2scanInfo != null, "Can't access X11Keyboard.keyc2scan through reflection. Did the Mono implementation change?");
			var keyc2scan = keyc2scanInfo.GetValue(x11KeyboardObj) as int[];

			IntPtr display = X11Helper.GetDisplayConnection();
			if (display != IntPtr.Zero && keyc2scan != null)
			{
				KeyMap = new Dictionary<int, int>();

				for (int keyCode = 0; keyCode < keyc2scan.Length; keyCode++)
				{
					var scanCode = keyc2scan[keyCode];
					for (int shifted = 0; shifted <= 1; shifted++)
					{
						var keySym = X11.Unmanaged.XKeycodeToKeysym(display, keyCode, shifted);
						if (!KeyMap.ContainsKey(keySym))
							KeyMap.Add(keySym, scanCode);
					}
				}
			}
		}

		/// <summary>
		/// Gets the scan code for the specified keySym.
		/// </summary>
		private int Convert(int keySym)
		{
			EnsureInitialized();

			if (KeyMap.TryGetValue(keySym, out var scanCode))
				return scanCode;
			return -1;
		}
	}
}
