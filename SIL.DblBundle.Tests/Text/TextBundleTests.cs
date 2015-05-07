using System.IO;
using ICSharpCode.SharpZipLib.Zip;
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

		private TempFile CreateZippedTextBundleFromResources()
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);

			string dirContainingContentsForZipping = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(dirContainingContentsForZipping);
			File.WriteAllBytes(Path.Combine(dirContainingContentsForZipping, "English.lds"), Resources.English_lds);
			File.WriteAllText(Path.Combine(dirContainingContentsForZipping, "metadata.xml"), Resources.metadata_xml);
			File.WriteAllText(Path.Combine(dirContainingContentsForZipping, "styles.xml"), Resources.styles_xml);
			File.WriteAllBytes(Path.Combine(dirContainingContentsForZipping, "versification.vrs"), Resources.versification_vrs);
			Directory.CreateDirectory(Path.Combine(dirContainingContentsForZipping, "USX_0"));
			File.WriteAllBytes(Path.Combine(dirContainingContentsForZipping, "USX_0", "MAT.usx"), Resources.MAT_usx);
			Directory.CreateDirectory(Path.Combine(dirContainingContentsForZipping, "empty"));

			var zipFile = new FastZip();
			zipFile.CreateEmptyDirectories = true;
			zipFile.CreateZip(bundle.Path, dirContainingContentsForZipping, true, null);

			DirectoryUtilities.DeleteDirectoryRobust(dirContainingContentsForZipping);

			return bundle;
		}
	}
}
