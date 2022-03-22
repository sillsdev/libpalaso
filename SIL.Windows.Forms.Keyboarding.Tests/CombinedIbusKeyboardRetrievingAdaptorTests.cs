// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
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

		[TestCase("['xkb:us::eng', '/usr/share/kmfl/IPA14.kmn', 'table:thai']\n",
			new string[] {
				"xkb:us::eng", "/usr/share/kmfl/IPA14.kmn", "table:thai"
			}
		)]
		[TestCase("['xkb:us::eng']\n", new string[] {"xkb:us::eng"})]
		// Gracefully handle an unexpected list of nothing.
		[TestCase("['']\n", new string[] {})]
		[TestCase("[]\n", new string[] {})]
		[TestCase("\n", new string[] {})]
		[TestCase("", new string[] {})]
		public void ParseIbusEnginesList(string keyboardList, string[] expected)
		{
			Assert.That(CombinedIbusKeyboardRetrievingAdaptor.ParseIbusEnginesList(keyboardList), Is.EqualTo(new List<string>(expected)));
		}
	}
}