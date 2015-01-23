using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class WritingSystemDefinitionVariantTests
	{
		[Test]
		public void LatestVersion_IsTwo()
		{
			Assert.AreEqual(2, WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
		}

		[Test]
		public void IpaStatus_SetToIpaWhenVariantIsEmpty_VariantNowFonIpa()
		{
			var ws = new WritingSystemDefinition {IpaStatus = IpaStatusChoices.Ipa};
			Assert.That(ws.Variants.Contains(WellKnownSubtags.IpaVariant), Is.True);
		}

		[Test]
		public void IpaStatus_SetToIpaWasAlreadyIpaWithOtherVariants_NoChange()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.Variants.Add("fonipa");
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske", "fonipa"}));
		}

		[Test]
		public void IpaStatus_SetToPhoneticOnEntirelyPrivateUseWritingSystem_MarkerForUnlistedLanguageIsInserted()
		{
			var ws = new WritingSystemDefinition("x-private");
			Assert.That(ws.Language, Is.EqualTo((LanguageSubtag) "private"));
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.That(ws.Language, Is.EqualTo((LanguageSubtag) "private"));
			Assert.That(ws.Script, Is.Null);
			Assert.That(ws.Region, Is.Null);
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"fonipa", "etic"}));
		}

		[Test]
		public void IpaStatus_SetToIpaOnEntirelyPrivateUseWritingSystem_MarkerForUnlistedLanguageIsInserted()
		{
			var ws = new WritingSystemDefinition("x-private");
			Assert.That(ws.Language, Is.EqualTo((LanguageSubtag) "private"));
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.That(ws.Language, Is.EqualTo((LanguageSubtag) "private"));
			Assert.That(ws.Script, Is.Null);
			Assert.That(ws.Region, Is.Null);
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"fonipa"}));
		}

		[Test]
		public void IpaStatus_SetToPhonemicOnEntirelyPrivateUseWritingSystem_MarkerForUnlistedLanguageIsInserted()
		{
			var ws = new WritingSystemDefinition("x-private");
			Assert.That(ws.Language, Is.EqualTo((LanguageSubtag) "private"));
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.That(ws.Language, Is.EqualTo((LanguageSubtag) "private"));
			Assert.That(ws.Script, Is.Null);
			Assert.That(ws.Region, Is.Null);
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"fonipa", "emic"}));
		}

		[Test]
		public void IpaStatus_SetToIpaWhenVariantHasContents_FonIpaAtEnd()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske", "fonipa"}));
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenWasOnlyVariant_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("fonipa");
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.That(ws.Variants, Is.Empty);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantEmpty_NothingChanges()
		{
			var ws = new WritingSystemDefinition();
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.That(ws.Variants, Is.Empty);
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantNotEmpty_NothingChanges()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske"}));
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantHasContents_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.Variants.Add("fonipa");
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske"}));
		}

		[Test]
		public void IpaStatus_SetToNotIpaWhenVariantHasContentsWithIpaInMiddle_FonIpaRemoved()
		{
			var ws = new WritingSystemDefinition();
			//this is actually a bad tag as of 2009, fonipa can't be extended
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.Variants.Add("fonipa");
			ws.Variants.Add("bauddha");
			ws.IpaStatus = IpaStatusChoices.NotIpa;
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske", "bauddha"}));
		}

		[Test]
		public void IpaStatus_IpaPhonetic_RoundTrips()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.IpaPhonetic));
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske", "fonipa", "etic"}));
		}

		[Test]
		public void IpaStatus_IpaPhonemic_RoundTrips()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.IpaPhonemic));
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske", "fonipa", "emic"}));
		}

		[Test]
		public void IpaStatus_IpaPhoneticToPhonemic_MakesChange()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.IpaPhonemic));
			Assert.That(ws.Variants, Is.EqualTo(new VariantSubtag[] {"1901", "biske", "fonipa", "emic"}));
		}

		[Test]
		public void SetIpaStatus_SetIpaWasVoice_RemovesVoice()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("1901");
			ws.Variants.Add("biske");
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void IpaStatus_PrivateUseSetToPrefixEticPostfix_ReturnsIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("fonipa");
			ws.Variants.Add("PrefixEticPostfix");
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.Ipa));
		}

		[Test]
		public void IpaStatus_VariantSetTofonipaDashXDashetic_ReturnsIpaPhonetic()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("fonipa");
			ws.Variants.Add("etic");
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.IpaPhonetic));
		}

		[Test]
		public void IpaStatus_VariantSetTofonipaDashXDashemic_ReturnsIpaPhonemic()
		{
			var ws = new WritingSystemDefinition();
			ws.Variants.Add("fonipa");
			ws.Variants.Add("emic");
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.IpaPhonemic));
		}

		[Test]
		public void IpaStatus_SetToIpaWhileIsVoiceIsTrue_IpaStatusIsIpa()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.Ipa));
		}

		[Test]
		public void IpaStatus_SetToIpaWhileIsVoiceIsTrue_IsVoiceIsFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.Ipa;
			Assert.That(ws.IsVoice, Is.False);
		}

		[Test]
		public void IpaStatus_SetToPhoneticWhileIsVoiceIsTrue_IpaStatusIsPhonetic()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.IpaPhonetic));
		}

		[Test]
		public void IpaStatus_SetToPhoneticWhileIsVoiceIsTrue_IsVoiceIsFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonetic;
			Assert.That(ws.IsVoice, Is.False);
		}

		[Test]
		public void IpaStatus_SetToPhonemicWhileIsVoiceIsTrue_IpaStatusIsPhonemic()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.That(ws.IpaStatus, Is.EqualTo(IpaStatusChoices.IpaPhonemic));
		}

		[Test]
		public void IpaStatus_SetToPhonemicWhileIsVoiceIsTrue_IsVoiceIsFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.IsVoice = true;
			ws.IpaStatus = IpaStatusChoices.IpaPhonemic;
			Assert.That(ws.IsVoice, Is.False);
		}
	}
}