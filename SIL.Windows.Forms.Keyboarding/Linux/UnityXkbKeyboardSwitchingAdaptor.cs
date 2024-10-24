// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using X11.XKlavier;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for dealing with xkb keyboards on Unity (as found in Trusty >= 13.10)
	/// </summary>
	public class UnityXkbKeyboardSwitchingAdaptor : XkbKeyboardSwitchingAdaptor
	{
		public UnityXkbKeyboardSwitchingAdaptor(IXklEngine engine) : base(engine)
		{
		}

		protected override void SelectKeyboard(KeyboardDescription keyboard)
		{
			var xkbKeyboard = keyboard as XkbKeyboardDescription;
			if (xkbKeyboard == null || xkbKeyboard.GroupIndex < 0)
				return;

			var switchingAdaptor = KeyboardController.Instance
				.Adaptors[KeyboardAdaptorType.OtherIm]
				.SwitchingAdaptor as IUnityKeyboardSwitchingAdaptor;
			switchingAdaptor.SelectKeyboard((uint) xkbKeyboard.GroupIndex);
		}
	}
}
