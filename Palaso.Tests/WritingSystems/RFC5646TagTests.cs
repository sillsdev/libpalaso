using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class RFC5646TagTests
	{
		[Test]
		public void IsValid_RFCTagIsValid_ReturnsTrue()
		{
			RFC5646Tag tag = new RFC5646Tag("az", "Ltn", "RS", String.Empty);
			Assert.IsTrue(RFC5646Tag.IsValid(tag));
		}

		[Test]
		public void GetValidTag_RFCTagIsValid_ReturnsSameTag()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("az", "Ltn", "RS", String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual(invalidTag, RFC5646Tag.GetValidTag(validTag));
		}

		[Test]
		public void IsValid_IsoContainsxDashaudio_ReturnsFalse()
		{
			RFC5646Tag tag = new RFC5646Tag("tpi-Zxxx-x-audio", String.Empty, String.Empty, String.Empty);
			Assert.IsFalse(RFC5646Tag.IsValid(tag));
		}

		[Test]
		public void GetValidTag_IsoContainsxDashaudio_IsoIsShortenedToEveryThingBeforeFirstDash()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("tpi-Zxxx-x-audio", String.Empty, String.Empty, String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("tpi", validTag.Language);
		}

		[Test]
		public void GetValidTag_IsoContainsOnlyxDashaudio_IsoEmpty()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("x-audio", String.Empty, String.Empty, String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("x", validTag.Language);
			Assert.AreEqual("x-audio", validTag.Variant);
		}

		[Test]
		public void GetValidTag_IsoContainsxDashaudio_ScriptRegionVariantFieldsAreSetCorrectly()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("tpi-Zxxx-x-audio", String.Empty, String.Empty, String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("Zxxx", validTag.Script);
			Assert.AreEqual(String.Empty, validTag.Region);
			Assert.AreEqual("x-audio", validTag.Variant);
		}
	}
}
