// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Linq;
using IBusDotNet;
using NUnit.Framework;
using SIL.Keyboarding;
using SIL.Windows.Forms.Keyboarding.Linux;
using SIL.Windows.Forms.Keyboarding.Tests.TestHelper;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	public class GnomeShellIbusKeyboardRetrievingAdaptorTests
	{
		[Test]
		public void Initialize_ReturnsAllKeyboards()
		{
			// Setup
			var adaptor =
				new GnomeShellIbusKeyboardRetrievingAdaptorDouble(new[] {
					"xkb;;us",
					"ibus;;m17n:da:post",
					"ibus;;/usr/share/kmfl/IPA14.kmn",
					"xkb;;de+neo",
					"ibus;;sunpinyin",
					"ibus;;km:/home/foo/.local/share/keyman/khmer_angkor/khmer_angkor.kmx"
				}, new[] {
					new BusEngineDescDouble { LongName = "xkb:us::eng", Name = "English (US)", Language = "en", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "xkb:de:neo:ger", Name = "German (Neo 2)", Language = "de", Layout = "de", LayoutVariant = "neo" },
					new BusEngineDescDouble { LongName = "m17n:da:post", Name = "post (m17n)", Language = "da", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "/usr/share/kmfl/IPA14.kmn", Name = "IPA Unicode 6.2 (ver 1.4) KMN", Language = "", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "sunpinyin", Name = "SunPinyin", Language = "zh_CN", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "km:/home/foo/.local/share/keyman/khmer_angkor/khmer_angkor.kmx", Name = "Khmer Angkor", Language = "km", Layout = "us", LayoutVariant = "" },
				});
			KeyboardController.Initialize(adaptor);

			// Execute
			var controllerAvailableKeyboards = Keyboard.Controller.AvailableKeyboards.ToList();

			// Verify
			Assert.That(controllerAvailableKeyboards, Is.EquivalentTo(new[] {
				new KeyboardDescription("en_xkb:us::eng", "English (US)", "xkb:us::eng", "en", true, adaptor.SwitchingAdaptor),
				new KeyboardDescription("de_xkb:de:neo:ger", "German (Neo 2)", "xkb:de:neo:ger", "de", true, adaptor.SwitchingAdaptor),
				new KeyboardDescription("da_m17n:da:post", "Danish - post (m17n)", "m17n:da:post", "da", true, adaptor.SwitchingAdaptor),
				new KeyboardDescription("_/usr/share/kmfl/IPA14.kmn", "Other Language - IPA Unicode 6.2 (ver 1.4) KMN", "/usr/share/kmfl/IPA14.kmn", "", true, adaptor.SwitchingAdaptor),
				new KeyboardDescription("zh_CN_sunpinyin", "Chinese (China) - SunPinyin", "sunpinyin", "zh_CN", true, adaptor.SwitchingAdaptor),
				new KeyboardDescription("km_km:/home/foo/.local/share/keyman/khmer_angkor/khmer_angkor.kmx", "Khmer - Khmer Angkor", "km:/home/foo/.local/share/keyman/khmer_angkor/khmer_angkor.kmx", "km", true, adaptor.SwitchingAdaptor),
			}));
		}

		[Test]
		public void Initialize_DoesNotReturnDuplicates()
		{
			// Setup
			var adaptor =
				new GnomeShellIbusKeyboardRetrievingAdaptorDouble(new[] {
					"xkb;;us",
					"xkb;;de+neo",
				}, new IBusEngineDesc[] {
					new BusEngineDescDouble { LongName = "xkb:us::eng", Name = "English (US)", Language = "en", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "xkb:de:neo:ger", Name = "German (Neo 2)", Language = "de", Layout = "de", LayoutVariant = "neo" },
					new XkbIbusEngineDesc { LongName = "xkb:de+neo", Name = "German (Neo 2)", Language = "de", Layout = "de", LayoutVariant = "neo"},
				});
			KeyboardController.Initialize(adaptor);

			// Execute
			var controllerAvailableKeyboards = Keyboard.Controller.AvailableKeyboards.ToList();

			// Verify
			Assert.That(controllerAvailableKeyboards, Is.EquivalentTo(new[] {
				new KeyboardDescription("en_xkb:us::eng", "English (US)", "xkb:us::eng", "en", true, adaptor.SwitchingAdaptor),
				new KeyboardDescription("de_xkb:de:neo:ger", "German (Neo 2)", "xkb:de:neo:ger", "de", true, adaptor.SwitchingAdaptor),
			}));
		}
	}
}