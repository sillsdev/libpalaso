using NUnit.Framework;
using SIL.Lift.Migration;
using SIL.Lift.Validation;

namespace SIL.Lift.Tests.Migration
{
	[TestFixture]
	public class V13To12MigratorTests : MigratorTestBase
	{

		[Test]
		public void Version13_Changed12()
		{
			using (TempFile f = new TempFile("<lift version='0.13'></lift>"))
			{
				using (TempFolder x = new TempFolder("13-12LiftMigrator"))
				{
					TempFile toFile = x.GetPathForNewTempFile(false);

					Migrator.ReverseMigrateFrom13ToFLEx12(f.Path, toFile.Path);
					Assert.AreEqual("0.12", Validator.GetLiftVersion(toFile.Path));
				}
			}
		}


		[Test]
		public void HasScientificName_MovedToOldStyleName()
		{
			using (TempFile fromFile = new TempFile("<lift version='0.13' producer='tester'>" +
				"<entry>" +
					"<trait name='MorphType' value='stem'></trait>"+
					"<sense>" +
						"<field type='scientific-name'><form lang='en'><text>word of science!</text></form></field>"+
					"</sense>" +
				"</entry>" +
				"</lift>"))
			{
				using (TempFolder x = new TempFolder("13-12LiftMigrator"))
				{
					TempFile toFile = x.GetPathForNewTempFile(false);

					Migrator.ReverseMigrateFrom13ToFLEx12(fromFile.Path, toFile.Path);
					AssertXPathAtLeastOne("//lift[@producer='tester']", toFile.Path);
					AssertXPathAtLeastOne("//entry/sense/field[@type='scientific_name']", toFile.Path);
				}
			}
		}
		[Test]
		public void SemanticDomainWeSayStyle_ConvertedToFLExStyle()
		{
			using (TempFile fromFile = new TempFile("<lift version='0.13' producer='tester'>" +
				"<entry>" +
					"<trait name='MorphType' value='stem'></trait>" +
					"<sense>" +
						"<trait name='semantic-domain-ddp4' value='2.5 something or other'></trait>" +
						"<trait name=\"semantic-domain-ddp4\" value=\"3.0.2.3 something using quotes\"></trait>" +
					"</sense>" +
				"</entry>" +
				"</lift>"))
			{
				using (TempFolder x = new TempFolder("13-12LiftMigrator"))
				{
					// NB: No need for Dispose on this one.
					TempFile toFile = x.GetPathForNewTempFile(false);

					Migrator.ReverseMigrateFrom13ToFLEx12(fromFile.Path, toFile.Path);
					AssertXPathAtLeastOne("//entry/sense/trait[@name='semantic_domain']", toFile.Path);
					AssertXPathAtLeastOne("//entry/sense/trait[@value='2.5']", toFile.Path);
					AssertXPathAtLeastOne("//entry/sense/trait[@value='3.0.2.3']", toFile.Path);
				}
			}
		}
		[Test]
		public void SemanticDomainWeSayStyleTwoSemDomsOnOneLine_BothConverted()
		{
			using (TempFile fromFile = new TempFile("<lift version='0.13' producer='tester'><entries>" +
			@"<entry id='color'><sense>
			<trait name='semantic_domain' value='5.2.6.2.2 Basket-ball' /><trait name='semantic_domain' value='4.2.6.2.1 Football, soccer(american)' />
				 </sense></entry></entries></lift>"))
			{
				using (TempFolder x = new TempFolder("13-12LiftMigrator"))
				{
					TempFile toFile = x.GetPathForNewTempFile(false);

					Migrator.ReverseMigrateFrom13ToFLEx12(fromFile.Path, toFile.Path);
					AssertXPathAtLeastOne("//entry/sense/trait[@name='semantic_domain']", toFile.Path);
					AssertXPathAtLeastOne("//entry/sense/trait[@value='5.2.6.2.2']", toFile.Path);
					AssertXPathAtLeastOne("//entry/sense/trait[@value='4.2.6.2.1']", toFile.Path);
				}
			}
		}
	}
}