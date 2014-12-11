// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using X11.XKlavier;
using SIL.WritingSystems.WindowsForms.Keyboarding.Interfaces;
using SIL.WritingSystems.WindowsForms.Keyboarding.InternalInterfaces;
using SIL.WritingSystems;
using Palaso.Reporting;

namespace SIL.WritingSystems.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// This is a base class for combined keyboard handlers that want to list keyboards
	/// in one place.  Trusty (Ubuntu 14.04) and Mint 17/Cinnamon (Wasta 14) both do this,
	/// but in somewhat different ways.
	/// </summary>
	public abstract class CommonBaseAdaptor: IKeyboardAdaptor
	{
		protected Dictionary<string,int> IbusKeyboards;
		protected Dictionary<string,int> XkbKeyboards;

		#region P/Invoke imports for glib and dconf
		// NOTE: we directly use glib/dconf methods here since we don't want to
		// introduce an otherwise unnecessary dependency on gconf-sharp/gnome-sharp.
		[DllImport("libgobject-2.0.so")]
		protected extern static void g_type_init();

		[DllImport("libgobject-2.0.so")]
		protected extern static IntPtr g_variant_new_uint32(UInt32 value);

		[DllImport("libgobject-2.0.so")]
		protected extern static void g_object_ref(IntPtr obj);

		[DllImport("libgobject-2.0.so")]
		protected extern static void g_object_unref(IntPtr obj);

		[DllImport("libgio-2.0.so")]
		protected extern static IntPtr g_settings_new(string schema_id);

		[DllImport("libgio-2.0.so")]
		protected extern static IntPtr g_settings_get_value(IntPtr settings, string key);

		[DllImport("libglib-2.0.so")]
		protected extern static void g_variant_unref(IntPtr value);

		[DllImport("libglib-2.0.so")]
		protected extern static uint g_variant_n_children(IntPtr value);

		[DllImport("libglib-2.0.so")]
		protected extern static IntPtr g_variant_get_child_value(IntPtr value, uint index_);

		[DllImport("libglib-2.0.so")]
		protected extern static IntPtr g_variant_get_string(IntPtr value, out int length);

		[DllImport("libglib-2.0.so")]
		protected extern static bool g_variant_get_boolean(IntPtr value);

		[DllImport("libdconf.dll")]
		protected extern static IntPtr dconf_client_new();

		[DllImport("libdconf.dll")]
		protected extern static IntPtr dconf_client_read(IntPtr client, string key);

		[DllImport("libdconf.dll")]
		protected extern static bool dconf_client_write_sync(IntPtr client, string key, IntPtr value,
			ref string tag, IntPtr cancellable, out IntPtr error);
		#endregion

		public CommonBaseAdaptor()
		{
			// g_type_init() is needed for Precise, but deprecated for Trusty.
			// Remove this (and the DllImport above) when we stop supporting Precise.
			g_type_init();
		}

		protected static T GetAdaptor<T>() where T: class
		{
			foreach (var adaptor in KeyboardController.Adaptors)
			{
				var tAdaptor = adaptor as T;
				if (tAdaptor != default(T))
					return tAdaptor;
			}
			return default(T);
		}

		protected abstract bool UseThisAdaptor { get; set; }
		protected abstract string GSettingsSchema { get; }
		protected abstract string[] GetMyKeyboards(IntPtr client, IntPtr settings);
		protected abstract void AddAllKeyboards(string[] list);

		#region IKeyboardAdaptor implementation
		public virtual void Initialize()
		{
			// Defaults to true, so if false we know we don't need to initialize any further.
			if (!UseThisAdaptor)
				return;
			IntPtr client = IntPtr.Zero;
			IntPtr settingsGeneral = IntPtr.Zero;
			try
			{
				IbusKeyboards = new Dictionary<string, int>();
				XkbKeyboards = new Dictionary<string, int>();
				UseThisAdaptor = false;
				try
				{
					client = dconf_client_new();
					if (!String.IsNullOrEmpty(GSettingsSchema))
						settingsGeneral = g_settings_new(GSettingsSchema);
				}
				catch (DllNotFoundException)
				{
					// Older versions of Linux have a version of the dconf library with a
					// different version number (different from what libdconf.dll gets
					// mapped to in app.config). However, since those Linux versions
					// don't have combined keyboards under IBus this really doesn't
					// matter.
					return;
				}
				var list = GetMyKeyboards(client, settingsGeneral);
				if (list == null)
					return;

				UseThisAdaptor = true;
				KeyboardController.Manager.ClearAllKeyboards();
				AddAllKeyboards(list);
			}
			finally
			{
				if (client != IntPtr.Zero)
					g_object_unref(client);
				if (settingsGeneral != IntPtr.Zero)
					g_object_unref(settingsGeneral);
			}
		}

		public virtual void UpdateAvailableKeyboards()
		{
			Initialize();
		}

		public virtual void Close()
		{
			throw new NotImplementedException();		// must be implemented by subclass
		}

		public virtual bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			throw new NotImplementedException();		// must be implemented by subclass
		}

		public virtual void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			throw new NotImplementedException();		// must be implemented by subclass
		}

		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		public IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			throw new NotImplementedException();
		}

		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IKeyboardDefinition DefaultKeyboard
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public KeyboardType Type
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

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
			uint size = g_variant_n_children(value);
			string[] list = new string[size];
			for (uint i = 0; i < size; ++i)
			{
				IntPtr child = g_variant_get_child_value(value, i);
				int length;
				// handle must not be freed -- it points into the actual GVariant memory for child!
				IntPtr handle = g_variant_get_string(child, out length);
				byte[] rawbytes = new byte[length];
				Marshal.Copy(handle, rawbytes, 0, length);
				list[i] = Encoding.UTF8.GetString(rawbytes);
				g_variant_unref(child);
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
			uint size = g_variant_n_children(value);
			string[] list = new string[size];
			for (uint i = 0; i < size; ++i)
			{
				IntPtr duple = g_variant_get_child_value(value, i);
				var values = GetStringArrayFromGVariantArray(duple);
				Debug.Assert(values.Length == 2);
				list[i] = String.Format("{0};;{1}", values[0], values[1]);
				g_variant_unref(duple);
				//Console.WriteLine("DEBUG GetStringArrayFromGVariantListArray(): list[{0}] = \"{1}\"", i, list[i]);
			}
			return list;
		}
	}
}
#endif
