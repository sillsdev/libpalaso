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
		public void GetValidTag_RFCTagIsValid_ReturnsSameTag()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("az", "Ltn", "RS", String.Empty);
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual(invalidTag, RFC5646Tag.GetValidTag(validTag));
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
			Assert.AreEqual(WellKnownSubTags.Audio.Script, validTag.Script);
			Assert.AreEqual(String.Empty, validTag.Region);
			Assert.AreEqual("x-audio", validTag.Variant);
		}

		[Test]
		public void GetValidTag_IsoContainsDashesAndVariantIsxDashaudio_IsoIsShortened()
		{
			RFC5646Tag invalidTag = new RFC5646Tag("de-Ltn-ch-1901", String.Empty, String.Empty, "x-audio");
			RFC5646Tag validTag = RFC5646Tag.GetValidTag(invalidTag);
			Assert.AreEqual("de", validTag.Language);
		}

		[Test]
		public void AddToSubtag_SubtagIsEmpty_SubtagEqualsStringToAdd()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "x-audio");
		}

		[Test]
		public void AddToSubtag_SubtagContainsExtensionWithUnderScore_Throws()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x_audio"));
		}

		[Test]
		public void AddToSubtag_SubtagIsNotEmpty_StringToAddIsAppendedToSubtagWithDashDelimiter()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "variant-x-audio");
		}

		[Test]
		public void AddToSubtag_StringToAddIsSubStringOfStringAlreadyContainedInSubTag_StringToAddIsAppendedToSubtagWithDashDelimiter()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-x-audios");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "variant-x-audios-x-audio");
		}

		[Test]
		public void AddToSubtag_StringToAddConsistsOfMultipleParts_StringToAddIsAppendedToSubtagWithDashDelimiter()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-x-audios");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio-variant2");
			Assert.AreEqual(rfcTag.Variant, "variant-x-audios-x-audio-variant2");
		}

		[Test]
		public void AddToSubtag_SubtagAlreadyContainsStringToAdd_Throws()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-x-audio");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void AddToSubtag_SubtagAlreadyContainsPartsOfStringToAdd_Throws()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-x-audio");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "variant2-x-audio"));
		}

		[Test]
		public void AddToSubtag_SubtagAlreadyContainsStringToAddInDifferentCase_Throws()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-X-AUDIO");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void AddToSubtag_StringToAddBeginsWithDash_StringIsAdded()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "-x-audio");
			Assert.AreEqual("variant-x-audio", rfcTag.Variant);
		}

		[Test]
		public void AddToSubtag_StringToAddBeginsWithUnderscore_StringIsAdded()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "_x-audio");
			Assert.AreEqual("variant-x-audio", rfcTag.Variant);
		}

		[Test]
		public void AddToSubtag_StringToAddEndsWithDash_StringIsAdded()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio-");
			Assert.AreEqual("variant-x-audio", rfcTag.Variant);
		}

		[Test]
		public void AddToSubtag_StringToAddEndsWithUnderscoreh_StringIsAdded()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio_");
			Assert.AreEqual("variant-x-audio", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagIsEmpty_SubTagRemainsUntouched()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(String.Empty, rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagDoesNotContainStringToRemove_SubTagRemainsUntouched()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual("variant", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagDoesNotContainPartsOfStringToRemove_SubTagRemainsUntouched()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-variant2");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "variant2-x-audio");
			Assert.AreEqual("variant-variant2", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsPartsOfStringToRemoveButNotConsecutively_RemovesPartsCorrectly()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-variant2-x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "variant-x-audio");
			Assert.AreEqual("variant2", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemove_SubtagIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "variant");
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveInDifferentCase_SubtagIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "variant-X-aUdiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "variant");
		}

		[Test]
		public void RemoveFromSubtag_StringToRemoveIsFirstInSubtag_SubtagIsStrippedOfStringToRemoveAndFollowingDash()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-audio-variant");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "variant");
		}

		[Test]
		public void RemoveFromSubtag_StringToRemoveInDifferentCaseIsFirstInSubtag_SubtagIsStrippedOfStringToRemoveAndFollowingDash()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "X-aUdiO-variant");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "variant");
		}

		[Test]
		public void RemoveFromSubtag_SubtagEqualsStringToRemove_SubtagIsEmpty()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagEqualsStringToRemoveInDifferentCase_SubtagIsEmpty()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "X-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveStartsWithDash_StringIsRemoved()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "X-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "-x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveStartsWithUnderscore_StringIsRemoved()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "X-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "_x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveEndsWithDash_StringIsRemoved()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "X-AudiO-");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio-");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveEndsWithUnderscore_StringIsRemoved()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "X-AudiO_");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio_");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void SubtagContainsPart_PartIsNotContainedInSubtag_ReturnsFalse()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "");
			Assert.IsFalse(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void SubtagContainsPart_PartIsContainedInSubtag_ReturnsTrue()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-audio");
			Assert.IsTrue(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void SubtagContainsPart_PartConsistsOfMultiplePartsAndNotAllPartsAreContainedInSubtag_ReturnsFalse()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "fonipa");
			Assert.IsFalse(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "fonipa-x-etic"));
		}

		[Test]
		public void SubtagContainsPart_PartConsistsOfMultiplePartsAndAllPartsAreContainedInSubtag_ReturnsTrue()
		{
			RFC5646Tag rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "fonipa-test-x-etic");
			Assert.IsTrue(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "fonipa-x-etic"));
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Variant_SetPrivateUseTag_VariantisSet()
		{
			throw new NotImplementedException();
		}
	}
}
