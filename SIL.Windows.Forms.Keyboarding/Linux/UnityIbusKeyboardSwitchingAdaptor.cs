// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for dealing with ibus keyboards on Unity (as found in Trusty >= 13.10)
	/// </summary>
	public class UnityIbusKeyboardSwitchingAdaptor : IbusKeyboardSwitchingAdaptor
	{
		public UnityIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator) :
			base(ibusCommunicator)
		{
		}

		internal static void SelectKeyboard(uint index)
		{
			const string schema = "org.gnome.desktop.input-sources";
			bool okay = true;
			try
			{
				okay &= KeyboardRetrievingHelper.SchemaIsInstalled(schema);
				if (!okay)
					return;

				var settings = Unmanaged.g_settings_new(schema);
				okay &= settings != IntPtr.Zero;
				if (!okay)
					return;

				okay &= Unmanaged.g_settings_set_uint(settings, "current", index);
			}
			finally
			{
				if (!okay)
				{
					Console.WriteLine("UnityIbusKeyboardAdaptor.SelectKeyboard({0}) failed", index);
					Logger.WriteEvent("UnityIbusKeyboardAdaptor.SelectKeyboard({0}) failed", index);
				}
			}
		}

		protected override void SelectKeyboard(KeyboardDescription keyboard)
		{
			var ibusKeyboard = (IbusKeyboardDescription) keyboard;
			uint systemIndex = ibusKeyboard.SystemIndex;
			SelectKeyboard(systemIndex);
		}

	}
}
