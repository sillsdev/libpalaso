// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	public class GnomeKeyboardRetrievingHelperTests
	{
		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Unmanaged.LibGnomeDesktopCleanup();
		}

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
					(keyboards, firstKeyboard) => registeredKeyboards = keyboards),
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
					(keyboards, firstKeyboard) => registeredKeyboards = keyboards),
				Throws.Nothing); // LT-20410
			Assert.That(registeredKeyboards, Is.EquivalentTo(new Dictionary<string, int> {
				{ "km:/home/user/.local/share/keyman/khmer_angkor/khmer_angkor.kmx", 0 },
				{ "us", 2 }
			}));
		}

		[TestCase("(<<[('xkb', 'us'), " +
			"('xkb', 'ru'), " +
			"('ibus', 'und-Latn:/home/USER/.local/share/keyman/sil_ipa/sil_ipa.kmx'), " +
			"('ibus', 'table:thai')]>>,)\n",
			new string[] {
				"xkb;;us",
				"xkb;;ru",
				"ibus;;und-Latn:/home/USER/.local/share/keyman/sil_ipa/sil_ipa.kmx",
				"ibus;;table:thai"
			},
			TestName = "Parses mixed list of xkb, keyman, and ibus keyboards")]
		[TestCase("(<<[('xkb', 'us')]>>,)\n",
			new string[] {"xkb;;us"}, TestName = "Parses list of one item")]
		// Gnome doesn't let you delete the last remaining keyboard in the list. But gracefully fail in case we
		// get an unexpected list with nothing.
		[TestCase("(<<[]>>,)\n", new string[] {})]
		[TestCase("(<<>>,)\n", new string[] {})]
		[TestCase("(,)\n", new string[] {})]
		[TestCase("()\n", new string[] {})]
		[TestCase("\n", new string[] {})]
		[TestCase("", new string[] {})]
		public void ParseGDBusKeyboardList(string keyboardList, string[] expected)
		{
			Assert.That(GnomeKeyboardRetrievingHelper.ParseGDBusKeyboardList(keyboardList), Is.EqualTo(new List<string>(expected)));
		}
	}
}
