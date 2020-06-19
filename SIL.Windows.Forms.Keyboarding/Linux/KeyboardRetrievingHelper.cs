// Copyright (c) 2015-2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using SIL.Reporting;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	public static class KeyboardRetrievingHelper
	{
		public static void AddIbusVersionAsErrorReportProperty()
		{
			var settingsGeneral = IntPtr.Zero;
			try
			{
				const string ibusSchema = "org.freedesktop.ibus.general";
				if (!GlibHelper.SchemaIsInstalled(ibusSchema))
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
	}
}
