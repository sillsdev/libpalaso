using System;
using System.Collections.Generic;
using NUnit.Framework;
using Palaso.Data;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	internal class Rfc5646CloneableTests:CloneableTests<Rfc5646Tag>
	{
		public override Rfc5646Tag CreateNewCloneable()
		{
			return new Rfc5646Tag();
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				var subtag = new Rfc5646Tag.Subtag();
				subtag.AddToSubtag("de");
				var unEqualSubtag = new Rfc5646Tag.Subtag();
				unEqualSubtag.AddToSubtag("en");
				return new List<ValuesToSet>
							 {
								 new ValuesToSet("to be", "!(to be)"),
								 new ValuesToSet(subtag, unEqualSubtag),
								 new ValuesToSet(false, true)
							 }; }
		}
	}

	[TestFixture]
	public class Rfc5646TagTests
	{
		[Test]
		public void AddToPrivateUse_PrivateUseIsEmpty_PrivateUseEqualsStringToAdd()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToPrivateUse("audio");
			Assert.AreEqual("x-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddContainsNonAlphaNumericCharacter_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToPrivateUse("_audio"));
		}

		[Test]
		public void AddToPrivateUse_StringToAddContainsUnderScoreAfterx_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToPrivateUse("x_audio"));
		}

		[Test]
		public void Constructor_LanguageQaaOthersNull_PropertiesAreEmptyString()
		{
			var tag = new Rfc5646Tag("qaa", null, null, null, null);
			Assert.AreEqual("qaa", tag.Language);
			Assert.AreEqual("", tag.Script);
			Assert.AreEqual("", tag.Region);
			Assert.AreEqual("", tag.Variant);
			Assert.AreEqual("", tag.PrivateUse);
		}

		[Test]
		public void Constructor_LanguageNullWithPrivateUse_PropertiesAreEmptyString()
		{
			var tag = new Rfc5646Tag(null, null, null, null, "any");
			Assert.AreEqual("", tag.Language);
			Assert.AreEqual("", tag.Script);
			Assert.AreEqual("", tag.Region);
			Assert.AreEqual("", tag.Variant);
			Assert.AreEqual("x-any", tag.PrivateUse);
		}

		[Test]
		public void PropertiesSetNull_AllExceptPrivateUse_PropertiesAreEmptyString()
		{
			var tag = new Rfc5646Tag("qaa", "Latn", "NZ", "1901", "any");
			tag.Script = null;
			tag.Region = null;
			tag.Variant = null;
			tag.Language = null; // Note the order is important here as validate is called after each property set.
			tag.PrivateUse = "any";
			Assert.AreEqual("", tag.Language);
			Assert.AreEqual("", tag.Script);
			Assert.AreEqual("", tag.Region);
			Assert.AreEqual("", tag.Variant);
			Assert.AreEqual("x-any", tag.PrivateUse);
		}

		[Test]
		public void PropertiesSetNull_AllExceptLanguage_PropertiesAreEmptyString()
		{
			var tag = new Rfc5646Tag("qaa", "Latn", "NZ", "1901", "any");
			tag.Language = "qaa";
			tag.Script = null;
			tag.Region = null;
			tag.Variant = null;
			tag.PrivateUse = null;
			Assert.AreEqual("qaa", tag.Language);
			Assert.AreEqual("", tag.Script);
			Assert.AreEqual("", tag.Region);
			Assert.AreEqual("", tag.Variant);
			Assert.AreEqual("", tag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddContainsIllegalCharacters_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToPrivateUse("x-audio_test"));
		}

		[Test]
		public void AddToPrivateUse_PrivateUseIsNotEmpty_StringToAddIsAppendedToPrivateUseWithDashDelimiter()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			rfcTag.AddToPrivateUse("audio");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddIsSubStringOfStringAlreadyContainedInPrivateUse_StringToAddIsAppendedToPrivateUseWithDashDelimiter()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audios-test");
			rfcTag.AddToPrivateUse("audio");
			Assert.AreEqual("x-audios-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddConsistsOfMultipleParts_StringToAddIsAppendedToPrivateUseWithDashDelimiter()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test-audios");
			rfcTag.AddToPrivateUse("audio-variant2");
			Assert.AreEqual("x-test-audios-audio-variant2", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_PrivateUseAlreadyContainsStringToAdd_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test-audio");
			Assert.Throws<ValidationException>(() => rfcTag.AddToPrivateUse("audio"));
		}

		[Test]
		public void AddToPrivateUse_PrivateUseAlreadyContainsPartsOfStringToAdd_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio-test");
			Assert.Throws<ValidationException>(() => rfcTag.AddToPrivateUse("smth-test"));
		}

		[Test]
		public void AddToPrivateUse_TagEndingInx_Set()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToPrivateUse("testx");
			Assert.AreEqual("x-testx", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_TagBeginningWithx_Set()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToPrivateUse("xtest");
			Assert.AreEqual("x-xtest", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_PrivateUseAlreadyContainsStringToAddInDifferentCase_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "AUDIO");
			Assert.Throws<ValidationException>(() => rfcTag.AddToPrivateUse("audio"));
		}

		[Test]
		public void AddToPrivateUse_StringToAddBeginsWithDash_StringIsAdded()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			rfcTag.AddToPrivateUse("-audio");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddWithx_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			Assert.Throws<ValidationException>(
				() => rfcTag.AddToPrivateUse("bbb-x-audio")
			);
		}

		[Test]
		public void AddToPrivateUse_StringToAddBeginsWithx_StringIsAdded()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			rfcTag.AddToPrivateUse("x-audio");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddDoesNotBeginWithx_StringIsAdded()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			rfcTag.AddToPrivateUse("audio");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToPrivateUse_StringToAddEndsWithDash_StringIsAdded()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			rfcTag.AddToPrivateUse("audio-");
			Assert.AreEqual("x-test-audio", rfcTag.PrivateUse);
		}

		[Test]
		public void AddToVariant_VariantIsEmpty_VariantEqualsStringToAdd()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.AddToVariant("1901");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_StringToAddContainsInvalidCharacter_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToVariant("1901_bauddha"));
		}

		[Test]
		public void AddToVariant_VariantIsNotEmpty_StringToAddIsAppendedToVariantWithDashDelimiter()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("bauddha");
			Assert.AreEqual("1901-bauddha", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_StringToAddConsistsOfMultipleParts_StringToAddIsAppendedToVariantWithDashDelimiter()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("bauddha-biske");
			Assert.AreEqual("1901-bauddha-biske", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_VariantAlreadyContainsStringToAdd_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToVariant("1901"));
		}

		[Test]
		public void AddToVariant_VariantAlreadyContainsPartsOfStringToAdd_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "bauddha-biske", String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToVariant("1901-bauddha"));
		}

		[Test]
		public void AddToVariant_VariantAlreadyContainsStringToAddInDifferentCase_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "BisKe", String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToVariant("biske"));
		}

		[Test]
		public void AddToVariant_StringToAddBeginsWithDash_StringIsAdded()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "biske", String.Empty);
			rfcTag.AddToVariant("-1901");
			Assert.AreEqual("biske-1901", rfcTag.Variant);
		}

		[Test]
		public void AddToVariant_StringToAddBeginsWithx_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "biske", String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToVariant("x-bauddha"));
		}

		[Test]
		public void AddToVariant_StringToAddIsNotValidVariant_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.AddToVariant("bogus"));
		}

		[Test]
		public void AddToVariant_StringToAddEndsWithDash_StringIsAdded()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.AddToVariant("biske-");
			Assert.AreEqual("1901-biske", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromPrivateUse_StringToTRemoveIsPrependedByxDash_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio");
			rfcTag.RemoveFromPrivateUse("x-audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_StringToremoveIsNotPrependedByxDash_StringIsRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio");
			rfcTag.RemoveFromPrivateUse("audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseIsEmpty_PrivateUseRemainsUntouched()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.RemoveFromPrivateUse("audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseDoesNotContainStringToRemove_PrivateUseRemainsUntouched()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			rfcTag.RemoveFromPrivateUse("audio");
			Assert.AreEqual("x-test", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseDoesNotContainPartsOfStringToRemove_PartsThatAreContainedAreRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			rfcTag.RemoveFromPrivateUse("test-audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseContainsPartsOfStringToRemoveButNotConsecutively_RemovesPartsCorrectly()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test-smth-audio");
			rfcTag.RemoveFromPrivateUse("test-audio");
			Assert.AreEqual("x-smth", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseContainsPartAndNoValidlanguageTag_Throws()
		{
			var rfcTag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "test");
			Assert.Throws<ValidationException>(() => rfcTag.RemoveFromPrivateUse("x-test"));
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseContainsStringToRemove_PrivateUseIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test-audio");
			rfcTag.RemoveFromPrivateUse("audio");
			Assert.AreEqual("x-test", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseContainsStringToRemoveInDifferentCase_PrivateUseIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty,String.Empty, "test-aUdiO");
			rfcTag.RemoveFromPrivateUse("audio");
			Assert.AreEqual("x-test", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseEqualsStringToRemove_PrivateUseIsEmpty()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio");
			rfcTag.RemoveFromPrivateUse("audio");
			Assert.AreEqual(String.Empty ,rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseEqualsStringToRemoveInDifferentCase_PrivateUseIsEmpty()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "AudiO");
			rfcTag.RemoveFromPrivateUse("audio");
			Assert.AreEqual(String.Empty, rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseContainsStringToRemoveAndStringToRemoveStartsWithDash_StringIsRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "AudiO");
			rfcTag.RemoveFromPrivateUse("-audio");
			Assert.AreEqual(rfcTag.PrivateUse, String.Empty);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseContainsStringToRemoveAndStringToRemoveStartsWithUnderscore_DoesNotRemoveString()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "AudiO");
			rfcTag.RemoveFromPrivateUse("_!@#$bogus");
			Assert.AreEqual("x-AudiO", rfcTag.PrivateUse);
		}

		[Test]
		public void RemoveFromPrivateUse_PrivateUseContainsStringToRemoveAndStringToRemoveEndsWithDash_StringIsRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "AudiO");
			rfcTag.RemoveFromPrivateUse("audio-");
			Assert.AreEqual(rfcTag.PrivateUse, String.Empty);
		}

		[Test]
		public void RemoveFromVariant_StringToRemoveIsFirstInVariant_VariantIsStrippedOfStringToRemoveAndFollowingDash()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "biske-1901", String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_StringToRemoveInDifferentCaseIsFirstInVariant_VariantIsStrippedOfStringToRemoveAndFollowingDash()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "bIsKe-1901", String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantIsEmpty_VariantRemainsUntouched()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual(String.Empty, rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantDoesNotContainStringToRemove_VariantRemainsUntouched()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantDoesNotContainPartsOfStringToRemove_PartsThatAreContainedAreRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901-bauddha", String.Empty);
			rfcTag.RemoveFromVariant("biske-1901");
			Assert.AreEqual("bauddha", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantContainsPartsOfStringToRemoveButNotConsecutively_RemovesPartsCorrectly()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901-bauddha-biske", String.Empty);
			rfcTag.RemoveFromVariant("1901-biske");
			Assert.AreEqual("bauddha", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantContainsStringToRemove_VariantIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901-biske", String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantContainsStringToRemoveInDifferentCase_VariantIsStrippedOfStringToRemoveAndPrecedingDash()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901-BisKe", String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual("1901", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantEqualsStringToRemove_VariantIsEmpty()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "biske", String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual(String.Empty, rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantEqualsStringToRemoveInDifferentCase_VariantIsEmpty()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "bIsKe", String.Empty);
			rfcTag.RemoveFromVariant("biske");
			Assert.AreEqual(String.Empty, rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantContainsStringToRemoveAndStringToRemoveStartsWithDash_StringIsRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "biske", String.Empty);
			rfcTag.RemoveFromVariant("-biske");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void RemoveFromVariant_VariantContainsStringToRemoveAndStringToRemoveStartsWithUnderscore_StringIsNotRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "biske", String.Empty);
			rfcTag.RemoveFromVariant("_biske");
			Assert.AreEqual("biske", rfcTag.Variant);
		}

		[Test]
		public void RemoveFromVariant_VariantContainsStringToRemoveAndStringToRemoveEndsWithDash_StringIsRemoved()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "biske", String.Empty);
			rfcTag.RemoveFromVariant("biske-");
			Assert.AreEqual(rfcTag.Variant, String.Empty);
		}

		[Test]
		public void PrivateUseContainsPart_PartIsNotContainedInPrivateUse_ReturnsFalse()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.IsFalse(rfcTag.PrivateUseContains("audio"));
		}

		[Test]
		public void PrivateUseContainsPart_PartIsContainedInPrivateUse_ReturnsTrue()
		{
			var rfcTag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "audio");
			Assert.IsTrue(rfcTag.PrivateUseContains("audio"));
		}

		[Test]
		public void PrivateUseContainsPart_PartIsContainedInPrivateUseAndpartDoesNotHavePrependedx_ReturnsTrue()
		{
			var rfcTag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "audio");
			Assert.IsTrue(rfcTag.PrivateUseContains("audio"));
		}

		[Test]
		public void PrivateUseContainsPart_PartConsistsOfMultiplePartsAndNotAllPartsAreContainedInPrivateUse_ReturnsFalse()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test");
			Assert.IsFalse(rfcTag.PrivateUseContains("etic-test"));
		}

		[Test]
		public void PrivateUseContainsPart_PartConsistsOfMultiplePartsAndAllPartsAreContainedInPrivateUse_ReturnsFalse()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "test-audio");
			Assert.IsFalse(rfcTag.PrivateUseContains("test-audio"));
		}

		[Test]
		public void VariantContainsPart_PartIsNotContainedInVariant_ReturnsFalse()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.IsFalse(rfcTag.VariantContains("1901"));
		}

		[Test]
		public void VariantContainsPart_PartIsContainedInVariant_ReturnsTrue()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			Assert.IsTrue(rfcTag.VariantContains("1901"));
		}

		[Test]
		public void VariantContainsPart_PartConsistsOfMultiplePartsAndNotAllPartsAreContainedInVariant_ReturnsFalse()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			Assert.IsFalse(rfcTag.VariantContains("biske-1901"));
		}

		[Test]
		public void VariantContainsPart_PartConsistsOfMultiplePartsAndAllPartsAreContainedInVariant_ReturnsFalse()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901-bauddha-biske", String.Empty);
			Assert.IsFalse(rfcTag.VariantContains("1901-bauddha"));
		}

		[Test]
		public void Constructor_SetWithInvalidLanguageTag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("bogus", String.Empty, String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidScriptTag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("en", "bogus", String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidRegionTag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("en", String.Empty, "bogus", String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_SetWithInvalidVariantTag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("en", String.Empty, String.Empty, "bogus", String.Empty));
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Language = "bogus");
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Script = "bogus");
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Region = "bogus");
		}

		[Test]
		public void Variant_SetWithInvalidVariant_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Variant = "bogus");
		}

		[Test]
		public void Variant_SetWhileNotEmpty_RemovesOldVariant()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			tag.Variant = "bauddha";
			Assert.AreEqual("bauddha", tag.Variant);
		}

		[Test]
		public void PrivateUse_SetWhileNotEmpty_RemovesOldPrivateUse()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty,String.Empty, "test");
			tag.PrivateUse = "audio";
			Assert.AreEqual("x-audio", tag.PrivateUse);
		}

		[Test]
		public void Variant_SetStringContainingxAsPart_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Variant = "x-1901");
		}

		[Test]
		public void Constructor_LanguageIsEmptyAndPrivateUseIsEmpty_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_LanguageSubtagIsQaa_IsValid()
		{
			var rfcTag = new Rfc5646Tag("qaa", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.AreEqual("qaa", rfcTag.Language);
		}

		[Test]
		public void LanguageSubtag_SetToQaa_IsSet()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			rfcTag.Language = "qaa";
			Assert.AreEqual("qaa", rfcTag.Language);
		}

		[Test]
		public void Constructor_ScriptIsSetButLanguageIsEmpty_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag(String.Empty, "Zxxx", String.Empty, String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_RegionIsSetButLanguageIsEmpty_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag(String.Empty, String.Empty, "US", String.Empty, String.Empty));
		}

		[Test]
		public void Constructor_VariantIsSetButLanguageIsEmpty_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag(String.Empty, String.Empty, String.Empty, "1901", String.Empty));
		}

		[Test]
		public void Language_SetToEmptyWhenScriptIsNotEmpty_Throws()
		{
		   var rfcTag = new Rfc5646Tag("en", "Zxxx", String.Empty, String.Empty, String.Empty);
		   Assert.Throws<ValidationException>(() => rfcTag.Language = String.Empty);
		}

		[Test]
		public void Language_SetToEmptyWhenRegionIsNotEmpty_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, "US", String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.Language = String.Empty);
		}

		[Test]
		public void Language_SetToEmptyWhenVariantIsNotEmpty_Throws()
		{
			var rfcTag = new Rfc5646Tag("en", String.Empty, String.Empty, "1901", String.Empty);
			Assert.Throws<ValidationException>(() => rfcTag.Language = String.Empty);
		}

		[Test]
		public void Script_SetWhileLanguageSubtagIsEmpty_Throws()
		{
			var rfcTag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "test");
			Assert.Throws<ValidationException>(() => rfcTag.Script = "Zxxx");
		}

		[Test]
		public void Region_SetWhileLanguageSubtagIsEmpty_Throws()
		{
			var rfcTag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "test");
			Assert.Throws<ValidationException>(() => rfcTag.Region = "US");
		}

		[Test]
		public void Variant_SetWhileLanguageSubtagIsEmpty_Throws()
		{
			var rfcTag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "test");
			Assert.Throws<ValidationException>(() => rfcTag.Variant = "1901");
		}

		[Test]
		public void Constructor_ScriptAndPrivateUseAreSetButLanguageIsEmpty_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag(String.Empty, "Zxxx", String.Empty, String.Empty, "test"));
		}

		[Test]
		public void Constructor_RegionAndPrivateUseAreSetButLanguageIsEmpty_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag(String.Empty, String.Empty, "US", String.Empty, "test"));
		}

		[Test]
		public void Constructor_LanguageIsEmptyAndPrivateUseIsSet_CompleteTagConsistsOnlyOfPrivateUse()
		{
			var tag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty, String.Empty, "audio");
			Assert.AreEqual("x-audio", tag.CompleteTag);
		}

		[Test]
		public void Constructor_LanguageIsEmptyAndPrivateUseIsSet_SetsPrivateUse()
		{
			var tag = new Rfc5646Tag(String.Empty, String.Empty, String.Empty,String.Empty, "audio");
			Assert.AreEqual("x-audio", tag.PrivateUse);
		}

		[Test]
		public void Language_SetWithCasedButValidLanguageTag_SetsLanguage()
		{
			var tag = new Rfc5646Tag("EN", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.AreEqual("EN", tag.Language);
		}

		[Test]
		public void Script_SetWithCasedButValidScriptTag_SetsScript()
		{
			var tag = new Rfc5646Tag("en", "LAtN", String.Empty, String.Empty, String.Empty);
			Assert.AreEqual("LAtN", tag.Script);
		}

		[Test]
		public void Region_SetWithCasedButValidRegionTag_SetsRegion()
		{
			var tag = new Rfc5646Tag("en", String.Empty, "us", String.Empty, String.Empty);
			Assert.AreEqual("us", tag.Region);
		}

		[Test]
		public void Variant_SetWithCasedButValidVariantTag_SetsVariant()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, "1694AcaD", String.Empty);
			Assert.AreEqual("1694AcaD", tag.Variant);
		}

		[Test]
		public void Language_SetWithTwoValidLanguageTags_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Language = "en-de");
		}

		[Test]
		public void Script_SetWithTwoValidScriptTags_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Script = "Latn-Afak");
		}

		[Test]
		public void Region_SetWithTwoValidRegionTags_Throws()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Region = "US-GB");
		}

		[Test]
		//this language tag is totally bogus but not technically invalid according to RFC5646
		public void Variant_SetWithTwoValidVariantTags_VariantIsSet()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			tag.Variant = "biske-1901";
			Assert.AreEqual("biske-1901", tag.Variant);
		}

		[Test]
		public void Constructor_PrivateuseSetWithTwoPrivateUseSubtagsBothPrefixedWithx_ThrowsArgumentException()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "x-private1-x-private2"));
		}

		[Test]
		public void Constructor_PrivateuseSetWithTwoPrivateUseSubtagsTheLatterPrefixedWithx_ThrowsValidationException()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "private1-x-private2"));
		}

		[Test]
		public void Constructor_SimpleEnglish_CompleteTagIsCorrect()
		{
			var rfctag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.AreEqual("en", rfctag.CompleteTag);
		}

		[Test]
		public void PrivateUse_SetWithTwoPrivateUseSubtagsBothPrefixedWithx_ThrowsArgumentException()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Variant = "x-private1-x-private2");
		}

		[Test]
		public void PrivateUse_SetWithTwoPrivateUseSubtagsTheLatterPrefixedWithx_ThrowsValidationException()
		{
			var tag = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.Throws<ValidationException>(() => tag.Variant = "private1-x-private2");
		}

		[Test]
		public void CompleteTag_AllSubtagsSet_IsSetCorrectly()
		{
			var tag = new Rfc5646Tag("en", "Latn", "US", "1901", "audio");
			Assert.AreEqual("en-Latn-US-1901-x-audio", tag.CompleteTag);
		}

		[Test]
		public void CompleteTag_OnlyPrivateUseSet_OnlyPrivateUseIsReturned()
		{
			var tag = new Rfc5646Tag("", "", "", "", "private");
			Assert.AreEqual("x-private", tag.CompleteTag);
		}

		[Test]
		public void Equals_IsoIsEqual_ReturnsTrue()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			var tag2 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.AreEqual(tag1, tag2);
		}

		[Test]
		public void Equals_IsoScriptIsEqual_ReturnsTrue()
		{
			var tag1 = new Rfc5646Tag("en", "Zxxx", String.Empty, String.Empty, String.Empty);
			var tag2 = new Rfc5646Tag("en", "Zxxx", String.Empty, String.Empty, String.Empty);
			Assert.AreEqual(tag1, tag2);
		}

		[Test]
		public void Equals_IsoRegionIsEqual_ReturnsTrue()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, "US", String.Empty, String.Empty);
			var tag2 = new Rfc5646Tag("en", String.Empty, "US", String.Empty, String.Empty);
			Assert.AreEqual(tag1, tag2);
		}

		[Test]
		public void Equals_IsoVariantIsEqual_ReturnsTrue()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, String.Empty, "bauddha", String.Empty);
			var tag2 = new Rfc5646Tag("en", String.Empty, String.Empty, "bauddha", String.Empty);
			Assert.AreEqual(tag1, tag2);
		}

		[Test]
		public void Equals_PrivateUseWithTwoTagsIsEqual_ReturnsTrue()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio-phonetic");
			var tag2 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio-phonetic");
			Assert.AreEqual(tag1, tag2);
		}

		[Test]
		public void Equals_PrivateUseIsEqual_ReturnsTrue()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio");
			var tag2 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio");
			Assert.AreEqual(tag1, tag2);
		}

		[Test]
		public void Equals_AlltagsAreEqual_ReturnsTrue()
		{
			var tag1 = new Rfc5646Tag("en", "Zxxx", "US", "1901", "audio");
			var tag2 = new Rfc5646Tag("en", "Zxxx", "US", "1901", "audio");
			Assert.AreEqual(tag1, tag2);
		}

		[Test]
		public void Equals_IsoNotEqual_ReturnsFalse()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, String.Empty);
			var tag2 = new Rfc5646Tag("de", String.Empty, String.Empty, String.Empty, String.Empty);
			Assert.AreNotEqual(tag1, tag2);
		}

		[Test]
		public void Equals_IsoScriptIsNotEqual_ReturnsFalse()
		{
			var tag1 = new Rfc5646Tag("en", "Zxxx", String.Empty, String.Empty, String.Empty);
			var tag2 = new Rfc5646Tag("en", "Latn", String.Empty, String.Empty, String.Empty);
			Assert.AreNotEqual(tag1, tag2);
		}

		[Test]
		public void Equals_IsoRegionIsNotEqual_ReturnsFalse()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, "US", String.Empty, String.Empty);
			var tag2 = new Rfc5646Tag("en", String.Empty, "GB", String.Empty, String.Empty);
			Assert.AreNotEqual(tag1, tag2);
		}

		[Test]
		public void Equals_IsoVariantIsNotEqual_ReturnsFalse()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, String.Empty, "bauddha", String.Empty);
			var tag2 = new Rfc5646Tag("en", String.Empty, String.Empty, "biske", String.Empty);
			Assert.AreNotEqual(tag1, tag2);
		}

		[Test]
		public void Equals_PrivateUseIsNotEqual_ReturnsFalse()
		{
			var tag1 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "audio");
			var tag2 = new Rfc5646Tag("en", String.Empty, String.Empty, String.Empty, "etic");
			Assert.AreNotEqual(tag1, tag2);
		}

		[Test]
		[Ignore("We are not implementing this right now. If it becomes necassary we will.")]
		public void Equals_VariantTagsAreSameButNotInSameOrder_ReturnsTrue()
		{
			throw new NotImplementedException();
		}

		[Test]
		[Ignore("We are not implementing this right now. If it becomes necassary we will.")]
		public void Equals_PrivateUseTagsAreSameButNotInSameOrder_ReturnsTrue()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Constructor_LanguageSubtagContainsScriptSubtag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("Latn", "", "", "", ""));
		}

		[Test]
		public void Constructor_ScriptSubtagContainsLanguageSubtag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("qaa", "en", "", "", ""));
		}

		[Test]
		public void Constructor_RegionSubtagContainsLanguageSubtag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("qaa", "", "en", "", ""));
		}

		[Test]
		public void Constructor_VariantSubtagContainsLanguageSubtag_Throws()
		{
			Assert.Throws<ValidationException>(() => new Rfc5646Tag("qaa", "", "", "en", ""));
		}

		private void AssertTag(Rfc5646Tag tag, string language, string script, string region, string variant, string privateUse)
		{
			Assert.AreEqual(language, tag.Language);
			Assert.AreEqual(script, tag.Script);
			Assert.AreEqual(region, tag.Region);
			Assert.AreEqual(variant, tag.Variant);
			Assert.AreEqual(privateUse, tag.PrivateUse);
		}

		[Test]
		public void Parse_HasOnlyPrivateUse_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("x-privuse");
			AssertTag(tag, string.Empty, string.Empty, string.Empty, string.Empty, "x-privuse");
		}

		[Test]
		public void Parse_HasMultiplePrivateUse_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("x-private-use");
			AssertTag(tag, string.Empty, string.Empty, string.Empty, string.Empty, "x-private-use");
		}

		[Test]
		public void Parse_HasLanguage_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("de");
			AssertTag(tag, "de", string.Empty, string.Empty, string.Empty, string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScript_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("en-Latn");
			AssertTag(tag, "en", "Latn", string.Empty, string.Empty, string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegion_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("en-Latn-US");
			AssertTag(tag, "en", "Latn", "US", string.Empty, string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndVariant_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("de-Latn-DE-1901");
			AssertTag(tag, "de", "Latn", "DE", "1901", string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndMultipleVariants_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("de-Latn-DE-1901-bauddha");
			AssertTag(tag, "de", "Latn", "DE", "1901-bauddha", string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndMultipleVariantsAndPrivateUse_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("de-Latn-DE-1901-bauddha-x-private");
			AssertTag(tag, "de", "Latn", "DE", "1901-bauddha", "x-private");
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndMultipleVariantsAndMultiplePrivateUse_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("de-Latn-DE-1901-bauddha-x-private-use");
			AssertTag(tag, "de", "Latn", "DE", "1901-bauddha", "x-private-use");
		}

		[Test]
		public void Constructor_LanguageIsSetToqaa_SetsLanguageToqaa()
		{
			var rfctag =  new Rfc5646Tag("qaa", "", "", "", "");
			Assert.AreEqual("qaa", rfctag.Language);
		}

		[Test, Ignore("NYI at this time")]
		public void Constructor_LanguageIsSetToqtz_SetsLanguageToqtz()
		{
			var rfctag = new Rfc5646Tag("qtz", "", "", "", "");
			Assert.AreEqual("qtz", rfctag.Language);
		}

		[Test, Ignore("NYI at this time")]
		public void Constructor_ScriptIsSetToQaaa_SetsScriptToQaaa()
		{
			var rfctag = new Rfc5646Tag("qaa", "Qaaa", "", "", "");
			Assert.AreEqual("Qaaa", rfctag.Script);
		}

		[Test, Ignore("NYI at this time")]
		public void Constructor_ScriptIsSetToQabx_SetsScriptToQabx()
		{
			var rfctag = new Rfc5646Tag("qaa", "Qabx", "", "", "");
			Assert.AreEqual("Qabx", rfctag.Script);
		}

		[Test, Ignore("NYI at this time")]
		public void Constructor_RegionIsSetToQM_SetsRegionToQM()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "QM", "", "");
			Assert.AreEqual("QM", rfctag.Region);
		}

		[Test, Ignore("NYI at this time")]
		public void Constructor_RegionIsSetToQZ_SetsRegionToQZ()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "QZ", "", "");
			Assert.AreEqual("QZ", rfctag.Region);
		}

		[Test, Ignore("NYI at this time")]
		public void Constructor_RegionIsSetToXA_SetsRegionToXA()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "XA", "", "");
			Assert.AreEqual("XA", rfctag.Region);
		}

		[Test, Ignore("NYI at this time")]
		public void Constructor_RegionIsSetToXZ_SetsRegionToXZ()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "XZ", "", "");
			Assert.AreEqual("XZ", rfctag.Region);
		}

		[Test]
		public void Language_IsSetToqaa_SetsLanguageToqaa()
		{
			var rfctag = new Rfc5646Tag("en", "", "", "", "");
			rfctag.Language = "qaa";
			Assert.AreEqual("qaa", rfctag.Language);
		}

		[Test, Ignore("NYI at this time")]
		public void Language_IsSetToqtz_SetsLanguageToqtz()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "", "", "");
			rfctag.Language = "qtz";
			Assert.AreEqual("qtz", rfctag.Language);
		}

		[Test]
		public void Parse_HasLanguageAndScriptAndRegionAndVariantAndMultiplePrivateUse_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("de-Latn-DE-bauddha-x-private-use");
			AssertTag(tag, "de", "Latn", "DE", "bauddha", "x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndRegionAndVariantAndMultiplePrivateUse_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("de-DE-bauddha-x-private-use");
			AssertTag(tag, "de", string.Empty, "DE", "bauddha", "x-private-use");
		}

		[Test]
		public void Parse_HasLanguageAndVariant_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("en-alalc97");
			AssertTag(tag, "en", string.Empty, string.Empty, "alalc97", string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndMultipleVariants_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("en-alalc97-aluku");
			AssertTag(tag, "en", string.Empty, string.Empty, "alalc97-aluku", string.Empty);
		}

		[Test]
		public void Parse_HasLanguageAndRegion_RFC5646TagHasExpectedFields()
		{
			var tag = Rfc5646Tag.Parse("en-US");
			AssertTag(tag, "en", string.Empty, "US", string.Empty, string.Empty);
		}

		[Test]
		public void RFC5646Tag_EmptyConstructor_HasDefaultLanguage()
		{
			var tag = new Rfc5646Tag();
			AssertTag(tag, "qaa", string.Empty, string.Empty, string.Empty, string.Empty);
		}

		[Test, Ignore("NYI at this time")]
		public void Script_IsSetToQaaa_SetsScriptToQaaa()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "", "", "");
			rfctag.Script = "Qaaa";
			Assert.AreEqual("Qaaa", rfctag.Script);
		}

		[Test, Ignore("NYI at this time")]
		public void Script_IsSetToQabx_SetsScriptToQabx()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "", "", "");
			rfctag.Script = "Qabx";
			Assert.AreEqual("Qabx", rfctag.Script);
		}

		[Test, Ignore("NYI at this time")]
		public void Region_IsSetToQM_SetsRegionToQM()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "QM", "", "");
			rfctag.Region = "QM";
			Assert.AreEqual("QM", rfctag.Region);
		}

		[Test, Ignore("NYI at this time")]
		public void Region_IsSetToQZ_SetsRegionToQZ()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "QZ", "", "");
			rfctag.Region = "QZ";
			Assert.AreEqual("QZ", rfctag.Region);
		}

		[Test, Ignore("NYI at this time")]
		public void Region_IsSetToXA_SetsRegionToXA()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "XA", "", "");
			rfctag.Region = "XA";
			Assert.AreEqual("XA", rfctag.Region);
		}

		[Test, Ignore("NYI at this time")]
		public void Region_IsSetToXZ_SetsRegionToXZ()
		{
			var rfctag = new Rfc5646Tag("qaa", "", "XZ", "", "");
			rfctag.Region = "XZ";
			Assert.AreEqual("XZ", rfctag.Region);
		}
	}
}
