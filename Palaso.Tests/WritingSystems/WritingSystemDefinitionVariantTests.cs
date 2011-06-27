using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Palaso.Data;
using Palaso.WritingSystems;
using System.Linq;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemDefinitionVariantTests
	{
		[Test]
		public void LatestVersion_IsOne()
		{
			Assert.AreEqual(1, WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
		}

		[Test]
		public void IpaStatus_SetToIpaWhenVariantIsEmpty_VariantNowFonIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual("fonipa", ws.Variant);
		}


		[Test]
		public void IpaStatus_SetToIpaWasAlreadyIpaAndOnyVariant_NoChange()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.Ipa;
			ws.Variant = "fonipa";
			Assert.AreEqual("fonipa", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToIpaWasAlreadyIpaWithOtherVariants_NoChange()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske-fonipa";
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual("1901-biske-fonipa", ws.Variant);
		}


		[Test]
		public void IpaStatus_SetToPhoneticOnEntirelyPrivateUseWritingSystem_MarkerForUnlistedLanguageIsInserted()
		{
			var ws = WritingSystemDefinition.Parse("x-private");
			Assert.That(ws.Variant, Is.EqualTo("x-private"));
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.That(ws.ISO639, Is.EqualTo(WellKnownSubTags.Unlisted.Language));
			Assert.That(ws.Script, Is.EqualTo(""));
			Assert.That(ws.Region, Is.EqualTo(""));
			Assert.That(ws.Variant, Is.EqualTo("fonipa-x-private-etic"));
		}


		[Test]
		public void IpaStatus_SetToIpaOnEntirelyPrivateUseWritingSystem_MarkerForUnlistedLanguageIsInserted()
		{
			var ws = WritingSystemDefinition.Parse("x-private");
			Assert.That(ws.Variant, Is.EqualTo("x-private"));
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.That(ws.ISO639, Is.EqualTo(WellKnownSubTags.Unlisted.Language));
			Assert.That(ws.Script, Is.EqualTo(""));
			Assert.That(ws.Region, Is.EqualTo(""));
			Assert.That(ws.Variant, Is.EqualTo("fonipa-x-private"));
		}


		[Test]
		public void IpaStatus_SetToPhonemicOnEntirelyPrivateUseWritingSystem_MarkerForUnlistedLanguageIsInserted()
		{
			var ws = WritingSystemDefinition.Parse("x-private");
			Assert.That(ws.Variant, Is.EqualTo("x-private"));
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.That(ws.ISO639, Is.EqualTo(WellKnownSubTags.Unlisted.Language));
			Assert.That(ws.Script, Is.EqualTo(""));
			Assert.That(ws.Region, Is.EqualTo(""));
			Assert.That(ws.Variant, Is.EqualTo("fonipa-x-private-emic"));
		}

		[Test]
		public void IpaStatus_VariantSetWithNumerousVariantIpaWasAlreadyIpa_VariantIsSet()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.Ipa;
			ws.Variant = "1901-biske-fonipa";
			Assert.AreEqual("1901-biske-fonipa", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToIpaWhenVariantHasContents_FonIpaAtEnd()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske";
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual("1901-biske-fonipa", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenWasOnlyVariant_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa";
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantEmpty_NothingChanges()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantNotEmpty_NothingChanges()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske";
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("1901-biske", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantHasContents_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske-fonipa";
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("1901-biske", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantHasContentsWithIpaInMiddle_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske-fonipa-bauddha";//this is actually a bad tag as of 2009, fonipa can't be extended
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("1901-biske-bauddha", ws.Variant);
		}


		[Test]
		public void IpaStatus_IpaPhonetic_RoundTrips()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske";
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonetic, ws.IpaStatus);
			Assert.AreEqual("1901-biske-fonipa-x-etic", ws.Variant);
		}

		[Test]
		public void IpaStatus_IpaPhonemic_RoundTrips()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske";
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonemic, ws.IpaStatus);
			Assert.AreEqual("1901-biske-fonipa-x-emic", ws.Variant);
		}
		[Test]
		public void IpaStatus_IpaPhoneticToPhonemic_MakesChange()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske";
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonemic, ws.IpaStatus);
			Assert.AreEqual("1901-biske-fonipa-x-emic", ws.Variant);
		}
		[Test]
		public void SetIpaStatus_SetIpaWasVoice_RemovesVoice()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "1901-biske";
			ws.IsVoice=true;
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.IsFalse(ws.IsVoice);
		}
		[Test]
		public void IpaStatus_PrivateUseSetToPrefixEticPostfix_ReturnsIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa-x-PrefixEticPostfix";
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}
		[Test]
		public void IpaStatus_VariantSetToFoNiPa_ReturnsIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "FoNiPa";
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}

		[Test]
		public void IpaStatus_VariantSetToPrefixFonipaDashXDashEticPostfix_Throws()
		{
			var ws = new WritingSystemDefinition();
			Assert.Throws<ValidationException>(()=>ws.Variant = "Prefixfonipa-x-eticPostfix");
		}

		[Test]
		public void IpaStatus_VariantSetToPrefixFonipaDashXDashEticPostfix_ReturnsIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa-x-PrefixeticPostfix";
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}

		[Test]
		public void IpaStatus_VariantSetToFoNiPaDashXDasheTiC_ReturnsIpaPhonetic()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "FoNiPa-x-eTiC";
			Assert.AreEqual(IpaStatusChoices.IpaPhonetic, ws.IpaStatus);
		}
		[Test]
		public void IpaStatus_VariantSetToFonipaDashXDashPrefixemicPostfix_ReturnsIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa-x-PrefixemicPostfix";
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}
		[Test]
		public void IpaStatus_VariantSetToFoNiPaDashXDasheMiC_ReturnsIpaPhonemic()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "FoNiPa-x-eMiC";
			Assert.AreEqual(IpaStatusChoices.IpaPhonemic, ws.IpaStatus);
		}

		[Test]
		public void IpaStatus_SetToIpaWhileIsVoiceIsTrue_IpaStatusIsIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}

		[Test]
		public void IpaStatus_SetToIpaWhileIsVoiceIsTrue_IsVoiceIsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void IpaStatus_SetToPhoneticWhileIsVoiceIsTrue_IpaStatusIsPhonetic()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonetic, ws.IpaStatus);
		}

		[Test]
		public void IpaStatus_SetToPhoneticWhileIsVoiceIsTrue_IsVoiceIsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void IpaStatus_SetToPhonemicWhileIsVoiceIsTrue_IpaStatusIsPhonemic()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonemic, ws.IpaStatus);
		}

		[Test]
		public void IpaStatus_SetToPhonemicWhileIsVoiceIsTrue_IsVoiceIsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		[Ignore("Flex doesn't seem to mind if you set Arabic or some other script for ipa.")]
		public void IpaStatus_SetToAnyThingButNotIpaWhileScriptIsNotDontKnowWhatScriptItShouldBe_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		[Ignore("Flex doesn't seem to mind if you set Arabic or some other script for ipa.")]
		public void Script_SetToIDontKnowWhatScriptItShouldBewhileIpaStatusIsSetToAnyThingButNotIpa_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void FilterWellKnownPrivateUseTags_HasOnlyWellKnownTags_EmptyList()
		{
			string[] listToFilter = {WellKnownSubTags.Audio.PrivateUseSubtag,
										WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag,
										WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag
									};
			IEnumerable<string> result = WritingSystemDefinition.FilterWellKnownPrivateUseTags(listToFilter);
			Assert.That(result, Is.Empty);
		}

		[Test]
		public void FilterWellKnownPrivateUseTags_HasWellKnownTagsAndUnknownTags_ListWithUnknownTags()
		{
			string[] listToFilter = { "v", WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag, WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag };
			IEnumerable<string> result = WritingSystemDefinition.FilterWellKnownPrivateUseTags(listToFilter);
			Assert.That(result, Has.Member("v"));
			Assert.That(result, Has.No.Member(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag));
			Assert.That(result, Has.No.Member(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag));
		}

		[Test]
		public void FilterWellKnownPrivateUseTags_HasUpperCaseWellKnownTagsAndUnknownTags_ListWithUnknownTags()
		{
			string[] listToFilter = { "v", WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag.ToUpper(), WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag.ToUpper() };
			IEnumerable<string> result = WritingSystemDefinition.FilterWellKnownPrivateUseTags(listToFilter);
			Assert.That(result, Has.Member("v"));
			Assert.That(result, Has.No.Member(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag.ToUpper()));
			Assert.That(result, Has.No.Member(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag.ToUpper()));
		}

		[Test]
		public void FilterWellKnownPrivateUseTags_EmptyList_EmptyList()
		{
			string[] listToFilter = {};
			IEnumerable<string> result = WritingSystemDefinition.FilterWellKnownPrivateUseTags(listToFilter);
			Assert.That(result, Is.Empty);
		}

		[Test]
		public void FilterWellKnownPrivateUseTags_HasOnlyUnknownTags_ListWithUnknownTags()
		{
			string[] listToFilter = { "v", "puu", "yuu" };
			IEnumerable<string> result = WritingSystemDefinition.FilterWellKnownPrivateUseTags(listToFilter);
			Assert.That(result, Has.Member("v"));
			Assert.That(result, Has.Member("puu"));
			Assert.That(result, Has.Member("yuu"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantOnly_ReturnsVariant()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("1901", String.Empty);
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithxDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("1901", "x-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithoutxDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("1901", "audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithoutxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("", "audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("", "x-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithCapitalXDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("", "X-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("X-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithCapitalXDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("1901", "X-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-X-audio"));
		}

		//this test shows that there is no checking involved as to wether your variants and private use are rfc/writingsystemdefinition conform. All the method does is glue two strings together while handling the "x-"
		[Test]
		public void ConcatenateVariantAndPrivateUse_BogusVariantBadprivateUse_HappilyGluesTheTwoTogether()
		{
			string concatenatedVariantAndPrivateUse = WritingSystemDefinition.ConcatenateVariantAndPrivateUse("bogusvariant", "etic-emic-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("bogusvariant-x-etic-emic-audio"));
		}

		//Split
		[Test]
		public void SplitVariantAndPrivateUse_VariantOnly_ReturnsVariant()
		{
			string variant;
			string privateUse;
			WritingSystemDefinition.SplitVariantAndPrivateUse("1901", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo(String.Empty));
		}

		[Test]
		public void SplitVariantAndPrivateUse_VariantAndPrivateUse_ReturnsVariantAndPrivateUse()
		{
			string variant;
			string privateUse;
			WritingSystemDefinition.SplitVariantAndPrivateUse("1901-x-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_NoxDash_ReturnsVariantOnly()
		{
			string variant;
			string privateUse;
			WritingSystemDefinition.SplitVariantAndPrivateUse("1901-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901-audio"));
			Assert.That(privateUse, Is.EqualTo(String.Empty));
		}

		[Test]
		public void SplitVariantAndPrivateUse_PrivateUseWithxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string variant;
			string privateUse;
			WritingSystemDefinition.SplitVariantAndPrivateUse("x-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo(String.Empty));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_PrivateUseWithCapitalXDashOnly_ReturnsPrivateUseWithxDash()
		{
			string variant;
			string privateUse;
			WritingSystemDefinition.SplitVariantAndPrivateUse("X-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo(String.Empty));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_VariantAndPrivateUseWithCapitalXDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string variant = String.Empty;
			string privateUse = String.Empty;
			WritingSystemDefinition.SplitVariantAndPrivateUse("1901-X-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		//this test shows that there is no checking involved as to wether your variants and private use are rfc/writingsystemdefinition conform. All the method does is split on x-
		[Test]
		public void SplitVariantAndPrivateUse_BogusVariantBadPrivateUse_HappilysplitsOnxDash()
		{
			string variant;
			string privateUse;
			WritingSystemDefinition.SplitVariantAndPrivateUse("bogusVariant-X-audio-emic-etic", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("bogusVariant"));
			Assert.That(privateUse, Is.EqualTo("audio-emic-etic"));
		}
	}
}