// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	public static class KeyboardRetrievingHelper
	{
		public static void InitGlib()
		{
			// g_type_init() is needed for Precise, but deprecated for Trusty.
			// Remove this (and the DllImport above) when we stop supporting Precise.
			Unmanaged.g_type_init();
		}

		/// <summary>
		/// Returns <c>true</c> if the <paramref name="schema"/> is installed on the machine,
		/// otherwise <c>false</c>.
		/// </summary>
		public static bool SchemaIsInstalled(string schema)
		{
			return Unmanaged.g_settings_schema_source_lookup(Unmanaged.g_settings_schema_source_get_default(),
				schema, recursive: true) != IntPtr.Zero;
		}

		public static void AddIbusVersionAsErrorReportProperty()
		{
			var settingsGeneral = IntPtr.Zero;
			try
			{
				const string ibusSchema = "org.freedesktop.ibus.general";
				if (!SchemaIsInstalled(ibusSchema))
					return;
				settingsGeneral = Unmanaged.g_settings_new(ibusSchema);
				if (settingsGeneral == IntPtr.Zero)
					return;
				var version = Unmanaged.g_settings_get_string(settingsGeneral, "version");
				ErrorReport.AddProperty("IbusVersion", version);
			}
			catch
			{
				// Ignore any error we might get
			}
			finally
			{
				if (settingsGeneral != IntPtr.Zero)
					Unmanaged.g_object_unref(settingsGeneral);
			}
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
				var handle = Unmanaged.g_variant_get_string(child, out var length);
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

	}
}
