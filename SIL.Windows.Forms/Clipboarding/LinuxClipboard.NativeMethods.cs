// // Copyright (c) 2025 SIL Global
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SIL.Windows.Forms.Miscellaneous;

namespace SIL.Windows.Forms.Clipboarding
{
	internal partial class LinuxClipboard
	{
		private const string gdk2lib = "libgdk-x11-2.0.so.0";
		private const string gdk3lib = "libgdk-3.so.0";
		private const string gtk2lib = "libgtk-x11-2.0.so.0";
		private const string gtk3lib = "libgtk-3.so.0";

		#region Native DLLImport'ed methods

		private const int RTLD_NOW = 2;

		private const string LIBDL_NAME   = "libdl.so.2";
		private const string LIBGLIB_NAME = "libglib-2.0.so.0";

		[DllImport(LIBDL_NAME, SetLastError = true)]
		private static extern IntPtr dlopen(string file, int mode);

		[DllImport(LIBDL_NAME, SetLastError = true)]
		private static extern int dlclose(IntPtr handle);

		[DllImport(LIBDL_NAME, SetLastError = true)]
		private static extern IntPtr dlsym(IntPtr handle, string name);

		[DllImport(LIBDL_NAME, EntryPoint = "dlerror")]
		private static extern IntPtr _dlerror();

		private static string dlerror()
		{
			// Don't free the string returned from _dlerror()!
			var ptr = _dlerror();
			return Marshal.PtrToStringAnsi(ptr);
		}

