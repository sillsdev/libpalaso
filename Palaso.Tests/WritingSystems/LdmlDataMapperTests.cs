using System;
using System.Xml;
using System.IO;
using NUnit.Framework;
using Palaso.Data;
using Palaso.IO;
using Palaso.TestUtilities;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using Palaso.Xml;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class LdmlDataMapperTests
	{
		private LdmlDataMapper _adaptor;
		private WritingSystemDefinition _ws;

		[SetUp]
		public void SetUp()
		{
			_adaptor = new LdmlDataMapper();
			_ws = new WritingSystemDefinition("en", "Latn", "US", string.Empty, "eng", false);
		}

		[Test]
		public void ReadFromFile_NullFileName_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read((string)null, _ws)
			);
		}

		[Test]
		public void ReadFromFile_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read("foo.ldml", null)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullXmlReader_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read((XmlReader)null, _ws)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read(XmlReader.Create(new StringReader("<ldml/>")), null)
			);
		}

		[Test]
		public void WriteToFile_NullFileName_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write((string)null, _ws, null)
			);
		}

		[Test]
		public void WriteToFile_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write("foo.ldml", null, null)
			);
		}

		[Test]
		public void WriteToXmlWriter_NullXmlReader_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write((XmlWriter)null, _ws, null)
			);
		}

		[Test]
		public void WriteSetsRequiresValidTagToTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.RequiresValidTag = false;
			ws.Language = "Kalaba";
			var sw = new StringWriter();
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			Assert.Throws(typeof(ValidationException), () => _adaptor.Write(writer, ws, null));
		}

		[Test]
		public void WriteToXmlWriter_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write(XmlWriter.Create(new MemoryStream()), null, null)
			);
		}

		[Test]
		public void ExistingUnusedLdml_Write_PreservesData()
		{
			var sw = new StringWriter();
			var ws = new WritingSystemDefinition("en");
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			_adaptor.Write(writer, ws, XmlReader.Create(new StringReader("<ldml><!--Comment--><dates/><special>hey</special></ldml>")));
			writer.Close();
			AssertThatXmlIn.String(sw.ToString()).HasAtLeastOneMatchForXpath("/ldml/special[text()=\"hey\"]");
		}

		[Test]
		public void RoundtripSimpleCustomSortRules_WS33715()
		{
			var ldmlAdaptor = new LdmlDataMapper();

			const string sortRules = "(A̍ a̍)";
			var wsWithSimpleCustomSortRules = new WritingSystemDefinition();
			wsWithSimpleCustomSortRules.SortUsing = WritingSystemDefinition.SortRulesType.CustomSimple;
			wsWithSimpleCustomSortRules.SortRules = sortRules;

			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				ldmlAdaptor.Write(tempFile.Path, wsWithSimpleCustomSortRules, null);
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}

			Assert.AreEqual(sortRules, wsFromLdml.SortRules);
		}


		[Test]
		//WS-33992
		public void Read_LdmlContainsEmptyCollationElement_SortUsingIsSetToSameAsIfNoCollationElementExisted()
		{
			const string ldmlWithEmptyCollationElement = "<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations><collation></collation></collations><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"2\" /></special></ldml>";
			const string ldmlwithNoCollationElement = "<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations/><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"2\" /></special></ldml>";

			string pathToLdmlWithEmptyCollationElement = Path.GetTempFileName();
			try
			{
				File.WriteAllText(pathToLdmlWithEmptyCollationElement, ldmlWithEmptyCollationElement);
				string pathToLdmlWithNoCollationElement = Path.GetTempFileName();
				try
				{
					File.WriteAllText(pathToLdmlWithNoCollationElement, ldmlwithNoCollationElement);


					var adaptor = new LdmlDataMapper();
					var wsFromEmptyCollationElement = new WritingSystemDefinition();
					adaptor.Read(pathToLdmlWithEmptyCollationElement, wsFromEmptyCollationElement);
					var wsFromNoCollationElement = new WritingSystemDefinition();
					adaptor.Read(pathToLdmlWithNoCollationElement, wsFromNoCollationElement);

					Assert.AreEqual(wsFromNoCollationElement.SortUsing, wsFromEmptyCollationElement.SortUsing);
				}
				finally
				{
					File.Delete(pathToLdmlWithNoCollationElement);
				}
			}
			finally
			{
				File.Delete(pathToLdmlWithEmptyCollationElement);
			}
		}


		[Test]
		public void Read_LdmlContainsOnlyPrivateUse_IsoAndprivateUseSetCorrectly()
		{
			const string ldmlWithOnlyPrivateUse = "<ldml><identity><version number=\"\" /><language type=\"\" /><variant type=\"x-private-use\" /></identity><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"2\" /></special></ldml>";


			string pathToLdmlWithEmptyCollationElement = Path.GetTempFileName();
			try
			{
				File.WriteAllText(pathToLdmlWithEmptyCollationElement, ldmlWithOnlyPrivateUse);

				var adaptor = new LdmlDataMapper();
				var wsFromLdml = new WritingSystemDefinition();
				adaptor.Read(pathToLdmlWithEmptyCollationElement, wsFromLdml);
				var ws = new WritingSystemDefinition();
				adaptor.Read(pathToLdmlWithEmptyCollationElement, ws);
				Assert.That(wsFromLdml.Language, Is.EqualTo(String.Empty));
				Assert.That(wsFromLdml.Variant, Is.EqualTo("x-private-use"));
			}
			finally
			{
				File.Delete(pathToLdmlWithEmptyCollationElement);
			}
		}

		[Test]
		public void Write_LdmlIsNicelyFormatted()
		{
#if MONO
				// mono inserts \r\n\t before xmlns where windows doesn't
			string expectedFileContent =
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='0001-01-01T00:00:00' />
		<language
			type='en' />
		<script
			type='Zxxx' />
		<territory
			type='US' />
		<variant
			type='x-audio' />
	</identity>
	<collations />
	<special
		xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='en' />
		<palaso:languageName
			value='English' />
		<palaso:version
			value='2' />
	</special>
</ldml>".Replace("'", "\"").Replace("\n", "\r\n");
#endregion

#else
			string expectedFileContent =
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='0001-01-01T00:00:00' />
		<language
			type='en' />
		<script
			type='Zxxx' />
		<territory
			type='US' />
		<variant
			type='x-audio' />
	</identity>
	<collations />
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='en' />
		<palaso:languageName
			value='English' />
		<palaso:version
			value='2' />
	</special>
</ldml>".Replace("'", "\"");
#endregion
#endif
			using (var file = new TempFile())
			{
				//Create an ldml fiel to read
				var adaptor = new LdmlDataMapper();
				var ws = WritingSystemDefinition.Parse("en-Zxxx-x-audio");
				adaptor.Write(file.Path, ws, null);

				//change the read writing system and write it out again
				var ws2 = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws2);
				ws2.Region = "US";
				adaptor.Write(file.Path, ws2, new MemoryStream(File.ReadAllBytes(file.Path)));

				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(expectedFileContent));
			}
		}

		[Test]
		public void Write_WritingSystemWasloadedFromLdmlThatContainedLayoutInfo_LayoutInfoIsOnlyWrittenOnce()
		{
			using (var file = new TempFile())
			{
				//create an ldml file to read that contains layout info
				var adaptor = new LdmlDataMapper();
				var ws = WritingSystemDefinition.Parse("en-Zxxx-x-audio");
				ws.RightToLeftScript = true;
				adaptor.Write(file.Path, ws, null);

				//read the file and write it out unchanged
				var ws2 = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws2);
				adaptor.Write(file.Path, ws2, new MemoryStream(File.ReadAllBytes(file.Path)));

				AssertThatXmlIn.File(file.Path).HasNoMatchForXpath("/ldml/layout[2]");
			}
		}

		[Test]
		public void Read_ValidLanguageTagStartingWithXButVersion0_Throws()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("xh", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				Assert.That(() => adaptor.Read(file.Path, new WritingSystemDefinition()), Throws.Exception.TypeOf<ApplicationException>());
			}
		}

		[Test]
		public void WriteNoRoundTrip_LdmlIsFlexPrivateUseFormatLanguageOnly_LdmlIsChanged()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path,ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), false), WritingSystemCompatibility.Strict);
				AssertThatLdmlMatches("", "", "", "x-en", file);
			}
		}

		[Test]
		public void WriteNoRoundTrip_LdmlIsFlexPrivateUseFormatlanguageAndScript_LdmlIsChanged()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), false), WritingSystemCompatibility.Strict);
				AssertThatLdmlMatches("qaa", "Zxxx", "", "x-en", file);
			}
		}

		[Test]
		public void WriteRoundTrip_LdmlIsFlexPrivateUseFormat_LdmlIsUnchanged()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), true), WritingSystemCompatibility.Flex7V0Compatible);
				AssertThatLdmlMatches("x-en", "", "", "", file);
			}
		}

		[Test]
		public void WriteRoundTrip_LdmlIsValidLanguageStartingWithX_LdmlIsUnchanged()
		{
			using (var file = new TempFile())
			{
				WriteVersion2Ldml("xh", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), true), WritingSystemCompatibility.Flex7V0Compatible);
				AssertThatLdmlMatches("xh", "", "", "", file);
				AssertThatVersionIs(2, file);
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatOnlyLanguageIsPopulated_WritingSystemHasDataInPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en");
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndScriptArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "", "x-en");
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndTerritoryArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "US", "x-en");
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndVariantArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "fonipa", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "", "fonipa-x-en");
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatAllFieldsArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "US", "1901-x-en-audio");
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndPrivateUseIsPopulated_LanguageTagIsMovedAndIsFirstInPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "x-private", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en-private");
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageIsPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndScriptPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "", "x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndTerritoryPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "US", "x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndVariantPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "fonipa", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "", "fonipa-x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageIsOnlyX_AllFieldsPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "US", "1901-x-audio");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_AllFieldsPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "US", "1901-x-en-audio");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndPrivateUsePopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "x-private", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en-private");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedLanguage_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en");
				ws.Language = "de";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("de", "", "", "x-en", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedScript_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "", "x-en");
				ws.Script = "Latn";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("qaa", "Latn", "", "x-en", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedRegion_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "US", "x-en");
				ws.Region = "GB";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("qaa", "", "GB", "x-en", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedVariant_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "fonipa", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "", "fonipa-x-en");
				ws.Variant = "1901-x-en";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("qaa", "", "", "1901-x-en", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedPrivateUse_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "x-private", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en-private");
				ws.Variant = "x-en-changed";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("", "", "", "x-en-changed", file);
			}
		}

		[Test]
		public void Read_ReadPrivateUseWsFromFieldWorksLdmlThenNormalLdmlMissingVersion1Element_Throws()
		{
			using (var badFlexLdml = new TempFile())
			{
				using (var version1Ldml = new TempFile())
				{
					WriteVersion0Ldml("x-en", "", "", "x-private", badFlexLdml);
					WriteVersion0Ldml("en", "", "", "", version1Ldml);
					var wsV1 = new WritingSystemDefinition();
					var wsV0 = new WritingSystemDefinition();
					var adaptor = new LdmlDataMapper();
					adaptor.Read(badFlexLdml.Path, wsV0);
					Assert.Throws<ApplicationException>(()=>adaptor.Read(version1Ldml.Path, wsV1));
				}
			}
		}

		[Test]
		public void Write_WritePrivateUseWsFromFieldWorksLdmlThenNormalLdml_ContainsVersion1()
		{
			using (var badFlexLdml = new TempFile())
			{
				using (var version1Ldml = new TempFile())
				{
					var namespaceManager = new XmlNamespaceManager(new NameTable());
					namespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
					WriteVersion0Ldml("x-en", "", "", "x-private", badFlexLdml);
					var wsV0 = new WritingSystemDefinition();
					var adaptor = new LdmlDataMapper();
					adaptor.Read(badFlexLdml.Path, wsV0);
					adaptor.Write(badFlexLdml.Path, wsV0, new MemoryStream(File.ReadAllBytes(badFlexLdml.Path)));
					var wsV1 = new WritingSystemDefinition();
					adaptor.Write(version1Ldml.Path, wsV1, null);
					AssertThatVersionIs(2, version1Ldml);
				}
			}
		}

		[Test]
		public void Read_NonDescriptLdml_WritingSystemIdIsSameAsRfc5646Tag()
		{
			using (var file = new TempFile())
			{
				WriteVersion2Ldml("en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("en-Zxxx-US-1901-x-audio"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguageScriptRegionVariant_WritingSystemIdIsConcatOfLanguageScriptRegionVariant()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en-Zxxx-US-1901-x-audio"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguage_WritingSystemIdIsLanguage()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguageScript_WritingSystemIdIsConcatOfLanguageScript()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en-Zxxx"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguageRegion_WritingSystemIdIsConcatOfLanguageRegion()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en-US"));
			}
		}

		[Test]
		public void Read_V0Ldml_ThrowFriendlyException()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();
				Assert.That(() => dataMapper.Read(file.Path, ws), Throws.Exception.TypeOf<ApplicationException>().With.Property("Message").EqualTo(String.Format("The LDML tag 'en' is version 0.  Version {1} was expected.", file.Path, WritingSystemDefinition.LatestWritingSystemDefinitionVersion)));
			}
		}

		private static void AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(WritingSystemDefinition ws, string language, string script, string territory, string variant)
		{
			Assert.That(ws.Language, Is.EqualTo(language));
			Assert.That(ws.Script, Is.EqualTo(script));
			Assert.That(ws.Region, Is.EqualTo(territory));
			Assert.That(ws.Variant, Is.EqualTo(variant));
		}

		private static void WriteVersion0Ldml(string language, string script, string territory, string variant, TempFile file)
		{
			//using a writing system V0 here because the real writing system can't cope with the way
			//flex encodes private-use language and shouldn't. But using a writing system + ldml adaptor
			//is the quickest way to generate ldml so I'm using it here.
			var ws = new WritingSystemDefinitionV0
						 {ISO639 = language, Script = script, Region = territory, Variant = variant};
			new LdmlAdaptorV0().Write(file.Path, ws, null);
		}

		private static void WriteVersion2Ldml(string language, string script, string territory, string variant, TempFile file)
		{
			var ws = new WritingSystemDefinition { Language = language, Script = script, Region = territory, Variant = variant };
			new LdmlDataMapper().Write(file.Path, ws, null);
		}

		private static void AssertThatLdmlMatches(string language, string script, string territory, string variant, TempFile file)
		{
			AssertThatIdentityElementIsCorrectForContent("language", language, file);
			AssertThatIdentityElementIsCorrectForContent("script", script, file);
			AssertThatIdentityElementIsCorrectForContent("territory", territory, file);
			AssertThatIdentityElementIsCorrectForContent("variant", variant, file);
		}

		private static void AssertThatIdentityElementIsCorrectForContent(string element, string content, TempFile file)
		{
			if (String.IsNullOrEmpty(content) && element != "language")
			{
				AssertThatXmlIn.File(file.Path).HasNoMatchForXpath(String.Format("/ldml/identity/{0}", element));
				return;
			}
			AssertThatXmlIn.File(file.Path).HasAtLeastOneMatchForXpath(String.Format("/ldml/identity/{0}[@type='{1}']", element, content));
		}

		private static void AssertThatVersionIs(int expectedVersion, TempFile file)
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");

			if(expectedVersion == 0)
			{
				AssertThatXmlIn.File(file.Path).HasNoMatchForXpath("/ldml/special/palaso:version[@value]", namespaceManager);
			}
			else
			{
				AssertThatXmlIn.File(file.Path).HasAtLeastOneMatchForXpath(
					String.Format("/ldml/special/palaso:version[@value='{0}']", expectedVersion), namespaceManager);
			}
		}
	}
}
