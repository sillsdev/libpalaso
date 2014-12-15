// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------
#if __MonoCS__
using System;
using SIL.WritingSystems.WindowsForms.Keyboarding;
using SIL.WritingSystems.WindowsForms.Keyboarding.InternalInterfaces;
using SIL.WritingSystems;

namespace SIL.WritingSystems.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Keyboard description for a XKB keyboard layout.
	/// </summary>
	public class XkbKeyboardDescription: KeyboardDescription
	{
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:SIL.WindowsForms.Keyboard.Linux.XkbKeyboardDescription"/> class.
		/// </summary>
		/// <param name='name'>Display name of the keyboard</param>
		/// <param name='layout'>Name of the keyboard layout</param>
		/// <param name='locale'>The locale of the keyboard</param>
		/// <param name='language'>The language of the keyboard</param>
		/// <param name='engine'>The keyboard adaptor that will handle this keyboard</param>
		/// <param name='groupIndex'>The group index of this xkb keyboard</param>
		/// <param name='isAvailable'>The keyboard is available</param>
		internal XkbKeyboardDescription(string name, string layout, string locale,
			IInputLanguage language, IKeyboardAdaptor engine, int groupIndex, bool isAvailable)
			: base(name, layout, locale, language, engine, KeyboardType.System, isAvailable)
		{
			GroupIndex = groupIndex;

		}

		/// <summary>
		/// Gets the group index of this keyboard.
		/// </summary>
		public int GroupIndex { get; private set; }
	}
}
#endif
