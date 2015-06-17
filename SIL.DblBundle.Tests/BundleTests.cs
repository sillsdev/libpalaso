using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.DblBundle.Tests.Text;
using SIL.DblBundle.Text;
using SIL.IO;

namespace SIL.DblBundle.Tests
{
	[TestFixture]
	public class BundleTests
	{
		private TestBundle m_bundle;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			using (var zippedBundle = CreateZippedTestBundleFromResources())
				m_bundle = new TestBundle(zippedBundle.Path);
		}

		private class TestBundle : Bundle<TestMetadata, DblMetadataLanguage>
		{
			public TestBundle(string pathToZippedBundle) : base(pathToZippedBundle)
			{
			}

			public override string Name
			{
				get { return "Test"; }
			}
		}

		[Test]
		public void LanguageIso_LanguageCodeSpecifiedInMetadata_ReturnsLanguageCode()
		{
			m_bundle.Metadata.Language.Iso = "ach";
			Assert.AreEqual("ach", m_bundle.LanguageIso);
		}

		[Test]
		public void LanguageIso_NoLanguageCodeSpecified_ReturnsCodeForUnknown()
		{
			m_bundle.Metadata.Language.Iso = null;
			Assert.AreEqual("qaa", m_bundle.LanguageIso);
			m_bundle.Metadata.Language.Iso = String.Empty;
			Assert.AreEqual("qaa", m_bundle.LanguageIso);
		}

		private static TempFile CreateZippedTestBundleFromResources()
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);

			string dirContainingContentsForZipping = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(dirContainingContentsForZipping);
			File.WriteAllText(Path.Combine(dirContainingContentsForZipping, "metadata.xml"), @"<TestMetadata type=""text""/>");

			var zipFile = new FastZip();
			zipFile.CreateZip(bundle.Path, dirContainingContentsForZipping, true, null);

			DirectoryUtilities.DeleteDirectoryRobust(dirContainingContentsForZipping);

			return bundle;
		}
	}

	/// <summary>
	/// Version that thwarts normal initlization of metadata
	/// </summary>
	public class TestMetadata : DblMetadataBase<DblMetadataLanguage>
	{
		protected override void InitializeMetadata()
		{
			Language = new DblMetadataLanguage();
		}

	}
}