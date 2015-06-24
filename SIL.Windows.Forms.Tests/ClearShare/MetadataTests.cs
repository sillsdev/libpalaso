﻿using System.Drawing;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;
using SIL.Windows.Forms.ClearShare;

namespace SIL.Windows.Forms.Tests.ClearShare
{
	[TestFixture]
	public class MetadataTests
	{
		private Bitmap _mediaFile;
		private TempFile _tempFile;
		private Metadata _outgoing;

		[SetUp]
		public void Setup()
		{
			_mediaFile = new Bitmap(10, 10);
			_tempFile = TempFile.WithExtension("png");
			_mediaFile.Save(_tempFile.Path);
		   _outgoing = Metadata.FromFile(_tempFile.Path);
		 }

		[TearDown]
		public void TearDown()
		{
			_tempFile.Dispose();
		}

		[Test]
		public void RoundTripPng_CopyrightNotice()
		{
			_outgoing.CopyrightNotice = "Copyright Test";
			_outgoing.Write();
			Assert.AreEqual("Copyright Test", Metadata.FromFile(_tempFile.Path).CopyrightNotice);
		}

		[Test]
		public void RoundTripPng_CopyrightNoticeWithNonAscii()
		{
			_outgoing.CopyrightNotice = "Copyright ŋoŋ";
			_outgoing.Write();
			Assert.AreEqual("Copyright ŋoŋ", Metadata.FromFile(_tempFile.Path).CopyrightNotice);
		}


		[Test]
		public void RoundTripPng_AttributionNameWithNonAscii()
		{
			_outgoing.Creator = "joŋ";
			_outgoing.Write();
			Assert.AreEqual("joŋ", Metadata.FromFile(_tempFile.Path).Creator);
		}

		[Test]
		public void RoundTripPng_PNGWithDangerousCharacters_PreservesCopyrightNotice()
		{
			_outgoing.CopyrightNotice = "Copyright <! ' <hello>";
			_outgoing.Write();
			Assert.AreEqual("Copyright <! ' <hello>", Metadata.FromFile(_tempFile.Path).CopyrightNotice);
		}

		[Test]
		public void RoundTripPng_CustomLicense_PreservesRightsStatement()
		{
			_outgoing.License = new CustomLicense() {RightsStatement = "Use this if you must."};
			_outgoing.Write();
			var license = (CustomLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(_outgoing.License.RightsStatement, license.RightsStatement);
		}

		[Test]
		public void RoundTripPng_HasCC_Permissive_License_ReadsInSameLicense()
		{
			_outgoing.License =new CreativeCommonsLicense(false,true,CreativeCommonsLicense.DerivativeRules.Derivatives);
			_outgoing.Write();
			var cc = (CreativeCommonsLicense) Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, false);
			Assert.AreEqual(cc.CommercialUseAllowed, true);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicense.DerivativeRules.Derivatives);
		}

		[Test]
		public void RoundTripPng_HasCC_Permissive_License_WithRights_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicense(false, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var cc = (CreativeCommonsLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.That(cc.RightsStatement, Is.EqualTo("Please attribute nicely"));
		}

		/// <summary>
		/// There is a distinct possibility that copyright and License.RightsStatement will interfere, since
		/// they are stored in two alternatives of dc:rights (default and en), especially since taglib's default
		/// method of writing a language alternative writes default and removes all others. Check this is not happening.
		/// </summary>
		[Test]
		public void RoundTripPng_ManyProperties_RecoversAll()
		{
			_outgoing.License = new CreativeCommonsLicense(false, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			_outgoing.CopyrightNotice = "Copyright © 2014 SIL";
			_outgoing.Creator = "JohnT";
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var incoming = Metadata.FromFile(_tempFile.Path);
			var cc = (CreativeCommonsLicense)incoming.License;
			Assert.That(cc.AttributionRequired, Is.False);
			Assert.That(cc.CommercialUseAllowed, Is.True);
			Assert.That(cc.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.Derivatives));
			Assert.That(cc.RightsStatement, Is.EqualTo("Please attribute nicely"));
			Assert.That(incoming.CopyrightNotice, Is.EqualTo("Copyright © 2014 SIL"));
			Assert.That(incoming.Creator, Is.EqualTo("JohnT"));
		}

		[Test]
		public void CanRemoveRightsStatment()
		{
			_outgoing.License = new CreativeCommonsLicense(false, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			_outgoing.CopyrightNotice = "Copyright © 2014 SIL";
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var intermediate = Metadata.FromFile(_tempFile.Path);
			intermediate.License = new CreativeCommonsLicense(false, true,
				CreativeCommonsLicense.DerivativeRules.Derivatives); // no rights
			intermediate.Write();
			var incoming = Metadata.FromFile(_tempFile.Path);
			var cc = (CreativeCommonsLicense)incoming.License;
			Assert.That(cc.AttributionRequired, Is.False);
			Assert.That(cc.CommercialUseAllowed, Is.True);
			Assert.That(cc.DerivativeRule, Is.EqualTo(CreativeCommonsLicense.DerivativeRules.Derivatives));
			Assert.That(cc.RightsStatement, Is.Null.Or.Empty);
			Assert.That(incoming.CopyrightNotice, Is.EqualTo("Copyright © 2014 SIL"));
		}

		[Test]
		public void RoundTripPng_HasCC_Strict_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicense(true, false, CreativeCommonsLicense.DerivativeRules.NoDerivatives);
			_outgoing.Write();
			var cc = (CreativeCommonsLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, true);
			Assert.AreEqual(cc.CommercialUseAllowed, false);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicense.DerivativeRules.NoDerivatives);
		}

