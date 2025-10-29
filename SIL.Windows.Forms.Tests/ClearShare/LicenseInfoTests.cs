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
			CreativeCommonsLicenseWithImage ccLicense = (CreativeCommonsLicenseWithImage)LicenseWithLogo.FromToken("by");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.True);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseWithImage.DerivativeRules.Derivatives));


			ccLicense = (CreativeCommonsLicenseWithImage)LicenseWithLogo.FromToken("by-sa");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.True);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike));

			ccLicense = (CreativeCommonsLicenseWithImage)LicenseWithLogo.FromToken("by-nd");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.True);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseWithImage.DerivativeRules.NoDerivatives));


			ccLicense = (CreativeCommonsLicenseWithImage)LicenseWithLogo.FromToken("by-nc");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.False);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseWithImage.DerivativeRules.Derivatives));


			ccLicense = (CreativeCommonsLicenseWithImage)LicenseWithLogo.FromToken("by-nc-sa");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.False);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike));

			ccLicense = (CreativeCommonsLicenseWithImage)LicenseWithLogo.FromToken("by-nc-nd");
			Assert.That(ccLicense.AttributionRequired, Is.True);
			Assert.That(ccLicense.CommercialUseAllowed, Is.False);
			Assert.That(ccLicense.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseWithImage.DerivativeRules.NoDerivatives));
		}


		[Test]
		public void FromToken_Ask_GiveNullLicense()
		{
			Assert.That(LicenseWithLogo.FromToken("ask"), Is.InstanceOf<NullLicense>());
		}


		[Test]
		public void FromToken_Ask_GiveCustomLicense()
		{
			Assert.That(LicenseWithLogo.FromToken("custom"), Is.InstanceOf<CustomLicenseWithImage>());
		}

		[Test]
		public void Token_GivenCustomLicense_IsCustom()
		{
			Assert.That(new CustomLicenseWithImage().Token, Is.EqualTo("custom"));
		}
		[Test]
		public void Token_GivenNullLicense_IsAsk()
		{
			Assert.That(new NullLicense().Token, Is.EqualTo("ask"));
		}

		[Test]
		public void Token_GivenCreativeCommonsLicense()
		{
			Assert.That(new CreativeCommonsLicenseWithImage(true,true,CreativeCommonsLicenseWithImage.DerivativeRules.Derivatives).Token, Is.EqualTo("cc-by"));
			Assert.That(new CreativeCommonsLicenseWithImage(true, true, CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike).Token, Is.EqualTo("cc-by-sa"));
			Assert.That(new CreativeCommonsLicenseWithImage(true, true, CreativeCommonsLicenseWithImage.DerivativeRules.NoDerivatives).Token, Is.EqualTo("cc-by-nd"));
			Assert.That(new CreativeCommonsLicenseWithImage(true, false, CreativeCommonsLicenseWithImage.DerivativeRules.Derivatives).Token, Is.EqualTo("cc-by-nc"));
			Assert.That(new CreativeCommonsLicenseWithImage(true, false, CreativeCommonsLicenseWithImage.DerivativeRules.DerivativesWithShareAndShareAlike).Token, Is.EqualTo("cc-by-nc-sa"));
			Assert.That(new CreativeCommonsLicenseWithImage(true, false, CreativeCommonsLicenseWithImage.DerivativeRules.NoDerivatives).Token, Is.EqualTo("cc-by-nc-nd"));
		}

	}
}
