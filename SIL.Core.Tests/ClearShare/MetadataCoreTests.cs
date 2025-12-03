// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;
using SIL.Core.ClearShare;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using TagLib.Xmp;

namespace SIL.Tests.ClearShare
{
	[TestFixture]
	public class MetadataCoreTests
	{
		private Image<Rgba32> _mediaFile;
		private TempFile _tempFile;
		private MetadataCore _outgoing;

		[SetUp]
		public void Setup()
		{
			_mediaFile = new Image<Rgba32>(10, 10);
			_tempFile = TempFile.WithExtension("png");
			_mediaFile.Save(_tempFile.Path, new PngEncoder());
			_outgoing = MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path);
		}

		[TearDown]
		public void TearDown()
		{
			_tempFile.Dispose();
			_mediaFile.Dispose();
		}

		[Test]
		public void RoundTripPng_CopyrightNotice()
		{
			_outgoing.CopyrightNotice = "Copyright Test";
			_outgoing.Write();
			Assert.AreEqual("Copyright Test", MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).CopyrightNotice);
		}

		[Test]
		public void RoundTripPng_CopyrightNoticeWithNonAscii()
		{
			_outgoing.CopyrightNotice = "Copyright ŋoŋ";
			_outgoing.Write();
			Assert.AreEqual("Copyright ŋoŋ", MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).CopyrightNotice);
		}


		[Test]
		public void RoundTripPng_AttributionNameWithNonAscii()
		{
			_outgoing.Creator = "joŋ";
			_outgoing.Write();
			Assert.AreEqual("joŋ", MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).Creator);
		}

		[Test]
		public void RoundTripPng_PNGWithDangerousCharacters_PreservesCopyrightNotice()
		{
			_outgoing.CopyrightNotice = "Copyright <! ' <hello>";
			_outgoing.Write();
			Assert.AreEqual("Copyright <! ' <hello>", MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).CopyrightNotice);
		}

		[Test]
		public void RoundTripPng_CustomLicense_PreservesRightsStatement()
		{
			_outgoing.License = new CustomLicenseInfo() { RightsStatement = "Use this if you must." };
			_outgoing.Write();
			var license = (CustomLicenseInfo)MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).License;
			Assert.AreEqual(_outgoing.License.RightsStatement, license.RightsStatement);
		}

		[Test]
		public void RoundTripPng_HasCC_Permissive_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicenseInfo(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			_outgoing.Write();
			var cc = (CreativeCommonsLicenseInfo)MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, false);
			Assert.AreEqual(cc.CommercialUseAllowed, true);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
		}

		[Test]
		public void RoundTripPng_HasCC_Permissive_License_WithRights_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicenseInfo(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var cc = (CreativeCommonsLicenseInfo)MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).License;
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
			_outgoing.License = new CreativeCommonsLicenseInfo(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			_outgoing.CopyrightNotice = "Copyright © 2014 SIL";
			_outgoing.Creator = "JohnT";
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var incoming = MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path);
			var cc = (CreativeCommonsLicenseInfo)incoming.License;
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
			_outgoing.License = new CreativeCommonsLicenseInfo(false, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			_outgoing.CopyrightNotice = "Copyright © 2014 SIL";
			_outgoing.License.RightsStatement = "Please attribute nicely";
			_outgoing.Write();
			var intermediate = MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path);
			intermediate.License = new CreativeCommonsLicenseInfo(false, true,
				CreativeCommonsLicenseInfo.DerivativeRules.Derivatives); // no rights
			intermediate.Write();
			var incoming = MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path);
			var cc = (CreativeCommonsLicenseInfo)incoming.License;
			Assert.That(cc.AttributionRequired, Is.False);
			Assert.That(cc.CommercialUseAllowed, Is.True);
			Assert.That(cc.DerivativeRule, Is.EqualTo(CreativeCommonsLicenseInfo.DerivativeRules.Derivatives));
			Assert.That(cc.RightsStatement, Is.Null.Or.Empty);
			Assert.That(incoming.CopyrightNotice, Is.EqualTo("Copyright © 2014 SIL"));
		}

		[Test]
		public void RoundTripPng_HasCC_Strict_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicenseInfo(true, false, CreativeCommonsLicenseInfo.DerivativeRules.NoDerivatives);
			_outgoing.Write();
			var cc = (CreativeCommonsLicenseInfo)MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, true);
			Assert.AreEqual(cc.CommercialUseAllowed, false);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicenseInfo.DerivativeRules.NoDerivatives);
		}

		[Test]
		public void RoundTripPng_FileNameHasNonAsciiCharacters()
		{
			using (var mediaFile = new Image<Rgba32>(10, 10))
			{
				using (var folder = new TemporaryFolder("LibPalaso exiftool Test"))
				{
					var path = folder.Combine("Love these non-áscii chárácters.png");
					mediaFile.Save(path, new PngEncoder());
					var outgoing = MetadataCore.CreateMetadataCoreFromFile(path);

					outgoing.Creator = "joe shmo";
					outgoing.Write();
					Assert.AreEqual("joe shmo", MetadataCore.CreateMetadataCoreFromFile(path).Creator);
				}
			}
		}
		[Test]
		public void RoundTripPng_InPathWithNonAsciiCharacters()
		{
			using (var mediaFile = new Image<Rgba32>(10, 10))
			{
				using (var folder =
				       new TemporaryFolder(
					       "LibPalaso exiftool Test with non-áscii chárácters"))
				{
					var path = folder.Combine("test.png");
					mediaFile.Save(path, new PngEncoder());
					var outgoing = MetadataCore.CreateMetadataCoreFromFile(path);

					outgoing.Creator = "joe shmo";
					outgoing.Write();
					Assert.AreEqual("joe shmo", MetadataCore.CreateMetadataCoreFromFile(path).Creator);
				}
			}
		}


		[Test]
		public void RoundTripPng_HasCC_Medium_License_ReadsInSameLicense()
		{
			_outgoing.License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike);
			_outgoing.Write();
			var cc = (CreativeCommonsLicenseInfo)MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).License;
			Assert.AreEqual(cc.AttributionRequired, true);
			Assert.AreEqual(cc.CommercialUseAllowed, true);
			Assert.AreEqual(cc.DerivativeRule, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike);
		}
		[Test]
		public void RoundTripPng_AttributionUrl()
		{
			_outgoing.AttributionUrl = "http://somewhere.com";
			_outgoing.Write();
			Assert.AreEqual("http://somewhere.com", MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).AttributionUrl);
		}

		[Test]
		public void RoundTripPng_AttributionName()
		{
			_outgoing.Creator = "joe shmo";
			_outgoing.Write();
			Assert.AreEqual("joe shmo", MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).Creator);
		}


		[Test]
		public void SetLicense_HasChanges_True()
		{
			var m = new MetadataCore();
			m.HasChanges = false;
			m.License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			Assert.IsTrue(m.HasChanges);
		}

		[Test]
		public void ChangeLicenseObject_HasChanges_True()
		{
			var m = new MetadataCore();
			m.License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			m.HasChanges = false;
			m.License = new NullLicense();
			Assert.IsTrue(m.HasChanges);
		}


		[Test]
		public void ChangeLicenseDetails_HasChanges_True()
		{
			var m = new MetadataCore();
			m.License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			m.HasChanges = false;
			((CreativeCommonsLicenseInfo)m.License).CommercialUseAllowed = false;
			Assert.IsTrue(m.HasChanges);
		}


		[Test]
		public void SetHasChangesFalse_AlsoClearsLicenseHasChanges()
		{
			var m = new MetadataCore();
			m.License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			((CreativeCommonsLicenseInfo)m.License).CommercialUseAllowed = false;
			Assert.IsTrue(m.HasChanges);
			m.HasChanges = false;
			Assert.IsFalse(m.License.HasChanges);
			Assert.IsFalse(m.HasChanges);
		}

		[Test]
		public void LoadFromFile_CopyrightNotSet_CopyrightGivesNull()
		{
			Assert.IsNull(MetadataCore.CreateMetadataCoreFromFile(_tempFile.Path).Creator);
		}

		[Test]
		public void LoadXmpFile_ValuesCopiedFromOtherFile()
		{
			var original = new MetadataCore();
			var another = new MetadataCore();
			original.Creator = "John";
			original.License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives);
			using (var f = TempFile.WithExtension("xmp"))
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
			var m = new MetadataCore();
			m.License = new CreativeCommonsLicenseInfo(true, true,
												   CreativeCommonsLicenseInfo.DerivativeRules.
													   DerivativesWithShareAndShareAlike);
			MetadataCore copy = m.DeepCopy();
			Assert.AreEqual(m.License.Url, copy.License.Url);
		}

		[Test]
		public void GetCopyrightBy_HasSymbolAndComma_ReturnsCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "© 2012, SIL Global";
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
		}

		[Test]
		public void GetCopyrightBy_HasCopyrightAndSymbolAndComma_ReturnsCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "Copyright © 2012, SIL Global";
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
		}

		[Test]
		public void GetCopyrightBy_HasCopyrightAndSymbolNoYear_ReturnsCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "Copyright © SIL Global";
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_HasCOPYRIGHTAndSymbolNoYear_ReturnsCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "COPYRIGHT © SIL Global";
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_HasSymbolNoComma_ReturnsCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "© 2012 SIL Global";
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_Empty_ReturnsEmpty()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "";
			Assert.AreEqual("", m.GetCopyrightBy());
		}

		[Test]
		public void GetCopyrightBy_HasSymbolNoYear_ReturnsCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "© SIL Global";
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
		}


		[Test]
		public void GetCopyrightBy_NoSymbolOrYear_ReturnsCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "SIL Global";
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
		}

		[Test]
		public void GetCopyrightBy_HandlesMultilineCopyrightHolder()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "Copyright © 2018, text1" + Environment.NewLine + "text2";
			var cHolder = m.GetCopyrightBy();
			Assert.AreEqual("text1" + Environment.NewLine + "text2", cHolder, "Copyright holder should have 2 lines of text.");
		}

		[Test]
		public void GetCopyrightInfo_ArtOfReading_ReturnsCopyrightInfo()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "Copyright, SIL Global 2009. "; // (from AOR_Cat3.png)
			Assert.AreEqual("SIL Global", m.GetCopyrightBy());
			Assert.AreEqual("2009", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightInfo_Vaccinations_ReturnsCopyrightInfo()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "Copyright SIL Papua New Guinea 1996";
			Assert.AreEqual("SIL Papua New Guinea", m.GetCopyrightBy());
			Assert.AreEqual("1996", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightInfo_StoryPrimer_ReturnsCopyrightInfo()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "Copyright SIL Australia and the Literacy Association of the Solomon Islands (LASI) 2014";
			Assert.AreEqual("SIL Australia and the Literacy Association of the Solomon Islands (LASI)", m.GetCopyrightBy());
			Assert.AreEqual("2014", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightYear_HasCopyrightAndSymbolAndComma_ReturnsCopyrightYear()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "Copyright © 2012, SIL Global";
			Assert.AreEqual("2012", m.GetCopyrightYear());
		}


		[Test]
		public void GetCopyrightYear_HasSymbolAndComma_ReturnsCopyrightYear()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "© 2012, SIL Global";
			Assert.AreEqual("2012", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightYear_NoSymbolOrComma_ReturnsCopyrightYear()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "2012 SIL Global";
			Assert.AreEqual("2012", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightYear_SymbolButNoYear_ReturnsEmptyString()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "© SIL Global";
			Assert.AreEqual("", m.GetCopyrightYear());
		}


		[Test]
		public void GetCopyrightYear_NoYear_ReturnsEmptyString()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "SIL Global";
			Assert.AreEqual("", m.GetCopyrightYear());
		}

		[Test]
		public void GetCopyrightYear_Empty_ReturnsEmptyString()
		{
			var m = new MetadataCore();
			m.CopyrightNotice = "";
			Assert.AreEqual("", m.GetCopyrightYear());
		}

		[Test]
		public void MinimalCredits_CustomLicense()
		{
			var m = new MetadataCore
			{
				CopyrightNotice = "Copyright © 2014 SIL",
				Creator = "Jane Doe",
				CollectionName = "My Collection",
				License = new CustomLicenseInfo()
				{
					RightsStatement = "Please attribute nicely"
				}
			};

			string idOfLanguageUsedForLicense;
			Assert.AreEqual("Jane Doe, My Collection, © 2014 SIL. Please attribute nicely", m.MinimalCredits(new[] { "en" }, out idOfLanguageUsedForLicense));
		}
		[Test]
		public void MinimalCredits_OnlyCopyright()
		{
			var m = new MetadataCore { CopyrightNotice = "Copyright 2011 Foo Incorporated" };
			string idOfLanguageUsedForLicense;
			Assert.AreEqual("© 2011 Foo Incorporated", m.MinimalCredits(new[] { "en" }, out idOfLanguageUsedForLicense));
		}

		[Test]
		public void MinimalCredits_CreativeCommonsNoCreator()
		{
			var m = new MetadataCore
			{
				CopyrightNotice = "Copyright © 2014 SIL",
				CollectionName = "My Collection",
				License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike)
				{
					IntergovernmentalOrganizationQualifier = true
				}
			};

			string idOfLanguageUsedForLicense;
			Assert.AreEqual("My Collection, © 2014 SIL. CC BY-SA IGO 3.0", m.MinimalCredits(new[] { "en" }, out idOfLanguageUsedForLicense));
		}

		[Test]
		public void MinimalCredits_CreativeCommonsWithExtraRightsStatement()
		{
			var m = new MetadataCore
			{
				CopyrightNotice = "Copyright © 2014 SIL",
				License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.DerivativesWithShareAndShareAlike)
				{
					IntergovernmentalOrganizationQualifier = true,
					RightsStatement = "Only people named Fred can use this."
				}
			};

			string idOfLanguageUsedForLicense;
			Assert.AreEqual("© 2014 SIL. CC BY-SA IGO 3.0. Only people named Fred can use this.", m.MinimalCredits(new[] { "en" }, out idOfLanguageUsedForLicense));
		}

		[Test]
		public void ChangingFromCCLicense_WorksOkay()
		{
			// Important: use the same XmpTag object throughout this test.
			// We're testing that we can change the type of license in a tag, not just the specifics of the license.
			var tag = new XmpTag();

			var meta1 = new MetadataCore
			{
				CopyrightNotice = "Copyright © 2021 SIL",
				License = new CreativeCommonsLicenseInfo(true, true, CreativeCommonsLicenseInfo.DerivativeRules.Derivatives)
			};
			VerifyMetadataUnchangedSavingToTag(meta1, tag, "Verify CC license: ");

			var meta2 = new MetadataCore
			{
				CopyrightNotice = "Copyright © 2021 LSDev",
				License = new CustomLicenseInfo
				{
					RightsStatement = "You can use this only on alternate Tuesdays."
				}
			};
			VerifyMetadataUnchangedSavingToTag(meta2, tag, "Verify custom license: ");

			var meta3 = new MetadataCore
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

		private void VerifyMetadataUnchangedSavingToTag(MetadataCore oldMetadata, XmpTag tag, string header)
		{
			// XmpTag objects are wretched to work with, so load it into another Metadata object for testing.
			// This way we test both SaveInImageTag and LoadProperties for round-tripping.
			oldMetadata.SaveInImageTag(tag);
			var newMetadata = new MetadataCore();
			MetadataCore.LoadProperties(tag, newMetadata);
			Assert.AreEqual(oldMetadata.CopyrightNotice, newMetadata.CopyrightNotice, header + "CopyrightNotice");
			Assert.AreEqual(oldMetadata.License.GetType().FullName, newMetadata.License.GetType().FullName, header + "License class type");
			Assert.AreEqual(oldMetadata.License.Token, newMetadata.License.Token, header + "License.Token");
			Assert.AreEqual(oldMetadata.License.Url, newMetadata.License.Url, header + "License.Url");
			Assert.AreEqual(oldMetadata.License.RightsStatement, newMetadata.License.RightsStatement, header + "License.RightsStatement");
		}
	}
}
