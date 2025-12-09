using System;
using System.Globalization;
using System.Threading;
using L10NSharp;
using NUnit.Framework;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.i18n;

namespace SIL.Windows.Forms.Tests.ClearShare
{
	[TestFixture]
	public class CreativeCommonsLicenseTests
	{
		[SetUp]
		public void Setup()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("sv-SE"); //sweden, which uses commas for decimal  point (regression test)
			LocalizationManager.StrictInitializationMode = false;
			Localizer.Default = new L10NSharpLocalizer();
		}

		[Test]
		public void RoundTrip_BY()
		{
			var original =new CreativeCommonsLicense(true,true,CreativeCommonsLicense.DerivativeRules.Derivatives);
			var copy = CreativeCommonsLicense.FromLicenseUrl(original.Url);
			Assert.AreEqual(copy.AttributionRequired, true);
			Assert.AreEqual(copy.CommercialUseAllowed, true);
			Assert.AreEqual(copy.DerivativeRule, CreativeCommonsLicense.DerivativeRules.Derivatives);
		}

		[Test]
		public void RoundTrip_BY_NC_ND()
		{
			var original = new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.NoDerivatives);
			var copy = CreativeCommonsLicense.FromLicenseUrl(original.Url);
			Assert.AreEqual(copy.AttributionRequired, true);
			Assert.AreEqual(copy.CommercialUseAllowed, false);
			Assert.AreEqual(copy.DerivativeRule, CreativeCommonsLicense.DerivativeRules.NoDerivatives);
		}

		[Test]
		public void RoundTrip_BY_SA()
		{
			var original = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			var copy = CreativeCommonsLicense.FromLicenseUrl(original.Url);
			Assert.AreEqual(copy.AttributionRequired, true);
			Assert.AreEqual(copy.CommercialUseAllowed, true);
			Assert.AreEqual(copy.DerivativeRule, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
		}

		[Test]
		public void RoundTrip_BY_IGO()
		{
			var original = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			original.IntergovernmentalOrganizationQualifier = true;
			var copy = CreativeCommonsLicense.FromLicenseUrl(original.Url);
			Assert.IsTrue(copy.IntergovernmentalOrganizationQualifier);
		}

		[Test]
		public void RoundTrip_CC0()
		{
			// CC0 is essentially just "do what you want". So we don't have a particular property for it, we
			// just set all three aspects to be permissive.
			var original = new CreativeCommonsLicense(false, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			var copy = CreativeCommonsLicense.FromLicenseUrl(original.Url);
			Assert.IsFalse(copy.AttributionRequired);
			Assert.IsTrue(copy.CommercialUseAllowed);
			Assert.AreEqual(copy.DerivativeRule, CreativeCommonsLicense.DerivativeRules.Derivatives);
		}

		[Test]
		public void Url_CC0_Correct()
		{
			// CC0 is essentially just "do what you want". So we don't have a particular property for it, we
			// just set all three aspects to be permissive.
			var original = new CreativeCommonsLicense(false, true, CreativeCommonsLicense.DerivativeRules.Derivatives, "1.0");
			//notice that the url for cc0 does not follow the conventions of the other licenses.
			Assert.AreEqual(CreativeCommonsLicense.CC0Url, original.Url);
		}

		[Test]
		public void Url_QualifierIsIGO_UrlHasIgo()
		{
			var original = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			original.Version = "3.0";
			original.IntergovernmentalOrganizationQualifier = true;
			Assert.AreEqual("http://creativecommons.org/licenses/by/3.0/igo/", original.Url);
		}

		[Test]
		public void FromLicenseUrl_VersionRead()
		{
			var original = CreativeCommonsLicense.FromLicenseUrl("http://creativecommons.org/licenses/by-nd/4.3/");
			Assert.AreEqual("4.3", original.Version);

		}
		[Test]
		public void FromLicenseUrl_IGO_VersionRead()
		{
			var original = CreativeCommonsLicense.FromLicenseUrl("http://creativecommons.org/licenses/by/3.0/IGO");
			Assert.AreEqual("3.0", original.Version);
		}
		[Test]
		public void FromLicenseUrl_IGO_IGORead()
		{
			var license = CreativeCommonsLicense.FromLicenseUrl("http://creativecommons.org/licenses/by/3.0/");
			Assert.IsFalse(license.IntergovernmentalOrganizationQualifier);
			license = CreativeCommonsLicense.FromLicenseUrl("http://creativecommons.org/licenses/by/3.0/IGO");
			Assert.IsTrue(license.IntergovernmentalOrganizationQualifier);
		}

		[Test]
		public void FromLicenseUrl_EmptyString_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(()=>CreativeCommonsLicense.FromLicenseUrl(""));
		}

		[Test]
		public void Url_GivesVersion()
		{
			var original = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			original.Version = "2.2";
			Assert.AreEqual("http://creativecommons.org/licenses/by/2.2/", original.Url);
		}


		[Test]
		public void ChangeVersion_HasChanges_True()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			l.HasChanges = false;
			l.Version = "3.23";
			Assert.IsTrue(l.HasChanges);
		}
		[Test]
		public void ChangeIGO_HasChanges_True()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives)
			{
				HasChanges = false,
				IntergovernmentalOrganizationQualifier = true
			};
			Assert.IsTrue(l.HasChanges);
		}

		[Test]
		public void SetIGO_VersionNumerIsAppropriate()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives)
			{
				IntergovernmentalOrganizationQualifier = true
			};
			Assert.AreEqual("3.0", l.Version, "The igo version of CC did not have a version beyond 3.0 as of Nov 2016");
		}

		[Test]
		public void UnSetIGO_VersionNumberIsHighestDefault()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives)
			{
				IntergovernmentalOrganizationQualifier = true,
				Version = "3.0"
			};
			// SUT
			l.IntergovernmentalOrganizationQualifier = false;
			Assert.AreEqual(CreativeCommonsLicense.kDefaultVersion, l.Version,
				"Setting igo to false when it was true should change version to the current default version.");
		}

		[Test]
		public void UnSetIGO_IGOAlreadyUnset_DoesntAffectPreviousVersionNumber()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives)
			{
				IntergovernmentalOrganizationQualifier = false,
				Version = "3.0"
			};
			// SUT
			l.IntergovernmentalOrganizationQualifier = false;
			Assert.AreEqual("3.0", l.Version,
				"Setting igo to false when it was already false should not change an older version.");
		}

		[Test]
		public void ChangeAttributionRequired_HasChanges_True()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			l.HasChanges = false;
			l.AttributionRequired = !l.AttributionRequired;
			Assert.IsTrue(l.HasChanges);
		}

		[Test]
		public void ChangeCommercialUseAllowed_HasChanges_True()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			l.HasChanges = false;
			l.CommercialUseAllowed = !l.CommercialUseAllowed;
			Assert.IsTrue(l.HasChanges);
		}

		[Test]
		public void ChangeDerivativeRule_HasChanges_True()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			l.HasChanges = false;
			l.DerivativeRule = CreativeCommonsLicense.DerivativeRules.NoDerivatives;
			Assert.IsTrue(l.HasChanges);
		}
		[Test]
		public void HasChanges_CanToggle()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			l.HasChanges = false;
			Assert.IsFalse(l.HasChanges);
			l.HasChanges = true;
			Assert.IsTrue(l.HasChanges);
		}
		[Test]
		public void GetDescription_NoTranslation_GivesEnglish()
		{
			var l = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			string languageUsed;
			Assert.AreEqual(l.GetDescription(new[] { "en" }, out languageUsed), l.GetDescription(new[]{"qx", "en"}, out languageUsed));
			Assert.AreEqual("en",languageUsed);
		}
	}
}
