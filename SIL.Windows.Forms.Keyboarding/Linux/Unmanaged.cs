// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	internal static class Unmanaged
	{
		// NOTE: we directly use glib methods here since we don't want to
		// introduce an otherwise unnecessary dependency on gconf-sharp/gnome-sharp.
		[DllImport("libgobject-2.0.so")]
		internal static extern void g_type_init();

		[DllImport("libgobject-2.0.so")]
		internal static extern void g_object_ref(IntPtr obj);

		[DllImport("libgobject-2.0.so")]
		internal static extern void g_object_unref(IntPtr obj);

		[DllImport("libgio-2.0.so")]
		internal static extern IntPtr g_settings_schema_source_get_default();

		[DllImport("libgio-2.0.so")]
		internal static extern IntPtr g_settings_schema_source_lookup(IntPtr source,
				string schema_id, bool recursive);

		[DllImport("libgio-2.0.so")]
		internal static extern IntPtr g_settings_new(string schema_id);

		[DllImport("libgio-2.0.so")]
		internal static extern IntPtr g_settings_get_value(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal static extern int g_settings_get_int(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal static extern bool g_settings_get_boolean(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal static extern string g_settings_get_string(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal static extern bool g_settings_set_uint(IntPtr settings, string key, uint value);

		[DllImport("libglib-2.0.so")]
		internal static extern void g_variant_unref(IntPtr value);

		[DllImport("libglib-2.0.so")]
		internal static extern uint g_variant_n_children(IntPtr value);

		[DllImport("libglib-2.0.so")]
		internal static extern IntPtr g_variant_get_child_value(IntPtr value, uint index_);

		[DllImport("libglib-2.0.so")]
		internal static extern IntPtr g_variant_get_string(IntPtr value, out int length);

		[DllImport("libglib-2.0.so")]
		internal static extern bool g_variant_get_boolean(IntPtr value);

		[DllImport("libglib-2.0.so")]
		internal static extern void g_list_free(IntPtr list);

		#region libgnome-desktop
		private class LibGnomeDesktopMethodsContainer
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate IntPtr gnome_xkb_info_newDelegate();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate IntPtr gnome_xkb_info_get_all_layoutsDelegate(IntPtr self);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate bool gnome_xkb_info_get_layout_infoDelegate(IntPtr self, IntPtr id,
				out IntPtr displayName, out IntPtr shortName, out IntPtr xkbLayout,
				out IntPtr xkbVariant);

			internal gnome_xkb_info_newDelegate             gnome_xkb_info_new;
			internal gnome_xkb_info_get_all_layoutsDelegate gnome_xkb_info_get_all_layouts;
			internal gnome_xkb_info_get_layout_infoDelegate gnome_xkb_info_get_layout_info;
		}

		private static LibGnomeDesktopMethodsContainer _LibGnomeDesktopMethods;

		private static LibGnomeDesktopMethodsContainer LibGnomeDesktopMethods => _LibGnomeDesktopMethods ??
			(_LibGnomeDesktopMethods = new LibGnomeDesktopMethodsContainer());

		private static IntPtr _LibGnomeDesktopHandle;

		private static IntPtr LibGnomeDesktopHandle
		{
			get
			{
				if (_LibGnomeDesktopHandle == IntPtr.Zero)
				{
					_LibGnomeDesktopHandle =
						LoadLibrary("libgnome-desktop-3.so", new[] { "17", "19" });
				}

				return _LibGnomeDesktopHandle;
			}
		}

		internal static void LibGnomeDesktopCleanup()
		{
			if (_LibGnomeDesktopHandle != IntPtr.Zero)
				dlclose(_LibGnomeDesktopHandle);

			_LibGnomeDesktopHandle = IntPtr.Zero;
		}

		internal static IntPtr gnome_xkb_info_new()
		{
			if (LibGnomeDesktopMethods.gnome_xkb_info_new == null)
			{
				LibGnomeDesktopMethods.gnome_xkb_info_new =
					GetMethod<LibGnomeDesktopMethodsContainer.gnome_xkb_info_newDelegate>
						(LibGnomeDesktopHandle, nameof(gnome_xkb_info_new));
			}

			return LibGnomeDesktopMethods.gnome_xkb_info_new();
		}

		internal static IntPtr gnome_xkb_info_get_all_layouts(IntPtr self)
		{
			if (LibGnomeDesktopMethods.gnome_xkb_info_get_all_layouts == null)
			{
				LibGnomeDesktopMethods.gnome_xkb_info_get_all_layouts =
					GetMethod<LibGnomeDesktopMethodsContainer.gnome_xkb_info_get_all_layoutsDelegate>
						(LibGnomeDesktopHandle, nameof(gnome_xkb_info_get_all_layouts));
			}

			return LibGnomeDesktopMethods.gnome_xkb_info_get_all_layouts(self);
		}

		internal static bool gnome_xkb_info_get_layout_info(IntPtr self, IntPtr id,
			out IntPtr displayName, out IntPtr shortName, out IntPtr xkbLayout,
			out IntPtr xkbVariant)
		{
			if (LibGnomeDesktopMethods.gnome_xkb_info_get_layout_info == null)
			{
				LibGnomeDesktopMethods.gnome_xkb_info_get_layout_info =
					GetMethod<LibGnomeDesktopMethodsContainer.gnome_xkb_info_get_layout_infoDelegate>
						(LibGnomeDesktopHandle, nameof(gnome_xkb_info_get_layout_info));
			}

			return LibGnomeDesktopMethods.gnome_xkb_info_get_layout_info(self, id, out displayName,
				out shortName, out xkbLayout, out xkbVariant);
		}

		#endregion

		#region Methods for dynamically loading a library

		private const int RTLD_NOW = 2;

		private const string LIBDL_NAME = "libdl.so.2";

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
		#endregion

		#region Dynamic loading of unmanaged library methods
		private static IntPtr LoadLibrary(string libraryName, string[] versionCandidates)
		{
			foreach (var version in versionCandidates)
			{
				var libName = $"{libraryName}.{version}";

				var handle = dlopen(libName, RTLD_NOW);
				if (handle != IntPtr.Zero)
					return handle;
			}

			throw new FileLoadException(
				$"Can't load library {libraryName}. Tried versions {string.Join(", ", versionCandidates)}.)",
				libraryName);
		}

		private static T GetMethod<T>(IntPtr handle, string methodName) where T : class
		{
			var methodPointer = dlsym(handle, methodName);

			if (methodPointer != IntPtr.Zero)
			{
				// NOTE: Starting in .NET 4.5.1, Marshal.GetDelegateForFunctionPointer(IntPtr, Type) is obsolete.
#if NET40
				return Marshal.GetDelegateForFunctionPointer(
					methodPointer, typeof(T)) as T;
#else
				return Marshal.GetDelegateForFunctionPointer<T>(methodPointer);
#endif
			}

			return default(T);
		}
		#endregion

	}
}