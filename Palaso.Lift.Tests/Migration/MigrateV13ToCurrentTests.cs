using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.Lift.Migration;

namespace Palaso.Lift.Tests.Migration
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class MigrateV13ToCurrentTests : MigratorTestBase
	{
		///--------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the method MigrateV13File.
		/// </summary>
		///--------------------------------------------------------------------------------------
		[Test]
		public void MigrateV13File()
		{
			// For this test to work, the file test20111130.lift MUST be copied to the current
			// working directory.
			var path = Migrator.MigrateToLatestVersion("test20111130.lift");
			var doc = new XmlDocument();
			try
			{
				doc.Load(path);
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				Console.WriteLine(File.ReadAllText(path));
			}
			// verify that field definition elements have been renamed to field-definition.
			AssertXPathNotFound(doc, "lift/header/fields/field");
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition");

			// verify that field definition element "tag" attribute has been renamed to "name"
			AssertXPathNotFound(doc, "/lift/header/fields/field-definition/@tag");
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition/@name");

			// verify that cv-pattern, tone, and comment are no longer defined as fields, and
			// that import-residue still is.
			AssertXPathNotFound(doc, "/lift/header/fields/field-definition[@name='cv-pattern']");
			AssertXPathNotFound(doc, "/lift/header/fields/field-definition[@name='tone']");
			AssertXPathNotFound(doc, "/lift/header/fields/field-definition[@name='comment']");
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition[@name='import-residue']");

			// verify that the descriptions in the field definitions have been moved inside a
			// description element
			AssertXPathNotFound(doc, "/lift/header/fields/field-definition/form");
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition/description/form");

			// verify that the pseudo-descriptions have been removed (and presumably converted)
			AssertXPathNotFound(doc, "/lift/header/fields/field-definition/description/form[@lang='x-spec']");
			AssertXPathNotFound(doc, "/lift/header/fields/field-definition/description/form[@lang='qaa-x-spec']");

			// verify that at least one of each new field definition attribute has been generated
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition/@class");
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition/@type");
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition/@option-range");
			AssertXPathAtLeastOne(doc, "/lift/header/fields/field-definition/@writing-system");
		}
	}
}
