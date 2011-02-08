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
		public void AddToSubtag_SubtagIsEmpty_SubtagEqualsStringToAdd()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "x-audio");
		}

		[Test]
		public void AddToSubtag_SubtagContainsExtensionWithUnderScore_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x_audio"));
		}

		[Test]
		public void AddToSubtag_SubtagIsNotEmpty_StringToAddIsAppendedToSubtagWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "biske-x-audio");
		}

		[Test]
		public void AddToSubtag_StringToAddIsSubStringOfStringAlreadyContainedInSubTag_StringToAddIsAppendedToSubtagWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-audios");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "biske-x-audios-x-audio");
		}

		[Test]
		public void AddToSubtag_StringToAddConsistsOfMultipleParts_StringToAddIsAppendedToSubtagWithDashDelimiter()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-audios");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio-variant2");
			Assert.AreEqual(rfcTag.Variant, "biske-x-audios-x-audio-variant2");
		}

		[Test]
		public void AddToSubtag_SubtagAlreadyContainsStringToAdd_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-audio");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void AddToSubtag_SubtagAlreadyContainsPartsOfStringToAdd_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-audio");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "biske2-x-audio"));
		}

		[Test]
		public void AddToSubtag_SubtagAlreadyContainsStringToAddInDifferentCase_Throws()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-AUDIO");
			Assert.Throws<ArgumentException>(() => rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void AddToSubtag_StringToAddBeginsWithDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "-x-audio");
			Assert.AreEqual("biske-x-audio", rfcTag.Variant);
		}

		[Test]
		public void AddToSubtag_StringToAddBeginsWithUnderscore_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "_x-audio");
			Assert.AreEqual("biske-x-audio", rfcTag.Variant);
		}

		[Test]
		public void AddToSubtag_StringToAddEndsWithDash_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio-");
			Assert.AreEqual("biske-x-audio", rfcTag.Variant);
		}

		[Test]
		public void AddToSubtag_StringToAddEndsWithUnderscoreh_StringIsAdded()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske");
			rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "x-audio_");
			Assert.AreEqual("biske-x-audio", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagIsEmpty_SubTagRemainsUntouched()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(String.Empty, rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagDoesNotContainStringToRemove_SubTagRemainsUntouched()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual("biske", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagDoesNotContainPartsOfStringToRemove_SubTagRemainsUntouched()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-bauddha");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "biske2-x-audio");
			Assert.AreEqual("biske-bauddha", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsPartsOfStringToRemoveButNotConsecutively_RemovesPartsCorrectly()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-bauddha-x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "biske-x-audio");
			Assert.AreEqual("bauddha", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemove_SubtagIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "biske");
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveInDifferentCase_SubtagIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-aUdiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, "biske");
		}

		[Test]
		public void RemoveFromSubtag_StringToRemoveIsFirstInSubtag_SubtagIsStrippedOfStringToRemoveAndFollowingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "biske-x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "biske");
			Assert.AreEqual(rfcTag.Variant, "x-audio");
		}

		[Test]
		public void RemoveFromSubtag_StringToRemoveInDifferentCaseIsFirstInSubtag_SubtagIsStrippedOfStringToRemoveAndFollowingDash()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "bIsKe-x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "biske");
			Assert.AreEqual(rfcTag.Variant, "x-audio");
		}

		[Test]
		public void RemoveFromSubtag_SubtagEqualsStringToRemove_SubtagIsEmpty()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-audio");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagEqualsStringToRemoveInDifferentCase_SubtagIsEmpty()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveStartsWithDash_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "-x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveStartsWithUnderscore_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-AudiO");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "_x-audio");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveEndsWithDash_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-AudiO-");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio-");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromSubtag_SubtagContainsStringToRemoveAndStringToRemoveEndsWithUnderscore_StringIsRemoved()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-AudiO_");
			rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio_");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void SubtagContainsPart_PartIsNotContainedInSubtag_ReturnsFalse()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.IsFalse(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void SubtagContainsPart_PartIsContainedInSubtag_ReturnsTrue()
		{
			var rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-audio");
			Assert.IsTrue(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "x-audio"));
		}

		[Test]
		public void SubtagContainsPart_PartConsistsOfMultiplePartsAndNotAllPartsAreContainedInSubtag_ReturnsFalse()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "fonipa");
			Assert.IsFalse(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "fonipa-x-etic"));
		}

		[Test]
		public void SubtagContainsPart_PartConsistsOfMultiplePartsAndAllPartsAreContainedInSubtag_ReturnsTrue()
		{
			var rfcTag = new RFC5646Tag("en", String.Empty, String.Empty, "fonipa-biske-x-etic");
			Assert.IsTrue(rfcTag.SubtagContainsPart(RFC5646Tag.SubTag.Variant, "fonipa-x-etic"));
		}

		[Test]
		public void Constructor_SetWithInvalidLanguageTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("bogus", String.Empty,String.Empty,String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidScriptTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", "bogus", String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidRegionTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", String.Empty, "bogus", String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidVariantTag_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", String.Empty, String.Empty, "bogus"));
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Language = "bogus");
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Script = "bogus");
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Region = "bogus");
		}

		[Test]
		public void Variant_SetWithInvalidVariant_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Variant = "bogus");
		}

		[Test]
		public void Variant_SetPrivateUseTag_VariantisSet()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			tag.Variant = "x-bogus";
			Assert.AreEqual("x-bogus", tag.Variant);
		}

		[Test]
		public void Constructor_LanguageIsEmptyAndVariantIsNotPrivateUse_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_LanguageIsEmptyAndVariantIsPrivateUse_SetsVariant()
		{
			var tag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, "x-audio");
			Assert.AreEqual("x-audio", tag.Variant);
		}

		[Test]
		public void Language_SetWithCasedButValidLanguageTag_SetsLanguage()
		{
			var tag = new RFC5646Tag("EN", String.Empty, String.Empty, String.Empty);
			Assert.AreEqual("EN", tag.Language);
		}

		[Test]
		public void Script_SetWithCasedButValidScriptTag_SetsScript()
		{
			var tag = new RFC5646Tag("en", "LAtN", String.Empty, String.Empty);
			Assert.AreEqual("LAtN", tag.Script);
		}

		[Test]
		public void Region_SetWithCasedButValidRegionTag_SetsRegion()
		{
			var tag = new RFC5646Tag("en", String.Empty, "us", String.Empty);
			Assert.AreEqual("us", tag.Region);
		}

		[Test]
		public void Variant_SetSeemingPrivateUseTagButWithCapitalX_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, "");
			Assert.Throws<ArgumentException>(() => tag.Variant = "X-audio");
		}

		[Test]
		public void Variant_SetWithCasedButValidVariantTag_SetsVariant()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, "1694AcaD");
			Assert.AreEqual(tag.Variant, "1694AcaD");
		}

		[Test]
		public void Language_SetWithTwoValidLanguageTags_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Language = "en-de");
		}

		[Test]
		public void Script_SetWithTwoValidScriptTags_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Script = "Latn-Afak");
		}

		[Test]
		public void Region_SetWithTwoValidRegionTags_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Region = "US-GB");
		}

		[Test]
		//this language tag is totally bogus but not technically invalid according to RFC5646
		public void Variant_SetWithTwoValidVariantTags_VariantIsSet()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			tag.Variant = "biske-1901";
			Assert.AreEqual("biske-1901", tag.Variant);
		}

		[Test]
		public void Constructor_VariantSetWithTwoPrivateUseSubtagsBothPrefixedWithx_Throws()
		{
			Assert.Throws<ArgumentException>(() => new RFC5646Tag("en", String.Empty, String.Empty, "x-private1-x-private2"));
		}

		[Test]
		public void Variant_SetWithTwoPrivateUseSubtagsBothPrefixedWithx_Throws()
		{
			var tag = new RFC5646Tag("en", String.Empty, String.Empty, String.Empty);
			Assert.Throws<ArgumentException>(() => tag.Variant = "x-private1-x-private2");
		}
	}
}
