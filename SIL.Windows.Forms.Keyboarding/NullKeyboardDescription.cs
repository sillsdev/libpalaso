// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace SIL.Windows.Forms.Keyboarding
{
	/// <summary>
	/// This implements a no-op keyboard that can be used where we don't know what keyboard to use
	/// </summary>
	internal class NullKeyboardDescription : KeyboardDescription
	{
		private const string DefaultKeyboardName = "(default)";

		public NullKeyboardDescription()
			: base(string.Empty, DefaultKeyboardName, DefaultKeyboardName, string.Empty, false, null)
		{
			
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.KeyboardDescriptionNull"/>.
		/// </summary>
		public override string ToString()
		{
			return "<no keyboard>";
		}
	}
}
