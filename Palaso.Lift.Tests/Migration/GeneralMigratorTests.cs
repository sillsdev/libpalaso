using System;
using System.IO;
using System.Xml;
using Palaso.Lift.Migration;
using Palaso.Lift.Validation;
using NUnit.Framework;

namespace Palaso.Lift.Tests.Migration
{
	[TestFixture]
	public class GeneralMigratorTests
	{
		[Test]
		public void IsMigrationNeeded_Latest_ReturnsFalse()
		{
			using (TempFile f = new TempFile(string.Format("<lift version='{0}'></lift>", Validator.LiftVersion)))
			{
				Assert.IsFalse(Migrator.IsMigrationNeeded(f.Path));
			}
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void MigrateToLatestVersion_HasCurrentVersion_Throws()
		{
			using (TempFile f = new TempFile(string.Format("<lift version='{0}'></lift>", Validator.LiftVersion)))
			{
				Migrator.MigrateToLatestVersion(f.Path);
			}
		}

		[Test]
		public void MigrateToLatestVersion_IsOldVersion_ReturnsDifferentPath()
		{
			using (TempFile f = new TempFile("<lift version='0.10'></lift>"))
			{
				Assert.AreNotEqual(f.Path, Migrator.MigrateToLatestVersion(f.Path));
			}
		}

		/// <summary>
		/// this is important because if we change the behavior to use, say, temp,
		/// that could be a different volumne, which can make some File operations
		/// fail (like rename).
		/// </summary>
		[Test]
		public void MigrateToLatestVersion_ResultingFileInSameDirectory()
		{
			using (TempFile f = new TempFile("<lift version='0.10'></lift>"))
			{
				Assert.AreEqual(Path.GetDirectoryName(f.Path), Path.GetDirectoryName(Migrator.MigrateToLatestVersion(f.Path)));
			}
		}

		[Test, ExpectedException(typeof(LiftFormatException))]
		public void MigrateToLatestVersion_VersionWithoutMigrationXsl_Throws()
		{
			using (TempFile f = new TempFile("<lift version='0.5'></lift>"))
			{
				Migrator.MigrateToLatestVersion(f.Path);
			}
		}


	}
}