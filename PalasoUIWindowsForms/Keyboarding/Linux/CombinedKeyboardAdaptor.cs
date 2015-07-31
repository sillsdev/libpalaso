// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using X11.XKlavier;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;
using Palaso.Reporting;

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
	public class CombinedKeyboardAdaptor: CommonBaseAdaptor
	{
		private static bool HasCombinedKeyboards = true;
		private int _kbdIndex;
		private IntPtr _client = IntPtr.Zero;

		public CombinedKeyboardAdaptor() : base()
		{
		}

		private void RegisterXkbKeyboards()
		{
			if (XkbKeyboards.Count <= 0)
				return;

			var xkbAdaptor = GetAdaptor<XkbKeyboardAdaptor>();
			var configRegistry = XklConfigRegistry.Create(xkbAdaptor.XklEngine);
			var layouts = configRegistry.Layouts;
			foreach (var kvp in layouts)
			{
				foreach (var layout in kvp.Value)
				{
					int index;
					// Custom keyboards may omit defining a country code.  Try to survive such cases.
					string codeToMatch;
					if (layout.CountryCode == null)
						codeToMatch = layout.LanguageCode.ToLowerInvariant();
					else
						codeToMatch = layout.CountryCode.ToLowerInvariant();
					if ((XkbKeyboards.TryGetValue(layout.LayoutId, out index) && (layout.LayoutId == codeToMatch)) ||
						XkbKeyboards.TryGetValue(string.Format("{0}+{1}", codeToMatch, layout.LayoutId), out index))
					{
						xkbAdaptor.AddKeyboardForLayout(layout, index, this);
					}
				}
			}
		}

		private void RegisterIbusKeyboards()
		{
			if (IbusKeyboards.Count <= 0)
				return;

			var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
			List<string> missingLayouts = new List<string>(IbusKeyboards.Keys);
			foreach (var ibusKeyboard in ibusAdaptor.GetAllIBusKeyboards())
			{
				if (IbusKeyboards.ContainsKey(ibusKeyboard.LongName))
				{
					missingLayouts.Remove(ibusKeyboard.LongName);
					var keyboard = new IbusKeyboardDescription(this, ibusKeyboard,
						IbusKeyboards[ibusKeyboard.LongName]);
					KeyboardController.Manager.RegisterKeyboard(keyboard);
				}
			}
			foreach (var layout in missingLayouts)
				Console.WriteLine("Didn't find " + layout);
		}

		private void AddKeyboard(string source)
		{
			var parts = source.Split(new String[]{";;"}, StringSplitOptions.None);
			Debug.Assert(parts.Length == 2);
			if (parts.Length != 2)
				return;
			var type = parts[0];
			var layout = parts[1];
			if (type == "xkb")
				XkbKeyboards.Add(layout, _kbdIndex);
			else
				IbusKeyboards.Add(layout, _kbdIndex);
			++_kbdIndex;
		}

		protected override void AddAllKeyboards(string[] source)
		{
			_kbdIndex = 0;
			for (int i = 0; i < source.Length; ++i)
				AddKeyboard(source[i]);
			RegisterIbusKeyboards();
			RegisterXkbKeyboards();
		}

		protected override bool UseThisAdaptor
		{
			get
			{
				return HasCombinedKeyboards;
			}
			set
			{
				HasCombinedKeyboards = value;
				KeyboardController.CombinedKeyboardHandling = value;
			}
		}

		protected override string GSettingsSchema { get { return null; } }	// we don't use GSettings

		/// <summary>
		/// Return the list of keyboards in the combined handler, or null if this adaptor should
		/// not be used.
		/// </summary>
		protected override string[] GetMyKeyboards(IntPtr client, IntPtr settingsGeneral)
		{
			// This is the proper path for the combined keyboard handling, not the path
			// given in the IBus reference documentation.
			var sources = dconf_client_read(client, "/org/gnome/desktop/input-sources/sources");
			if (sources == IntPtr.Zero)
				return null;
			var list = GetStringArrayFromGVariantListArray(sources);
			g_variant_unref(sources);

			// Save the connection to dconf since we use it in keyboard switching.
			_client = client;
			g_object_ref(_client);

			return list;
		}

		#region IKeyboardAdaptor implementation
		public override void Close()
		{
			if (_client != IntPtr.Zero)
			{
				g_object_unref(_client);
				_client = IntPtr.Zero;
			}
		}

		public override bool ActivateKeyboard(IKeyboardDefinition keyboard)
		{
			Debug.Assert(keyboard is KeyboardDescription);
			Debug.Assert(((KeyboardDescription)keyboard).Engine == this);
			if (keyboard is XkbKeyboardDescription)
			{
				var xkbKeyboard = keyboard as XkbKeyboardDescription;
				if (xkbKeyboard.GroupIndex >= 0)
					SelectKeyboard(xkbKeyboard.GroupIndex);
			}
			else if (keyboard is IbusKeyboardDescription)
			{
				var ibusKeyboard = keyboard as IbusKeyboardDescription;
				try
				{
					var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
					if (!ibusAdaptor.CanSetIbusKeyboard())
						return false;
					if (ibusAdaptor.IBusKeyboardAlreadySet(ibusKeyboard))
						return true;
					SelectKeyboard(ibusKeyboard.SystemIndex);
					GlobalCachedInputContext.Keyboard = ibusKeyboard;
				}
				catch (Exception e)
				{
					Debug.WriteLine(string.Format("Changing keyboard failed, is kfml/ibus running? {0}", e));
					return false;
				}
			}
			else
			{
				throw new ArgumentException();
			}
			return true;
		}

		public override void DeactivateKeyboard(IKeyboardDefinition keyboard)
		{
			if (keyboard is IbusKeyboardDescription)
			{
				var ibusAdaptor = GetAdaptor<IbusKeyboardAdaptor>();
				if (ibusAdaptor.CanSetIbusKeyboard())
					GlobalCachedInputContext.InputContext.Reset();
			}
			GlobalCachedInputContext.Keyboard = null;
		}
		#endregion

		private void SelectKeyboard(int index)
		{
			if (!HasCombinedKeyboards || _client == IntPtr.Zero)
				return;
			IntPtr value = g_variant_new_uint32((uint)index);
			string tag = null;
			IntPtr cancellable = IntPtr.Zero;
			IntPtr error = IntPtr.Zero;
			bool okay = dconf_client_write_sync(_client, "/org/gnome/desktop/input-sources/current", value,
				ref tag, cancellable, out error);
			// Do not call g_variant_unref(value) here.  The documentation says that g_variant_new_uint32
			// returns "a floating reference to a new uint32 GVariant instance.", i.e.
			// "Don't free data after the code is done."
			if (!okay)
			{
				Console.WriteLine("CombinedKeyboardAdaptor.SelectKeyboard({0}) failed", index);
				Logger.WriteEvent("CombinedKeyboardAdaptor.SelectKeyboard({0}) failed", index);
			}
		}
	}
}
#endif
