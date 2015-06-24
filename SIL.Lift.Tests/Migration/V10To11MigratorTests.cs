using System.IO;
using NUnit.Framework;
using SIL.Lift.Migration;
using SIL.Lift.Validation;

namespace SIL.Lift.Tests.Migration
{
	[TestFixture]
	public class V10To11MigratorTests : MigratorTestBase
	{
		[Test]
		public void IsMigrationNeeded_ReturnsTrue()
		{
			using (TempFile f = new TempFile("<lift version='0.10'></lift>"))
			{
				Assert.IsTrue(Migrator.IsMigrationNeeded(f.Path));
			}
		}

		[Test]
		public void MigrateToLatestVersion_ConvertedToLatest()
		{
			using (TempFile f = new TempFile("<lift version='0.10'></lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					Assert.AreEqual(Validator.LiftVersion, Validator.GetLiftVersion(path));
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void RelationHasNameAttribute_ChangedToType()
		{
			using (TempFile f = new TempFile("<lift version='0.10'><entry><relation order='2' ref='xyz' name='foo'/></entry></lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					using (TempFile.TrackExisting(path))
					{
						AssertXPathAtLeastOne("//relation[@order='2' and @ref='xyz' and @type='foo']", path);
						AssertXPathNotFound("//relation/@name", path);
					}
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void HasPicture_ChangedToIllustration()
		{
			using (TempFile f = new TempFile(@"
				<lift version='0.10'>
					  <entry>
						<sense>
						  <illustration href='waterBasket1.png'/>
						</sense>
					  </entry>
				</lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					AssertXPathAtLeastOne("//sense/illustration[@href='waterBasket1.png']", path);
					AssertXPathNotFound("//sense/picture", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void GlossHasTrait_ChangedToAnnotation()
		{
			using (TempFile f = new TempFile(@"
				<lift version='0.10'>
					  <entry>
						<sense>
						  <gloss lang='en'>
							<text>water carrying basket</text>
							<trait name='flag' value='1' />
						  </gloss>
						</sense>
					  </entry>
				</lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					AssertXPathAtLeastOne("//entry/sense/gloss/annotation[@value='1']", path);
					AssertXPathNotFound("//entry/sense/gloss/trait", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}


		[Test]
		public void FormHasTrait_ChangedToAnnotation()
		{
			using (TempFile f = new TempFile(@"
				<lift version='0.10'>
					  <entry>
						<lexical-unit>
						  <form lang='bth'>
							<text>abit</text>
							 <trait name='flag' value='1' />
						 </form>
						</lexical-unit>
					  </entry>
				</lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					using (TempFile.TrackExisting(path))
					{
						AssertXPathAtLeastOne("//entry/lexical-unit/form/annotation[@value='1']", path);
						AssertXPathNotFound("//entry/lexical-unit/form/trait", path);
					}
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void FieldHasTag_ChangedToType()
		{
			using (TempFile f = new TempFile(@"
				<lift version='0.10'>
					  <entry>
						<field tag='test'/>
					  </entry>
				</lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					AssertXPathAtLeastOne("//entry/field[@type='test']", path);
					AssertXPathNotFound("//entry/field[@tag='test']", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}


		[Test]
		public void FieldInHeaderHasTag_NotChangedToType()
		{
			using (TempFile f = new TempFile(@"
				<lift version='0.10'>
					  <header>
						<fields>
							<field tag='test'>
							  <form lang='en'><text>item 1</text></form>
							</field>
						</fields>
					  </header>
				</lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					AssertXPathAtLeastOne("//header//field[@tag='test']", path);
					AssertXPathNotFound("//header//field[@type='test']", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}

		[Test]
		public void PreservesProducer()
		{
			using (TempFile f = new TempFile("<lift version='0.10' producer='p'/>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					AssertXPathAtLeastOne("//lift[@producer='p']", path);
				}
				finally
				{
					File.Delete(path);
				}
			}
		}
	}
}