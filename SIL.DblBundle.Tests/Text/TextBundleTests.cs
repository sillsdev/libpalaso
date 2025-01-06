using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.WritingSystems;
using static System.String;

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
			using (var zippedBundle = CreateZippedTextBundleFromResources(MetadataVersion.V1_4, true, false))
				_legacyBundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(MetadataVersion.V1_4, false, false))
				_legacyBundleWithoutLdml = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(MetadataVersion.V2_1))
				_bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(MetadataVersion.V2_1, false))
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
		/// Tests that metadata can be read with three-.
		/// </summary>
		[Test]
		public void CreateBundle_VersionHasRevision_MetadataLoadedCorrectly()
		{
			using (var zippedBundle = CreateZippedTextBundleFromResources(MetadataVersion.V2_2_1))
				using (var bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path))
			{
				Assert.AreEqual("55ec700d9e0d77ea", bundle.Id);
				Assert.AreEqual("English Majority Text Version", bundle.Name);
				Assert.AreEqual("eng", bundle.LanguageIso);
				Assert.AreEqual("English Majority Text Version", bundle.Metadata.Name);
				Assert.AreEqual("English Majority Text Version", bundle.Metadata.Identification.NameLocal);
				Assert.AreEqual("xhtml", bundle.Metadata.Copyright.Statement.ContentType);
				Assert.AreEqual("<p>© 2014 Dr. Paul W. Esposito</p>",
					bundle.Metadata.Copyright.Statement.InternalNodes.Single().OuterXml);
				Assert.AreEqual("xhtml",
					bundle.Metadata.Promotion.PromoVersionInfo.ContentType);
				Assert.AreEqual("<p>Translated by Dr. Paul W. Esposito.</p>",
					bundle.Metadata.Promotion.PromoVersionInfo.InternalNodes.Single().OuterXml);
				Assert.AreEqual(27, bundle.Metadata.AvailableBibleBooks.Count);
				Assert.AreEqual("MAT", bundle.Metadata.Canons.Single().CanonBooks.First().Code);
				Assert.AreEqual("REV", bundle.Metadata.Canons.Single().CanonBooks.Last().Code);
			}
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
			Assert.IsTrue(bundle.TryGetBook("MAT", out var book));
			Assert.AreEqual("MAT", book.BookId);
		}

		[Test]
		public void GetVersification_ValidBundle_ReturnsValidReader()
		{
			using (var reader = _bundle.GetVersification())
			{
				Assert.IsTrue(reader.ReadLine().StartsWith("#"));
			}
		}

		[Test]
		public void GetFonts_BundleWithFont_ReturnsEnumerationWithFont()
		{
			using (var zippedBundle = CreateZippedTextBundleFromResources(MetadataVersion.V2_1, false, false,  true))
				using (var bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path))
				{
					var fontInfo = bundle.GetFonts().Single();
					Assert.AreEqual("AppSILI.ttf", fontInfo.Item1);
					Assert.IsTrue(fontInfo.Item2.ReadByte() >= 0);
			}
		}

		[Test]
		public void GetFonts_BundleWithNoFonts_ReturnsEmptyEnumeration()
		{
			Assert.IsFalse(_bundle.GetFonts().Any());
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

		[Test]
		public void GetLdml_BundleContainsLdmlFile_ReturnsValidReader()
		{
			using (var reader = _bundle.GetLdml())
			{
				reader.ReadLine(); // Skip past ?xml element
				Assert.IsTrue(reader.ReadLine().StartsWith("<ldml"));
			}
		}

		[Test]
		public void GetLdml_BundleDoesNotContainsLdmlFile_ReturnsNull()
		{
			Assert.IsNull(_bundleWithoutLdml.GetLdml());
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
		/// <remarks>ENHANCE: This version of this method should probably be retired and replaced
		/// by the more useful and meaningful (private) version, but it didn't seem worth a
		/// breaking change.</remarks>
		public static TempFile CreateZippedTextBundleFromResources(bool includeLdml = true,
			bool invalidUsxDirectory = false, bool legacy = false, bool includeFont = false)
		{
			return CreateZippedTextBundleFromResources(legacy ? MetadataVersion.V1_4 : MetadataVersion.V2_1,
				includeLdml, invalidUsxDirectory, includeFont);
		}

		private enum MetadataVersion
		{
			V1_4,
			V2_1,
			V2_2_1,
		}

		/// <summary>
		/// Helper method for tests to create a zipped text bundle.
		/// </summary>
		private static TempFile CreateZippedTextBundleFromResources(MetadataVersion version,
			bool includeLdml = true, bool invalidUsxDirectory = false, bool includeFont = false)
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);
			RobustFile.Delete(bundle.Path);

			using (var englishLds = TempFile.WithFilename("English.lds"))
			using (var metadataXml = TempFile.WithFilename("metadata.xml"))
			using (var stylesXml = TempFile.WithFilename("styles.xml"))
			using (var versificationVrs = TempFile.WithFilename(DblBundleFileUtils.kVersificationFileName))
			using (var ldmlXml = TempFile.WithFilename(DblBundleFileUtils.kLegacyLdmlFileName))
			using (var ttf = TempFile.WithFilename("AppSILI.ttf"))
			using (var matUsx = TempFile.WithFilename("MAT.usx"))
			using (var zip = ZipFile.Open(bundle.Path, ZipArchiveMode.Create))
			{
				string xml;
				var subdirectory = "release";
				switch (version)
				{
					case MetadataVersion.V1_4:
						xml = Resources.metadata_xml;
						subdirectory = Empty;
						break;
					case MetadataVersion.V2_1:
						xml = Resources.metadataVersion2_1_xml;
						break;
					case MetadataVersion.V2_2_1:
						xml = Resources.metadataVersion2_2_1_xml;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(version), version, null);
				}
				File.WriteAllText(metadataXml.Path, xml);
				zip.CreateEntryFromFile(metadataXml.Path, Path.GetFileName(metadataXml.Path));

				File.WriteAllBytes(englishLds.Path, Resources.English_lds);
				zip.CreateEntryFromFile(englishLds.Path,
					Path.Combine(subdirectory, Path.GetFileName(englishLds.Path)));
				File.WriteAllText(stylesXml.Path, Resources.styles_xml);
				zip.CreateEntryFromFile(stylesXml.Path,
					Path.Combine(subdirectory, Path.GetFileName(stylesXml.Path)));
				File.WriteAllBytes(versificationVrs.Path, Resources.versification_vrs);
				zip.CreateEntryFromFile(versificationVrs.Path,
					Path.Combine(subdirectory, Path.GetFileName(versificationVrs.Path)));
				if (includeLdml)
				{
					File.WriteAllText(ldmlXml.Path, Resources.ldml_xml);
					zip.CreateEntryFromFile(ldmlXml.Path,
						Path.Combine(subdirectory, Path.GetFileName(ldmlXml.Path)));
				}
				if (includeFont)
				{
					File.WriteAllBytes(ttf.Path,
						(byte[])Resources.ResourceManager.GetObject("AppSILI", CultureInfo.InvariantCulture));
					zip.CreateEntryFromFile(ttf.Path,
						Path.Combine(subdirectory, Path.GetFileName(ttf.Path)));
				}
				File.WriteAllBytes(matUsx.Path, Resources.MAT_usx);
				if (subdirectory != Empty)
					subdirectory += "/";
				zip.CreateEntryFromFile(matUsx.Path, Path.Combine(subdirectory +
					(invalidUsxDirectory ? "USX_999" : "USX_1"), Path.GetFileName(matUsx.Path)));
			}

			return bundle;
		}
	}
}
