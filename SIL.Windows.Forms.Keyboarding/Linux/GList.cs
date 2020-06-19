// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Runtime.InteropServices;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	internal struct GList
	{
#pragma warning disable 649 // Field is never assigned to (it is however through FromPtr)
		private IntPtr data;
		private IntPtr next;
		private IntPtr prev;
#pragma warning restore 649

		public string Data => Marshal.PtrToStringAuto(data);

		public GList Next => Marshal.PtrToStructure<GList>(next);

		public GList Prev => Marshal.PtrToStructure<GList>(prev);

		public bool HasNext => next != IntPtr.Zero;

		public bool HasPrev => prev != IntPtr.Zero;

		public static GList FromPtr(IntPtr ptr)
		{
			return Marshal.PtrToStructure<GList>(ptr);
		}

	}
}