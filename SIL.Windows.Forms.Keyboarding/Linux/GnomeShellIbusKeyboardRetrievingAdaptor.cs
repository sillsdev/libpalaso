// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles initializing the list of keyboards on Ubuntu versions >= 18.04.
	/// The keyboard retrieving part is identical to previous versions but switching keyboards
	/// changed with 18.04.
	/// </summary>
	public class GnomeShellIbusKeyboardRetrievingAdaptor: UnityIbusKeyboardRetrievingAdaptor
	{
		protected override IKeyboardSwitchingAdaptor CreateSwitchingAdaptor()
		{
			return new GnomeShellIbusKeyboardSwitchingAdaptor(IbusCommunicator);
		}

		public override bool IsApplicable => _helper.IsApplicable && Platform.IsGnomeShell;
	}
}