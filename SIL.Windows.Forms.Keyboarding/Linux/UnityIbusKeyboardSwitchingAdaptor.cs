﻿// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for dealing with ibus keyboards on Unity (as found in Trusty &gt;= 13.10 &lt; 18.04)
	/// </summary>
	public class UnityIbusKeyboardSwitchingAdaptor : IbusKeyboardSwitchingAdaptor, IUnityKeyboardSwitchingAdaptor
	{
		public UnityIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator) :
			base(ibusCommunicator)
		{
		}

		void IUnityKeyboardSwitchingAdaptor.SelectKeyboard(uint index)
		{
			const string schema = "org.gnome.desktop.input-sources";
			bool okay = true;
			IntPtr settings = IntPtr.Zero;
			try
			{
				okay &= GlibHelper.SchemaIsInstalled(schema);
				if (!okay)
					return;

				settings = Unmanaged.g_settings_new(schema);
				okay &= settings != IntPtr.Zero;
				if (!okay)
					return;

				okay &= Unmanaged.g_settings_set_uint(settings, "current", index);
			}
			finally
			{
				if (settings != IntPtr.Zero)
					Unmanaged.g_object_unref(settings);

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
			var systemIndex = ibusKeyboard.SystemIndex;
			((IUnityKeyboardSwitchingAdaptor)this).SelectKeyboard(systemIndex);
		}

	}
}
