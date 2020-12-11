using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.Lift.Parsing;
using SIL.Lift.Validation;

namespace SIL.Lift.Tests.Validation
{
	[TestFixture]
	public class ValidatorTests
	{

		[Test]
		public void LiftVersion_MatchesEmbeddedSchemaVersion()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(typeof (LiftMultiText).Assembly.GetManifestResourceStream("SIL.Lift.Validation.lift.rng"));
			string query = String.Format("//x:attribute/x:value[.='{0}']", Validator.LiftVersion);
			XmlNamespaceManager m = new XmlNamespaceManager(doc.NameTable);
			m.AddNamespace("x", "http://relaxng.org/ns/structure/1.0");
			Assert.IsNotNull(doc.FirstChild.SelectSingleNode(query, m));
		}

		[Test]
		public void Validate_NoEntriesValidates()
		{
			string contents = string.Format("<lift version='{0}'></lift>", Validator.LiftVersion);
			Validate(contents, ValidationOptions.All, true);
		}

		[Test]
		public void Validate_FileHasEscapedDangerousUnicodeCharsInText_Validates()
		{
			string contents = string.Format(@"
			<lift version='{0}'>
				 <entry id='one'>
					<lexical-unit>
						  <form lang='bth'>
							<text>&#x1F;</text>
						 </form>
					</lexical-unit>
				</entry>
				</lift>", Validator.LiftVersion);
			Validate(contents, ValidationOptions.All, true);
		}


		[Test]
		public void Validate_FileHasEscapedDangerousUnicodeCharsInId_Validates()
		{
			string contents = string.Format(@"
			<lift version='{0}'>
				 <entry id='one&#x1F;'>
				</entry>
				</lift>", Validator.LiftVersion);
			Validate(contents, ValidationOptions.All, true);
		}


		[Test]
		public void Validate_BadLift_DoesNotValidate()
		{
			string contents = "<lift version='0.10'><header></header><header></header></lift>";
			Validate(contents, ValidationOptions.All, false);
		}

		[Test]
		public void WrongVersionNumberGivesHelpfulMessage()
		{
			string contents = "<lift version='0.8'><header></header><header></header></lift>";
			string errors = Validate(contents, ValidationOptions.All, false);
			Assert.IsTrue(errors.Contains("This file claims to be version "));
		}

		[Test]
		public void ValidateGUIDs_FileHasDuplicateEntryGuids_DoesNotValidate()
		{
			string contents = string.Format(@"
			<lift version='{0}'>
				 <entry guid='1'>
				</entry>
				 <entry guid='1'>
				</entry>
				</lift>", Validator.LiftVersion);
			Validate(contents, ValidationOptions.CheckGUIDs, false);
		}
		[Test]
		public void ValidateGUIDs_FileHasNoDuplicateGuids_Validates()
		{
			string contents = string.Format(@"
			<lift version='{0}'>
				 <entry guid='1'>
				</entry>
				 <entry guid='2'>
				</entry>
				</lift>", Validator.LiftVersion);
			Validate(contents, ValidationOptions.CheckGUIDs, true);
		}


		private static string Validate(string contents, ValidationOptions validationOptions, bool shouldPass)
		{
			string f = Path.GetTempFileName();
			File.WriteAllText(f, contents);
			string errors;
			try
			{
				errors = Validator.GetAnyValidationErrors(f, new NullValidationProgress(),  validationOptions);
				if (shouldPass)
				{
					if (errors != null)
					{
						Console.WriteLine(errors);
					}
					Assert.That(errors, Is.Null.Or.Empty);
				}
				else
				{
					Assert.Greater(errors.Length,0);
				}
			}
			finally
			{
				File.Delete(f);
			}
			return errors;
		}
	}
}