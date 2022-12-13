// Copyright (c) 2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include = "Linux", Reason = "Linux specific tests")]
	public class GnomeShellIbusKeyboardSwitchingAdaptorTests
	{
		[TestCase(new string[] {
				"xkb;;us+mac",
				"xkb;;ru",
				"ibus;;und-Latn:/home/USER/.local/share/keyman/sil_ipa/sil_ipa.kmx",
				"ibus;;table:thai"
			},
			"[('xkb', 'us+mac'), " +
			"('xkb', 'ru'), " +
			"('ibus', 'und-Latn:/home/USER/.local/share/keyman/sil_ipa/sil_ipa.kmx'), " +
			"('ibus', 'table:thai')]",
			TestName = "Transforms mixed list of xkb, keyman, and ibus keyboards")]
		[TestCase(new string[] {"xkb;;us"},
			"[('xkb', 'us')]",
			TestName = "Transforms list of one item")]
		[TestCase(new string[] {}, "[]")]
		public void ToInputSourcesFormat(string[] input, string expected)
		{
			Assert.That(GnomeShellIbusKeyboardSwitchingAdaptor.ToInputSourcesFormat(input), Is.EqualTo(expected));
		}
	}
}