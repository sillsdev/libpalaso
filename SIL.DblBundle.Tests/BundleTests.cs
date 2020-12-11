using System;
using System.IO;
using System.Xml.Serialization;
using Ionic.Zip;
using NUnit.Framework;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.WritingSystems;

namespace SIL.DblBundle.Tests
{
	/// <summary>
	/// Tests for a Digital Bible Library text bundle.
	/// </summary>
	[TestFixture]
	public class BundleTests
	{
		private TestBundle m_bundle;

		/// <summary>
		/// Setup for test fixture.
		/// </summary>
		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			using (var zippedBundle = CreateDummyVersion1_5ZippedTestBundle())
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

		/// <summary>
		/// Tests getting language code from metadata.
		/// </summary>
		[Test]
		public void LanguageIso_LanguageCodeSpecifiedInMetadata_ReturnsLanguageCode()
		{
			Bundle<TestMetadata, DblMetadataLanguage>.DefaultLanguageIsoCode = WellKnownSubtags.UnlistedLanguage;
			m_bundle.Metadata.Language.Iso = "ach";
			Assert.AreEqual("ach", m_bundle.LanguageIso);
		}

		/// <summary>
		/// Tests getting language code when no language code is specified.
		/// </summary>
		[Test]
		public void LanguageIso_NoLanguageCodeSpecified_ReturnsCodeForUnlistedLanguage()
		{
			Bundle<TestMetadata, DblMetadataLanguage>.DefaultLanguageIsoCode = WellKnownSubtags.UnlistedLanguage;
			m_bundle.Metadata.Language.Iso = null;
			Assert.AreEqual(WellKnownSubtags.UnlistedLanguage, m_bundle.LanguageIso);
			m_bundle.Metadata.Language.Iso = String.Empty;
			Assert.AreEqual(WellKnownSubtags.UnlistedLanguage, m_bundle.LanguageIso);
		}

		private static TempFile CreateDummyVersion1_5ZippedTestBundle()
		{
			TempFile bundle = TempFile.WithExtension(DblBundleFileUtils.kDblBundleExtension);

			using (var metadataXml = TempFile.WithFilename("metadata.xml"))
			using (var zip = new ZipFile())
			{
				File.WriteAllText(metadataXml.Path, @"<?xml version=""1.0"" encoding=""utf-8""?><DBLMetadata type=""text"" typeVersion=""1.5""></DBLMetadata>");
				zip.AddFile(metadataXml.Path, string.Empty);
				zip.Save(bundle.Path);
			}

			return bundle;
		}
	}

	/// <summary>
	/// Version that thwarts normal initialization of metadata
	/// </summary>
	[XmlRoot("DBLMetadata")]
	public class TestMetadata : DblMetadataBase<DblMetadataLanguage>
	{
		/// <summary>
		/// Creates empty Metadata for testing
		/// </summary>
		protected override void InitializeMetadata()
		{
			Language = new DblMetadataLanguage();
		}

		/// <summary>
		/// Gets name test value
		/// </summary>
		public override string Name { get { return "Test"; } }
	}
}