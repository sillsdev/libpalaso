#if __MonoCS__
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using IBusDotNet;
using NDesk.DBus;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// a global cache used only to reduce traffic with ibus via dbus.
	/// </summary>
	internal static class GlobalCachedInputContext
	{
		/// <summary>
		/// Caches the current InputContext.
		/// </summary>
		public static InputContext InputContext { get; set; }
		/// <summary>
		/// Cache the keyboard of the InputContext.
		/// </summary>
		public static IbusKeyboardDescription Keyboard { get; set; }

		/// <summary>
		/// Clear the cached InputContext details.
		/// </summary>
		public static void Clear()
		{
			InputContext = null;
		}
	}
}
#endif