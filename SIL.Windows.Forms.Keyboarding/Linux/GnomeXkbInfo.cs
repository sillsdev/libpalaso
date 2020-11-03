// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	internal class GnomeXkbInfo
	{
		private readonly IntPtr _gnomeXkbInfo;

		public GnomeXkbInfo()
		{
			_gnomeXkbInfo = Unmanaged.gnome_xkb_info_new();
		}

		public IEnumerable<string> GetAllLayouts()
		{
			var list = Unmanaged.gnome_xkb_info_get_all_layouts(_gnomeXkbInfo);

			var array = GlibHelper.GetStringArrayFromGList(list);
			Unmanaged.g_list_free(list);
			return array;
		}

		public bool GetLayoutInfo(string id, out string displayName, out string shortName,
			out string xkbLayout, out string xkbVariant)
		{
			IntPtr displayNamePtr;
			IntPtr shortNamePtr;
			IntPtr xkbLayoutPtr;
			IntPtr xkbVariantPtr;
			var exists = Unmanaged.gnome_xkb_info_get_layout_info(_gnomeXkbInfo,
				Marshal.StringToHGlobalAuto(id), out displayNamePtr, out shortNamePtr,
				out xkbLayoutPtr, out xkbVariantPtr);

			if (!exists)
			{
				displayName = null;
				shortName = null;
				xkbLayout = null;
				xkbVariant = null;
				return false;
			}

			displayName = Marshal.PtrToStringAuto(displayNamePtr);
			shortName = Marshal.PtrToStringAuto(shortNamePtr);
			xkbLayout = Marshal.PtrToStringAuto(xkbLayoutPtr);
			xkbVariant = Marshal.PtrToStringAuto(xkbVariantPtr);
			return exists;
		}
	}
}