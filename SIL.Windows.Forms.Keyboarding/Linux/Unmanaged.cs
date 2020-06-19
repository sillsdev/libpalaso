// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
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

		[DllImport("libgnome-desktop-3.so.17")]
		internal static extern IntPtr gnome_xkb_info_new();

		[DllImport("libgnome-desktop-3.so.17")]
		internal static extern IntPtr gnome_xkb_info_get_all_layouts(IntPtr self);

		[DllImport("libgnome-desktop-3.so.17")]
		internal static extern bool gnome_xkb_info_get_layout_info(IntPtr self, IntPtr id,
			out IntPtr displayName, out IntPtr shortName, out IntPtr xkbLayout,
			out IntPtr xkbVariant);
	}
}