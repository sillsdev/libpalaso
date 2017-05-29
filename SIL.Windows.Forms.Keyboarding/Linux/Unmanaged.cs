﻿// Copyright (c) 2015 SIL International
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
		internal extern static void g_type_init();

		[DllImport("libgobject-2.0.so")]
		internal extern static void g_object_ref(IntPtr obj);

		[DllImport("libgobject-2.0.so")]
		internal extern static void g_object_unref(IntPtr obj);

		[DllImport("libgio-2.0.so")]
		internal extern static IntPtr g_settings_schema_source_get_default();

		[DllImport("libgio-2.0.so")]
		internal extern static IntPtr g_settings_schema_source_lookup(IntPtr source,
				string schema_id, bool recursive);

		[DllImport("libgio-2.0.so")]
		internal extern static IntPtr g_settings_new(string schema_id);

		[DllImport("libgio-2.0.so")]
		internal extern static IntPtr g_settings_get_value(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal extern static int g_settings_get_int(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal extern static bool g_settings_get_boolean(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal extern static string g_settings_get_string(IntPtr settings, string key);

		[DllImport("libgio-2.0.so")]
		internal extern static bool g_settings_set_uint(IntPtr settings, string key, uint value);

		[DllImport("libglib-2.0.so")]
		internal extern static void g_variant_unref(IntPtr value);

		[DllImport("libglib-2.0.so")]
		internal extern static uint g_variant_n_children(IntPtr value);

		[DllImport("libglib-2.0.so")]
		internal extern static IntPtr g_variant_get_child_value(IntPtr value, uint index_);

		[DllImport("libglib-2.0.so")]
		internal extern static IntPtr g_variant_get_string(IntPtr value, out int length);

		[DllImport("libglib-2.0.so")]
		internal extern static bool g_variant_get_boolean(IntPtr value);
	}
}