        [Test]
        public void RoundTripPng_FileNameHasNonAsciiCharacters()
        {
            var mediaFile = new Bitmap(10, 10);
            using (var folder = new TemporaryFolder("LibPalaso exiftool Test"))
            {
                var path = folder.Combine("Love these non-áscii chárácters.png");
                mediaFile.Save(path);
                var outgoing = Metadata.FromFile(path);

                outgoing.Creator = "joe shmo";
                outgoing.Write();
                Assert.AreEqual("joe shmo", Metadata.FromFile(path).Creator);
            }
        }
        [Test]
        public void RoundTripPng_InPathWithNonAsciiCharacters()
        {
            var mediaFile = new Bitmap(10, 10);
            using (var folder = new TemporaryFolder("LibPalaso exiftool Test with non-áscii chárácters"))
            {
                var path = folder.Combine("test.png");
                mediaFile.Save(path);
                var outgoing = Metadata.FromFile(path);

                outgoing.Creator = "joe shmo";
                outgoing.Write();
                Assert.AreEqual("joe shmo", Metadata.FromFile(path).Creator);
            }
        }


		[Test]
		public void RoundTripPng_HasCC_Medium_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
			_outgoing.Write();
			var cc = (CreativeCommonsLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, true);
			Assert.AreEqual(cc.CommercialUseAllowed, true);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike);
		}
		[Test]
		public void RoundTripPng_AttributionUrl()
		{
			_outgoing.AttributionUrl = "http://somewhere.com";
			_outgoing.Write();
			Assert.AreEqual("http://somewhere.com", Metadata.FromFile(_tempFile.Path).AttributionUrl);
		}

		[Test]
		public void RoundTripPng_AttributionName()
		{
			_outgoing.Creator = "joe shmo";
			_outgoing.Write();
			Assert.AreEqual("joe shmo", Metadata.FromFile(_tempFile.Path).Creator);
		}


		[Test]
		public void SetLicense_HasChanges_True()
		{
			var m = new Metadata();
			m.HasChanges = false;
			m.License=new CreativeCommonsLicense(true,true,CreativeCommonsLicense.DerivativeRules.Derivatives);
			Assert.IsTrue(m.HasChanges);
		}

		[Test]
		public void ChangeLicenseObject_HasChanges_True()
		{
			var m = new Metadata();
			m.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			m.HasChanges = false;
			m.License = new NullLicense();
			Assert.IsTrue(m.HasChanges);
		}


		[Test]
		public void ChangeLicenseDetails_HasChanges_True()
		{
			var m = new Metadata();
			m.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			m.HasChanges = false;
			((CreativeCommonsLicense) m.License).CommercialUseAllowed = false;
			Assert.IsTrue(m.HasChanges);
		}


		[Test]
		public void SetHasChangesFalse_AlsoClearsLicenseHasChanges()
		{
			var m = new Metadata();
			m.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			 ((CreativeCommonsLicense)m.License).CommercialUseAllowed = false;
			 Assert.IsTrue(m.HasChanges);
			 m.HasChanges = false;
			 Assert.IsFalse(m.License.HasChanges);
			 Assert.IsFalse(m.HasChanges);
		}

		[Test]
		public void LoadFromFile_CopyrightNotSet_CopyrightGivesNull()
		{
			Assert.IsNull(Metadata.FromFile(_tempFile.Path).Creator);
		}

		[Test]
		public void LoadXmpFile_ValuesCopiedFromOtherFile()
		{
			var original = new Metadata();
			var another = new Metadata();
			original.Creator = "John";
			original.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives);
			using(var f = TempFile.WithExtension("xmp"))
			{
				original.SaveXmpFile(f.Path);
				another.LoadXmpFile(f.Path);
			}
			Assert.AreEqual("John", another.Creator);
			Assert.AreEqual(original.License.Url, another.License.Url);
		}


		[Test]
		public void DeepCopy()
		{
			var m = new Metadata();
			m.License = new CreativeCommonsLicense(true, true,
												   CreativeCommonsLicense.DerivativeRules.
													   DerivativesWithShareAndShareAlike);
			Metadata copy = m.DeepCopy();
			Assert.AreEqual(m.License.Url,copy.License.Url);
		}

		[Test]
		public void GetCopyrightBy_HasSymbolAndComma_ReturnsCopyrightHolder()
		{
			var m = new Metadata();
			m.CopyrightNotice = "© 2012, SIL International";
			Assert.AreEqual("SIL International", m.GetCopyrightBy());
		}

		[Test]
		public void GetCopyrightBy_HasCopyrightAndSymbolAndComma_ReturnsCopyrightHolder()
		{
			var m = new Metadata();
			m.CopyrightNotice = "Copyright © 2012, SIL International";
			Assert.AreEqual("SIL International", m.GetCopyrightBy());
		}

		[Test]
		public void GetCopyrightBy_HasCopyrightAndSymbolNoYear_ReturnsCopyrightHolder()
		{
			var m = new Metadata();
			m.CopyrightNotice = "Copyright © SIL International";
			Assert.AreEqual("SIL International", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_HasCOPYRIGHTAndSymbolNoYear_ReturnsCopyrightHolder()
		{
			var m = new Metadata();
			m.CopyrightNotice = "COPYRIGHT © SIL International";
			Assert.AreEqual("SIL International", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_HasSymbolNoComma_ReturnsCopyrightHolder()
		{
			var m = new Metadata();
			m.CopyrightNotice = "© 2012 SIL International";
			Assert.AreEqual("SIL International", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_Empty_ReturnsEmpty()
		{
			var m = new Metadata();
			m.CopyrightNotice = "";
			Assert.AreEqual("", m.GetCopyrightBy());
		}

		[Test]
		public void GetCopyrightBy_HasSymbolNoYear_ReturnsCopyrightHolder()
		{
			var m = new Metadata();
			m.CopyrightNotice = "© SIL International";
			Assert.AreEqual("SIL International", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_NoSymbolOrYear_ReturnsCopyrightHolder()
		{
			var m = new Metadata();
			m.CopyrightNotice = "SIL International";
			Assert.AreEqual("SIL International", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightYear_HasCopyrightAndSymbolAndComma_ReturnsCopyrightYear()
		{
			var m = new Metadata();
			m.CopyrightNotice = "Copyright © 2012, SIL International";
			Assert.AreEqual("2012", m.GetCopyrightYear());
		}


		[Test]
		public void GetCopyrightYear_HasSymbolAndComma_ReturnsCopyrightYear()
		{
			var m = new Metadata();
			m.CopyrightNotice = "© 2012, SIL International";
			Assert.AreEqual("2012", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightYear_NoSymbolOrComma_ReturnsCopyrightYear()
		{
			var m = new Metadata();
			m.CopyrightNotice = "2012 SIL International";
			Assert.AreEqual("2012", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightYear_SymbolButNoYear_ReturnsEmptyString()
		{
			var m = new Metadata();
			m.CopyrightNotice = "© SIL International";
			Assert.AreEqual("", m.GetCopyrightYear());
		}


		[Test]
		public void GetCopyrightYear_NoYear_ReturnsEmptyString()
		{
			var m = new Metadata();
			m.CopyrightNotice = "SIL International";
			Assert.AreEqual("", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightYear_Empty_ReturnsEmptyString()
		{
			var m = new Metadata();
			m.CopyrightNotice = "";
			Assert.AreEqual("", m.GetCopyrightYear());
		}

	}
}
