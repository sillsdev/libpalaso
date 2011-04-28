using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Palaso.Data;
using Palaso.WritingSystems;

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
		public void IpaStatus_VariantIsSetToXDashEtic_ReturnsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.Variant = "x-etic";
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IpaStatus_VariantIsSetToXDashEmic_ReturnsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.Variant = "x-emic";
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
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
	}
}