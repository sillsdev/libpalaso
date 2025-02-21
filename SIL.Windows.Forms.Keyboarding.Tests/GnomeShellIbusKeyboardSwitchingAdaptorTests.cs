// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;
using SIL.Windows.Forms.Keyboarding.Tests.TestHelper;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include="Linux", Reason="Linux specific tests")]
	public class GnomeShellIbusKeyboardSwitchingAdaptorTests
	{
		[Test]
		public void DefaultKeyboard_TakesFirstKeyboard()
		{
			var adaptor =
				new GnomeShellIbusKeyboardRetrievingAdaptorDouble(new[] {
					"ibus;;m17n:da:post",
					"ibus;;/usr/share/kmfl/IPA14.kmn",
					"xkb;;de+neo",
					"ibus;;sunpinyin",
					"ibus;;km:/home/foo/.local/share/keyman/khmer_angkor/khmer_angkor.kmx",
					"xkb;;us",
				}, new[] {
					new BusEngineDescDouble { LongName = "m17n:da:post", Name = "post (m17n)", Language = "da", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "xkb:de:neo:ger", Name = "German (Neo 2)", Language = "de", Layout = "de", LayoutVariant = "neo" },
					new BusEngineDescDouble { LongName = "/usr/share/kmfl/IPA14.kmn", Name = "IPA Unicode 6.2 (ver 1.4) KMN", Language = "", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "sunpinyin", Name = "SunPinyin", Language = "zh_CN", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "km:/home/foo/.local/share/keyman/khmer_angkor/khmer_angkor.kmx", Name = "Khmer Angkor", Language = "km", Layout = "us", LayoutVariant = "" },
					new BusEngineDescDouble { LongName = "xkb:us::eng", Name = "English (US)", Language = "en", Layout = "us", LayoutVariant = "" },
				});
			KeyboardController.Initialize(adaptor);

			Assert.That((adaptor.SwitchingAdaptor as GnomeShellIbusKeyboardSwitchingAdaptor).DefaultKeyboard.Name,
				Is.EqualTo("Danish - post (m17n)"));
		}

	}
}