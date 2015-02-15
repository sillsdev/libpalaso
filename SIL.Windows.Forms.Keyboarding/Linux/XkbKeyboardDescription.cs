// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
// --------------------------------------------------------------------------------------------

#if __MonoCS__
using SIL.Windows.Forms.Keyboarding;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Keyboard description for a XKB keyboard layout.
	/// </summary>
	internal class XkbKeyboardDescription: KeyboardDescription
	{
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="SIL.WritingSystems.WindowsForms.Keyboarding.Linux.XkbKeyboardDescription"/> class.
		/// </summary>
		public XkbKeyboardDescription(string id, string name, string layout, string locale, bool isAvailable,
			IInputLanguage language, IKeyboardAdaptor engine, int groupIndex)
			: base(id, name, layout, locale, isAvailable, engine)
		{
			InputLanguage = language;
			GroupIndex = groupIndex;
		}

		/// <summary>
		/// Gets the group index of this keyboard.
		/// </summary>
		public int GroupIndex { get; set; }

		public void SetIsAvailable(bool isAvailable)
		{
			IsAvailable = isAvailable;
		}

		public void SetName(string name)
		{
			Name = name;
		}

		public void SetInputLanguage(IInputLanguage language)
		{
			InputLanguage = language;
		}
	}
}
#endif
