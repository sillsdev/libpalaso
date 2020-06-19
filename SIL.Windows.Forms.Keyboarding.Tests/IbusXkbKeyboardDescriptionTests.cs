// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using IBusDotNet;
using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;
using SIL.Windows.Forms.Keyboarding.Tests.TestHelper;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	public class IbusXkbKeyboardDescriptionTests
	{
		[Test]
		public void Constructor_UsesName()
		{
			// Setup
			var ibusDesc = new BusEngineDescDouble {
				LongName = "xkb:us::eng",
				Name = "English (US)",
				Language = "en",
				Layout = "us",
				LayoutVariant = ""
			};

			// Execute
			var sut = new IbusXkbKeyboardDescription("en_xkb:us::eng", ibusDesc, null);

			// Verify
			Assert.That(sut.Name, Is.EqualTo("English (US)"));
		}
	}
}