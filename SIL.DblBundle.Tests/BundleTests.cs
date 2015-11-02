using System;
using System.IO;
using Ionic.Zip;
using NUnit.Framework;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.WritingSystems;

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
			Bundle<TestMetadata, DblMetadataLanguage>.DefaultLanguageIsoCode = WellKnownSubtags.UnlistedLanguage;
			m_bundle.Metadata.Language.Iso = "ach";
			Assert.AreEqual("ach", m_bundle.LanguageIso);
		}

		[Test]
		public void LanguageIso_NoLanguageCodeSpecified_ReturnsCodeForUnlistedLanguage()
		{
			Bundle<TestMetadata, DblMetadataLanguage>.DefaultLanguageIsoCode = WellKnownSubtags.UnlistedLanguage;
			m_bundle.Metadata.Language.Iso = null;
			Assert.AreEqual(WellKnownSubtags.UnlistedLanguage, m_bundle.LanguageIso);
			m_bundle.Metadata.Language.Iso = String.Empty;
			Assert.AreEqual(WellKnownSubtags.UnlistedLanguage, m_bundle.LanguageIso);
		}

		private static TempFile CreateZippedTestBundleFromResources()
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);

			using (var metadataXml = TempFile.WithFilename("metadata.xml"))
			using (var zip = new ZipFile())
			{
				File.WriteAllText(metadataXml.Path, @"<TestMetadata type=""text""/>");
				zip.AddFile(metadataXml.Path, string.Empty);
				zip.Save(bundle.Path);
			}

			return bundle;
		}
	}

	/// <summary>
	/// Version that thwarts normal initialization of metadata
	/// </summary>
	public class TestMetadata : DblMetadataBase<DblMetadataLanguage>
	{
		protected override void InitializeMetadata()
		{
			Language = new DblMetadataLanguage();
		}

		public override string Name { get { return "Test"; } }
	}
}