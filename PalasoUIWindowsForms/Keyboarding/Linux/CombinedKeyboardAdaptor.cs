// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Ubuntu versions >= 13.10.
	/// Previously there were two separate keyboard icons for XKB and for IBus keyboards.
	/// Starting with 13.10 there is now just one and the list of keyboards gets stored in
	/// a different dconf key (/org/gnome/desktop/input-sources/sources). This adaptor reads
	/// the list of keyboards and then delegates the individual keyboards to the respective
	/// real keyboard controller.
	/// XKB keyboards get handled by IBus as well.
	/// </summary>
	public class CombinedKeyboardAdaptor: IKeyboardAdaptor
	{
		private static bool HasCombinedKeyboards = true;
		private List<string> IbusKeyboards;
		private List<string> XkbKeyboards;

		#region P/Invoke imports for glib and dconf
		[DllImport("libgobject-2.0-0.dll")]
		private extern static void g_type_init();

		[DllImport("libgobject-2.0-0.dll")]
		private extern static string g_variant_print(IntPtr variant, bool typeAnnotate);

		[DllImport("libdconf.dll")]
		private extern static IntPtr dconf_client_new();

		[DllImport("libdconf.dll")]
		private extern static IntPtr dconf_client_read(IntPtr client, string key);
		#endregion

		public CombinedKeyboardAdaptor()
		{
			g_type_init();
		}

		private static string GetValue(string raw)
		{
			return raw.Trim().Trim('\'');
		}

		private void RegisterXkbKeyboards()
		{
			if (XkbKeyboards.Count <= 0)
				return;

			var xkbAdaptor = GetAdaptor<XkbKeyboardAdaptor>();
			xkbAdaptor.AddKeyboards(XkbKeyboards);
		}

		private void RegisterIbusKeyboards()
		{
			if (IbusKeyboards.Count <= 0)
				return;

			var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
			ibusAdaptor.AddKeyboards(IbusKeyboards);
		}

		private static T GetAdaptor<T>() where T: class
		{
			foreach (var adaptor in KeyboardController.Adaptors)
			{
				var tAdaptor = adaptor as T;
				if (tAdaptor != default(T))
					return tAdaptor;
			}
			return default(T);
		}

		private void AddKeyboard(string source)
		{
			var parts = source.Trim('(', ')').Split(',');
			Debug.Assert(parts.Length == 2);
			if (parts.Length != 2)
				return;

			var type = GetValue(parts[0]);
			var layout = GetValue(parts[1]);
			if (type == "xkb")
				XkbKeyboards.Add(layout);
//				// IBus also handles the XKB keyboards in Ubuntu >= 13.10
//				IbusKeyboards.Add(string.Format("{0}:{1}", type, layout.Replace("+", "::")));
			else
				IbusKeyboards.Add(layout);
		}

		private void AddKeyboards(string source)
		{
			int startNextSegment = source.IndexOf("), (", StringComparison.InvariantCulture);
			if (startNextSegment < 0)
				AddKeyboard(source);
			else
			{
				AddKeyboard(source.Substring(0, startNextSegment));
				AddKeyboards(source.Substring(startNextSegment + 4));
			}
		}

		private void AddAllKeyboards(string source)
		{
			AddKeyboards(source);
			RegisterIbusKeyboards();
			RegisterXkbKeyboards();
		}

		#region IKeyboardAdaptor implementation
		public void Initialize()
		{
			if (!HasCombinedKeyboards)
				return;

			IbusKeyboards = new List<string>();
			XkbKeyboards = new List<string>();

			// NOTE: we directly use glib/dconf methods here since we don't want to
			// introduce an otherwise unnecessary dependency on gconf-sharp/gnome-sharp.
			IntPtr client = IntPtr.Zero;
			try
			{
				client = dconf_client_new();
			}
			catch (DllNotFoundException)
			{
				// Older Ubuntu versions have a version of the dconf library with a
				// different version number. However, since those Ubuntu versions don't
				// have combined keyboards this really doesn't matter.
				HasCombinedKeyboards = false;
				return;
			}

			var sources = dconf_client_read(client, "/org/gnome/desktop/input-sources/sources");
			if (sources == IntPtr.Zero)
			{
				HasCombinedKeyboards = false;
				return;
			}

			AddAllKeyboards(g_variant_print(sources, false).Trim('[', ']'));
		}

		public void UpdateAvailableKeyboards()
		{
			Initialize();
		}

		public void Close()
		{
		}

		public bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			throw new NotImplementedException();
		}

		public void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			throw new NotImplementedException();
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
	}
}

