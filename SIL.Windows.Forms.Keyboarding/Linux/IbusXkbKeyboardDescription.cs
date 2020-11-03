// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using IBusDotNet;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Keyboard description for XKB keyboards handled by IBus
	/// </summary>
	internal class IbusXkbKeyboardDescription: IbusKeyboardDescription
	{
		public IbusXkbKeyboardDescription(string id, IBusEngineDesc ibusKeyboard, IKeyboardSwitchingAdaptor engine)
			: base(id, ibusKeyboard, engine)
		{
		}

		protected override string KeyboardIdentifier => _ibusKeyboard.Name;
	}
}