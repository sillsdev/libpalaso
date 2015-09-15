using System.IO;
using Ionic.Zip;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;

namespace SIL.DblBundle.Tests.Text
{
	[TestFixture]
	public class TextBundleTests
	{
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> m_bundle;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			using (var zippedBundle = CreateZippedTextBundleFromResources())
				m_bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(zippedBundle.Path);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			m_bundle.Dispose();
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
		public void TryGetBook()
		{
			UsxDocument book;
			Assert.IsTrue(m_bundle.TryGetBook("MAT", out book));
			Assert.AreEqual("MAT", book.BookId);
		}

		private static TempFile CreateZippedTextBundleFromResources()
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);

			using (var englishLds = TempFile.WithFilename("English.lds"))
			using (var metadataXml = TempFile.WithFilename("metadata.xml"))
			using (var stylesXml = TempFile.WithFilename("styles.xml"))
			using (var versificationVrs = TempFile.WithFilename("versification.vrs"))
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
				File.WriteAllBytes(matUsx.Path, Resources.MAT_usx);
				zip.AddFile(matUsx.Path, "USX_0");
				zip.Save(bundle.Path);
			}

			return bundle;
		}
	}
}
