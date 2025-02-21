// Copyright (c) 2025 SIL Global
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
using System;

namespace X11.XKlavier
{
	/// <summary>
	/// Interface for the xklavier XKB keyboarding engine methods. Extracted as interface so
	/// that unit tests can easily provide test doubles.
	/// </summary>
	/// <seealso href="https://developer.gnome.org/libxklavier/stable/libxklavier-xkl-engine.html"/>
	public interface IXklEngine
	{
		void Close();
		string Name { get;}
		int NumGroups { get;}
		string[] GroupNames { get;}
		string[] LocalizedGroupNames { get; }
		int NextGroup { get;}
		int PrevGroup { get;}
		int CurrentWindowGroup { get;}
		int DefaultGroup { get; set;}
		void SetGroup(int grp);
		void SetToplevelWindowGroup(bool fGlobal);
		bool IsToplevelWindowGroup { get;}
		int CurrentState { get;}
		int CurrentWindowState { get;}
		string LastError { get;}
		IntPtr Engine { get; }
	}
}
