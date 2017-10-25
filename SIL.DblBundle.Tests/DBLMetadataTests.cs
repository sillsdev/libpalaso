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
	[TestFixture]
	class DblMetadataTests
	{
		private DblTextMetadata<DblMetadataLanguage> _metadata;
		private DblTextMetadata<DblMetadataLanguage> _metadata2;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			var xs = new XmlSerializer(typeof(DblTextMetadata<DblMetadataLanguage>));
			using (TextReader reader = new StringReader(Resources.AcholiMetadataVersion1_5_xml))
				_metadata = (DblTextMetadata<DblMetadataLanguage>)xs.Deserialize(reader);

			using (TextReader reader = new StringReader(Resources.AcholiMetadataVersion2_1_xml))
				_metadata2 = (DblTextMetadata<DblMetadataLanguage>)xs.Deserialize(reader);
		}

		[Test]
		public void GetId()
		{
			Assert.AreEqual("3b9fdc679b9319c3", _metadata.Id);
		}

		[Test]
		public void GetName()
		{
			Assert.AreEqual("Acholi New Testament 1985", _metadata.Identification.Name);
		}

		[Test]
		public void GetParatextSystemId()
		{
			Assert.AreEqual("3b9fdc679b9319c3ee45ab86cc1c0c42930c2979", _metadata.Identification.SystemIds.FirstOrDefault(sid => sid.Type.Equals("paratext")).Id);
		}

		[Test]
		public void GetLanguageIso()
		{
			Assert.AreEqual("ach", _metadata.Language.Iso);
		}

		[Test]
		public void GetCopyrightStatement()
		{
			const string expectedValue = "<p>© 1985 The Bible Society of Uganda</p>";
			Assert.AreEqual(expectedValue, _metadata.Copyright.Statement.Xhtml);
			Assert.AreEqual("xhtml", _metadata.Copyright.Statement.ContentType);
		}

		[Test]
		public void GetPromoVersionInfo()
		{
			const string expectedValue = @"<h1>Acholi New Testament 1985</h1><p>This translation, published by the Bible Society " +
				@"of Uganda, was first published in 1985.</p><p>If you are interested in obtaining a printed copy, please contact " +
				@"the Bible Society of Uganda at <a href=""http://www.biblesociety-uganda.org/"">www.biblesociety-uganda.org</a>.</p>";
			Assert.AreEqual(expectedValue, _metadata.Promotion.PromoVersionInfo.Xhtml);
			Assert.AreEqual("xhtml", _metadata.Promotion.PromoVersionInfo.ContentType);
		}

		[Test]
		public void GetDateArchived()
		{
			Assert.AreEqual("2014-05-28T15:18:31.080800", _metadata.ArchiveStatus.DateArchived);
		}

		[Test]
		public void IsTextReleaseBundle()
		{
			Assert.True(_metadata.IsTextReleaseBundle);
			Assert.True(_metadata2.IsTextReleaseBundle);
		}

		[Test]
		public void Load_Successful()
		{
			using (var metadataFile = new TempFile())
			{
				File.WriteAllText(metadataFile.Path, Resources.metadata_xml);
				Exception exception;
				DblTextMetadata<DblMetadataLanguage>.Load<DblTextMetadata<DblMetadataLanguage>>(metadataFile.Path, out exception);
				Assert.Null(exception);
			}
		}

		[Test]
		public void Load_FileDoesNotExist_HandlesException()
		{
			Exception exception;
			DblTextMetadata<DblMetadataLanguage>.Load<DblTextMetadata<DblMetadataLanguage>>("", out exception);
			Assert.NotNull(exception);
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
					Statement = new DblMetadataXhtmlContentNode { Xhtml = @"© 2015, SIL Inc. All rights reserved." }
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
		<statement contentType=""xhtml"">© 2015, SIL Inc. All rights reserved.</statement>
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
}