		[DllImport(LIBGLIB_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern void g_free(IntPtr mem);

		[DllImport(LIBGLIB_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr g_utf8_to_utf16(IntPtr native_str, IntPtr len, IntPtr items_read,
			ref IntPtr items_written, out GError error);

		[DllImport(LIBGLIB_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr g_utf16_to_utf8(IntPtr native_str, IntPtr len, IntPtr items_read,
			IntPtr items_written, out GError error);


		[StructLayout(LayoutKind.Sequential)]
		private struct GError
		{
			internal uint   domain;
			internal int    code;
			internal IntPtr message;
		}
		#endregion

		private static IntPtr _GdkLibHandle;
		private static IntPtr GdkLibHandle
		{
			get
			{
				if (_GdkLibHandle != IntPtr.Zero)
					return _GdkLibHandle;

				_GdkLibHandle = LoadLibrary(GraphicsManager.GtkVersionInUse == GraphicsManager.GtkVersion.Gtk2
					? gdk2lib
					: gdk3lib);
				return _GdkLibHandle;
			}
		}

		private static IntPtr _GtkLibHandle;
		private static IntPtr GtkLibHandle
		{
			get
			{
				if (_GtkLibHandle != IntPtr.Zero)
					return _GtkLibHandle;

				_GtkLibHandle = LoadLibrary(GraphicsManager.GtkVersionInUse == GraphicsManager.GtkVersion.Gtk2
						? gtk2lib
						: gtk3lib);
				return _GtkLibHandle;
			}
		}

		private static IntPtr LoadLibrary(string libraryName)
		{
			var handle = dlopen(libraryName, RTLD_NOW);
			var lastError = Marshal.GetLastWin32Error();

			Trace.WriteLineIf(handle == IntPtr.Zero && lastError != 0,
				$"Unable to load '{libraryName}'. Error: {lastError} ({dlerror()})");

			return handle;
		}

		// This method is thread-safe and idempotent
		private static T GetMethod<T>(IntPtr handle, string methodName) where T : class
		{
			var methodPointer = dlsym(handle, methodName);
			return methodPointer != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<T>(methodPointer) : default(T);
		}

		private class MethodsContainer
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate IntPtr gdk_atom_internDelegate(IntPtr atomName, bool onlyIfExists);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate bool gdk_pixbuf_save_to_buffervDelegate(IntPtr pixbuf,
				out IntPtr buffer, out IntPtr buffer_size, IntPtr type, IntPtr[] option_keys,
				IntPtr[] option_values, out GError error);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate IntPtr gtk_clipboard_getDelegate(IntPtr atom);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate void gtk_clipboard_storeDelegate(IntPtr clipboard);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate void gtk_clipboard_set_textDelegate(IntPtr clipboard, IntPtr text, int len);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate bool gtk_clipboard_wait_is_text_availableDelegate(IntPtr clipboard);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate IntPtr gtk_clipboard_wait_for_textDelegate(IntPtr clipboard);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate bool gtk_clipboard_wait_is_image_availableDelegate(IntPtr clipboard);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			internal delegate IntPtr gtk_clipboard_wait_for_imageDelegate(IntPtr raw);

			internal gdk_atom_internDelegate            gdk_atom_intern;
			internal gdk_pixbuf_save_to_buffervDelegate gdk_pixbuf_save_to_bufferv;

			internal gtk_clipboard_getDelegate              gtk_clipboard_get;
			internal gtk_clipboard_storeDelegate            gtk_clipboard_store;
			internal gtk_clipboard_set_textDelegate         gtk_clipboard_set_text;
			internal gtk_clipboard_wait_is_text_availableDelegate
															gtk_clipboard_wait_is_text_available;
			internal gtk_clipboard_wait_for_textDelegate    gtk_clipboard_wait_for_text;
			internal gtk_clipboard_wait_is_image_availableDelegate
															gtk_clipboard_wait_is_image_available;

			internal gtk_clipboard_wait_for_imageDelegate   gtk_clipboard_wait_for_image;
		}

		private static MethodsContainer _Methods;

		private static MethodsContainer Methods => _Methods ??= new MethodsContainer();

		private static IntPtr gdk_atom_intern(string atomName, bool onlyIfExists)
		{
			Methods.gdk_atom_intern ??=
				GetMethod<MethodsContainer.gdk_atom_internDelegate>(GdkLibHandle, nameof(gdk_atom_intern));
			var atomPtr = StringToUtf8Ptr(atomName, atomName.Length);
			return Methods.gdk_atom_intern(atomPtr, onlyIfExists);
		}

		private static bool gdk_pixbuf_save_to_bufferv(IntPtr pixBuf,
			out IntPtr buffer, out IntPtr bufferSize, IntPtr type, IntPtr[] option_keys,
			IntPtr[] option_values, out GError error)
		{
			Methods.gdk_pixbuf_save_to_bufferv ??=
				GetMethod<MethodsContainer.gdk_pixbuf_save_to_buffervDelegate>(GdkLibHandle, nameof(gdk_pixbuf_save_to_bufferv));
			return Methods.gdk_pixbuf_save_to_bufferv(pixBuf, out buffer, out bufferSize, type,
				option_keys, option_values, out error);
		}

		private static IntPtr gtk_clipboard_get(IntPtr atom)
		{
			Methods.gtk_clipboard_get ??=
				GetMethod<MethodsContainer.gtk_clipboard_getDelegate>(GtkLibHandle, nameof(gtk_clipboard_get));
			return Methods.gtk_clipboard_get(atom);
		}

		private static void gtk_clipboard_store(IntPtr clipboard)
		{
			Methods.gtk_clipboard_store ??=
				GetMethod<MethodsContainer.gtk_clipboard_storeDelegate>(GtkLibHandle, nameof(gtk_clipboard_store));
			Methods.gtk_clipboard_store(clipboard);
		}

		private static void gtk_clipboard_set_text(IntPtr clipboard, string text, int len)
		{
			Methods.gtk_clipboard_set_text ??=
				GetMethod<MethodsContainer.gtk_clipboard_set_textDelegate>(GtkLibHandle, nameof(gtk_clipboard_set_text));

			var utf8 = StringToUtf8Ptr(text, len);
			Methods.gtk_clipboard_set_text(clipboard, utf8, len);
		}

		private static bool gtk_clipboard_wait_is_text_available(IntPtr clipboard)
		{
			Methods.gtk_clipboard_wait_is_text_available ??=
				GetMethod<MethodsContainer.gtk_clipboard_wait_is_text_availableDelegate>(
					GtkLibHandle, nameof(gtk_clipboard_wait_is_text_available));

			return Methods.gtk_clipboard_wait_is_text_available(clipboard);
		}

		private string gtk_clipboard_wait_for_text(IntPtr clipboard)
		{
			Methods.gtk_clipboard_wait_for_text ??=
				GetMethod<MethodsContainer.gtk_clipboard_wait_for_textDelegate>(
					GtkLibHandle, nameof(gtk_clipboard_wait_for_text));

			var utf8Ptr = Methods.gtk_clipboard_wait_for_text(clipboard);
			var text = Utf8PtrToString(utf8Ptr);
			g_free(utf8Ptr);
			return text;
		}

		private bool gtk_clipboard_wait_is_image_available(IntPtr clipboard)
		{
			Methods.gtk_clipboard_wait_is_image_available ??=
				GetMethod<MethodsContainer.gtk_clipboard_wait_is_image_availableDelegate>(
					GtkLibHandle, nameof(gtk_clipboard_wait_is_image_available));

			return Methods.gtk_clipboard_wait_is_image_available(clipboard);
		}

		private IntPtr gtk_clipboard_wait_for_image(IntPtr clipboard)
		{
			Methods.gtk_clipboard_wait_for_image ??=
				GetMethod<MethodsContainer.gtk_clipboard_wait_for_imageDelegate>(
					GtkLibHandle, nameof(gtk_clipboard_wait_for_image));

			return Methods.gtk_clipboard_wait_for_image(clipboard);
		}

		private static string Utf8PtrToString(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			var zero = IntPtr.Zero;
			var utf16Ptr = g_utf8_to_utf16(ptr, new IntPtr(-1), IntPtr.Zero, ref zero, out var error);
			if (utf16Ptr == IntPtr.Zero)
				ThrowException(nameof(Utf8PtrToString), error);

			var utf16 = Marshal.PtrToStringUni(utf16Ptr);
			g_free(utf16Ptr);
			return utf16;
		}

		private static IntPtr StringToUtf8Ptr(string text, int len = -1)
		{
			if (string.IsNullOrEmpty(text))
				return IntPtr.Zero;

			if (len == -1)
				len = text.Length;

			var utf16Ptr = Marshal.StringToHGlobalUni(text);
			var utf8Ptr = g_utf16_to_utf8(utf16Ptr, new IntPtr(len), IntPtr.Zero,
				IntPtr.Zero, out var error);
			Marshal.FreeHGlobal(utf16Ptr);
			if (utf8Ptr == IntPtr.Zero)
				ThrowException(nameof(StringToUtf8Ptr), error);
			return utf8Ptr;
		}

		private static void ThrowException(string methodName, GError error)
		{
			var zero = IntPtr.Zero;
			var utf16Ptr = g_utf8_to_utf16(error.message, new IntPtr(-1), IntPtr.Zero,
				ref zero, out _);
			var utf16Error = Marshal.PtrToStringUni(utf16Ptr);
			g_free(utf16Ptr);
			throw new ApplicationException($"Error in {methodName}: {utf16Error}");
		}

		private byte[] SaveToBuffer(IntPtr pixBuf, string type)
		{
			if (pixBuf == IntPtr.Zero)
				return null;

			var utf8Ptr = StringToUtf8Ptr(type, type.Length);
			var success = gdk_pixbuf_save_to_bufferv(pixBuf, out var buffer, out var bufferSize,
				utf8Ptr, null, null, out var error);
			g_free(utf8Ptr);
			if (!success)
				ThrowException(nameof(SaveToBuffer), error);

			var destination = new byte[(int) bufferSize];
			Marshal.Copy(buffer, destination, 0, (int) bufferSize);
			g_free(buffer);
			return destination;
		}

	}
}