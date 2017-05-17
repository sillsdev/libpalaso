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
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> m_bundle;
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> m_bundleWithoutLdml;

		/// <summary>
		/// Setup test fixture.
		/// </summary>
		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			using (var zippedBundle = CreateZippedTextBundleFromResources())
				m_bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
			using (var zippedBundle = CreateZippedTextBundleFromResources(false))
				m_bundleWithoutLdml = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
		}

		/// <summary>
		/// Tear down test fixture.
		/// </summary>
		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			m_bundle.Dispose();
			m_bundleWithoutLdml.Dispose();
		}

		/// <summary>
		/// Tests that properties are correctly created in a bundle.
		/// </summary>
		[Test]
		public void CreateBundle_PropertiesReadCorrectly()
		{
			Assert.AreEqual("ce61dd3decd6b8a8", m_bundle.Id);
			Assert.AreEqual("Test Bundle Publication", m_bundle.Name);
			Assert.AreEqual("eng", m_bundle.LanguageIso);
		}

		/// <summary>
		/// Tests that the stylesheet is loaded correctly from a bundle.
		/// </summary>
		[Test]
		public void CreateBundle_ProperlyLoadsStylesheet()
		{
			var stylesheet = m_bundle.Stylesheet;
			IStyle style = stylesheet.GetStyle("mt1");
			Assert.NotNull(style);
			Assert.AreEqual("Cambria", stylesheet.FontFamily);
			Assert.AreEqual(14, stylesheet.FontSizeInPoints);
		}

		/// <summary>
		/// Tests that writing system for a bundle is created correctly.
		/// </summary>
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

		/// <summary>
		/// Tests that a book is obtained from a bundle.
		/// </summary>
		[Test]
		public void TryGetBook()
		{
			UsxDocument book;
			Assert.IsTrue(m_bundle.TryGetBook("MAT", out book));
			Assert.AreEqual("MAT", book.BookId);
		}

		/// <summary>
		/// Tests that a bundle with an LDML file correctly reports that.
		/// </summary>
		[Test]
		public void ContainsLdmlFile_FileExists_ReturnsTrue()
		{
			Assert.IsTrue(m_bundle.ContainsLdmlFile());
		}

		/// <summary>
		/// Tests that a bundle without an LDML file correctly reports that.
		/// </summary>
		[Test]
		public void ContainsLdmlFile_FileDoesNotExist_ReturnsFalse()
		{
			Assert.False(m_bundleWithoutLdml.ContainsLdmlFile());
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
		public static TempFile CreateZippedTextBundleFromResources(bool includeLdml = true, bool invalidUsxDirectory = false)
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
				File.WriteAllBytes(englishLds.Path, Resources.English_lds);
				zip.AddFile(englishLds.Path, string.Empty);
				File.WriteAllText(metadataXml.Path, Resources.metadata_xml);
				zip.AddFile(metadataXml.Path, string.Empty);
				File.WriteAllText(stylesXml.Path, Resources.styles_xml);
				zip.AddFile(stylesXml.Path, string.Empty);
				File.WriteAllBytes(versificationVrs.Path, Resources.versification_vrs);
				zip.AddFile(versificationVrs.Path, string.Empty);
				if (includeLdml)
				{
					File.WriteAllText(ldmlXml.Path, Resources.ldml_xml);
					zip.AddFile(ldmlXml.Path, string.Empty);
				}
				File.WriteAllBytes(matUsx.Path, Resources.MAT_usx);
				zip.AddFile(matUsx.Path, invalidUsxDirectory ? "USX_1" : "USX_0");
				zip.Save(bundle.Path);
			}

			return bundle;
		}
	}
}
