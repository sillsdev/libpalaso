using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.TestUtilities;

namespace SIL.DblBundle.Tests
{
	class DblMetadataTests
	{
		private DblTextMetadata<DblMetadataLanguage> m_metadata;

		private const string TestXml =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DBLMetadata id=""3b9fdc679b9319c3"" revision=""1"" mediatype=""text"" typeVersion=""1.3"">
	<identification>
		<name>Acholi New Testament 1985</name>
		<nameLocal>Acoli Baibul 1985</nameLocal>
		<systemId type=""tms"">b9236acd-66f3-44d0-98fc-3970b3d017cd</systemId>
		<systemId type=""paratext"">3b9fdc679b9319c3ee45ab86cc1c0c42930c2979</systemId>
	</identification>
	<language>
		<iso>ach</iso>
	</language>
	<copyright>
		<statement contentType=""xhtml"">© 2025 SIL Global. All rights reserved.</statement>
	</copyright>
	<promotion>
		<promoVersionInfo contentType=""xhtml"">
			<h1>Acholi New Testament 1985</h1>
			<p>This translation, published by the Bible Society of Uganda, was first published in 1985.</p>
			<p>If you are interested in obtaining a printed copy, please contact the Bible Society of Uganda at <a href=""http://www.biblesociety-uganda.org/"">www.biblesociety-uganda.org</a>.</p>
		</promoVersionInfo>
		<promoEmail contentType=""xhtml"">
			<p>Hi YouVersion friend,</p>
			<p>Sincerely, Your Friends at YouVersion</p>
		</promoEmail>
	</promotion>
	<archiveStatus>
		<archivistName>Emma Canales -archivist</archivistName>
		<dateArchived>2014-05-28T15:18:31.080800</dateArchived>
		<dateUpdated>2014-05-28T15:18:31.080800</dateUpdated>
		<comments>First submit</comments>
	</archiveStatus>
</DBLMetadata>";

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			var xs = new XmlSerializer(typeof(DblTextMetadata<DblMetadataLanguage>));
			using (TextReader reader = new StringReader(TestXml))
				m_metadata = (DblTextMetadata<DblMetadataLanguage>)xs.Deserialize(reader);
		}

		[Test]
		public void GetId()
		{
			Assert.AreEqual("3b9fdc679b9319c3", m_metadata.Id);
		}

		[Test]
		public void GetName()
		{
			Assert.AreEqual("Acholi New Testament 1985", m_metadata.Identification.Name);
		}

		[Test]
		public void GetParatextSystemId()
		{
			Assert.AreEqual("3b9fdc679b9319c3ee45ab86cc1c0c42930c2979", m_metadata.Identification.SystemIds.FirstOrDefault(sid => sid.Type.Equals("paratext")).Id);
		}

		[Test]
		public void GetLanguageIso()
		{
			Assert.AreEqual("ach", m_metadata.Language.Iso);
		}

		[Test]
		public void GetCopyrightStatement()
		{
			const string expectedValue = @"© 2025 SIL Global. All rights reserved.";
			Assert.AreEqual(expectedValue, m_metadata.Copyright.Statement.Xhtml);
			Assert.AreEqual("xhtml", m_metadata.Copyright.Statement.ContentType);
		}

		[Test]
		public void GetPromoVersionInfo()
		{
			const string expectedValue = @"<h1>Acholi New Testament 1985</h1><p>This translation, published by the Bible Society " +
				@"of Uganda, was first published in 1985.</p><p>If you are interested in obtaining a printed copy, please contact " +
				@"the Bible Society of Uganda at <a href=""http://www.biblesociety-uganda.org/"">www.biblesociety-uganda.org</a>.</p>";
			Assert.AreEqual(expectedValue, m_metadata.Promotion.PromoVersionInfo.Xhtml);
			Assert.AreEqual("xhtml", m_metadata.Promotion.PromoVersionInfo.ContentType);
		}

		[Test]
		public void GetPromoEmail()
		{
			const string expectedValue = @"<p>Hi YouVersion friend,</p><p>Sincerely, Your Friends at YouVersion</p>";
			Assert.AreEqual(expectedValue, m_metadata.Promotion.PromoEmail.Xhtml);
		}

		[Test]
		public void GetDateArchived()
		{
			Assert.AreEqual("2014-05-28T15:18:31.080800", m_metadata.ArchiveStatus.DateArchived);
		}

