// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

#if __MonoCS__
using System;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Keyboard description for a XKB keyboard layout.
	/// </summary>
	public class XkbKeyboardDescription: KeyboardDescription
	{
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Linux.XkbKeyboardDescription"/> class.
		/// </summary>
		/// <param name='name'>Display name of the keyboard</param>
		/// <param name='layout'>Name of the keyboard layout</param>
		/// <param name='locale'>The locale of the keyboard</param>
		/// <param name='engine'>The keyboard adaptor that will handle this keyboard</param>
		/// <param name='groupIndex'>The group index of this xkb keyboard</param>
		internal XkbKeyboardDescription(string name, string layout, string locale,
			IInputLanguage language, IKeyboardAdaptor engine, int groupIndex)
			: base(name, layout, locale, language, engine)
		{
			GroupIndex = groupIndex;
		}

		internal XkbKeyboardDescription(XkbKeyboardDescription other): base(other)
		{
			GroupIndex = other.GroupIndex;
		}

		public override IKeyboardDefinition Clone()
		{
			return new XkbKeyboardDescription(this);
		}

		/// <summary>
		/// Gets the group index of this keyboard.
		/// </summary>
		public int GroupIndex { get; private set; }
	}
}
#endif
