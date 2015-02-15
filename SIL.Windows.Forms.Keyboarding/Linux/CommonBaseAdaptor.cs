// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

#if __MonoCS__
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This is a base class for combined keyboard handlers that want to list keyboards
	/// in one place.  Trusty (Ubuntu 14.04) and Mint 17/Cinnamon (Wasta 14) both do this,
	/// but in somewhat different ways.
	/// </summary>
	internal abstract class CommonBaseAdaptor : IKeyboardAdaptor
	{
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

		protected CommonBaseAdaptor()
		{
			// g_type_init() is needed for Precise, but deprecated for Trusty.
			// Remove this (and the DllImport above) when we stop supporting Precise.
			g_type_init();
		}

		protected abstract string GSettingsSchema { get; }
		protected abstract string[] GetMyKeyboards(IntPtr client, IntPtr settings);
		protected abstract void AddAllKeyboards(string[] list);

		#region IKeyboardAdaptor implementation
		public virtual void Initialize()
		{
			IntPtr client = IntPtr.Zero;
			IntPtr settingsGeneral = IntPtr.Zero;
			try
			{
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
				string[] list = GetMyKeyboards(client, settingsGeneral);
				if (list == null)
					return;

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

		public abstract bool ActivateKeyboard(KeyboardDescription keyboard);

		public abstract void DeactivateKeyboard(KeyboardDescription keyboard);

		public KeyboardDescription GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		public abstract KeyboardDescription CreateKeyboardDefinition(string id);

		public abstract KeyboardDescription DefaultKeyboard
		{
			get;
		}

		public KeyboardAdaptorType Type
		{
			get
			{
				return KeyboardAdaptorType.System;
			}
		}

		public bool CanHandleFormat(KeyboardFormat format)
		{
			return format == KeyboardFormat.Unknown;
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

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~CommonBaseAdaptor()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			IsDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}
#endif
