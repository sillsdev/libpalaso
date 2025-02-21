// Copyright (c) 2025 SIL Global
// Distributable under the terms of the MIT license (http://opensource.org/licenses/MIT).
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
		/// <see cref="SIL.Windows.Forms.Keyboarding.Linux.XkbKeyboardDescription"/> class.
		/// </summary>
		public XkbKeyboardDescription(string id, string name, string layout, string locale, bool isAvailable,
			IInputLanguage language, IKeyboardSwitchingAdaptor engine, int groupIndex)
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
