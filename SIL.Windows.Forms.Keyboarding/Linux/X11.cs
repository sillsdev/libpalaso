// Copyright (c) 2011-2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace X11
{
	/// <summary>
	/// Declarations of unmanaged X11 functions
	/// </summary>
	internal static class Unmanaged
	{
		/// <summary/>
		[DllImport("libX11", EntryPoint="XOpenDisplay")]
		public extern static IntPtr XOpenDisplay(IntPtr display);
		/// <summary/>
		[DllImport("libX11", EntryPoint="XCloseDisplay")]
		public extern static int XCloseDisplay(IntPtr display);
		/// <summary/>
		[DllImport ("libX11", EntryPoint="XKeycodeToKeysym")]
		public extern static int XKeycodeToKeysym(IntPtr display, int keycode, int index);
	}

	internal static class X11Helper
	{
		/// <summary>
		/// Gets the X11 display connection that Mono already has open, rather than
		/// carefully opening and closing it on our own in a way that doesnt crash (FWNX-895).
		/// </summary>
		/// <remarks><para>NOTE: The display connection should not be closed!</para>
		/// <para>NOTE: when running unit tests Mono's DisplayHandle might not be initialized.
		/// One way to get it intialized is to create a control.</para></remarks>
		internal static IntPtr GetDisplayConnection()
		{
			var swfAssembly = Assembly.GetAssembly(typeof(System.Windows.Forms.Form));
			var xplatuix11Type = swfAssembly.GetType("System.Windows.Forms.XplatUIX11");
			xplatuix11Type.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

			var displayHandleField = xplatuix11Type.GetField("DisplayHandle", BindingFlags.Static | BindingFlags.NonPublic);
			var displayHandleValue = displayHandleField.GetValue(null);
			var displayConnection = (IntPtr)displayHandleValue;

			Debug.Assert(displayConnection != IntPtr.Zero, "Expected to have a handle on X11 display connection.");
			return displayConnection;
		}

	}
}
