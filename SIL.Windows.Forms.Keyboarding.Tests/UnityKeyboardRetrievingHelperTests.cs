// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include = "Linux", Reason = "Linux specific tests")]
	public class UnityKeyboardRetrievingHelperTests
	{
		[Test]
		public void InitKeyboards_XkbAndIbus()
		{
			IDictionary<string, uint> registeredKeyboards = null;
			var installedKeyboards = new[] {
				"xkb;;us",
				"ibus;;km:/home/user/.local/share/keyman/khmer_angkor/khmer_angkor.kmx"
			};
			var sut = new GnomeKeyboardRetrievingHelper(() => installedKeyboards);
			Assert.That(() => sut.InitKeyboards(s => true,
				(keyboards, _) => registeredKeyboards = keyboards),
				Throws.Nothing);
			Assert.That(registeredKeyboards, Is.EquivalentTo(new Dictionary<string, int> {
				{ "us", 0 },
				{ "km:/home/user/.local/share/keyman/khmer_angkor/khmer_angkor.kmx", 1 }
			}));
		}

		[Test]
		public void InitKeyboards_DuplicateKeyboard()
		{
			IDictionary<string, uint> registeredKeyboards = null;
			var installedKeyboards = new[] {
				"ibus;;km:/home/user/.local/share/keyman/khmer_angkor/khmer_angkor.kmx",
				"ibus;;km:/home/user/.local/share/keyman/khmer_angkor/khmer_angkor.kmx",
				"xkb;;us"
			};
			var sut = new GnomeKeyboardRetrievingHelper(() => installedKeyboards);
			Assert.That(() => sut.InitKeyboards(s => true,
					(keyboards, _) => registeredKeyboards = keyboards),
				Throws.Nothing); // LT-20410
			Assert.That(registeredKeyboards, Is.EquivalentTo(new Dictionary<string, int> {
				{ "km:/home/user/.local/share/keyman/khmer_angkor/khmer_angkor.kmx", 0 },
				{ "us", 2 } // 2 is the 0-based index of the 'us' layout in `installedKeyboards`
			}));
		}

	}
}