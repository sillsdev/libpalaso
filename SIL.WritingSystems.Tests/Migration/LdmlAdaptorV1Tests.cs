using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.IO;
using SIL.WritingSystems.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	public class LdmlAdaptorV1Tests
	{
		[Test]
		public void Read_ValidLanguageTagStartingWithXButVersion0_Throws()
		{
			using (TempFile version0Ldml = new TempFile())
			{
				using (var writer = new StreamWriter(version0Ldml.Path, false, Encoding.UTF8))
				{
					writer.Write(LdmlContentForTests.Version0("xh", "", "", ""));
				}
				var wsV1 = new WritingSystemDefinitionV1();
				var adaptor = new LdmlAdaptorV1();
				Assert.Throws<ApplicationException>(() => adaptor.Read(version0Ldml.Path, wsV1));
			}
		}

		[Test]
		public void Read_ReadPrivateUseWsFromFieldWorksLdmlThenNormalLdmlMissingVersion1Element_Throws()
		{
			using (TempFile badFlexLdml = new TempFile(),
							version0Ldml = new TempFile())
			{
				using (var writer = new StreamWriter(badFlexLdml.Path, false, Encoding.UTF8))
				{
					writer.Write(LdmlContentForTests.Version0("x-en", "", "", "x-private"));
				}
				using (var writer = new StreamWriter(version0Ldml.Path, false, Encoding.UTF8))
				{
					writer.Write(LdmlContentForTests.Version0("en", "", "", ""));
				}
				var wsV1 = new WritingSystemDefinitionV1();
				var wsV0 = new WritingSystemDefinitionV1();
				var adaptor = new LdmlAdaptorV1();
				adaptor.Read(badFlexLdml.Path, wsV0);
				Assert.Throws<ApplicationException>(() => adaptor.Read(version0Ldml.Path, wsV1));
			}
		}

		[Test]
		public void Write_WritePrivateUseWsFromFieldWorksLdmlThenNormalLdml_ContainsVersion2()
		{
			using (TempFile badFlexLdml = new TempFile(),
							version1Ldml = new TempFile())
			{
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
				using (var writer = new StreamWriter(badFlexLdml.Path, false, Encoding.UTF8))
				{
					writer.Write(LdmlContentForTests.Version0("x-en", "", "", "x-private"));
				}
				var wsV0 = new WritingSystemDefinitionV1();
				var adaptor = new LdmlAdaptorV1();
				adaptor.Read(badFlexLdml.Path, wsV0);
				adaptor.Write(badFlexLdml.Path, wsV0, new MemoryStream(File.ReadAllBytes(badFlexLdml.Path)));
				var wsV1 = new WritingSystemDefinitionV1();
				adaptor.Write(version1Ldml.Path, wsV1, null);
				var versionReader = new WritingSystemLdmlVersionGetter();
				Assert.That(versionReader.GetFileVersion(version1Ldml.Path), Is.EqualTo(2));
			}
		}
	}
}