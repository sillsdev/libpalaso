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
		public void AddToPrivateUse_PrivateUseIsEmpty_PrivateUseEqualsStringToAdd()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToPrivateUse("x-audio");
			Assert.AreEqual(rfcTag.PrivateUse, "x-audio");
		}

		[Test]
		public void AddToPrivateUse_StringToAddContainsUnderScoreAfterx_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => rfcTag.AddToPrivateUse("x_audio"));
		}

		[Test]
		public void AddToPrivateUse_StringToAddContainsUnderBetweenSubTags_PrivateUseIsSet()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToPrivateUse("x-audio_test");
			Assert.AreEqual("x-audio_test", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_PrivateUseIsNotEmpty_StringToAddIsAppendedToPrivateUseWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("x-audio");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddIsSubStringOfStringAlreadyContainedInPrivateUse_StringToAddIsAppendedToPrivateUseWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-audios-test");
			rfcTag.AddToPrivateUse("x-audio");
			Assert.AreEqual("x-audios-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddConsistsOfMultipleParts_StringToAddIsAppendedToPrivateUseWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test-audios");
			rfcTag.AddToPrivateUse("x-audio-variant2");
			Assert.AreEqual("x-test-audios-audio-variant2", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_PrivateUseAlreadyContainsStringToAdd_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test-audio");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToPrivateUse("x-audio"));
		}

		[Test]
		public void AddToPrivateUse_PrivateUseAlreadyContainsPartsOfStringToAdd_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-audio-test");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToPrivateUse("x-smth-test"));
		}

		[Test]
		public void AddToPrivateUse_PrivateUseAlreadyContainsStringToAddInDifferentCase_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-AUDIO");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToPrivateUse("x-audio"));
		}

		[Test]
		public void AddToPrivateUse_StringToAddBeginsWithDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("-x-audio");
			Assert.AreEqual(rfcTag.PrivateUse, "x-test-audio");
		}

		[Test]
		public void AddToPrivateUse_StringToAddBeginsWithx_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("x-audio");
			Assert.AreEqual(rfcTag.PrivateUse, "x-test-audio");
		}

		[Test]
		public void AddToPrivateUse_StringToAddDoesNotBeginWithx_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("audio");
			Assert.AreEqual(rfcTag.PrivateUse, "x-test-audio");
		}

		[Test]
		public void AddToPrivateUse_StringToAddBeginsWithUnderscore_StringIsAddedWithUnderscore()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("_x-audio");
			Assert.AreEqual("x-test_audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddEndsWithDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("x-audio-");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddEndsWithUnderScore_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("x-audio_");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_PrivateUseStringToAddIsPrependedByxDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("x-audio");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_PrivateUseIsPrivateUseStringToAddIsNotPrependedByxDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.AddToPrivateUse("audio");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToVariant_VariantIsEmpty_VariantEqualsStringToAdd()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToVariant("1901");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_StringToAddContainsUnderScore_IsSet()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToVariant("1901_bauddha");
		}

		[Test]
		public void AddToVariant_VariantIsNotEmpty_StringToAddIsAppendedToVariantWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("bauddha");
			Assert.AreEqual("1901-bauddha", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_StringToAddConsistsOfMultipleParts_StringToAddIsAppendedToVariantWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("bauddha-biski");
			Assert.AreEqual("1901-bauddha-biski", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_VariantAlreadyContainsStringToAdd_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			Assert.Throws<ArgumentException>(() => rfcTag.AddToVariant("1901"));
		}

		[Test]
		public void AddToVariant_VariantAlreadyContainsPartsOfStringToAdd_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "bauddha-biski", String.Empty);
			Assert.Throws<ArgumentException>(() => rfcTag.AddToVariant("1901-bauddha"));
		}

		[Test]
		public void AddToVariant_VariantAlreadyContainsStringToAddInDifferentCase_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "BiSkI", String.Empty);
			Assert.Throws<ArgumentException>(() => rfcTag.AddToVariant("biski"));
		}

		[Test]
		public void AddToVariant_StringToAddBeginsWithDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biski", String.Empty);
			rfcTag.AddToVariant("-1901");
			Assert.AreEqual(rfcTag.Variant, "biski-1901");
		}

		[Test]
		public void AddToVariant_StringToAddBeginsWithx_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biski", String.Empty);
			Assert.Throws<ArgumentException>(() => rfcTag.AddToVariant("x-bauddha"));
		}

		[Test]
		public void AddToVariant_StringToAddIsNotValidVariant_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => rfcTag.AddToVariant("bogus"));
		}

		[Test]
		public void AddToVariant_StringToAddBeginsWithUnderscore_StringIsAddedWithUnderscore()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("_biski");
			Assert.AreEqual("1901_biski", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_StringToAddEndsWithDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("biski-");
			Assert.AreEqual("1901-biski", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_StringToAddEndsWithUnderScore_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("biski_");
			Assert.AreEqual("1901-biski", rfcTag.Variant);
		}

		//**********************
		[Test]
		public void RemoveFromSubtag_SubTagIsPrivateUseStringToAddIsPrependedByxDash_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubTagIsPrivateUseStringToAddIsNotPrependedByxDash_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubtagIsEmpty_SubTagRemainsUntouched()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubtagDoesNotContainStringToRemove_SubTagRemainsUntouched()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio");
			Assert.AreEqual("biske", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubtagDoesNotContainPartsOfStringToRemove_PartsThatAreContainedAreRemoved()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-test-audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsPartsOfStringToRemoveButNotConsecutively_RemovesPartsCorrectly()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test-smth-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-test-audio");
			Assert.AreEqual(rfcTag.PrivateUse, "x-smth");
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemove_SubtagIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio");
			Assert.AreEqual("x-test", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveInDifferentCase_SubtagIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty,String.Empty, "x-test-aUdiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual("x-test", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_StringToRemoveIsFirstInSubtag_SubtagIsStrippedOfStringToRemoveAndFollowingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-1901", String.Empty);
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "biske");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_StringToRemoveInDifferentCaseIsFirstInSubtag_SubtagIsStrippedOfStringToRemoveAndFollowingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "bIsKe-1901", String.Empty);
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "biske");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagEqualsStringToRemove_SubtagIsEmpty()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio");
			Assert.AreEqual(String.Empty ,rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubtagEqualsStringToRemoveInDifferentCase_SubtagIsEmpty()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "x-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveStartsWithDash_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "x-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "-x-audio");
			Assert.AreEqual(rfcTag.PrivateUse, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveStartsWithUnderscore_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "x-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "_x-audio");
			Assert.AreEqual(rfcTag.PrivateUse, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveEndsWithDash_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "x-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio-");
			Assert.AreEqual(rfcTag.PrivateUse, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveEndsWithUnderscore_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "x-AudiO_");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.PrivateUse, "x-audio_");
			Assert.AreEqual(rfcTag.PrivateUse, String.Empty);
		}

		[Test]
		public void SubtagContainsPart_PartIsNotContainedInSubtag_ReturnsFalse()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.IsFalse(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void SubtagContainsPart_PartIsContainedInSubtag_ReturnsTrue()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "x-audio");
			Assert.IsTrue(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void SubtagContainsPart_PartConsistsOfMultiplePartsAndNotAllPartsAreContainedInSubtag_ReturnsFalse()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test");
			Assert.IsFalse(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-etic-test"));
		}

		[Test]
		public void SubtagContainsPart_PartConsistsOfMultiplePartsAndAllPartsAreContainedInSubtag_ReturnsTrue()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-test-smth-audio");
			Assert.IsTrue(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-audio-test"));
		}

		[Test]
		public void Constructor_SetWithInvalidLanguageTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("bogus", String.Empty, String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidScriptTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", "bogus", String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidRegionTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", String.Empty, "bogus", String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidVariantTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", String.Empty, String.Empty, "bogus", String.Empty));
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Language = "bogus");
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Script = "bogus");
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Region = "bogus");
		}

		[Test]
		public void Variant_SetWithInvalidVariant_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Variant = "bogus");
		}

		[Test]
		public void Variant_SetPrivateUseTag_VariantisSet()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			tag.Variant = "x-bogus";
			Assert.AreEqual("x-bogus", tag.Variant);
		}

		[Test]
		public void Constructor_LanguageIsEmptyAndPrivateUseIsEmpty_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_LanguageIsEmptyAndPrivateUseIsSet_SetsPrivateUse()
		{
			var tag = new RFC5646Tag(String.Empty, String.Empty, String.Empty,String.Empty, "x-audio");
			Assert.AreEqual("x-audio", tag.PrivateUse);
		}

		[Test]
		public void Language_SetWithCasedButValidLanguageTag_SetsLanguage()
		{
			var tag = new RFC5646Tag("EN", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.AreEqual("EN", tag.Language);
		}

		[Test]
		public void Script_SetWithCasedButValidScriptTag_SetsScript()
		{
			var tag = new RFC5646Tag("en", "LAtN", String.Empty, String.Empty, String.Empty);
			Assert.AreEqual("LAtN", tag.Script);
		}

		[Test]
		public void Region_SetWithCasedButValidRegionTag_SetsRegion()
		{
			var tag = new RFC5646Tag("en", String.Empty, "us", String.Empty, String.Empty);
			Assert.AreEqual("us", tag.Region);
		}

		[Test]
		public void Variant_SetSeemingPrivateUseTagButWithCapitalX_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.PrivateUse = "X-audio");
		}

		[Test]
		public void Variant_SetWithCasedButValidVariantTag_SetsVariant()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, "1694AcaD", String.Empty);
			Assert.AreEqual(tag.Variant, "1694AcaD");
		}

		[Test]
		public void Language_SetWithTwoValidLanguageTags_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Language = "en-de");
		}

		[Test]
		public void Script_SetWithTwoValidScriptTags_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Script = "Latn-Afak");
		}

		[Test]
		public void Region_SetWithTwoValidRegionTags_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Region = "US-GB");
		}

		[Test]
		//this language tag is totally bogus but not technically invalid according to RFC5646
		public void Variant_SetWithTwoValidVariantTags_VariantIsSet()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			tag.Variant = "biske-1901";
			Assert.AreEqual("biske-1901", tag.Variant);
		}

		[Test]
		public void Constructor_PrivateuseSetWithTwoPrivateUseSubtagsBothPrefixedWithx_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, "x-private1-x-private2"));
		}

		[Test]
		public void PrivateUse_SetWithTwoPrivateUseSubtagsBothPrefixedWithx_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Variant = "x-private1-x-private2");
		}
	}
}
