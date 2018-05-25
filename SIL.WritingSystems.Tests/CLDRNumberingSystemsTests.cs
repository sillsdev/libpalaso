// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	class CLDRNumberingSystemsTests
	{
		[Test]
		public void CLDRNumberingSystems_FindNumberingSystemID_Returns_Null_For_Unknown()
		{
			Assert.IsNull(CLDRNumberingSystems.FindNumberingSystemID("!@#$%^&*()"));
		}

		[Test]
		public void CLDRNumberingSystems_FindNumberingSystemID_Returns_ID_For_MatchingDigits()
		{
			var latinDigits = "0123456789";
			Assert.AreEqual("latn", CLDRNumberingSystems.FindNumberingSystemID(latinDigits));
		}

		[Test]
		public void CLDRNumberingSystems_GetDigitsForID_Returns_Null_For_Unknown()
		{
			Assert.IsNull(CLDRNumberingSystems.GetDigitsForID("bogusID"));
		}

		[Test]
		public void CLDRNumberingSystems_GetDigitsForID_Returns_Digits_From_ID()
		{
			Assert.AreEqual("0123456789", CLDRNumberingSystems.GetDigitsForID("latn"));
		}
	}
}
