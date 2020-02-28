using System;
using System.IO;
using Ionic.Zip;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.WritingSystems;

namespace SIL.DblBundle.Tests.Text
{
	/// <summary>
	/// Tests methods in the TextBundle class.
	/// </summary>
	[TestFixture]
	public class TextBundleTests
	{
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> _legacyBundle;
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> _legacyBundleWithoutLdml;
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> _bundle;
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> _bundleWithoutLdml;

		/// <summary>
		/// Setup test fixture.
		/// </summary>
		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			using (var zippedBundle = CreateZippedTextBundleFromResources(true, false, true))
				_legacyBundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(false, false, true))
				_legacyBundleWithoutLdml = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources())
				_bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(false))
				_bundleWithoutLdml = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
		}

		/// <summary>
		/// Tear down test fixture.
		/// </summary>
		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			_legacyBundle.Dispose();
			_legacyBundleWithoutLdml.Dispose();
			_bundle.Dispose();
			_bundleWithoutLdml.Dispose();
		}

		/// <summary>
		/// Tests that properties are correctly created in a bundle.
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void CreateBundle_PropertiesReadCorrectly(bool legacy)
		{
			var bundle = legacy ? _legacyBundle : _bundle;
			Assert.AreEqual("7881095a69332502", bundle.Id);
			Assert.AreEqual("Ri utzilaj tzij re ri kanimajawal Jesucristo", bundle.Name);
			Assert.AreEqual("acr", bundle.LanguageIso);
		}

		/// <summary>
		/// Tests that the stylesheet is loaded correctly from a bundle.
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void CreateBundle_ProperlyLoadsStylesheet(bool legacy)
		{
			var bundle = legacy ? _legacyBundle : _bundle;
			var stylesheet = bundle.Stylesheet;
			IStyle style = stylesheet.GetStyle("mt1");
			Assert.IsNotNull(style);
			Assert.AreEqual("Cambria", stylesheet.FontFamily);
			Assert.AreEqual(14, stylesheet.FontSizeInPoints);
		}

		/// <summary>
		/// Tests that writing system for a bundle is created correctly.
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void CreateBundle_ProperlyLoadsWritingSystemDefinition(bool legacy)
		{
			var bundle = legacy ? _legacyBundle : _bundle;
			var ws = bundle.WritingSystemDefinition;
			Assert.IsNotNull(ws);

			Assert.AreEqual(WellKnownSubtags.UnlistedLanguage, ws.LanguageTag);

			// Quotation Marks
			Assert.AreEqual(4, ws.QuotationMarks.Count);
			Assert.AreEqual("“", ws.QuotationMarks[0].Open);
			Assert.AreEqual("”", ws.QuotationMarks[0].Close);
			Assert.AreEqual("―", ws.QuotationMarks[3].Open);
			Assert.AreEqual(QuotationMarkingSystemType.Narrative, ws.QuotationMarks[3].Type);

			Assert.AreEqual("Cambria", ws.DefaultFont.Name);
			Assert.IsFalse(ws.RightToLeftScript);
		}

		/// <summary>
		/// Tests that a book is obtained from a bundle.
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void TryGetBook(bool legacy)
		{
			var bundle = legacy ? _legacyBundle : _bundle;
			UsxDocument book;
			Assert.IsTrue(bundle.TryGetBook("MAT", out book));
			Assert.AreEqual("MAT", book.BookId);
		}

		/// <summary>
		/// Tests that a bundle with an LDML file correctly reports that.
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void ContainsLdmlFile_FileExists_ReturnsTrue(bool legacy)
		{
			var bundle = legacy ? _legacyBundle : _bundle;
			Assert.IsTrue(bundle.ContainsLdmlFile());
		}

		/// <summary>
		/// Tests that a bundle without an LDML file correctly reports that.
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void ContainsLdmlFile_FileDoesNotExist_ReturnsFalse(bool legacy)
		{
			var bundleWithoutLdml = legacy ? _legacyBundleWithoutLdml : _bundleWithoutLdml;
			Assert.IsFalse(bundleWithoutLdml.ContainsLdmlFile());
		}

		/// <summary>
		/// Tests that a bundle with an invalid USX path throws an exception.
		/// </summary>
		[Test]
		public void ContainsInvalidUsxPath_ThrowsApplicationException()
		{
			Assert.Throws<ApplicationException>(() =>
			{
				using (var zippedBundle = CreateZippedTextBundleFromResources(false, true))
				{
					// ReSharper disable once ObjectCreationAsStatement
					new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
				}
			});
		}

		/// <summary>
		/// Helper method for tests to create a zipped text bundle.
		/// </summary>
		public static TempFile CreateZippedTextBundleFromResources(bool includeLdml = true, bool invalidUsxDirectory = false, bool legacyFormat = false)
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);

			using (var englishLds = TempFile.WithFilename("English.lds"))
			using (var metadataXml = TempFile.WithFilename("metadata.xml"))
			using (var stylesXml = TempFile.WithFilename("styles.xml"))
			using (var versificationVrs = TempFile.WithFilename(DblBundleFileUtils.kVersificationFileName))
			using (var ldmlXml = TempFile.WithFilename(DblBundleFileUtils.kLegacyLdmlFileName))
			using (var matUsx = TempFile.WithFilename("MAT.usx"))
			using (var zip = new ZipFile())
			{
				File.WriteAllText(metadataXml.Path, legacyFormat ? Resources.metadata_xml : Resources.metadataVersion2_1_xml);
				zip.AddFile(metadataXml.Path, string.Empty);

				var subdirectory = legacyFormat ? string.Empty : "release";

				File.WriteAllBytes(englishLds.Path, Resources.English_lds);
				zip.AddFile(englishLds.Path, subdirectory);
				File.WriteAllText(stylesXml.Path, Resources.styles_xml);
				zip.AddFile(stylesXml.Path, subdirectory);
				File.WriteAllBytes(versificationVrs.Path, Resources.versification_vrs);
				zip.AddFile(versificationVrs.Path, subdirectory);
				if (includeLdml)
				{
					File.WriteAllText(ldmlXml.Path, Resources.ldml_xml);
					zip.AddFile(ldmlXml.Path, subdirectory);
				}
				File.WriteAllBytes(matUsx.Path, Resources.MAT_usx);
				zip.AddFile(matUsx.Path, (legacyFormat ? "" : "release/") + (invalidUsxDirectory ? "USX_999" : "USX_1"));
				zip.Save(bundle.Path);
			}

			return bundle;
		}
	}
}
