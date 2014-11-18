using NUnit.Framework;
using Palaso.UI.WindowsForms.ClearShare;

namespace PalasoUIWindowsForms.Tests.ClearShare
{
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class LicenseTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_OtherIsNull_ReturnsFalse()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsFalse(l.AreContentsEqual(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_AreDifferent_ReturnsFalse()
		{
			var l1 = License.CreativeCommons_Attribution_ShareAlike;
			var l2 = License.CreativeCommons_Attribution;
			Assert.IsFalse(l1.AreContentsEqual(l2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreContentsEqual_AreSame_ReturnsTrue()
		{
			var l1 = License.CreativeCommons_Attribution_ShareAlike;
			var l2 = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsTrue(l1.AreContentsEqual(l2));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_SameInstance_ReturnsTrue()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsTrue(l.Equals(l));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToNull_ReturnsFalse()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsFalse(l.Equals(null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_CompareToObjOfDifferentType_ReturnsFalse()
		{
			var l = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsFalse(l.Equals("junk"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void Equals_AreSame_ReturnsTrue()
		{
			var l1 = License.CreativeCommons_Attribution_ShareAlike;
			var l2 = License.CreativeCommons_Attribution_ShareAlike;
			Assert.IsTrue(l1.Equals(l2));
		}

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
