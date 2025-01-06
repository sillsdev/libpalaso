// Copyright (c) 2011-2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Runtime.InteropServices;
using Mono.Unix;

namespace X11.XKlavier
{
	/// <summary>
	/// Provides access to the xklavier XKB keyboarding engine methods.
	/// </summary>
	/// <seealso href="https://developer.gnome.org/libxklavier/stable/libxklavier-xkl-engine.html"/>
	internal class XklEngine : IXklEngine
	{
		private struct XklState
		{
#pragma warning disable 649 // the struct is initialized by marshaling pointers
			public int Group;
			public int Indicators;
#pragma warning restore 649
		}

		private string[] m_GroupNames;
		private string[] m_LocalizedGroupNames;

		public XklEngine() : this(X11Helper.GetDisplayConnection())
		{
		}

		public XklEngine(IntPtr display)
		{
			Engine = xkl_engine_get_instance(display);
			Catalog.Init("xkeyboard-config", string.Empty);
		}

		public void Close()
		{
		}

		public IntPtr Engine { get; private set; }

		public string Name
		{
			get
			{
				var name = xkl_engine_get_backend_name(Engine);
				return Marshal.PtrToStringAuto(name);
			}
		}

		public int NumGroups => xkl_engine_get_num_groups(Engine);

		/// <summary>
		/// Gets the non-localized, English names of the installed XKB keyboards
		/// </summary>
		public virtual string[] GroupNames
		{
			get
			{
				if (m_GroupNames == null)
				{
					var count = NumGroups;
					var names = xkl_engine_get_groups_names(Engine);
					var namePtrs = new IntPtr[count];
					Marshal.Copy(names, namePtrs, 0, count);
					m_GroupNames = new string[count];
					for (var i = 0; i < count; i++)
					{
						m_GroupNames[i] = Marshal.PtrToStringAuto(namePtrs[i]);
					}
				}
				return m_GroupNames;
			}
		}

		/// <summary>
		/// Gets the localized names of the installed XKB keyboards
		/// </summary>
		public virtual string[] LocalizedGroupNames
		{
			get
			{
				if (m_LocalizedGroupNames == null)
				{
					var count = GroupNames.Length;
					m_LocalizedGroupNames = new string[count];
					for (int i = 0; i < count; i++)
					{
						m_LocalizedGroupNames[i] = Catalog.GetString(GroupNames[i]);
					}
				}
				return m_LocalizedGroupNames;
			}
		}

		public int NextGroup => xkl_engine_get_next_group(Engine);

		public int PrevGroup => xkl_engine_get_prev_group(Engine);

		public int CurrentWindowGroup => xkl_engine_get_current_window_group(Engine);

		public int DefaultGroup
		{
			get => xkl_engine_get_default_group(Engine);
			set => xkl_engine_set_default_group(Engine, value);
		}

		public void SetGroup(int grp)
		{
			xkl_engine_lock_group(Engine, grp);
		}

		public void SetToplevelWindowGroup(bool fGlobal)
		{
			xkl_engine_set_group_per_toplevel_window(Engine, fGlobal);
		}

		public bool IsToplevelWindowGroup => xkl_engine_is_group_per_toplevel_window(Engine);

		public int CurrentState
		{
			get
			{
				var statePtr = xkl_engine_get_current_state(Engine);
				var state = (XklState)Marshal.PtrToStructure(statePtr, typeof(XklState));
				return state.Group;
			}
		}

		public int CurrentWindowState
		{
			get
			{
				var window = xkl_engine_get_current_window(Engine);
				if (xkl_engine_get_state(Engine, window, out var statePtr))
				{
					var state = (XklState)Marshal.PtrToStructure(statePtr, typeof(XklState));
					return state.Group;
				}
				return -1;
			}
		}

		public string LastError
		{
			get
			{
				var error = xkl_get_last_error();
				return Marshal.PtrToStringAuto(error);
			}
		}

		// from libXKlavier
		[DllImport("libxklavier")]
		private static extern IntPtr xkl_engine_get_instance(IntPtr display);

		[DllImport("libxklavier")]
		private static extern IntPtr xkl_engine_get_backend_name(IntPtr engine);

		[DllImport("libxklavier")]
		private static extern int xkl_engine_get_num_groups(IntPtr engine);

		[DllImport("libxklavier")]
		private static extern IntPtr xkl_engine_get_groups_names(IntPtr engine);

		[DllImport("libxklavier")]
		private static extern int xkl_engine_get_next_group(IntPtr engine);
		[DllImport("libxklavier")]
		private static extern int xkl_engine_get_prev_group(IntPtr engine);
		[DllImport("libxklavier")]
		private static extern int xkl_engine_get_current_window_group(IntPtr engine);
		[DllImport("libxklavier")]
		private static extern void xkl_engine_lock_group(IntPtr engine, int grp);
		[DllImport("libxklavier")]
		private static extern int xkl_engine_get_default_group(IntPtr engine);
		[DllImport("libxklavier")]
		private static extern void xkl_engine_set_default_group(IntPtr engine, int grp);
		[DllImport("libxklavier")]
		private static extern void xkl_engine_set_group_per_toplevel_window(IntPtr engine, bool isGlobal);
		[DllImport("libxklavier")]
		private static extern bool xkl_engine_is_group_per_toplevel_window(IntPtr engine);
		[DllImport("libxklavier")]
		private static extern IntPtr xkl_engine_get_current_state(IntPtr engine);
		[DllImport("libxklavier")]
		private static extern IntPtr xkl_engine_get_current_window(IntPtr engine);
		[DllImport("libxklavier")]
		private static extern bool xkl_engine_get_state(IntPtr engine, IntPtr win, out IntPtr stateOut);
		[DllImport("libxklavier")]
		private static extern IntPtr xkl_get_last_error();
	}
}
