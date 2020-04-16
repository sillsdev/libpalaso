// Copyright (c) 2012-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Miscellaneous
{
	public static class X11
	{
		#region NativeReplacement methods
		/// <summary>
		/// Native methods
		/// </summary>
		private static class NativeReplacements
		{
			private static Assembly _monoWinFormsAssembly;

			// internal mono WinForms type
			private static Type _xplatUIX11;

			// internal mono WinForms type
			private static Type _hwnd;

			private static Assembly MonoWinFormsAssembly
			{
				get
				{
					if (_monoWinFormsAssembly == null)
					{
#pragma warning disable 0612,0618 // Using Obsolete method LoadWithPartialName.
						_monoWinFormsAssembly = Assembly.LoadWithPartialName("System.Windows.Forms");
#pragma warning restore 0612,0618
					}

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

			private static Type Hwnd
			{
				get
				{
					if (_hwnd == null)
						_hwnd = MonoWinFormsAssembly.GetType("System.Windows.Forms.Hwnd");

					return _hwnd;
				}
			}

			// internal mono Winforms static instance handle to the X server.
			private static FieldInfo _displayHandleFieldInfo;

			// internal mono WinForms Hwnd.whole_window
			private static FieldInfo _wholeWindowFieldInfo;

			// internal mono WinForms method Hwnd.ObjectFromHandle
			private static MethodInfo _objectFromHandleMethodInfo;

			/// <summary>
			/// Get mono's internal display handle to the X server
			/// </summary>
			public static IntPtr MonoGetDisplayHandle()
			{
				if (_displayHandleFieldInfo == null)
					_displayHandleFieldInfo = XplatUIX11.GetField("DisplayHandle",
						BindingFlags.NonPublic | BindingFlags.Static);

				return (IntPtr)_displayHandleFieldInfo.GetValue(null);
			}

			private static object GetHwnd(IntPtr handle)
			{
				// first call Hwnd.ObjectFromHandle to get the hwnd.
				if (_objectFromHandleMethodInfo == null)
					_objectFromHandleMethodInfo =
						Hwnd.GetMethod("ObjectFromHandle", BindingFlags.Public | BindingFlags.Static);

				return _objectFromHandleMethodInfo.Invoke(null, new object[] { handle });
			}

			/// <summary>
			/// Get an x11 Window Id from a winforms Control handle
			/// </summary>
			public static IntPtr MonoGetX11Window(IntPtr handle)
			{
				if (handle == IntPtr.Zero)
					return IntPtr.Zero;

				var hwnd = GetHwnd(handle);

				if (_wholeWindowFieldInfo == null)
					_wholeWindowFieldInfo = Hwnd.GetField("whole_window", BindingFlags.NonPublic | BindingFlags.Instance);

				return (IntPtr)_wholeWindowFieldInfo.GetValue(hwnd);
			}
		}
		#endregion

		[DllImport ("libX11", EntryPoint="XSetClassHint", CharSet=CharSet.Ansi)]
		private static extern int XSetClassHint(IntPtr display, IntPtr window, IntPtr classHint);

		// Managed struct of XSetClassHint classHint.
		private struct XClassHint
		{
			public IntPtr res_name;
			public IntPtr res_class;
		}

		/// <summary>
		/// Set WM_CLASS property
		/// </summary>
		public static void SetWmClass(string name, IntPtr handle)
		{
			SetWmClass(name, name, handle);
		}

		/// <summary>
		/// Set WM_CLASS property
		/// </summary>
		/// <remarks>You should call this method after you opened your main form so that the
		/// icon in the launcher and the app menu will show the correct name/icon when running
		/// on Ubuntu 18.04. The <paramref name="name"/> should match the name of the
		/// application/launcher used to start the application, e.g. fieldworks-flex.</remarks>
		public static void SetWmClass(string name, string @class, IntPtr handle)
		{
			if (!Platform.IsLinux)
				return;

			var classHint = new XClassHint { res_name = Marshal.StringToCoTaskMemAnsi(name),
				res_class = Marshal.StringToCoTaskMemAnsi(@class) };
			var classHints = Marshal.AllocCoTaskMem(Marshal.SizeOf(classHint));
			Marshal.StructureToPtr(classHint, classHints, true);

			XSetClassHint(NativeReplacements.MonoGetDisplayHandle(),
				NativeReplacements.MonoGetX11Window(handle), classHints);

			Marshal.FreeCoTaskMem(classHint.res_name);
			Marshal.FreeCoTaskMem(classHint.res_class);
			Marshal.FreeCoTaskMem(classHints);
		}
	}
}
