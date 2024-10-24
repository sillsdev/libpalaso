// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Runtime.InteropServices;

namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	public static class NativeX11Methods
	{
		#region Native datatypes

		public enum RevertTo
		{
			None = 0,
			PointerRoot = 1,
			Parent = 2
		}

		// Managed struct of XSetClassHint classHint.
		public struct XClassHint
		{
			public IntPtr res_name;
			public IntPtr res_class;
		}

		#region Code from mono project (www.mono-project.com/‎) X11Structs.cs

		[Flags]
		internal enum XWMHintsFlags
		{
			InputHint = (1 << 0),
			StateHint = (1 << 1),
			IconPixmapHint = (1 << 2),
			IconWindowHint = (1 << 3),
			IconPositionHint = (1 << 4),
			IconMaskHint = (1 << 5),
			WindowGroupHint = (1 << 6),
			AllHints = (InputHint | StateHint | IconPixmapHint | IconWindowHint | IconPositionHint | IconMaskHint | WindowGroupHint)
		}

		internal enum XInitialState
		{
			DontCareState = 0,
			NormalState = 1,
			ZoomState = 2,
			IconicState = 3,
			InactiveState = 4
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct XWMHints
		{
			internal IntPtr flags;
			internal bool input;
			internal XInitialState initial_state;
			internal IntPtr icon_pixmap;
			internal IntPtr icon_window;
			internal int icon_x;
			internal int icon_y;
			internal IntPtr icon_mask;
			internal IntPtr window_group;
		}

		#endregion

		#endregion

		#region Linux/Mono only pinvokes

		[DllImport("libX11", EntryPoint = "XOpenDisplay")]
		public static extern IntPtr XOpenDisplay(IntPtr displayName);

		[DllImport("libX11", EntryPoint = "XCloseDisplay")]
		public static extern uint XCloseDisplay(IntPtr display);

		[DllImport("libX11", EntryPoint = "XQueryKeymap")]
		public static extern void XQueryKeyMap(IntPtr display, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] byte[] keys_return);

		[DllImport("libX11", EntryPoint = "XKeysymToKeycode")]
		public static extern uint XKeysymToKeycode(IntPtr display, uint keysym);

		[DllImport("libX11", EntryPoint = "XSetInputFocus")]
		public extern static int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);

		[DllImport("libX11", EntryPoint = "XGetInputFocus")]
		public extern static int XGetInputFocus(IntPtr display, out IntPtr focus_return, out RevertTo revert_to_return);

		[DllImport("libX11", EntryPoint = "XSetClassHint", CharSet = CharSet.Ansi)]
		public extern static int XSetClassHint(IntPtr display, IntPtr window, IntPtr classHint);

		[DllImport("libX11", EntryPoint = "XSetWMHints")]
		internal extern static void XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmhints);

		[DllImport("libX11", EntryPoint = "XGetWMHints")]
		internal extern static IntPtr XGetWMHints(IntPtr display, IntPtr window);

		[DllImport("libX11", EntryPoint = "XFree")]
		internal extern static void XFree(IntPtr data);

		#endregion
	}
}
