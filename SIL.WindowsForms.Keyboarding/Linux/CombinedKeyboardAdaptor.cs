// Copyright (c) 2013 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using X11.XKlavier;
using SIL.Keyboarding;
using SIL.Reporting;
using IBusDotNet;

namespace SIL.WindowsForms.Keyboarding.Linux
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
	internal class CombinedKeyboardAdaptor : CommonBaseAdaptor
	{
		private static bool DetermineIsRequired()
		{
			IntPtr client = IntPtr.Zero;
			try
			{
				try
				{
					client = dconf_client_new();
				}
				catch (DllNotFoundException)
				{
					// Older versions of Linux have a version of the dconf library with a
					// different version number (different from what libdconf.dll gets
					// mapped to in app.config). However, since those Linux versions
					// don't have combined keyboards under IBus this really doesn't
					// matter.
					return false;
				}

				// This is the proper path for the combined keyboard handling, not the path
				// given in the IBus reference documentation.
				IntPtr sources = dconf_client_read(client, "/org/gnome/desktop/input-sources/sources");
				if (sources == IntPtr.Zero)
					return false;
				g_variant_unref(sources);
			}
			finally
			{
				if (client != IntPtr.Zero)
					g_object_unref(client);
			}

			return true;
		}

		private static bool? _isRequired;

		public static bool IsRequired
		{
			get
			{
				// only compute this once
				if (_isRequired == null)
					_isRequired = DetermineIsRequired();
				return (bool) _isRequired;
			}
		}
			
		private int _kbdIndex;
		private IntPtr _client = IntPtr.Zero;
		private readonly XkbKeyboardAdaptor _xkbAdaptor;
		private readonly IbusKeyboardAdaptor _ibusAdaptor;
		private readonly Dictionary<string,int> _ibusKeyboards;
		private readonly Dictionary<string,int> _xkbKeyboards;

		public CombinedKeyboardAdaptor()
		{
			_xkbAdaptor = new XkbKeyboardAdaptor();
			_ibusAdaptor = new IbusKeyboardAdaptor();
			_xkbKeyboards = new Dictionary<string, int>();
			_ibusKeyboards = new Dictionary<string, int>();
		}

		private void RegisterXkbKeyboards()
		{
			if (_xkbKeyboards.Count == 0)
				return;
				
			var configRegistry = XklConfigRegistry.Create(_xkbAdaptor.XklEngine);
			Dictionary<string, List<XklConfigRegistry.LayoutDescription>> layouts = configRegistry.Layouts;
			Dictionary<string, XkbKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<XkbKeyboardDescription>().ToDictionary(kd => kd.Id);
			foreach (KeyValuePair<string, List<XklConfigRegistry.LayoutDescription>> kvp in layouts)
			{
				foreach (XklConfigRegistry.LayoutDescription layout in kvp.Value)
				{
					// Custom keyboards may omit defining a country code.  Try to survive such cases.
					string codeToMatch = layout.CountryCode == null ? layout.LanguageCode.ToLowerInvariant() : layout.CountryCode.ToLowerInvariant();
					int index;
					if ((_xkbKeyboards.TryGetValue(layout.LayoutId, out index) && (layout.LayoutId == codeToMatch))
						|| _xkbKeyboards.TryGetValue(string.Format("{0}+{1}", codeToMatch, layout.LayoutId), out index))
					{
						XkbKeyboardAdaptor.AddKeyboardForLayout(curKeyboards, layout, index, this);
					}
				}
			}

			foreach (XkbKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);
		}

		private void RegisterIbusKeyboards()
		{
			if (_ibusKeyboards.Count == 0)
				return;

			Dictionary<string, IbusKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			foreach (IBusEngineDesc ibusKeyboard in _ibusAdaptor.GetAllIBusKeyboards())
			{
				if (_ibusKeyboards.ContainsKey(ibusKeyboard.LongName))
				{
					string id = string.Format("{0}_{1}", ibusKeyboard.Language, ibusKeyboard.LongName);
					IbusKeyboardDescription keyboard;
					if (curKeyboards.TryGetValue(id, out keyboard))
					{
						if (!keyboard.IsAvailable)
						{
							keyboard.SetIsAvailable(true);
							keyboard.IBusKeyboardEngine = ibusKeyboard;
						}
						curKeyboards.Remove(id);
					}
					else
					{
						keyboard = new IbusKeyboardDescription(id, ibusKeyboard, this);
						KeyboardController.Instance.Keyboards.Add(keyboard);
					}
					keyboard.SystemIndex = _ibusKeyboards[ibusKeyboard.LongName];
				}
			}

			foreach (IbusKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);
		}

		private void AddKeyboard(string source)
		{
			var parts = source.Split(new []{";;"}, StringSplitOptions.None);
			Debug.Assert(parts.Length == 2);
			if (parts.Length != 2)
				return;
			var type = parts[0];
			var layout = parts[1];
			if (type == "xkb")
				_xkbKeyboards.Add(layout, _kbdIndex);
			else
				_ibusKeyboards.Add(layout, _kbdIndex);
			++_kbdIndex;
		}

		protected override void AddAllKeyboards(string[] list)
		{
			_xkbKeyboards.Clear();
			_ibusKeyboards.Clear();
			_kbdIndex = 0;
			for (int i = 0; i < list.Length; ++i)
				AddKeyboard(list[i]);
			RegisterIbusKeyboards();
			RegisterXkbKeyboards();
		}

		protected override string GSettingsSchema { get { return null; } }	// we don't use GSettings

		/// <summary>
		/// Return the list of keyboards in the combined handler, or null if this adaptor should
		/// not be used.
		/// </summary>
		protected override string[] GetMyKeyboards(IntPtr client, IntPtr settings)
		{
			// This is the proper path for the combined keyboard handling, not the path
			// given in the IBus reference documentation.
			IntPtr sources = dconf_client_read(client, "/org/gnome/desktop/input-sources/sources");
			if (sources == IntPtr.Zero)
				return null;
			string[] list = GetStringArrayFromGVariantListArray(sources);
			g_variant_unref(sources);

			// Save the connection to dconf since we use it in keyboard switching.
			_client = client;
			g_object_ref(_client);

			return list;
		}

		#region IKeyboardAdaptor implementation

		public override bool ActivateKeyboard(KeyboardDescription keyboard)
		{
			Debug.Assert(keyboard.Engine == this);
			if (keyboard is XkbKeyboardDescription)
			{
				var xkbKeyboard = (XkbKeyboardDescription) keyboard;
				if (xkbKeyboard.GroupIndex >= 0)
					SelectKeyboard(xkbKeyboard.GroupIndex);
			}
			else if (keyboard is IbusKeyboardDescription)
			{
				var ibusKeyboard = (IbusKeyboardDescription) keyboard;
				try
				{
					if (!_ibusAdaptor.CanSetIbusKeyboard())
						return false;
					if (_ibusAdaptor.IBusKeyboardAlreadySet(ibusKeyboard))
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

		public override void DeactivateKeyboard(KeyboardDescription keyboard)
		{
			if (keyboard is IbusKeyboardDescription)
			{
				if (_ibusAdaptor.CanSetIbusKeyboard())
					GlobalCachedInputContext.InputContext.Reset();
			}
			GlobalCachedInputContext.Keyboard = null;
		}

		public override KeyboardDescription DefaultKeyboard
		{
			get { return _xkbAdaptor.DefaultKeyboard; }
		}

		public override KeyboardDescription CreateKeyboardDefinition(string id)
		{
			return XkbKeyboardAdaptor.CreateKeyboardDefinition(id, this);
		}

		#endregion

		private void SelectKeyboard(int index)
		{
			if (_client == IntPtr.Zero)
				return;
			IntPtr value = g_variant_new_uint32((uint)index);
			string tag = null;
			IntPtr cancellable = IntPtr.Zero;
			IntPtr error;
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

		protected override void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				_xkbAdaptor.Dispose();
				_ibusAdaptor.Dispose();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.
			if (_client != IntPtr.Zero)
			{
				g_object_unref(_client);
				_client = IntPtr.Zero;
			}

			base.Dispose(disposing);
		}
	}
}
#endif