		[Test]
		public void Load_FromValidFile_Successful()
		{
			using (var metadataFile = new TempFile())
			{
				File.WriteAllText(metadataFile.Path, Resources.metadata_xml);
				var metadata = DblTextMetadata<DblMetadataLanguage>
					.Load<DblTextMetadata<DblMetadataLanguage>>(metadataFile.Path,
						out var exception);
				Assert.IsTrue(metadata is DblTextMetadata<DblMetadataLanguage>);
				Assert.IsNull(exception);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Load_FromTextReader_Successful(bool skipXmlElement)
		{
			using (var textReader = new StringReader(Resources.metadata_xml))
			{
				if (skipXmlElement)
					textReader.ReadLine(); // Skip past XML header - not required by deserializer
				Assert.IsNotNull(DblTextMetadata<DblMetadataLanguage>
					.Load<DblTextMetadata<DblMetadataLanguage>>(textReader,
						"Resources.metadata_xml", out var exception));
				Assert.IsNull(exception);
			}
		}

		[TestCase("", ExpectedResult = typeof(ArgumentException))]
		[TestCase("doNot-ExpectThis~FileToExist", ExpectedResult = typeof(FileNotFoundException))]
		public Type Load_FileDoesNotExist_HandlesException(string nonExistentFilename)
		{
			Assert.IsNull(
				DblTextMetadata<DblMetadataLanguage>.Load<DblTextMetadata<DblMetadataLanguage>>(
					nonExistentFilename, out var exception));
			return exception.GetType();
		}

		[Test]
		public void Load_FromTextReaderNotPositionedAtStartOfMetadata_SetsException()
		{
			using (var textReader = new StringReader(Resources.metadata_xml))
			{
				textReader.ReadLine(); // Skip past XML header - not required by deserializer
				textReader.ReadLine(); // Skip past next line - root element!
				Assert.IsNull(DblTextMetadata<DblMetadataLanguage>
					.Load<DblTextMetadata<DblMetadataLanguage>>(textReader,
						"Resources.metadata_xml", out var exception));
				Assert.IsTrue(exception is InvalidOperationException);
			}
		}

		[Test]
		public void Load_FromValidFileWithSubclassThatHasInitializer_InitializeMetadataIsCalled()
		{
			using (var metadataFile = new TempFile())
			{
				File.WriteAllText(metadataFile.Path, Resources.metadata_xml);
				var metadata = DblTextMetadata<DblMetadataLanguage>
					.Load<ExtendedDblTextMetadata>(metadataFile.Path, out var exception);
				Assert.IsNull(exception);
				Assert.IsTrue(metadata.IsInitialized);
			}
		}

		[Test]
		public void Serialize()
		{
			var metadata = new DblTextMetadata<DblMetadataLanguage>
			{
				Id = "id",
				Revision = 1,
				Identification = new DblMetadataIdentification
				{
					Name = "name",
					SystemIds = new HashSet<DblMetadataSystemId> { new DblMetadataSystemId { Type = "type", Id = "Idvalue" } }
				},
				Copyright = new DblMetadataCopyright
				{
					Statement = new DblMetadataXhtmlContentNode { Xhtml = @"© 2025 SIL Global. All rights reserved." }
				},
				Promotion = new DblMetadataPromotion
				{
					PromoVersionInfo = new DblMetadataXhtmlContentNode { Xhtml = @"<h1>Acholi New Testament 1985</h1><p>More text</p>" },
					PromoEmail = new DblMetadataXhtmlContentNode { Xhtml = "<p>Email Text</p>" }
				},
				ArchiveStatus = new DblMetadataArchiveStatus { DateArchived = "dateArchived" }
			};

			const string expectedResult =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<DBLMetadata id=""id"" revision=""1"">
	<identification>
		<name>name</name>
		<systemId type=""type"">Idvalue</systemId>
	</identification>
	<copyright>
		<statement contentType=""xhtml"">© 2025 SIL Global. All rights reserved.</statement>
	</copyright>
	<promotion>
		<promoVersionInfo contentType=""xhtml"">
			<h1>Acholi New Testament 1985</h1>
			<p>More text</p>
		</promoVersionInfo>
		<promoEmail contentType=""xhtml"">
			<p>Email Text</p>
		</promoEmail>
	</promotion>
	<archiveStatus>
		<dateArchived>dateArchived</dateArchived>
	</archiveStatus>
</DBLMetadata>";

			AssertThatXmlIn.String(expectedResult).EqualsIgnoreWhitespace(metadata.GetAsXml());
		}
	}

	[XmlRoot("DBLMetadata")]
	public class ExtendedDblTextMetadata : DblTextMetadata<DblMetadataLanguage>
	{
		[XmlIgnore]
		public bool IsInitialized { get; private set; }
		protected override void InitializeMetadata()
		{
			base.InitializeMetadata();
			IsInitialized = true;
		}
	}
}
