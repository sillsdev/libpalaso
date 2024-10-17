// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	public class CombinedIbusKeyboardRetrievingAdaptorTests
	{
		private class CombinedIbusKeyboardRetrievingAdaptorDouble : CombinedIbusKeyboardRetrievingAdaptor
		{
			private string[] _keyboards;

			public CombinedIbusKeyboardRetrievingAdaptorDouble(string[] keyboards)
			{
				_keyboards = keyboards;
			}

			protected override string[] GetMyKeyboards(IntPtr settingsGeneral)
			{
				return _keyboards;
			}

			public bool CallHasKeyboards()
			{
				return HasKeyboards;
			}
		}

		[TestCase("en,fr", ExpectedResult = true)]
		[TestCase("en", ExpectedResult = true)]
		[TestCase("", ExpectedResult = false)]
		[TestCase(null, ExpectedResult = false)]
		public bool HasKeyboards(string installedKeyboards)
		{
			var keyboards = installedKeyboards?.Split(new[] { ',' },
				StringSplitOptions.RemoveEmptyEntries);
			var sut = new CombinedIbusKeyboardRetrievingAdaptorDouble(keyboards);

			return sut.CallHasKeyboards();
		}
	}
}