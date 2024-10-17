// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.Windows.Forms.Keyboarding.Linux;
using SIL.Windows.Forms.Keyboarding.Tests.TestHelper;

namespace SIL.Windows.Forms.Keyboarding.Tests
{
	[TestFixture]
	[Platform(Include = "Linux", Reason = "Linux specific tests")]
	public class IbusKeyboardDescriptionTests
	{
		[Test]
		public void Constructor_UsesLanguageAndName()
		{
			// Setup
			var ibusDesc = new BusEngineDescDouble {
				LongName = "m17n:da:post",
				Name = "post (m17n)",
				Language = "da",
				Layout = "us",
				LayoutVariant = ""
			};

			// Execute
			var sut = new IbusKeyboardDescription("da_m17n:da:post", ibusDesc, null);

			// Verify
			Assert.That(sut.Name, Is.EqualTo("Danish - post (m17n)"));
		}
	}
}