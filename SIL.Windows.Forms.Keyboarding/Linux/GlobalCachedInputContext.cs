// Copyright (c) 2013-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using IBusDotNet;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// a global cache used only to reduce traffic with ibus via dbus.
	/// </summary>
	internal static class GlobalCachedInputContext
	{
		/// <summary>
		/// Caches the current InputContext.
		/// </summary>
		public static IInputContext InputContext { get; set; }
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
