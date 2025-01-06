// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Runtime.InteropServices;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Doubly-linked list of strings.
	/// </summary>
	/// <remarks>This wraps the glist struct defined in glibc and helps to interact with native
	/// code.</remarks>
	internal struct GList
	{
#pragma warning disable 649 // Field is never assigned to (it is however through FromPtr)
		private IntPtr data;
		private IntPtr next;
		private IntPtr prev;
#pragma warning restore 649

		public string Data => Marshal.PtrToStringAuto(data);

		public GList? Next => FromPtr(next);

		public GList? Prev => FromPtr(prev);

		public bool HasNext => next != IntPtr.Zero;

		public bool HasPrev => prev != IntPtr.Zero;

		public static GList? FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;
			return Marshal.PtrToStructure<GList>(ptr);
		}

	}
}