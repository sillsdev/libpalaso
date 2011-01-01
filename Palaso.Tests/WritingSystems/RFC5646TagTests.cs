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
			Assert.IsTrue(tag.IsValid());
		}

		[Test]
		public void GetValidTag_RFCTagIsValid_ReturnsSameTag()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("az", "Ltn", "RS", String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual(invalidTag, RFC5646Tag.GetValidTag(validTag));
		}


		[Test]
		public void IsValid_LanguageCodeContainsxDashaudio_ReturnsFalse()
		{
			//NB: what's wrong here is not the format, but the way we pushed the whole tag into the language parameter.
			RFC5646Tag tag = new RFC5646Tag("tpi-Zxxx-x-audio", String.Empty, String.Empty, String.Empty);
			Assert.IsFalse(tag.IsValid());
		}

		[Test]
		public void IsValid_AudioInCannoncialFormat_ReturnsTrue()
		{
			RFC5646Tag tag = RFC5646Tag.RFC5646TagForVoiceWritingSystem("tpi", string.Empty);
			Assert.IsTrue(tag.IsValid());
		}

		[Test]
		public void GetValidTag_AudioIsValid_ReturnsSameTag()
		{
			RFC5646Tag validTag = RFC5646Tag.RFC5646TagForVoiceWritingSystem("tpi", string.Empty);
			Assert.AreEqual(validTag, RFC5646Tag.GetValidTag(validTag));
		}

		[Test]
		public void GetValidTag_LanguageCodeContainsxDashaudio_LanguageCodeIsShortenedToEveryThingBeforeFirstDash()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("tpi-Zxxx-x-audio", String.Empty, String.Empty, String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("tpi", validTag.Language);
		}

		[Test]
		public void GetValidTag_LanguageCodeContainsOnlyxDashaudio_LanguageCodeEmpty()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("x-audio", String.Empty, String.Empty, String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("x", validTag.Language);
			Assert.AreEqual("x-audio", validTag.Variant);
		}

		[Test]
		public void GetValidTag_LanguageCodeContainsxDashaudio_ScriptRegionVariantFieldsAreSetCorrectly()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("tpi-Zxxx-x-audio", String.Empty, String.Empty, String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("Zxxx", validTag.Script);
			Assert.AreEqual(String.Empty, validTag.Region);
			Assert.AreEqual("x-audio", validTag.Variant);
		}

		[Test]
		public void IsValid_LanguageCodeContainsDashesAndVariantIsxDashaudio_ReturnsFalse()
		{
			RFC5646Tag tag = new RFC5646Tag("de-Ltn-ch-1901", String.Empty, String.Empty, "x-audio");
			Assert.IsFalse(tag.IsValid());
		}

		[Test]
		public void GetValidTag_LanguageCodeContainsDashesAndVariantIsxDashaudio_LanguageCodeIsShortened()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("de-Ltn-ch-1901", String.Empty, String.Empty, "x-audio");
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("de", validTag.Language);
		}
	}
}
