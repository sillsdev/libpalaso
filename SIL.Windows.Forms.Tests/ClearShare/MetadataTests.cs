// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;
using SIL.Core.ClearShare;
using SIL.Windows.Forms.ClearShare;
using TagLib.Xmp;

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
			_outgoing.License = new CustomLicense() { RightsStatement = "Use this if you must." };
			_outgoing.Write();
			var license = (CustomLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(_outgoing.License.RightsStatement, license.RightsStatement);
		}

		[Test]
		public void RoundTripPng_CustomLicense_ReadsInSameObjectType()
		{
			_outgoing.License = new CustomLicense() { RightsStatement = "Use this if you must." };
			_outgoing.Write();
			Assert.AreEqual(_outgoing.License.GetType(), Metadata.FromFile(_tempFile.Path).License.GetType());
		}

		[Test]
		public void RoundTripPng_HasCC_Permissive_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicense(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			_outgoing.Write();
			var cc = (CreativeCommonsLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, false);
			Assert.AreEqual(cc.CommercialUseAllowed, true);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
		}

		[Test]
		public void RoundTripPng_HasCC_Permissive_License_WithRights_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicense(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
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
			_outgoing.License = new CreativeCommonsLicense(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			_outgoing.CopyrightNotice = "Copyright © 2014 SIL";
			_outgoing.Creator = "JohnT";
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var incoming = Metadata.FromFile(_tempFile.Path);
			var cc = (CreativeCommonsLicense)incoming.License;
			Assert.That(cc.AttributionRequired, Is.False);
			Assert.That(cc.CommercialUseAllowed, Is.True);
			Assert.That(cc.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseInfo.DerivativeRules.Derivatives));
			Assert.That(cc.RightsStatement, Is.EqualTo("Please attribute nicely"));
			Assert.That(incoming.CopyrightNotice, Is.EqualTo("Copyright © 2014 SIL"));
			Assert.That(incoming.Creator, Is.EqualTo("JohnT"));
		}

		[Test]
		public void CanRemoveRightsStatment()
		{
			_outgoing.License = new CreativeCommonsLicense(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			_outgoing.CopyrightNotice = "Copyright © 2014 SIL";
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var intermediate = Metadata.FromFile(_tempFile.Path);
			intermediate.License = new CreativeCommonsLicense(false, true,
				CreativeCommonsLicenseInfo.DerivativeRules.Derivatives); // no rights
			intermediate.Write();
			var incoming = Metadata.FromFile(_tempFile.Path);
			var cc = (CreativeCommonsLicense)incoming.License;
			Assert.That(cc.AttributionRequired, Is.False);
			Assert.That(cc.CommercialUseAllowed, Is.True);
			Assert.That(cc.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseInfo.DerivativeRules.Derivatives));
			Assert.That(cc.RightsStatement, Is.Null.Or.Empty);
			Assert.That(incoming.CopyrightNotice, Is.EqualTo("Copyright © 2014 SIL"));
		}

		[Test]
		public void RoundTripPng_HasCC_Strict_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicense(true, false, CreativeCommonsLicenseInfo.DerivativeRules.NoDerivatives);
			_outgoing.Write();
			var cc = (CreativeCommonsLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, true);
			Assert.AreEqual(cc.CommercialUseAllowed, false);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicenseInfo.DerivativeRules.NoDerivatives);
		}

		[Test]
		public void RoundTripPng_FileNameHasNonAsciiCharacters()
		{
			using (var mediaFile = new Bitmap(10, 10))
			{
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
		}

		[Test]
		public void RoundTripPng_InPathWithNonAsciiCharacters()
		{
			using (var mediaFile = new Bitmap(10, 10))
			{
				using (var folder =
				       new TemporaryFolder(
					       "LibPalaso exiftool Test with non-áscii chárácters"))
				{
					var path = folder.Combine("test.png");
					mediaFile.Save(path);
					var outgoing = Metadata.FromFile(path);

					outgoing.Creator = "joe shmo";
					outgoing.Write();
					Assert.AreEqual("joe shmo", Metadata.FromFile(path).Creator);
				}
			}
		}

		[Test]
		public void RoundTripPng_HasCC_Medium_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike);
			_outgoing.Write();
			var cc = (CreativeCommonsLicense)Metadata.FromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, true);
			Assert.AreEqual(cc.CommercialUseAllowed, true);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike);
			Assert.AreEqual(typeof(CreativeCommonsLicense), Metadata.FromFile(_tempFile.Path).License.GetType());
		}

		[Test]
		public void RoundTripPng_HasCC_ReadsInSameObjectType()
		{
			_outgoing.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike);
			_outgoing.Write();
			Assert.AreEqual(_outgoing.License.GetType(), Metadata.FromFile(_tempFile.Path).License.GetType());
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
			original.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			using (var f = TempFile.WithExtension("xmp"))
			{
				original.SaveXmpFile(f.Path);
				another.LoadXmpFile(f.Path);
			}
			Assert.AreEqual("John", another.Creator);
			Assert.AreEqual(original.License.Url, another.License.Url);
			Assert.AreEqual(original.License.GetType(), another.License.GetType());
		}

		[Test]
		public void LoadXmpFile_LoadsCorrectLicenseType()
		{
			MetadataCore originalMetadata = new Metadata();
			MetadataCore loadedMetadata = new Metadata();
			originalMetadata.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			using (var f = TempFile.WithExtension("xmp"))
			{
				originalMetadata.SaveXmpFile(f.Path);
				loadedMetadata.LoadXmpFile(f.Path);
			}
			// Ensure that the loaded License type is CreativeCommonsLicense and not CreativeCommonsLicenseInfo
			Assert.AreEqual(originalMetadata.License.GetType(), loadedMetadata.License.GetType());
		}

		[Test]
		public void DeepCopy()
		{
			var m = new Metadata();
			m.License = new CreativeCommonsLicense(true, true, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike);
			Metadata copy = m.DeepCopy();
			Assert.AreEqual(m.License.Url, copy.License.Url);
		}

		[Test]
		public void ChangingFromCCLicense_WorksOkay()
		{
			// Important: use the same XmpTag object throughout this test.
			// We're testing that we can change the type of license in a tag, not just the specifics of the license.
			var tag = new XmpTag();

			var meta1 = new Metadata
			{
				CopyrightNotice = "Copyright © 2021 SIL",
				License = new CreativeCommonsLicense(true, true, CreativeCommonsLicense.DerivativeRules.Derivatives)
			};
			VerifyMetadataUnchangedSavingToTag(meta1, tag, "Verify CC license: ");

			var meta2 = new Metadata
			{
				CopyrightNotice = "Copyright © 2021 LSDev",
				License = new CustomLicense
				{
					RightsStatement = "You can use this only on alternate Tuesdays."
				}
			};
			VerifyMetadataUnchangedSavingToTag(meta2, tag, "Verify custom license: ");

			var meta3 = new Metadata
			{
				CopyrightNotice = "Copyright © 2021 Steve",
				License = new NullLicense()
			};
			VerifyMetadataUnchangedSavingToTag(meta3, tag, "Verify null license: ");

			// Go through the license changes one more time in a different order.
			VerifyMetadataUnchangedSavingToTag(meta2, tag, "Verify custom license again: ");
			VerifyMetadataUnchangedSavingToTag(meta1, tag, "Verify CC license again: ");
			VerifyMetadataUnchangedSavingToTag(meta3, tag, "Verify null license again: ");
		}

		private void VerifyMetadataUnchangedSavingToTag(Metadata oldMetadata, XmpTag tag, string header)
		{
			// XmpTag objects are wretched to work with, so load it into another Metadata object for testing.
			// This way we test both SaveInImageTag and LoadProperties for round-tripping.
			oldMetadata.SaveInImageTag(tag);
			var newMetadata = new Metadata();
			newMetadata.LoadProperties(tag);
			Assert.AreEqual(oldMetadata.CopyrightNotice, newMetadata.CopyrightNotice, header + "CopyrightNotice");
			Assert.AreEqual(oldMetadata.License.GetType().FullName, newMetadata.License.GetType().FullName, header + "License class type");
			Assert.AreEqual(oldMetadata.License.Token, newMetadata.License.Token, header + "License.Token");
			Assert.AreEqual(oldMetadata.License.Url, newMetadata.License.Url, header + "License.Url");
			Assert.AreEqual(oldMetadata.License.RightsStatement, newMetadata.License.RightsStatement, header + "License.RightsStatement");
			Assert.AreEqual(oldMetadata.License.GetType(), newMetadata.License.GetType());
		}
	}
}