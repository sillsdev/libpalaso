using System;
using System.Collections;
using System.Collections.Generic;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace org.freedesktop.IBus
{
	// element type returned in array by ListEngines/ListActiveEngines
	// dbus type: (sa{sv}ssssssss)
	public struct IBusEngineDesc
	{
		public string a;
		public IDictionary<string, object> b;
		public string longname;
		public string name;
		public string description;
		public string language;
		public string license;
		public string author;
		public string icon;
		public string layout;

		public override string ToString ()
		{
			return string.Format("[IBusEngineDesc] '{0}' '{1}' \nname:'{2}' \nlongname:'{3}' \ndescription:'{4}' \nlanguage:'{5}' \nlicense:'{6}' \nauthor:'{7}' \nicon:'{8}' \nlayout:'{9}' ",
								 a,b, name, longname, description, language, license, author, icon, layout);
		}
	}

	[Interface("org.freedesktop.IBus")]
	public interface IIBus : Introspectable
	{
		[return: Argument ("address")]
		string GetAddress ();

		[return: Argument ("context")]
		object CreateInputContext (string name);

		[return: Argument ("name")]
		string CurrentInputContext ();

		void RegisterComponent (object components);

		[return: Argument ("engines")]
		object[] ListEngines();

		[return: Argument ("engines")]
		object[] ListActiveEngines();

		void Exit (bool restart);

		[return: Argument ("data")]
		object Ping (object data);
	}

	// TODO: add signals?
	[Interface("org.freedesktop.IBus.InputContext")]
	public interface InputContext : Introspectable
	{
		void ProcessKeyEvent(uint keyval, uint keycode, uint state, out bool handled);

		void SetCursorLocation(int x, int y, int w, int h);

		void FocusIn();

		void FocusOut();

		void Reset();

		void Enable();

		void Disable();

		[return: Argument ("isEnabled")]
		bool IsEnabled();

		void SetCapabilities(UInt32 caps);

		void SetEngine(string name);

		[return: Argument ("desc")]
		object GetEngine();

		void Destroy();
	}

	[Interface("org.freedesktop.IBus.Panel")]
	public interface Panel : Introspectable
	{
		void UpdateLookupTable(object lookup_table, bool visible);
		void StartSetup();
		void SetCursorLocation(int x, int y, int w, int h);
		void UpdateAuxiliaryText(object text, bool visible);
		void FocusOut(object ic);
		void HideAuxiliaryText();
		void Destroy();
		void PageDownLookupTable();
		void StateChanged();
		void ShowAuxiliaryText();
		void ShowPreeditText();
		void Reset();
		void UpdateProperty(object prop);
		void HidePreeditText();
		void CursorUpLookupTable();
		void UpdatePreeditText(object text, uint cursor_pos, bool visible);
		void RegisterProperties(object props);
		void ShowLookupTable();
		void CursorDownLookupTable();
		void HideLookupTable();
		void HideLanguageBar();
		void FocusIn(object ic);
		void PageUpLookupTable();
		void ShowLanguageBar();
	}
}
