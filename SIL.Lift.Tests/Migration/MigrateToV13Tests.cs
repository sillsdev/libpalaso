using System.IO;
using NUnit.Framework;
using SIL.Lift.Migration;
using SIL.Lift.Validation;

namespace SIL.Lift.Tests.Migration
{
	[TestFixture]
	public class MigrateToV13Tests : MigratorTestBase
	{
		[Test]
		public void LiftVersion_Was0Point12_IsSetTo0Point13()
		{
			using (var f = new TempFile("<lift version='0.12' producer='testing'/>"))
			{
				var path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					Assert.AreEqual(Validator.LiftVersion, Validator.GetLiftVersion(path));
					AssertXPathAtLeastOne("//lift[@producer='testing']", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void SenseLiteralDefinition_WasOnSense_MovedToEntry()
		{
			using (TempFile f = new TempFile("<lift version='0.12' producer='tester'>" +
				"<entry>" +
				"<sense>" +
				"<field type='LiteralMeaning' dateCreated='2009-03-31T08:28:37Z'><form lang='en'><text>trial</text></form></field>" +
				"<trait name='SemanticDomainDdp4' value='6.1.2.9 Opportunity'/>" +
				"</sense>" +
				"</entry>" +
				"</lift>"))
			{
				var path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					Assert.AreEqual(Validator.LiftVersion, Validator.GetLiftVersion(path));
					AssertXPathAtLeastOne("//lift[@producer='tester']", path);
					AssertXPathAtLeastOne("//entry/field[@type='literal-meaning']", path);
					AssertXPathNotFound("//entry/sense/field", path);
					AssertXPathAtLeastOne("//entry/sense/trait[@name='semantic-domain-ddp4']", path);
					AssertXPathNotFound("//entry/sense/trait[@name='SemanticDomainDdp4']", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void FileWas0Point11_EtymologyInSense_MovedToEntry()
		{
			using (TempFile f = new TempFile("<lift version='0.11' producer='testing'><entry><sense><etymology/></sense></entry></lift>"))
			{
				var path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					using (TempFile.TrackExisting(path))
					{
						Assert.AreEqual(Validator.LiftVersion, Validator.GetLiftVersion(path));
						AssertXPathAtLeastOne("//lift[@producer='testing']", path);
						AssertXPathAtLeastOne("//entry/etymology", path);
					}
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void TraitHasOldSkipFlag_ChangedToHyphenatedForm()
		{
			using (TempFile f = new TempFile(@"
				<lift version='0.12'>
					  <entry>
						<trait name='flag_skip_FooBar' value='set' />
					  </entry>
				</lift>"))
			{
				var path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					AssertXPathNotFound("//entry/trait[@name='flag_skip_FooBar']", path);
					AssertXPathAtLeastOne("//entry/trait[@name='flag-skip-FooBar']", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}
	}
}
