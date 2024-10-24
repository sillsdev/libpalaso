// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	public static class NativeReplacements
	{
		private static Assembly _monoWinFormsAssembly;

		// internal mono WinForms type
		private static Type _xplatUIX11;

		// internal mono WinForms type
		private static Type _xplatUI;

		// internal mono WinForms type
		private static Type _hwnd;

		// internal mono WinForms type
		private static Type _X11Keyboard;

		internal static Assembly MonoWinFormsAssembly
		{
			get
			{
				if (_monoWinFormsAssembly == null)
#pragma warning disable 0618 // Using Obsolete method LoadWithPartialName.
					_monoWinFormsAssembly = Assembly.LoadWithPartialName("System.Windows.Forms");
#pragma warning restore 0618


				return _monoWinFormsAssembly;
			}
		}

		private static Type XplatUIX11
		{
			get
			{
				if (_xplatUIX11 == null)
					_xplatUIX11 = MonoWinFormsAssembly.GetType("System.Windows.Forms.XplatUIX11");

				return _xplatUIX11;
			}
		}

		private static Type XplatUI
		{
			get
			{
				if (_xplatUI == null)
					_xplatUI = MonoWinFormsAssembly.GetType("System.Windows.Forms.XplatUI");

				return _xplatUI;
			}
		}

		private static Type Hwnd
		{
			get
			{
				if (_hwnd == null)
					_hwnd = MonoWinFormsAssembly.GetType("System.Windows.Forms.Hwnd");

				return _hwnd;
			}
		}

		private static Type X11Keyboard
		{
			get
			{
				if (_X11Keyboard == null)
					_X11Keyboard = MonoWinFormsAssembly.GetType("System.Windows.Forms.X11Keyboard");

				return _X11Keyboard;
			}
		}

		#region keyboard

		/// <summary>
		/// Sets XplatUI.State.ModifierKeys, which is what the Control.ModifierKeys WinForm property returns.
		/// </summary>
		public static void SetKeyStateTable(int virtualKey, byte value)
		{
			var keyboard = XplatUIX11.GetField("Keyboard",
												System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			if (keyboard == null)
				return;

			var key_state_table = X11Keyboard.GetField("key_state_table", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			if (key_state_table == null)
				return;

			var b = (byte[])key_state_table.GetValue(keyboard.GetValue(null));

			b[virtualKey] = value;

			key_state_table.SetValue(keyboard.GetValue(null), b);

		}

		#endregion

		#region GetFocus

		/// <summary>
		/// Gets the focus.
		/// </summary>
		/// <returns>
		/// The focus.
		/// </returns>
		public static IntPtr GetFocus()
		{
			if (Platform.IsWindows)
				return WindowsGetFocus();

			return MonoGetFocus();
		}

		/// <summary></summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetFocus")]
		static extern IntPtr WindowsGetFocus();

		// internal mono WinForms static instance that traces focus
		private static FieldInfo _focusWindow;

		// internal mono Winforms static instance handle to the X server.
		public static FieldInfo _displayHandle;

		// internal mono WinForms Hwnd.whole_window
		internal static FieldInfo _wholeWindow;

		// internal mono WinForms Hwnd.GetHandleFromWindow
		internal static MethodInfo _getHandleFromWindow;

		// internal mono WinForms method Hwnd.ObjectFromHandle
		internal static MethodInfo _objectFromHandle;

		/// <summary>
		/// Gets mono's internal Focused Window Ptr/Handle.
		/// </summary>
		public static IntPtr MonoGetFocus()
		{
			if (_focusWindow == null)
				_focusWindow = XplatUIX11.GetField("FocusWindow",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			// Get static field to determine Focused Window.
			return (IntPtr)_focusWindow.GetValue(null);
		}

		/// <summary>
		/// Get mono's internal display handle to the X server
		/// </summary>
		public static IntPtr MonoGetDisplayHandle()
		{
			if (_displayHandle == null)
				_displayHandle = XplatUIX11.GetField("DisplayHandle",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			return (IntPtr)_displayHandle.GetValue(null);
		}

		private static object GetHwnd(IntPtr handle)
		{
			// first call call Hwnd.ObjectFromHandle to get the hwnd.
			if (_objectFromHandle == null)
				_objectFromHandle = Hwnd.GetMethod("ObjectFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

			return _objectFromHandle.Invoke(null, new object[] { handle });
		}

		/// <summary>
		/// Get an x11 Window Id from a winforms Control handle
		/// </summary>
		public static IntPtr MonoGetX11Window(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return IntPtr.Zero;

			object hwnd = GetHwnd(handle);

			if (_wholeWindow == null)
				_wholeWindow = Hwnd.GetField("whole_window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			return (IntPtr)_wholeWindow.GetValue(hwnd);
		}

		/// <summary>
		/// Get a WinForm Control/Handle from an x11 Window Id / windowHandle
		/// </summary>
		/// <returns>
		/// The Control Handle or IntPtr.Zero if window id doesn't represent an winforms control.
		/// </returns>
		/// <param name='windowHandle'>
		/// Window handle / x11 Window Id.
		/// </param>
		public static IntPtr MonoGetHandleFromWindowHandle(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero)
				return IntPtr.Zero;

			if (_getHandleFromWindow == null)
				_getHandleFromWindow = Hwnd.GetMethod("GetHandleFromWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

			return (IntPtr)_getHandleFromWindow.Invoke(null, new object[] { windowHandle });

		}

		#endregion

		#region SendSetFocusWindowsMessage

		public static void SendSetFocusWindowsMessage(Control control, IntPtr fromHandle)
		{
			if (control == null)
				throw new ArgumentNullException("control");

			if (control.IsDisposed)
				throw new ObjectDisposedException("control");

			if (Platform.IsWindows)
				NativeSendMessage(control.Handle, WM_SETFOCUS, (int)fromHandle, 0);
			else
			{
				// NativeSendMessage seem to force creation of the control.
				if (!control.IsHandleCreated)
					control.CreateControl();

				try
				{
					control.Focus();
				}
				catch
				{ /* FB36027 */
				}
			}
		}

		public const int WM_SETFOCUS = 0x7;

		[DllImport("user32.dll", EntryPoint = "SendMessage")]
		static extern int NativeSendMessage(
			IntPtr hWnd,      // handle to destination window
			uint Msg,     // message
			int wParam,  // first message parameter
			int lParam   // second message parameter
		);

		#endregion

		#region SendMessage

		private static MethodInfo _sendMessage;

		/// <summary>
		/// Please don't use this unless your really have to, and then only if its for sending messages internaly within the application.
		/// For example sending WM_NCPAINT maybe portable but sending WM_USER + N to another application is definitely not poratable.
		/// </summary>
		public static void SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam)
		{
			if (Platform.IsDotNet)
			{
				NativeSendMessage(hWnd, Msg, wParam, lParam);
			}
			else
			{
				if (_sendMessage == null)
					_sendMessage = XplatUI.GetMethod("SendMessage",
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new Type[] { typeof(IntPtr), typeof(int), typeof(IntPtr), typeof(IntPtr) }, null);

				_sendMessage.Invoke(null, new object[] { hWnd, (int)Msg, (IntPtr)wParam, (IntPtr)lParam });
			}
		}

		#endregion

		#region X window properties methods

		public static void SetWmClass(string name, string @class, IntPtr handle)
		{
			var a = new NativeX11Methods.XClassHint { res_name = Marshal.StringToCoTaskMemAnsi(name), res_class = Marshal.StringToCoTaskMemAnsi(@class) };
			IntPtr classHints = Marshal.AllocCoTaskMem(Marshal.SizeOf(a));
			Marshal.StructureToPtr(a, classHints, true);

			NativeX11Methods.XSetClassHint(NativeReplacements.MonoGetDisplayHandle(), NativeReplacements.MonoGetX11Window(handle), classHints);

			Marshal.FreeCoTaskMem(a.res_name);
			Marshal.FreeCoTaskMem(a.res_class);

			Marshal.FreeCoTaskMem(classHints);
		}

		/// <summary>
		/// Set a winform windows "X group leader" value.
		/// By default all mono winform applications get the same group leader (WM_HINTS property)
		/// (use xprop to see a windows WM_HINTS values)
		/// </summary>
		public static void SetGroupLeader(IntPtr handle, IntPtr newValue)
		{
			var x11Handle = MonoGetX11Window(handle);
			IntPtr ptr = NativeX11Methods.XGetWMHints(NativeReplacements.MonoGetDisplayHandle(), x11Handle);
			var wmhints = (NativeX11Methods.XWMHints)Marshal.PtrToStructure(ptr, typeof(NativeX11Methods.XWMHints));
			NativeX11Methods.XFree(ptr);
			wmhints.window_group = NativeReplacements.MonoGetX11Window(newValue);
			NativeX11Methods.XSetWMHints(NativeReplacements.MonoGetDisplayHandle(), NativeReplacements.MonoGetX11Window(x11Handle), ref wmhints);
		}

		#endregion
	}
}
