// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests.TestHelper
{
	internal class GnomeKeyboardRetrievingHelperDouble : GnomeKeyboardRetrievingHelper
	{
		private readonly string[] _keyboards;

		public GnomeKeyboardRetrievingHelperDouble(string[] keyboards)
		{
			_keyboards = keyboards;
		}

		protected override string[] GetMyKeyboards()
		{
			return _keyboards;
		}
	}
}