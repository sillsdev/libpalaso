using System.IO;
using NUnit.Framework;
using SIL.Lift.Migration;
using SIL.Lift.Validation;

namespace SIL.Lift.Tests.Migration
{
	[TestFixture]
	public class V11To12MigratorTests : MigratorTestBase
	{
		[Test]
		public void HasEtymologyInSense_EtymologyMovedToEntry()
		{
			using (TempFile f = new TempFile("<lift version='0.11'><entry><sense><etymology/></sense></entry></lift>"))
			{
				string path = Migrator.MigrateToLatestVersion(f.Path);
				try
				{
					using (TempFile.TrackExisting(path))
					{
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
		public void Version11_ChangedToVersionLatest()
		{
			using (TempFile f = new TempFile("<lift version='0.11'></lift>"))
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
		public void PreservesProducer()
		{
			using (TempFile f = new TempFile("<lift version='0.11' producer='p'/>"))
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