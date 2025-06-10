using NUnit.Framework;
using SIL.Windows.Forms.ClearShare;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.Tests.ClearShare
{
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class LicenseInfoTests
	{
		[Test]
		public void FromToken_CreativeCommons_GiveExpectedAttributes()
		{
			CreativeCommonsLicense ccLicense = (CreativeCommonsLicense)LicenseInfo.FromToken("by");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.True);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.Derivatives));


			ccLicense = (CreativeCommonsLicense)LicenseInfo.FromToken("by-sa");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.True);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike));

			ccLicense = (CreativeCommonsLicense)LicenseInfo.FromToken("by-nd");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.True);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.NoDerivatives));


			ccLicense = (CreativeCommonsLicense)LicenseInfo.FromToken("by-nc");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.False);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.Derivatives));


			ccLicense = (CreativeCommonsLicense)LicenseInfo.FromToken("by-nc-sa");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.False);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike));

			ccLicense = (CreativeCommonsLicense)LicenseInfo.FromToken("by-nc-nd");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.False);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.NoDerivatives));
		}


		[Test]
		public void FromToken_Ask_GiveNullLicense()
		{
			Assert.That(LicenseInfo.FromToken("ask"), Is.InstanceOf<NullLicense>());
		}


		[Test]
		public void FromToken_Ask_GiveCustomLicense()
		{
			Assert.That(LicenseInfo.FromToken("custom"), Is.InstanceOf<CustomLicense>());
		}

		[Test]
		public void Token_GivenCustomLicense_IsCustom()
		{
			Assert.That(new CustomLicense().Token, Is.EqualTo("custom"));
		}
		[Test]
		public void Token_GivenNullLicense_IsAsk()
		{
			Assert.That(new NullLicense().Token, Is.EqualTo("ask"));
		}

		[Test]
		public void Token_GivenCreativeCommonsLicense()
		{
			Assert.That(new CreativeCommonsLicense(true,true,CreativeCommonsLicense.DerivativeRules.Derivatives).Token, Is.EqualTo("cc-by"));
			Assert.That(new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike).Token, Is.EqualTo("cc-by-sa"));
			Assert.That(new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.NoDerivatives).Token, Is.EqualTo("cc-by-nd"));
			Assert.That(new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.Derivatives).Token, Is.EqualTo("cc-by-nc"));
			Assert.That(new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike).Token, Is.EqualTo("cc-by-nc-sa"));
			Assert.That(new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.NoDerivatives).Token, Is.EqualTo("cc-by-nc-nd"));
		}

	}
}
