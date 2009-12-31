using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemDefinitionVariantTests
	{
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
			ws.IpaStatus = IpaStatusChoices.Ipa;
			ws.Variant = "a-b-fonipa";
			Assert.AreEqual("a-b-fonipa", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToIpaWhenVariantHasContents_FonIpaAtEnd()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "a-b";
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual("a-b-fonipa", ws.Variant);
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
			ws.Variant = "a-b";
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("a-b", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantHasContents_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "a-b-fonipa";
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("a-b", ws.Variant);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantHasContentsWithIpaInMiddle_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "a-b-fonipa-c";//this is actually a bad tag as of 2009, fonipa can't be extended
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.AreEqual("a-b-c", ws.Variant);
		}


		[Test]
		public void IpaStatus_IpaPhonetic_RoundTrips()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "a-b";
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonetic, ws.IpaStatus);
			Assert.AreEqual("a-b-fonipa-x-etic", ws.Variant);
		}
		[Test]
		public void IpaStatus_IpaPhonemic_RoundTrips()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "a-b";
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonemic, ws.IpaStatus);
			Assert.AreEqual("a-b-fonipa-x-emic", ws.Variant);
		}
		[Test]
		public void IpaStatus_IpaPhoneticToPhonemic_MakesChange()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "a-b";
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.AreEqual(IpaStatusChoices.IpaPhonemic, ws.IpaStatus);
			Assert.AreEqual("a-b-fonipa-x-emic", ws.Variant);
		}
		[Test]
		public void SetIpaStatus_SetIpaWasVoice_RemovesVoice()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "a-b";
			ws.IsVoice=true;
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.AreEqual("a-b-fonipa", ws.Variant);
		}
	}
}