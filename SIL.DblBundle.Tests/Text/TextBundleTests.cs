using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.WritingSystems;

namespace SIL.DblBundle.Tests.Text
{
	[TestFixture]
	public class TextBundleTests
	{
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> m_bundle;
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> m_bundleWithVersion2Metadata;
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> m_bundleWithoutLdml;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			Sldr.Initialize();
			using (var zippedBundle = CreateZippedTextBundleFromResources())
				m_bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(metadataVersion2: true))
				m_bundleWithVersion2Metadata = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(false))
				m_bundleWithoutLdml = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			m_bundle.Dispose();
			m_bundleWithoutLdml.Dispose();
			Sldr.Cleanup();
		}

		[Test]
		public void CreateBundle_PropertiesReadCorrectly()
		{
			Assert.AreEqual("ce61dd3decd6b8a8", m_bundle.Id);
			Assert.AreEqual("Test Bundle Publication", m_bundle.Name);
			Assert.AreEqual("eng", m_bundle.LanguageIso);
		}

		[Test]
		public void CreateBundle_ProperlyLoadsStylesheet()
		{
			var stylesheet = m_bundle.Stylesheet;
			IStyle style = stylesheet.GetStyle("mt1");
			Assert.NotNull(style);
			Assert.AreEqual("Cambria", stylesheet.FontFamily);
			Assert.AreEqual(14, stylesheet.FontSizeInPoints);
		}

		[Test]
		public void CreateBundle_ProperlyLoadsWritingSystemDefinition()
		{
			var ws = m_bundle.WritingSystemDefinition;
			Assert.NotNull(ws);

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

		[TestCase("1")]
		[TestCase("p1")]
		public void CreateBundle_GetPathToCanon(string canonId)
		{
			Assert.True(m_bundle.GetPathToCanon(canonId).EndsWith("USX_1"));
		}

		[TestCase(false)]
		[TestCase(true)]
		public void UsxBooksToInclude(bool version2)
		{
			var bundle = version2 ? m_bundleWithVersion2Metadata : m_bundle;
			Assert.AreEqual(1, bundle.UsxBooksToInclude.Count());
		}

		[Test]
		public void TryGetBook()
		{
			UsxDocument book;
			Assert.IsTrue(m_bundle.TryGetBook("MAT", out book));
			Assert.AreEqual("MAT", book.BookId);
		}

		[Test]
		public void ContainsLdmlFile_FileExists_ReturnsTrue()
		{
			Assert.IsTrue(m_bundle.ContainsLdmlFile());
		}

		[Test]
		public void ContainsLdmlFile_FileDoesNotExist_ReturnsFalse()
		{
			Assert.False(m_bundleWithoutLdml.ContainsLdmlFile());
		}

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

		public static TempFile CreateZippedTextBundleFromResources(bool includeLdml = true, bool invalidUsxDirectory = false, bool metadataVersion2 = false)
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);

			using (var englishLds = TempFile.WithFilename("English.lds"))
			using (var metadataXml = TempFile.WithFilename("metadata.xml"))
			using (var stylesXml = TempFile.WithFilename("styles.xml"))
			using (var versificationVrs = TempFile.WithFilename(DblBundleFileUtils.kVersificationFileName))
			using (var ldmlXml = TempFile.WithFilename(DblBundleFileUtils.kLdmlFileName))
			using (var matUsx = TempFile.WithFilename("MAT.usx"))
			using (var zip = new ZipFile())
			{
				var ldsResource = Resources.English_lds;
				var metadataResource = Resources.metadata_xml;
				var styleResource = Resources.styles_xml;
				var versificationResource = Resources.versification_vrs;
				var ldmlResource = Resources.ldml_xml;
				var MATResource = Resources.MAT_usx;
				var usxDirectory = "USX_0";

				if (metadataVersion2)
				{
					ldsResource = Resources.v2_eng_lds;
					metadataResource = Resources.v2_metadata_xml;
					styleResource = Resources.v2_styles_xml;
					versificationResource = Resources.v2_versification_vrs;
					ldmlResource = Resources.v2_eng_en_ldml;
					MATResource = Resources.v2_MAT_usx;
					usxDirectory = "USX_1";
				}
				if (invalidUsxDirectory)
					usxDirectory = "USX_A";

				File.WriteAllBytes(englishLds.Path, ldsResource);
				zip.AddFile(englishLds.Path, string.Empty);
				File.WriteAllText(metadataXml.Path, metadataResource);
				zip.AddFile(metadataXml.Path, string.Empty);
				File.WriteAllText(stylesXml.Path, styleResource);
				zip.AddFile(stylesXml.Path, string.Empty);
				File.WriteAllBytes(versificationVrs.Path, versificationResource);
				zip.AddFile(versificationVrs.Path, string.Empty);
				if (includeLdml)
				{
					File.WriteAllText(ldmlXml.Path, ldmlResource);
					zip.AddFile(ldmlXml.Path, string.Empty);
				}
				File.WriteAllBytes(matUsx.Path, MATResource);
				zip.AddFile(matUsx.Path, usxDirectory);
				zip.Save(bundle.Path);
			}

			return bundle;
		}
	}
}
