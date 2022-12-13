// Copyright (c) 2015-2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Helps interacting with GIO (https://docs.gtk.org/gio/index.html).
	/// </summary>
	public static class GlibHelper
	{
		/// <summary>
		/// Returns <c>true</c> if the <paramref name="schema"/> is installed on the machine,
		/// otherwise <c>false</c>.
		/// </summary>
		public static bool SchemaIsInstalled(string schema)
		{
			return Unmanaged.g_settings_schema_source_lookup(Unmanaged.g_settings_schema_source_get_default(),
				schema, recursive: true) != IntPtr.Zero;
		}

		/// <summary>
		/// Convert a GVariant handle that points to a list of strings to a C# string array.
		/// Without leaking memory in the process!
		/// </summary>
		/// <remarks>
		/// No check is made to verify that the input value actually points to a list of strings.
		/// </remarks>
		public static string[] GetStringArrayFromGVariantArray(IntPtr value)
		{
			if (value == IntPtr.Zero)
				return new string[0];
			var size = Unmanaged.g_variant_n_children(value);
			var list = new string[size];
			for (uint i = 0; i < size; ++i)
			{
				var child = Unmanaged.g_variant_get_child_value(value, i);
				// handle must not be freed -- it points into the actual GVariant memory for child!
				int length;
				var handle = Unmanaged.g_variant_get_string(child, out length);
				var rawbytes = new byte[length];
				Marshal.Copy(handle, rawbytes, 0, length);
				list[i] = Encoding.UTF8.GetString(rawbytes);
				Unmanaged.g_variant_unref(child);
				//Console.WriteLine("DEBUG GetStringArrayFromGVariant(): list[{0}] = \"{1}\" (length = {2})", i, list[i], length);
			}
			return list;
		}

		/// <summary>
		/// Convert a GVariant handle that points to a list of lists of two strings into
		/// a C# string array.  The original strings in the inner lists are separated by
		/// double semicolons in the output elements of the C# string array.
		/// </summary>
		/// <remarks>
		/// No check is made to verify that the input value actually points to a list of
		/// lists of two strings.
		/// </remarks>
		public static string[] GetStringArrayFromGVariantListArray(IntPtr value)
		{
			if (value == IntPtr.Zero)
				return new string[0];
			var size = Unmanaged.g_variant_n_children(value);
			var list = new string[size];
			for (uint i = 0; i < size; ++i)
			{
				var duple = Unmanaged.g_variant_get_child_value(value, i);
				var values = GetStringArrayFromGVariantArray(duple);
				Debug.Assert(values.Length == 2);
				list[i] = $"{values[0]};;{values[1]}";
				Unmanaged.g_variant_unref(duple);
				//Console.WriteLine("DEBUG GetStringArrayFromGVariantListArray(): list[{0}] = \"{1}\"", i, list[i]);
			}
			return list;
		}

		public static string[] GetStringArrayFromGList(IntPtr value)
		{
			var list = new List<string>();

			if (value == IntPtr.Zero)
				return list.ToArray();

			var glist = GList.FromPtr(value);
			for (; glist != null; glist = glist.Value.Next)
			{
				list.Add(glist.Value.Data);
			}

			return list.ToArray();
		}

	}
}