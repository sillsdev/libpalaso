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
		private TempFile m_zippedBundle;
		private TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage> m_bundle;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			m_zippedBundle = TempFile.FromResource(Resources.TestTextBundle, DblBundleFileUtils.kDblBundleExtension);
			m_bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(m_zippedBundle.Path);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			if (m_bundle != null)
				m_bundle.Dispose();
			if (m_zippedBundle != null)
				m_zippedBundle.Dispose();
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
	}
}
